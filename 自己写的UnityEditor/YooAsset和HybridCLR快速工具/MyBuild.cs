using UnityEngine;
using HybridCLR.Editor;
using UnityEditor;
using HybridCLR.Editor.Commands;
using System.IO;
using YooAsset;
using UnityEditor.SceneManagement;
using YooAsset.Editor;


/// <summary>
/// 我的构建总
/// </summary>
public static partial class MyBuild
{
    
    /// <summary>
    /// 游戏资源包名
    /// </summary>
    static string PackageNameData="DataAndPrefab";
    /// <summary>
    /// 原生文件资源包名
    /// </summary>
    static string PackageNameRaw="RawFileAndHotUpdate";
    /// <summary>
    /// 构建模式
    /// </summary>
    static EBuildMode BuildMode=EBuildMode.ForceRebuild;
    /// <summary>
    /// 构建版本
    /// </summary>
    static string PackageVersion="V0.1";
    /// <summary>
    /// 补充元数据目标目录
    /// </summary> 
    static string PatchAOTAssembliesPath=Application.dataPath + "/AssetData/MetadataForAOTAssemblies";
    
    /// <summary>
    /// 热更新代码的资源目录
    /// </summary> 
    static string hotfixAssembliesPath = Application.dataPath+ "/AssetData/HotUpdateDll";


    [MenuItem("工具/一键构建（首包）",false,1)]
    public static void First_AutoBuildHotFixAndHotPackage(){

        if(!MyEditorTool.AutoSaveScence()){
            Debug.LogError("场景保存失败");
            return;
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成DLL，目标平台：{target}");

        AOTDllsMethod(target);
        HotDllsMethod(target);

        Debug.Log($"构建资源包，目标平台");
        AutoBuildDataAndPrefab();
        AutoBuildRawFileAndHotUpdate();

        Debug.Log($"构建完成");
    }

    [MenuItem("工具/构建热更新DLL到包",false,2)]
    public static void BuildHotfixDllsToPackage()
    {
        if(!MyEditorTool.AutoSaveScence()){
            Debug.LogError("场景保存失败");
            return;
        }
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"构建生成DLL，目标平台：{target}");

        HotDllsMethod(target);
    }

    
}


