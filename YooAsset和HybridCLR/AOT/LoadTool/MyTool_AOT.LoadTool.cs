using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

public partial class MyTool_AOT
{

    /// <summary>
    /// 获取唯一GUID
    /// </summary>
    public static string GenerateRandomString()
    {
        string guid = Guid.NewGuid().ToString("N");
        return guid.Substring(0, 15);
    }

    /// <summary>
    /// 返回时间戳
    /// </summary>
    public static string UpdateTime()
    {
        //获取当前时间
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;

        ////格式化显示当前时间
        return string.Format
            ("{0:D2}_{1:D2}_{2:D2}_" + "{3:D4}_{4:D2}_{5:D2}",
            year, month, day, hour, minute, second);
    }
    /// <summary>
    /// 获取服务器资源路径
    /// </summary>
    /// <param name="host">服务器地址</param>
    /// <param name="version">资源大版本</param>
    /// <param name="packageName">资源包名</param>
    /// <returns></returns>
    public static string GetPackageURL(string host, string version, string packageName)
    {
        return $"{host}/{version}/{packageName}";
    }

    /// <summary>
    /// 加密密钥
    /// </summary>
    public static string AESkey = @"d=5p#iz(\U39%afhDP4x@u7)Sl[gWbOe-C,>I{VLNSA=Qp%)l|DchBH8qu*~yjOv&BM!#,R1FnY[R!tjq*$kBux`cjZ$c@'[)P-y?.TC3{gypB}e[kZnj5n4OK,^D[bMk=*%;X5r>g5%iMvju@rl3ZZK&*i^c%BzL{:&~27G~o5qe%7!e|Ow1a|-HQz5dErE[+w:%r$k&*<wjz(2JIz0>4Eo>da(d8;z7ucEvg3$Zq5mMPtQ{S<bCJBKgs{aT<3";

    /// <summary>
    /// 获取基于YooAsset的路径加载名
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public static string GetYooAssetWebRequestPath(string asset)
    {
        string path = $"Assets/AssetData/{asset}";
        //if (!path.Contains("://"))
        //{
        //    path = "file://" + path;
        //}
        return path;
    }

    public static byte[] GetAESDecryptByte(byte[] data)
    {
        byte[] fileData = AESDecrypt(data, AESkey);
        return fileData;
    }
    public static string GetAESDecryptString(byte[] data)
    {
        byte[] fileData = AESDecrypt(data, AESkey);
        string text = System.Text.Encoding.UTF8.GetString(fileData);
        return text;
    }

    #region 加载方法
    /// <summary>
    /// 编辑器模式
    /// </summary>
    /// <returns></returns>
    public static async UniTask EditorMode(ResourcePackage packagePrefab, ResourcePackage packageRawFile)
    {
        //配置编辑器模式
        var e_initParametersData = new EditorSimulateModeParameters();
        var e_initParametersRawFile = new EditorSimulateModeParameters();
        e_initParametersData.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), PackageNameEnum.DataAndPrefab.ToString());
        e_initParametersRawFile.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.RawFileBuildPipeline.ToString(), PackageNameEnum.RawFileAndHotCode.ToString());
        await packagePrefab.InitializeAsync(e_initParametersData);
        await packageRawFile.InitializeAsync(e_initParametersRawFile);
    }
    /// <summary>
    /// 本地模式
    /// </summary>
    /// <returns></returns>
    public static async UniTask<bool> OfflinePlayMode(ResourcePackage resourcePackage)
    {
        //配置为本地执行
        var r_initParameters = new OfflinePlayModeParameters();
        //配置解密服务
        r_initParameters.DecryptionServices = new AESDecryption();

        var initOperation = resourcePackage.InitializeAsync(r_initParameters);
        await initOperation.Task;
        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"包:{resourcePackage.PackageName}资源包初始化成功！版本为{resourcePackage.GetPackageVersion()}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}资源包初始化成功！版本为{resourcePackage.GetPackageVersion()}");
        }
        else
        {
            Debug.LogError($"包:{resourcePackage.PackageName}资源包初始化失败：{initOperation.Error}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}资源包初始化失败：{initOperation.Error}");
            return false;
        }
        return true;
    }
    /// <summary>
    /// 联机模式
    /// </summary>
    /// <param name="resourcePackage">初始化好的包</param>
    /// <returns></returns>
    public static async UniTask<bool> HostPlayMode(ResourcePackage resourcePackage)
    {
        //初始化
        var r_initParameters = new HostPlayModeParameters
        {
            //传入远程路径地址
            //RemoteServices = new RemoteServices(LoadTool.GetPackageURL(LoadJsonBasic.instance.m_JObject.HostPlayURL.MainURL, LoadJsonBasic.instance.m_JObject.HostPlayURL.version, resourcePackage.PackageName), LoadTool.GetPackageURL(LoadJsonBasic.instance.m_JObject.HostPlayURL.FallbackURL, LoadJsonBasic.instance.m_JObject.HostPlayURL.version, resourcePackage.PackageName)),
            //创建内置资源查询服务
            BuildinQueryServices = new MyQueryServices(),
            //配置解密服务
            DecryptionServices = new AESDecryption()
        };

        //初始化prefab
        var initOperation = resourcePackage.InitializeAsync(r_initParameters);
        await initOperation.Task;
        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"包:{resourcePackage.PackageName}资源包初始化成功！当前版本为：{resourcePackage.GetPackageVersion()}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}资源包初始化成功！当前版本为：{resourcePackage.GetPackageVersion()}");
        }
        else
        {
            Debug.LogError($"包:{resourcePackage.PackageName}资源包初始化失败：{initOperation.Error}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}资源包初始化失败：{initOperation.Error}");
            return false;
        }

        //2.获取资源版本
        var operation = resourcePackage.UpdatePackageVersionAsync(false);
        await operation.Task;
        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"包:{resourcePackage.PackageName}向网络端请求最新的资源版本成功！\n本地版本为：{resourcePackage.GetPackageVersion()} —— 远端最新版本为{operation.PackageVersion}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}向网络端请求最新的资源版本成功！\n本地版本为：{resourcePackage.GetPackageVersion()} —— 远端最新版本为{operation.PackageVersion}");
        }
        else
        {
            //更新失败
            Debug.LogError($"包:{resourcePackage.PackageName}向网络端请求最新的资源版本失败！！错误信息：{operation.Error}\n联网更新失败，执行本地资源完整性检查");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}向网络端请求最新的资源版本失败！！错误信息：{operation.Error}\n联网更新失败，执行本地资源完整性检查");
            //TODO

            //网络更新失败，检查本地资源是否完整
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            int timeout = 60;

            var downloader = resourcePackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
            if (downloader.TotalDownloadCount > 0)
            {
                downloader.CancelDownload();
                // 资源内容本地并不完整，需要提示玩家联网更新。
                Debug.LogError($"包:{resourcePackage.PackageName}更新资源包失败！！请检查本地网络，有新的内容需要更新！本地版本为：：{resourcePackage.GetPackageVersion()} ");
                LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}更新资源包失败！！请检查本地网络，有新的内容需要更新！本地版本为：：{resourcePackage.GetPackageVersion()} ");
                return false;
            }
            downloader.CancelDownload();
            Debug.LogWarning($"包:{resourcePackage.PackageName}更新资源包失败！！但本地资源包相对完整，可能会影响正常使用。本地版本为：：{resourcePackage.GetPackageVersion()} ");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}更新资源包失败！！但本地资源包相对完整，可能会影响正常使用。本地版本为：：{resourcePackage.GetPackageVersion()} ");
            return true;
            /* 弱联网环境

            // 如果获取远端资源版本失败，说明当前网络无连接。
            // 在正常开始游戏之前，需要验证本地清单内容的完整性。
            string local_PackageVersion = resourcePackage.GetPackageVersion();
            var local_operation = resourcePackage.PreDownloadContentAsync(local_PackageVersion);
            await operation.Task;
            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("请检查本地网络，有新的内容需要更新！");
                return false;
            }

            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            int timeout = 60;
            var downloader = local_operation.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
            if (downloader.TotalDownloadCount > 0)
            {
                // 资源内容本地并不完整，需要提示玩家联网更新。
                Debug.LogError("请检查本地网络，有新的内容需要更新！");
                return false;
            }
            return true;
            */
        }

        //获取新的包裹版本
        string packageVersion = operation.PackageVersion;

        //3.更新补丁清单
        var prefab_operation2 = resourcePackage.UpdatePackageManifestAsync(packageVersion);
        await prefab_operation2.Task;
        if (prefab_operation2.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"包:{resourcePackage.PackageName}向网络端请求并更新资源成功，并自动保存到本地\n本地版本为：{resourcePackage.GetPackageVersion()} —— 远端最新版本为{operation.PackageVersion}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}向网络端请求并更新资源成功，并自动保存到本地\n本地版本为：{resourcePackage.GetPackageVersion()} —— 远端最新版本为{operation.PackageVersion}");
        }
        else
        {
            //更新失败
            Debug.LogError($"包:{resourcePackage.PackageName}向网络端请求并更新资源清单失败！！错误信息：{prefab_operation2.Error}");
            LoadUI.instance.UptdateLoadInfo($"包:{resourcePackage.PackageName}向网络端请求并更新资源清单失败！！错误信息：{prefab_operation2.Error}");
            //TODO:
            return false;
        }

        //4.执行下载更新
        bool isState = await Download(resourcePackage);
        return isState;//返回状态
    }
    #endregion

    #region 下载方法
    public static async UniTask<bool> Download(ResourcePackage resourcePackage)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;

        var downloader = resourcePackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log($"包{resourcePackage.PackageName}下载文件总数量为0，不需要执行下载");
            LoadUI.instance.UptdateLoadInfo($"包{resourcePackage.PackageName}下载文件总数量为0，不需要执行下载");
            return true;
        }
        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        Debug.Log($"包{resourcePackage.PackageName}下载文件总数量为{totalDownloadCount}，下载文件的总大小为{totalDownloadBytes}");
        LoadUI.instance.UptdateLoadInfo($"包{resourcePackage.PackageName}下载文件总数量为{totalDownloadCount}，下载文件的总大小为{totalDownloadBytes}");

        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        await downloader.Task;

        //清理缓存文件
        await resourcePackage.ClearAllCacheFilesAsync().Task;
        await resourcePackage.ClearUnusedCacheFilesAsync().Task;
        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功
            Debug.Log($"包{resourcePackage.PackageName}更新完成!");
            LoadUI.instance.UptdateLoadInfo($"包{resourcePackage.PackageName}更新完成!");
            //TODO:
        }
        else
        {
            //下载失败
            Debug.LogError($"包{resourcePackage.PackageName}更新失败！失败原因：{downloader.Error}");
            LoadUI.instance.UptdateLoadInfo($"包{resourcePackage.PackageName}更新失败！失败原因：{downloader.Error}");
            return false;
            //TODO:
        }
        return true;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log($"开始下载：文件名：{fileName}, 文件大小：{sizeBytes}");
        LoadUI.instance.UptdateLoadInfo($"开始下载：文件名：{fileName}, 文件大小：{sizeBytes}");
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
        LoadUI.instance.UptdateLoadInfo("下载" + (isSucceed ? "成功" : "失败"));
    }

    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.Log($"文件总数：{totalDownloadCount}, 已下载文件数：{currentDownloadCount}, 下载总大小：{totalDownloadBytes}, 已下载大小：{currentDownloadBytes}");
        LoadUI.instance.UptdateLoadInfo($"文件总数：{totalDownloadCount}, 已下载文件数：{currentDownloadCount}, 下载总大小：{totalDownloadBytes}, 已下载大小：{currentDownloadBytes}");
    }

    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError($"下载出错：文件名：{fileName}, 错误信息：{error}");
        LoadUI.instance.UptdateLoadInfo($"下载出错：文件名：{fileName}, 错误信息：{error}");
    }

    static int GetBaiFenBi(int now, int sizeBytes)
    {
        return (int)sizeBytes / now * 100;
    }
    #endregion

}


public enum Modles
{
    /// <summary>
    /// 单机模式
    /// </summary>
    OfflinePlay,
    /// <summary>
    /// 编辑器模式
    /// </summary>
    Editor
}


/// <summary>
/// 远端资源地址查询服务类
/// </summary>
public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }
    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }
    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}

public class AESDecryption : IDecryptionServices
{
    /// <summary>
    /// 同步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        Debug.Log($"解密文件：{fileInfo.BundleName}");
        managedStream = null;
        byte[] AESFileData = File.ReadAllBytes(fileInfo.FileLoadPath);
        byte[] fileData = MyTool_AOT.AESDecrypt(AESFileData, MyTool_AOT.AESkey);
        return AssetBundle.LoadFromMemory(fileData);
    }

    /// <summary>
    /// 异步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        Debug.Log($"解密文件：{fileInfo.BundleName}");
        managedStream = null;
        byte[] AESFileData = File.ReadAllBytes(fileInfo.FileLoadPath);
        byte[] fileData = MyTool_AOT.AESDecrypt(AESFileData, MyTool_AOT.AESkey);
        return AssetBundle.LoadFromMemoryAsync(fileData);
    }



}


/// <summary>
/// 包名枚举
/// </summary>
public enum PackageNameEnum
{
    DataAndPrefab, RawFileAndHotCode
}


