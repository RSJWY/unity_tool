using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline.Utilities;
using YooAsset;
using YooAsset.Editor;


public class MyBuildpiplieline : IBuildPipeline
{
    public BuildResult Run(BuildParameters buildParameters, bool enableLog)
    {
        AssetBundleBuilder builder = new AssetBundleBuilder();
		return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
    }

    /// <summary>
    /// 获取默认的构建流程
    /// </summary>
    private List<IBuildTask> GetDefaultBuildPipeline()
    {
        List<IBuildTask> pipeline = new List<IBuildTask>
            {
                new TaskPrepare_RFBP(),
                new TaskGetBuildMap_RFBP(),
                new TaskBuilding_RFBP(),
                new TaskEncryption_MBP(),
                new TaskUpdateBundleInfo_RFBP(),
                new TaskCreateManifest_RFBP(),
                new TaskCreateReport_RFBP(),
                new TaskCreatePackage_RFBP(),
                new TaskCopyBuildinFiles_RFBP(),
            };
        return pipeline;
    }
}

public class TaskEncryption_MBP : TaskEncryption, IBuildTask
{
    void IBuildTask.Run(BuildContext context)
    {
        var buildParameters = context.GetContextObject<BuildParametersContext>();
        var buildMapContext = context.GetContextObject<BuildMapContext>();

        var buildMode = buildParameters.Parameters.BuildMode;
        if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
        {
            MyEncryptingBundleFiles(buildParameters, buildMapContext);
        }
    }

    void MyEncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			var encryptionServices = buildParametersContext.Parameters.EncryptionServices;
			if (encryptionServices == null)
				return;

			if (encryptionServices.GetType() == typeof(EncryptionNone))
				return;

			int progressValue = 0;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				EncryptFileInfo fileInfo = new EncryptFileInfo();
				fileInfo.BundleName = bundleInfo.BundleName;
				fileInfo.FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
				var encryptResult = encryptionServices.Encrypt(fileInfo);		
				if (encryptResult.Encrypted)
				{
					string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
					FileUtility.WriteAllBytes(filePath, encryptResult.EncryptedData);
					bundleInfo.EncryptedFilePath = filePath;
					bundleInfo.Encrypted = true;
					BuildLogger.Log($"捆绑文件加密完成: {filePath}");
				}
				else
				{
					bundleInfo.Encrypted = false;
				}

				// 进度条
				EditorTools.DisplayProgressBar("加密捆绑包", ++progressValue, buildMapContext.Collection.Count);
			}
			EditorTools.ClearProgressBar();
		}
}


