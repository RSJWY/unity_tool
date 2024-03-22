using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public class LeapMotionPrefab : MonoBehaviour
{
    /// <summary>
    /// 手部模型
    /// </summary>
    public GameObject handModle;
    /// <summary>
    /// leapmotion设备连接器
    /// </summary>
    public LeapServiceProvider leapmotionServer;
    /// <summary>
    /// 握拳手势数据
    /// </summary>
    public HandPoseDetector woQuanPose;
    /// <summary>
    /// 翻书手势数据
    /// </summary>
    public HandPoseDetector fanShuPose;

    private void Awake() {
        leapmotionServer.OnDeviceSafe+=(device)=>{
            Debug.Log($"连接到设备\n设备ID：{device.DeviceID}\n序列号：{device.SerialNumber}\n设备型号：{device.Type}");
        };
    }
}
