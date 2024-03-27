using System.Collections;
using System.Collections.Generic;
using YooAsset.Editor;
using UnityEngine;
using System;
using UnityEditor;
using YooAsset;
using System.IO;

public class CompareYooAsset
{
    public static void Compare(string low, string high)
    {
        var lowReport = JsonUtility.FromJson<BuildReport>(low);
        var highReport = JsonUtility.FromJson<BuildReport>(high);
        Compare(lowReport, highReport);
    }
    public static void Compare(BuildReport lowReport, BuildReport highReport)
    {
        if (lowReport == null || highReport == null)
        {
            Debug.Log($"[CompareYooAsset] lowReport == null || highReport == null");
            return;
        }
        CompareReport cr = new CompareReport();
        cr.OldVersion = lowReport.Summary.BuildPackageVersion;
        cr.NewVersion = highReport.Summary.BuildPackageVersion;
        cr.NeedUpdateBundleInfoes = new List<CompareInfo>();
        cr.NeedUpdateFileInfoes = new List<CompareInfo>();
        Dictionary<string, ReportBundleInfo> lowDic = new Dictionary<string, ReportBundleInfo>();
        Dictionary<string, ReportAssetInfo> lowFileDic = new Dictionary<string, ReportAssetInfo>();
        for (int i = 0; i < lowReport.BundleInfos.Count; i++)
        {
            var bundleInfo = lowReport.BundleInfos[i];
            lowDic.Add(bundleInfo.BundleName, bundleInfo);
        }
        for (int i = 0; i < lowReport.AssetInfos.Count; i++)
        {
            var assetInfo = lowReport.AssetInfos[i];
            lowFileDic.Add(assetInfo.AssetPath, assetInfo);
        }
        for (int i = 0; i < highReport.BundleInfos.Count; i++)
        {
            var highInfo = highReport.BundleInfos[i];
            ReportBundleInfo lowInfo;
            if (lowDic.TryGetValue(highInfo.BundleName, out lowInfo))
            {
                if (lowInfo.FileHash != highInfo.FileHash || lowInfo.FileSize != highInfo.FileSize)
                {
                    cr.NeedUpdateBundleInfoes.Add(new CompareInfo(highInfo.BundleName, highInfo.FileSize));
                }
            }
            else
            {
                cr.NeedUpdateBundleInfoes.Add(new CompareInfo(highInfo.BundleName, highInfo.FileSize));
            }
        }
        for (int i = 0; i < highReport.AssetInfos.Count; i++)
        {
            var highInfo = highReport.AssetInfos[i];
            ReportAssetInfo lowInfo;
            if (lowFileDic.TryGetValue(highInfo.AssetPath, out lowInfo))
            {
                if (highInfo.MainBundleSize != lowInfo.MainBundleSize && highInfo.MainBundleSize != 0 && lowInfo.MainBundleSize != 0)
                {
                    cr.NeedUpdateFileInfoes.Add(new CompareInfo(highInfo.AssetPath, highInfo.MainBundleSize));
                }
            }
            else
            {
                cr.NeedUpdateFileInfoes.Add(new CompareInfo(highInfo.AssetPath, highInfo.MainBundleSize));
            }
        }
        long size = 0;
        for (int i = 0; i < cr.NeedUpdateBundleInfoes.Count; i++)
        {
            size += cr.NeedUpdateBundleInfoes[i].Size;
            var sizef = cr.NeedUpdateBundleInfoes[i].Size / 1048576f;
            sizef = (float)Math.Round(sizef, 2);
            Debug.Log($"[CompareYooAsset] BundleName:{cr.NeedUpdateBundleInfoes[i].Name},size:{sizef}M");
        }
        float updateSize = (float)size / 1048576;
        updateSize = (float)Math.Round(updateSize, 2);
        cr.UpdateSize = $"{updateSize}M";
        var text = JsonUtility.ToJson(cr);
        Debug.Log($"[CompareYooAsset]{cr.OldVersion} ->{cr.NewVersion}, 有{cr.NeedUpdateBundleInfoes.Count}个Bundle文件需要更新，大小为{cr.UpdateSize}");
        var path = $"{Application.dataPath}/../CompareReport.json";
        File.WriteAllText(path, text);
        Debug.Log($"[CompareYooAsset] Report:{path}");
    }
}
[Serializable]
public class CompareReport 
{
    public string UpdateSize;
    public string OldVersion;
    public string NewVersion;
    public List<CompareInfo> NeedUpdateBundleInfoes;
    public List<CompareInfo> NeedUpdateFileInfoes;
}
[Serializable]

public class CompareInfo 
{
    public string Name;
    public long Size;
    public CompareInfo(string name,long size) 
    {
        Name = name;
        Size = size;
    }
}


[Serializable]
public class CompareReportBundleInfo
{
    public ReportBundleInfo low, high;
    public CompareReportBundleInfo(ReportBundleInfo high, ReportBundleInfo low)
    {
        this.low = low;
        this.high = high;
    }
}
public class AssetBundleReporterWindow : EditorWindow
{
    string low = string.Empty;
    string high = string.Empty;
    [MenuItem("工具/YooAsset报表对比工具", false, 103)]
    public static void OpenWindow()
    {
        AssetBundleReporterWindow window = GetWindow<AssetBundleReporterWindow>("对比报告", true, WindowsDefine.DockedWindowTypes);
        window.minSize = new Vector2(800, 600);
        window.Show();
    }
    public void OnGUI()
    {
        var titleLow = "导入LowReport";
        var titleHigh = "导入HighReport";
        var output = "输出报告";
        if (GUILayout.Button(titleLow))
        {
            BtnClick(titleLow, ref low);
        }
        if (GUILayout.Button(titleHigh))
        {
            BtnClick(titleHigh, ref high);
        }
        if (GUILayout.Button(output))
        {
            if (string.IsNullOrEmpty(low) || string.IsNullOrEmpty(high))
            {
                Debug.LogError("[CompareYooAsset] 请选择报告");
                //file.Close();
                //file.Dispose();
            }
            else 
            {
                CompareYooAsset.Compare(low, high);
                low = null;
                high = null;
            }
        }
    }
    private void BtnClick(string title,ref string json)
    {
        string selectFilePath = EditorUtility.OpenFilePanel(title, EditorTools.GetProjectPath(), "json");
        if (string.IsNullOrEmpty(selectFilePath)) return;
        json = FileUtility.ReadAllText(selectFilePath);
        var report = JsonUtility.FromJson<BuildReport>(json);
        Debug.Log($"[CompareYooAsset] title:{title},package:{report.Summary.BuildPackageVersion}");
    }
}
