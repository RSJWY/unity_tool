using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class LoadJsonBasic
{
    static LoadJsonBasic _instance;
    static Object thisLock = new Object();
    public static LoadJsonBasic instance
    {
        get
        {
            if (_instance == null)
            {
                lock (thisLock)
                {
                    if (_instance == null)
                    {
                        _instance = new();
                    }
                }

            }
            return _instance;
        }
    }
    static readonly string jsonPath = Application.streamingAssetsPath + "/config.json";

    public ConfigJsonRoot m_JObject;

    /// <summary>
    /// 读取json配置信息
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public async UniTask LoadBasicConfig()
    {
        Debug.Log($"加载基础配置json，路径：{jsonPath}");
        LoadUI.instance.UptdateLoadInfo($"加载基础配置json，路径：{jsonPath}");
        //异步读取配置信息
        UnityWebRequest _json = UnityWebRequest.Get(jsonPath);
        await _json.SendWebRequest();
        //信息转存
        m_JObject = JsonConvert.DeserializeObject<ConfigJsonRoot>(_json.downloadHandler.text);
    }
}

public class TCP_Listen
{
    /// <summary>
    /// 
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int Port { get; set; }
}

public class UDP_Listen
{
    /// <summary>
    /// 
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int Port { get; set; }
}

public class Server_Setting
{
    /// <summary>
    /// 
    /// </summary>
    public TCP_Listen TCP_Listen { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public UDP_Listen UDP_Listen { get; set; }
}

public class TCP_Connect
{
    /// <summary>
    /// 
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int Port { get; set; }
}

public class Client_Setting
{
    /// <summary>
    /// 
    /// </summary>
    public TCP_Connect TCP_Connect { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public UDP_Listen UDP_Listen { get; set; }
}

public class ConfigJsonRoot
{
    /// <summary>
    /// 
    /// </summary>
    public string TCPModle { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Server_Setting Server_Setting { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Client_Setting Client_Setting { get; set; }
}
