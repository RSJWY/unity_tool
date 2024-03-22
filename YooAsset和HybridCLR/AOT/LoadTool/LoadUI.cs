using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadUI : MonoBehaviour
{
    public static LoadUI instance { get; private set; }
    public Progressbar Load;
    public Text packageVersion;
    public Text LoadInfo;
    private void Awake()
    {
        instance = this;
    }

    public void UpdatePackageVersion(string tuntimemodle, string RawfileVersion, string DatafileVersion)
    {
        packageVersion.text = $"资源运行模式：{tuntimemodle}\n产品名称：{Application.productName}\n版本：{Application.version}\n数据资源版本：{RawfileVersion}\n原生文件资源版本:{DatafileVersion}";
    }

    public void UptdateLoadInfo(string info)
    {
        LoadInfo.text = info;
    }
}
