using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.Main.MyTool
{
    public static partial class MyTool
    {
        public static T LoadJson<T>(string str) where T : MediaRoot
        {
            var jObject = JsonConvert.DeserializeObject<T>(str);
            return jObject;
        }
        public static async UniTask<Texture2D> GetImageUWRT(string path)
        {
            if (File.Exists(path))
            {
                var data = await File.ReadAllBytesAsync(path);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                return texture;
            }
            else
            {
                Debug.LogWarning($"路径错误！！：{path}");
                return null;
            }
        }

        public static (float NewWidth, float NewHeight) ScaleImageToFit(float containerWidth, float containerHeight, float imageWidth, float imageHeight)
        {
            // 计算图片的原始宽高比  
            double imageAspectRatio = (float)imageWidth / imageHeight;

            // 计算容器的宽高比  
            double containerAspectRatio = (float)containerWidth / containerHeight;

            // 确定图片应该如何缩放以适应容器  
            bool scaleWidth = imageAspectRatio > containerAspectRatio;

            // 根据需要缩放的维度计算新的宽高  
            float newWidth, newHeight;
            if (scaleWidth)
            {
                // 按容器的宽度缩放图片，并计算新的高度以保持宽高比  
                newWidth = containerWidth;
                newHeight = (float)(newWidth / imageAspectRatio);

                // 如果新的高度超过了容器的高度，则需要以容器的高度为准来缩放宽度  
                if (newHeight > containerHeight)
                {
                    newHeight = containerHeight;
                    newWidth = (float)(newHeight * imageAspectRatio);
                }
            }
            else
            {
                // 按容器的高度缩放图片，并计算新的宽度以保持宽高比  
                newHeight = containerHeight;
                newWidth = (float)(newHeight * imageAspectRatio);

                // 如果新的宽度超过了容器的宽度，则需要以容器的宽度为准来缩放高度  
                if (newWidth > containerWidth)
                {
                    newWidth = containerWidth;
                    newHeight = (float)(newWidth / imageAspectRatio);
                }
            }

            // 返回新的图片宽高  
            return (newWidth, newHeight);
        }

    }


    /// <summary>
    /// 包名枚举
    /// </summary>
    public enum PackageNameEnum
    {
        SYSData, VideoData
    }

    public enum Modles
    {
        /// <summary>
        /// 单机模式
        /// </summary>
        OfflinePlay,
        /// <summary>
        /// 编辑器模式
        /// </summary>
        Editor
    }

    #region Json结构体
    public class MediaInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        public bool VideoORPhoto { get; set; }
        public bool Loop { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string command { get; set; }
    }

    public class UDPListenSetting
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

    public class PlayCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public string Play { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pause { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Stop { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VOL_UP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VOL_DOWN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fast_forward { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Fast_rewind { get; set; }
    }


    public class MediaRoot
    {
        /// <summary>
        /// 
        /// </summary>
        public MediaInfo ScreenSaverInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<MediaInfo> MediaList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UDPListenSetting UDPListenSetting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PlayCommand PlayCommand { get; set; }
    }

    #endregion
}