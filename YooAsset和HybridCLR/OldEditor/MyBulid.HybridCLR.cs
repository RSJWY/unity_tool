using System.Collections;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;


/// <summary>
/// 我的构建——HybridCLR
/// </summary>
public static partial class MyBuild
{
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
    /// 补充元数据程序集方法
    /// </summary>
    static void AOTDllsMethod(BuildTarget buildTarget)
    {
        Debug.Log($"构建生成补充元数据程序集DLL，目标平台：{buildTarget}");
        //构建补充元数据
        StripAOTDllCommand.GenerateStripedAOTDlls();
        //拷贝到资源包
        CopyPatchAOTToHotPackage(buildTarget);
    }
    /// <summary>
    /// 拷贝热更新代码数据到热更包
    /// </summary>
    static void CopyHotfixDLLToHotPackage(BuildTarget target)
    {
        Debug.Log($"拷贝热更新代码到资源包，构建模式为{target.ToString()}");
        //获取构建DLL的路径
        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            string dllBytesPath = $"{hotfixAssembliesPath}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
            //AESEncrypt(dllBytesPath);
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
        //迭代资源目录
        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                Debug.LogError($"添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            string dllBytesPath = $"{PatchAOTAssembliesPath}/{dll}.dll.bytes";
            File.Copy(srcDllPath, dllBytesPath, true);
            //AESEncrypt(dllBytesPath);
            Debug.Log($"[拷贝补充元数据到热更包] 拷贝 {srcDllPath} -> 到{dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
}
