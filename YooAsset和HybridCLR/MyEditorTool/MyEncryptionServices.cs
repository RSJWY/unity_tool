using System.IO;
using UnityEngine;
using YooAsset;

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
        Debug.Log($"加密资源包：{fileInfo.BundleName}");
        //根据文件路径读取文件
        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        //加密
        byte[] AESByte = Script.AOT.LoadTool.MyTool_AOT.AESEncrypt(fileData, Script.AOT.LoadTool.MyTool_AOT.AESkey);
        EncryptResult result = new EncryptResult();
        //标记已加密
        result.Encrypted = true;
        result.EncryptedData = AESByte;
        return result;
    }
}