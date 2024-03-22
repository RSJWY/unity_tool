using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

/// <summary>
/// 我的构建——YooAsset
/// </summary>
public static partial class MyBuild
{
    /// <summary>
    /// 构建预制体和数据包
    /// </summary>
    static void AutoBuildDataAndPrefab()
    {
        BuiltinBuildParameters buildParameterData = new()
        {
            CompressOption = ECompressOption.LZ4,//设置压缩方法
            VerifyBuildingResult = true,//写入类型树结构
            BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),//默认路径
            BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),//首包内置资源路径
            BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString(),//构建管线
            BuildTarget=BuildTarget.StandaloneWindows64,//构建平台
            BuildMode = BuildMode,//构建模式
            PackageName=PackageNameData,//包裹名称
            PackageVersion=PackageVersion,//构建包裹版本
            FileNameStyle=EFileNameStyle.BundleName_HashName,//资源包名称样式
            BuildinFileCopyOption=EBuildinFileCopyOption.ClearAndCopyAll,//首包（内置）资源拷贝方法
            EncryptionServices=new AESEncryption()//资源包加密服务类
        };
        BuildPackage(buildParameterData);
    }

    /// <summary>
    /// 构建原生文件和热更新程序包
    /// </summary>
    static void AutoBuildRawFileAndHotUpdate()
    {
        BuiltinBuildParameters buildParameterRaw = new()
        {
            CompressOption = ECompressOption.LZ4,//设置压缩方法
            VerifyBuildingResult = true,//写入类型树结构
            BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),//默认路径
            BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),//首包内置资源路径
            BuildPipeline = EBuildPipeline.RawFileBuildPipeline.ToString(),//构建管线
            BuildTarget=BuildTarget.StandaloneWindows64,//构建平台
            BuildMode = BuildMode,//构建模式
            PackageName=PackageNameRaw,//包裹名称
            PackageVersion=PackageVersion,//构建包裹版本
            FileNameStyle=EFileNameStyle.BundleName_HashName,//资源包名称样式
            BuildinFileCopyOption=EBuildinFileCopyOption.ClearAndCopyAll,//首包（内置）资源拷贝方法
            //EncryptionServices=new AESEncryption()//资源包加密服务类（原生文件目前不支持加密，已通过拷贝完成加密）
        };
        BuildPackage(buildParameterRaw);
    }

    /// <summary>
    /// 构建包
    /// </summary>
    static void BuildPackage(BuiltinBuildParameters builtinBuildParameters)
    {
        
        Debug.Log($"开始构建资源包：{builtinBuildParameters.PackageName}\n版本：{builtinBuildParameters.PackageVersion}");

        BuiltinBuildPipeline builder = new();
        var buildResult = builder.Run(builtinBuildParameters, true);
        if (buildResult.Success)
        {
            Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
        }
        else
        {
            Debug.LogError($"构建失败 : {buildResult.ErrorInfo}，\n失败任务：{buildResult.FailedTask}");
        }
    }

}
