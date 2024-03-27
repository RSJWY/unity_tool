using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using Newtonsoft.Json.Linq;
using Script.AOT.LoadTool;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using YooAsset;



/// <summary>
/// 我的工具
/// </summary>
public static class MyEditorTool
{
    /// <summary>
    /// Yoo数据目录
    /// </summary> 
    private const string YooAssetsFolderName = "ShiJieYiCanAsset";

    /// <summary>
    /// 需要加载的DLL列表
    /// </summary>
    private const string HclrHotDLLJsonFileName = "HCLRHotDLL.json";


    [MenuItem("我的工具/一键构建热更新所需要的DLL并生成Json文件", false, 1)]
    public static void First_AutoBuildHotFixAndHotPackage()
    {

        if (!MyEditorTool.AutoSaveScence())
        {
            Debug.LogError("场景保存失败");
            return;
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成DLL，目标平台：{target}");

        // AOTDllsMethod();
        // HotDllsMethod(target);
        AddAotArr();
        AOTDllsMethod();
        BuildHotfixDllsToPackage();
        BuildDLLJson();
        RunBuildinFileManifest();


        Debug.Log($"构建完成");
    }
    /// <summary>
    /// 构建热更新DLL到资源包文件夹
    /// </summary>
    [MenuItem("我的工具/构建热更新DLL到资源包文件夹", false, 4)]
    public static void BuildHotfixDllsToPackage()
    {
        if (!MyEditorTool.AutoSaveScence())
        {
            Debug.LogError("场景保存失败");
            return;
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成热更新程序集DLL，目标平台：{target}");
        //构建热更新代码
        CompileDllCommand.CompileDll(target);
        //拷贝到资源包
        Debug.Log($"拷贝热更新代码到资源包，构建模式为{target.ToString()}");
        //获取构建DLL的路径
        var hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        var yoodllPath = $"{Application.dataPath}/{YooAssetsFolderName}/HotCodeDll";
        MyTool_AOT.CheckDirectoryExistsAndCreate(yoodllPath);
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            string dllBytesPath = $"{yoodllPath}/{dll}.bytes";//File.Copy(srcDllPath, dllBytesPath, true);
            File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[拷贝热更代码到热更包] 拷贝 {dllPath} -> 到{dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 根据列表生成Json文件
    /// </summary>
    [MenuItem("我的工具/根据列表生成Json文件", false, 5)]
    public static void BuildDLLJson()
    {
        //读取列表
        var aotAssemblies = HybridCLRSettings.Instance.patchAOTAssemblies.ToList();
        var hotDllDef = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions.ToList();
        //List<string> _HotdllName = HybridCLRSettings.Instance.hotUpdateAssemblies.ToList();
        List<string> hotdlls = new();
        //整理
        foreach (AssemblyDefinitionAsset dlldef in hotDllDef)
        {
            hotdlls.Add(dlldef.name);
        }
        JObject hclrLoadDLLJsonFile = new();
        hclrLoadDLLJsonFile.Add("MetadataForAOTAssemblies", JArray.FromObject(aotAssemblies));
        hclrLoadDLLJsonFile.Add("HotUpdateDLL", JArray.FromObject(hotdlls));
        string path = Application.dataPath + $"/{YooAssetsFolderName}/Config/{HclrHotDLLJsonFileName}";
        MyTool_AOT.CheckDirectoryAndFileCreate($"Application.dataPath/{YooAssetsFolderName}/Config",
            HclrHotDLLJsonFileName);
        File.WriteAllText(path, hclrLoadDLLJsonFile.ToString());
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 构建补充元数据列表到HybridCLR设置
    /// </summary>
    [MenuItem("我的工具/构建补充元数据列表到HybridCLR设置", false, 2)]
    public static void AddAotArr()
    {
        //生成补充元数据表
        AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
        AssetDatabase.Refresh();

        var aotdlls = AOTGenericReferences.PatchedAOTAssemblyList.ToList();
        //处理信息
        var temp = new List<string>();
        foreach (string str in aotdlls)
        {
            var _s = str.Replace(".dll", "");
            temp.Add(_s);
        }
        //保存处理的数据
        HybridCLRSettings.Instance.patchAOTAssemblies = temp.ToArray();
        HybridCLRSettings.Save();
        AssetDatabase.Refresh();
        //输出结果方便复制
        StringBuilder listStr = new();
        listStr.Append($"\n");
        foreach (string str in aotdlls)
        {
            listStr.Append($"\n{str}.bytes");
        }
        listStr.Append($"\n");
        AssetDatabase.Refresh();
        Debug.Log($"补充元数据表如下{listStr.ToString()}");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 构建补充元数据到资源目录
    /// </summary>
    [MenuItem("我的工具/构建补充元数据到资源目录", false, 3)]
    static void AOTDllsMethod()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成补充元数据程序集DLL，目标平台：{buildTarget}");
        //构建补充元数据
        StripAOTDllCommand.GenerateStripedAOTDlls();
        //拷贝到资源包
        Debug.Log($"拷贝补充元数据到资源包，构建模式为{buildTarget.ToString()}");
        //获取构建DLL的路径
        var aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
        var yoodllPath = $"{Application.dataPath}/{YooAssetsFolderName}/MetadataForAOTAssemblies";
        MyTool_AOT.CheckDirectoryExistsAndCreate(yoodllPath);
        //迭代资源目录
        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                Debug.LogError(
                    $"添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先运行一次ALL后再打包。");
                continue;
            }
            string dllBytesPath = $"{yoodllPath}/{dll}.dll.bytes"; 
            //File.Copy(srcDllPath, dllBytesPath, true);
            // byte[] _rawByte=File.ReadAllBytes(srcDllPath);
            // byte[] _aesByte= MyTool_AOT.AESEncrypt(_rawByte,MyTool_AOT.AESkey);
            // File.WriteAllBytes(dllBytesPath,_aesByte);
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[拷贝补充元数据到热更包] 拷贝 {srcDllPath} -> 到{dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 生成内置资源内容清单
    /// </summary>
    [MenuItem("我的工具/生成内置资源内容清单", false, 6)]
    public static void RunBuildinFileManifest()
    {
        var saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
        DirectoryInfo root = new DirectoryInfo(folderPath);
        if (root.Exists == false)
        {
            Debug.LogWarning($"没有发现YooAsset内置目录 : {folderPath}，请构建一次");
            return;
        }

        var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();
        FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
        foreach (var fileInfo in files)
        {
            if (fileInfo.Extension == ".meta")
                continue;
            if (fileInfo.Name.StartsWith("PackageManifest_"))
                continue;

            BuildinFileManifest.Element element = new BuildinFileManifest.Element();
            element.PackageName = fileInfo.Directory.Name;
            element.FileCRC32 = YooAsset.Editor.EditorTools.GetFileCRC32(fileInfo.FullName);
            element.FileName = fileInfo.Name;
            manifest.BuildinFiles.Add(element);
        }

        if (Directory.Exists("Assets/Resources") == false)
            Directory.CreateDirectory("Assets/Resources");
        UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"一共{manifest.BuildinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
    }
    /// <summary>
    /// 保存场景
    /// </summary>
    public static bool AutoSaveScence()
    {
        // if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        // {
        //     Debug.Log("Scene not saved");
        // }
        // else
        // {
        //     Debug.Log("Scene saved");
        // }
        Debug.Log("场景保存");
        bool issave = EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        return issave;
    }



}
