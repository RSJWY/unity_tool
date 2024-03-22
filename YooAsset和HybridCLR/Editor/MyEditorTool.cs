using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YooAsset;



/// <summary>
/// 我的构建——工具
/// </summary>
public static class MyEditorTool
{

    [MenuItem("工具/添加补充元数据信息到HybridCLR设置", false, 1)]
    public static void ADDAotArr()
    {
        //生成补充元数据表
        AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
        List<string> aotdlls = AOTGenericReferences.PatchedAOTAssemblyList.ToList();
        //处理信息
        List<string> _temp = new List<string>();
        foreach (string str in aotdlls)
        {
            string _s = str.Replace(".dll", "");
            _temp.Add(_s);
        }
        //保存处理的数据
        HybridCLRSettings.Instance.patchAOTAssemblies = _temp.ToArray();
        HybridCLRSettings.Save();
        AssetDatabase.Refresh();
        //输出结果方便复制
        StringBuilder listStr = new();
        foreach (string str in aotdlls)
        {
            listStr.Append($"\n{str}.bytes");
        }
        Debug.Log($"补充元数据表如下{listStr.ToString()}");
    }


    /// <summary>
    /// 加密文件（传入路径，直接加密文件）
    /// </summary>
    /// <param name="path">路径</param> <summary>
    static void AESEncrypt(string path)
    {
        Debug.Log($"加密文件{path}");
        MyAES.AESFileEncrypt(path, MyTool.AESkey);
    }

    /// <summary>
    /// 保存场景
    /// </summary>
    public static bool AutoSaveScence()
    {
        // if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        // {
        //     Debug.Log("Scene not saved");
        // }
        // else
        // {
        //     Debug.Log("Scene saved");
        // }
        Debug.Log("场景保存");
        bool issave = EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        return issave;
    }
}


/// <summary>
/// 文件流加密方式
/// </summary>
public class AESEncryption : IEncryptionServices
{
    /// <summary>
    /// 通过AES文件信息加密文件
    /// </summary>
    /// <param name="fileInfo">加密文件信息</param>
    /// <returns></returns>
	public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        Debug.Log($"加密文件：{fileInfo.BundleName}");
        //根据文件路径读取文件
        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        //加密
        byte[] AESByte = MyAES.AESEncrypt(fileData, MyTool.AESkey);
        EncryptResult result = new EncryptResult();
        //标记已加密
        result.Encrypted = true;
        result.EncryptedData = AESByte;
        return result;
    }
}