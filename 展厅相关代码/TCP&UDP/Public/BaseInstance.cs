

/// <summary>
/// 父类单例模式
/// </summary>
/// <typeparam name="T">类型</typeparam>
internal class BaseInstance<T> where T : class, new()
{
    static object thislock = new();
    static T _instance = null;
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                lock (thislock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}