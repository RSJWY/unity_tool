using Cysharp.Threading.Tasks;
//using HybridCLR;
using IngameDebugConsole;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;
using HybridCLR;

/// <summary>
/// 加载热更入口
/// </summary>
public class LoadHotUpdate : MonoBehaviour
{
    [SerializeField]
    private Modles modle = Modles.OfflinePlay;
    /// <summary>
    /// 补充元DLL列表
    /// </summary>
    private static List<string> AOTMetaAssemblyFiles = new();
    /// <summary>
    /// 热更新程序集
    /// </summary>
    private static List<string> HotDllFiles = new();

    /// <summary>
    /// 预制体资源包
    /// </summary>
    public static ResourcePackage packagePrefab;
    /// <summary>
    /// 原生文件资源包
    /// </summary>
    public static ResourcePackage packageRawFile;
    static Dictionary<string, Assembly> hotCode = new();
    /// <summary>
    /// debug模式是否启用
    /// </summary>
    public static bool isDebug = true;
    /// <summary>
    /// dll资源列表
    /// </summary>
    static JObject dllListJson;
    private void Awake()
    {
    }

    private async void Start()
    {
        LoadUI.instance.Load.Reset();
        //加载配置文件
        await LoadJsonBasic.instance.LoadBasicConfig();
        LoadUI.instance.Load.Update("配置文件加载完成", 0.2f);
#if !UNITY_EDITOR
        modle=Modles.OfflinePlay;
#endif
        SetYooAssetAndLoadHotDll().Forget();
    }

    /// <summary>
    /// 读取的数据存储
    /// </summary>
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
    //获取加载好的数据
    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    /// <summary>
    /// 设置YooAsset加载热更新代码
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid SetYooAssetAndLoadHotDll()
    {
        //初始化资源管理系统
        await InitYooAsset();

        Debug.Log($"资源运行模式：{modle.ToString()}\n产品名称：{Application.productName}\n版本：{Application.version}\n数据资源版本：{packagePrefab.GetPackageVersion()}\n原生文件资源版本:{packageRawFile.GetPackageVersion()}");
        LoadUI.instance.UpdatePackageVersion(modle.ToString(), packagePrefab.GetPackageVersion(), packageRawFile.GetPackageVersion());
        LoadUI.instance.Load.Update("初始化资源管理系统完成，加载热更资源", 0.3f);
        //获取补充元数据和热更新DLL
        await LoadAssetsData();
        LoadUI.instance.Load.Update("热更资源加载完成，加载补充元", 0.5f);
        //加载补充元数据
        LoadMetadataForAOTAssemblies();
        LoadUI.instance.Load.Update("补充元加载完成，加载热更", 0.7f);
        //加载热更新DLL
        LoadHotCodeDll();
        LoadUI.instance.Load.Update("热更模块加载完成，加载启动器", 0.9f);

        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(packagePrefab);

        Load().Forget();
    }




    async UniTask Load()
    {
        Debug.Log("加载启动器");
        LoadUI.instance.UptdateLoadInfo("加载启动器");
        AssetHandle startsys = packagePrefab.LoadAssetAsync<GameObject>("Prefab_MainStart");
        await startsys.Task;
        LoadUI.instance.Load.Update("加载启动器完成", 1f);
        GameObject testPrefab = Instantiate(startsys.GetAssetObject<GameObject>());
        //从配置文件获取配置以并启动服务
    }



    #region 资源初始化与加载
    /// <summary>
    /// 初始化资源系统
    /// </summary>
    async UniTask InitYooAsset()
    {
        // 初始化资源系统
        YooAssets.Initialize();
        // 创建资源包
        packageRawFile = YooAssets.CreatePackage(PackageNameEnum.RawFileAndHotCode.ToString());
        packagePrefab = YooAssets.CreatePackage(PackageNameEnum.DataAndPrefab.ToString());

        switch (modle)
        {
            case Modles.OfflinePlay:
                LoadUI.instance.UptdateLoadInfo("本地加载模式");
                await UniTask.WhenAll(MyTool_AOT.OfflinePlayMode(packageRawFile), MyTool_AOT.OfflinePlayMode(packagePrefab));
                break;
            case Modles.Editor:
                LoadUI.instance.UptdateLoadInfo("编辑模式");
                await MyTool_AOT.EditorMode(packagePrefab, packageRawFile);
                break;
            default:
                LoadUI.instance.UptdateLoadInfo("本地加载模式");
                await UniTask.WhenAll(MyTool_AOT.OfflinePlayMode(packageRawFile), MyTool_AOT.OfflinePlayMode(packagePrefab));
                break;
        }

    }




    /// <summary>
    /// 从YooAsset加载数据
    /// </summary>
    /// <param name="onDownloadComplete"></param>
    /// <returns></returns>
    async UniTask LoadAssetsData()
    {
        try
        {
            //读取加载文件
            AssetHandle load_json = packagePrefab.LoadAssetAsync<TextAsset>("Config_HCLR_Load_DLL");
            await load_json.Task;
            string json_data;
            json_data = load_json.GetAssetObject<TextAsset>().text;
            dllListJson = JsonConvert.DeserializeObject<JObject>(json_data);
            //补充元DLL
            JToken[] _AOTMetaAssemblys = dllListJson["MetadataForAOTAssemblies"].ToArray();
            foreach (var _name in _AOTMetaAssemblys)
            {
                AOTMetaAssemblyFiles.Add(_name.Value<string>());
            }
            //热更新DLL
            JToken[] _HotDllFiles = dllListJson["HotUpdateDLL"].ToArray();
            foreach (var _name in _HotDllFiles)
            {
                HotDllFiles.Add(_name.Value<string>());
            }
            //拼接成一个整体
            List<string> _DLL = AOTMetaAssemblyFiles.Union(HotDllFiles).ToList();
            //从包中获取
            foreach (var asset in _DLL)
            {
                //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                string _n = $"{asset}.dll";
                //Debug.Log($"加载资产:{_n}");
                LoadUI.instance.UptdateLoadInfo($"加载资产:{_n}");
                //资源地址是否有效
                if (packagePrefab.CheckLocationValid(_n))
                {
                    //执行加载
                    AssetHandle _rfh = packagePrefab.LoadAssetAsync<TextAsset>(_n);
                    //等待加载完成
                    await _rfh.Task;
                    //转byte数组
                    byte[] assetData = _rfh.GetAssetObject<TextAsset>().bytes;
                    s_assetDatas[asset] = assetData;
                    //Debug.Log($"dll:{asset}  size:{assetData.Length}");
                }
                else
                {
                    Debug.LogError($"资源文件：{asset}无效");
                    LoadUI.instance.UptdateLoadInfo($"资源文件：{asset}无效");
                }

            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载热更相关信息时出错：{ex}");
        }
    }
    #endregion

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        string _str_err_name = "";
        try
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (string aotDllName in AOTMetaAssemblyFiles)
            {
                _str_err_name = aotDllName;
                byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载补充元：{_str_err_name} 时发生异常，{ex}");
        }
    }

    /// <summary>
    /// 加载热更新DLL
    /// </summary>
    void LoadHotCodeDll()
    {
        string _str_err_name = "";
        try
        {
#if UNITY_EDITOR
            foreach (var _hotAss in HotDllFiles)
            {
                _str_err_name = _hotAss;
                hotCode.Add(_hotAss, System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == _hotAss));
            }
#else
        foreach(var _hotAss in HotDllFiles)
        {
            _str_err_name=_hotAss;
            byte[]_dll= ReadBytesFromStreamingAssets(_hotAss);
            hotCode.Add(_hotAss,Assembly.Load(_dll));
        }
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载热更：{_str_err_name} 时发生异常，{ex}");
        }
    }


}

