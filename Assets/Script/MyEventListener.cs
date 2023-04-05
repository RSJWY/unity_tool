using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 事件工具
/// </summary>
public class MyEventListener : MonoBehaviour
{
    public Action<GameObject> onClick;//事件存储器
    /// <summary>
    /// 获取组件
    /// </summary>
    /// <param name="obj">物体</param>
    /// <param name="isUI">是否是UI</param>
    /// <returns></returns>
    public static MyEventListener Get(GameObject obj,bool isUI)
    {
        //检查是否挂载有此脚本
        MyEventListener mel = obj.GetComponent<MyEventListener>();
        if (mel==null)
        {
            mel = obj.AddComponent<MyEventListener>();
        }
        //挂载物理组件(触发)
        //检查是否有碰撞器和刚体（满足物理碰撞触发基本条件）
        Rigidbody rig = obj.GetComponent<Rigidbody>();
        if (rig==null)
        {
            rig = obj.AddComponent<Rigidbody>();
        }
        Collider[] col = obj.GetComponentsInChildren<Collider>();
        if (col==null&&col.Length==0)
        {
            col = new Collider[1];//创建一个新的碰撞器
            //增加一个方形碰撞器
            col[0] = obj.AddComponent<BoxCollider>();
        }
        //是否是UI
        if (isUI)
        {
            //关闭重力
            rig.useGravity = false;
            rig.isKinematic = true;
            //获取尺寸
            RectTransform rt = obj.GetComponent<RectTransform>();
            //碰撞器尺寸匹配
            for (int i = 0; i < col.Length; i++)
            {
                col[i].isTrigger = true;
                BoxCollider boxc = col[i] as BoxCollider;//碰撞器强转雷形
                //碰撞器尺寸设置
                boxc.size = new Vector3(rt.sizeDelta.x, rt.sizeDelta.y, 1f);
            }
        }

        return mel;//返回配置完成的组件
    }




}
