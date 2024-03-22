using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using Newtonsoft.Json.Linq;
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
    static string AssetName = "ShengMingShuData";




    [MenuItem("我的工具/一键构建热更新所需要的DLL", false, 1)]
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
        ADDAotArr();
        AOTDllsMethod();
        BuildHotfixDllsToPackage();
        BuildDLLJson();
        RunBuildinFileManifest();


        Debug.Log($"构建完成");
    }

    [MenuItem("我的工具/构建热更新DLL到资源包文件夹", false, 4)]
    public static void BuildHotfixDllsToPackage()
    {
        if (!MyEditorTool.AutoSaveScence())
        {
            Debug.LogError("场景保存失败");
            return;
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成DLL，目标平台：{target}");

        HotDllsMethod(target);
    }
    [MenuItem("我的工具/根据列表生成Json文件", false, 5)]
    public static void BuildDLLJson()
    {
        //读取列表
        List<string> _AOTAssemblies = HybridCLRSettings.Instance.patchAOTAssemblies.ToList();
        List<AssemblyDefinitionAsset> _HotDllDef = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions.ToList();
        //List<string> _HotdllName = HybridCLRSettings.Instance.hotUpdateAssemblies.ToList();
        List<string> hotdll = new();
        //整理
        foreach (AssemblyDefinitionAsset dlldef in _HotDllDef)
        {
            hotdll.Add(dlldef.name);
        }

        JObject HCLR_Load_DLL = new();
        HCLR_Load_DLL.Add("MetadataForAOTAssemblies", JArray.FromObject(_AOTAssemblies));
        HCLR_Load_DLL.Add("HotUpdateDLL", JArray.FromObject(hotdll));
        string path = Application.dataPath + $"/{AssetName}/Config/HCLR_Load_DLL.json";
        //文件是否存在
        if (!File.Exists(path))
        {
            File.Create(path);
        }
        File.WriteAllText(path, HCLR_Load_DLL.ToString());
        AssetDatabase.Refresh();
    }

    [MenuItem("我的工具/构建补充元数据列表到HybridCLR设置", false, 2)]
    public static void ADDAotArr()
    {
        //生成补充元数据表
        AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
        AssetDatabase.Refresh();

        List<string> aotdlls = AOTGenericReferences.PatchedAOTAssemblyList.ToList();
        //处理信息
        List<string> _temp = new List<string>();
        foreach (string str in aotdlls)
        {
            string _s = str.Replace(".dll", "");
            _temp.Add(_s);
        }
        //保存处理的数据
        HybridCLRSettings.Instance.patchAOTAssemblies = _temp.ToArray();
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
    }

    /// <summary>
    /// 补充元数据程序集方法
    /// </summary>
    [MenuItem("我的工具/构建补充元数据到资源目录", false, 3)]
    static void AOTDllsMethod()
    {

        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成补充元数据程序集DLL，目标平台：{buildTarget}");
        //构建补充元数据
        StripAOTDllCommand.GenerateStripedAOTDlls();
        //拷贝到资源包
        CopyPatchAOTToHotPackage(buildTarget);
    }


    [MenuItem("我的工具/生成内置资源内容清单", false, 6)]
    public static void RunBuildinFileManifest()
    {
        string saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
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

    /// <summary>
    /// 热更新程序集方法
    /// </summary>
    static void HotDllsMethod(BuildTarget buildTarget)
    {
        Debug.Log($"构建生成热更新程序集DLL，目标平台：{buildTarget}");
        //构建热更新代码
        CompileDllCommand.CompileDll(buildTarget);
        //拷贝到资源包
        CopyHotfixDLLToHotPackage(buildTarget);
    }

    /// <summary>
    /// 拷贝热更新代码数据到热更包
    /// </summary>
    static void CopyHotfixDLLToHotPackage(BuildTarget target)
    {
        Debug.Log($"拷贝热更新代码到资源包，构建模式为{target.ToString()}");
        //获取构建DLL的路径
        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string yoodllPath = $"{Application.dataPath}/{AssetName}/HotCodeDll";
        if (!Directory.Exists(yoodllPath))
        {
            Directory.CreateDirectory(yoodllPath);
        }
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
    /// 拷贝补充元数据到热更包
    /// </summary>
    static void CopyPatchAOTToHotPackage(BuildTarget target)
    {
        Debug.Log($"拷贝补充元数据到资源包，构建模式为{target.ToString()}");
        //获取构建DLL的路径
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string yoodllPath = $"{Application.dataPath}/{AssetName}/MetadataForAOTAssemblies";
        if (!Directory.Exists(yoodllPath))
        {
            Directory.CreateDirectory(yoodllPath);
        }
        //迭代资源目录
        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                Debug.LogError($"添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            string dllBytesPath = $"{yoodllPath}/{dll}.dll.bytes";//File.Copy(srcDllPath, dllBytesPath, true);
            // byte[] _rawByte=File.ReadAllBytes(srcDllPath);
            // byte[] _aesByte= MyTool_AOT.AESEncrypt(_rawByte,MyTool_AOT.AESkey);
            // File.WriteAllBytes(dllBytesPath,_aesByte);
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[拷贝补充元数据到热更包] 拷贝 {srcDllPath} -> 到{dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
}
