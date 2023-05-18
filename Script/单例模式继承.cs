public class InstanceMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T instance
    {
        get
        {
            return _instance;
        }
    }
    protected virtual void Awake()
    {
        _instance = this as T;//强转类型，保存单例数据
    }

}


/// <summary>
/// 单例创建器
/// </summary>
public class SingleMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static GameObject gObj = null;
    private static T _instance;
    private static readonly object syslock = new object();
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syslock)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        if (gObj == null) gObj = new GameObject("SingleMonoBehaviour");
                        _instance = gObj.AddComponent<T>();
                        //GameObject.DontDestroyOnLoad(gObj);
                    }
                    else
                    {
                        //GameObject.DontDestroyOnLoad(_instance.gameObject);
                    }
                }
            }
            return _instance;
        }
    }
}

/// <summary>
/// 非unity脚本
/// </summary>
public class Singleton<T> where T : new()
{
    private static T ms_instance;

    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                ms_instance = new T();
            }

            return ms_instance;
        }
    }
}