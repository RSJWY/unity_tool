#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// unity的Console输出页快速输出为文件
/// 引用于：https://blog.csdn.net/sinat_34870723/article/details/120229157
/// </summary>
public class UnityOutputLogHelper
{
    private class UnityOutputLogHelperWindow : EditorWindow
    {
        private static UnityOutputLogHelperWindow m_Instance;
        [MenuItem("RSJWY/UnityLog输出/输出")]
        private static void Open()
        {
            if (m_Instance != null)
            {
                EditorWindow.DestroyImmediate(m_Instance);
            }
            m_Instance = EditorWindow.GetWindow<UnityOutputLogHelperWindow>("UnityLog输出");
        }
        private UnityOutputLogHelper m_UnityOutputLogHelper;
        private void OnEnable()
        {
            m_Instance = this;
            m_UnityOutputLogHelper = new UnityOutputLogHelper();
            m_UnityOutputLogHelper.Init();
        }
        private void OnDestroy()
        {
            m_UnityOutputLogHelper.UnInit();
        }
        private void OnGUI()
        {
            if (GUILayout.Button("输出"))
            {
                m_UnityOutputLogHelper.OutputLog();
            }
        }
    }
    public class LogAssetsImport : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int index = 0; index < importedAsset.Length; index++)
            {
                string asset = importedAsset[index];
                if (asset == m_LastCreateLogPath)
                {
                    UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(m_LastCreateLogPath, typeof(UnityEngine.Object));
                    m_LastCreateLogPath = string.Empty;
                    EditorGUIUtility.PingObject(codeObject);
                    return;
                }
            }
        }
    }
    private static string m_LastCreateLogPath;
    public void Init()
    {
        EditorUtility.ClearProgressBar();
    }
    public void UnInit()
    {
    }
    private void OutputLog()
    {
        StringBuilder outputStringBuilder = new StringBuilder();
        Type consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
        MethodInfo consoleWindow_SetFlagMethod = consoleWindowType.GetMethod("SetFlag", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        //设置log类型全开
        consoleWindow_SetFlagMethod.Invoke(null, new object[] { 128, true });    //LogLevelLog
        consoleWindow_SetFlagMethod.Invoke(null, new object[] { 256, true });    //LogLevelWarning
        consoleWindow_SetFlagMethod.Invoke(null, new object[] { 512, true });    //LogLevelError
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
        MethodInfo logEntries_GetEntryInternalMethodInfo = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
        MethodInfo logEntries_GetCountMethodInfo = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
        MethodInfo logEntries_StartGettingEntriesMethodInfo = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
        MethodInfo logEntries_EndGettingEntriesMethodInfo = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
        Type logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
        FieldInfo LogEntry_MessageFieldInfo = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
        //得到log的数量
        int totalCount = (int)logEntries_GetCountMethodInfo.Invoke(null, null);
        //LogEntry实体
        object instance_LogEntry = Activator.CreateInstance(logEntryType);
        object[] paramObjs = new object[] { 0, instance_LogEntry };
        logEntries_StartGettingEntriesMethodInfo.Invoke(null, null);
        for (int index = 0; index < totalCount; index++)
        {
            EditorUtility.DisplayProgressBar("Log输出", "", index / (float)totalCount);
            paramObjs[0] = index;
            logEntries_GetEntryInternalMethodInfo.Invoke(null, paramObjs);
            object messageObj = LogEntry_MessageFieldInfo.GetValue(instance_LogEntry);
            if (messageObj != null)
            {
                string condition = messageObj as string;
                outputStringBuilder.Append(condition);
                outputStringBuilder.Append("\n");
            }
        }
        logEntries_EndGettingEntriesMethodInfo.Invoke(null, null);
        string time = string.Format("{3:D4}_{4:D2}_{5:D2}_{0:D2}_{1:D2}_{2:D2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        string logPath = "log-" + time + ".txt";//日志名

        string str_filePath = Application.streamingAssetsPath + "/Log/EditorLog/";
        //判断要输出的路径是否存在，如果不存在，则创建
        if (Directory.Exists(str_filePath))
        {
            Debug.Log("输出文件夹存在 ,路径是" + str_filePath);
        }
        else
        {
            Debug.LogError("输出文件夹不存在,创建");
            Directory.CreateDirectory(str_filePath);
        }

        //File.WriteAllText(Application.dataPath + "/" + logPath, outputStringBuilder.ToString());
        File.WriteAllText(Application.streamingAssetsPath + "/Log/EditorLog/" + logPath, outputStringBuilder.ToString());
        outputStringBuilder.Clear();
        //m_LastCreateLogPath = "Assets/" + logPath;
        m_LastCreateLogPath = Application.streamingAssetsPath + "/Log/EditorLog/" + logPath;
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();


    }
}
#endif
