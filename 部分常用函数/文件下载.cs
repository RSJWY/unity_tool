using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 
/// </summary>
public class 文件下载 : MonoBehaviour
{
    /// <summary>
    /// 下载进度
    /// </summary>
    float Progree;


    /// <summary>
    /// 文本保存到文档
    /// </summary>
    /// <param name="path">本地保存路径</param>
    /// <param name="fileName">文件名称带后缀</param>
    /// <param name="jsonData">文字数据</param>
    public void StrFileWrite(string path, string fileName, string jsonData)
    {
        try
        {
            string filePath = path + fileName;
            if (File.Exists(filePath))
            {
                //如果文件存在删除该文件
                File.Delete(filePath);
            }
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine(jsonData);
            sw.Close();
            fs.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    /// <summary>
    /// 读取文档文字
    /// </summary>
    /// <param name="path">本地保存路径</param>
    /// <param name="fileName">文件名称带后缀</param>
    /// <returns>文件里的文字</returns>
    public string StrFileRead(string path, string fileName)
    {
        string filePath = path + fileName;
        string data = string.Empty;
        FileInfo fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)//如果本地没有文件
        {
            return null;
        }
        else
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                data = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
    /// <summary>
    /// 保存资源到本地
    /// </summary>
    /// <param name="path">本地保存路径</param>
    /// <param name="fileName">文件名称带后缀</param>
    /// <param name="bytes">byte数据</param>
    /// <param name="byteLength">byte数据长度</param>
    public void SaveAssetFile(string path, string fileName, byte[] bytes, int byteLength)
    {
        string filePath = path + fileName;
        Stream sw = null;
        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists)
        {
            //如果文件存在删除该文件
            File.Delete(filePath);
            Debug.Log("删除文件:" + filePath);
        }
        sw = fileInfo.Create();
        sw.Write(bytes, 0, byteLength);//写入
        sw.Flush();//关闭流
        sw.Close();//销毁流
        sw.Dispose();
        Debug.Log(fileName + "成功保存到本地~");
    }
    /// <summary>
    /// 下载资源
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="receiveFunction"></param>
    public void DownloadData(string url)
    {
        Progree = 0;
        StartCoroutine(DownloadRequestData(url));
    }
    IEnumerator DownloadRequestData(string url)
    {
        using (var webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {

                Debug.LogError(webRequest.error);
            }
            else
            {
                while (!webRequest.isDone)
                {
                    Progree = webRequest.downloadProgress * 100f;
                    yield return 0;
                }
                if (webRequest.isDone)
                {
                    Progree = 1;
                    byte[] bytes = webRequest.downloadHandler.data;
                }
            }
        }
    }

}
