using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class 触摸缩放
{
    /// <summary>
    /// 是单指触摸
    /// </summary>
    private bool isSingleFinger;
    /// <summary>
    /// 第一次单指触摸位置
    /// </summary>
    private Vector2 preSingleTouchPosition;

    private Transform currentModel;
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;
    private float beginTouchDistance;
    private Vector3 currentScale;
    private Vector3 currentPosition;

    public void SingleTouch()
    {
        Debug.Log("SingleTouch");//单指触摸
        if (Input.GetTouch(0).phase == TouchPhase.Began || !isSingleFinger)
        {
            //在开始触摸或者从两字手指放开回来的时候记录一下触摸的位置  
            preSingleTouchPosition = Input.GetTouch(0).position;
        }
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            currentModel.transform.Rotate(Vector3.up, -Input.GetTouch(0).deltaPosition.x * 0.5f);//单指控制物体旋转
            preSingleTouchPosition = Input.GetTouch(0).position;
        }
        isSingleFinger = true;

    }
    public void DoubleTouch()
    {
        Debug.Log("DoubleTouch:" + Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position));//手指触摸到了屏幕
        if (isSingleFinger)
        {
            //记录触摸开始值
            oldPosition1 = Input.GetTouch(0).position;
            oldPosition2 = Input.GetTouch(1).position;
            beginTouchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);//两点距离
            currentScale = currentModel.transform.localScale;//被操控物体当前缩放倍数
            currentPosition = currentModel.transform.localPosition;//被操控物体当前的位置信息
        }

        if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)//手指移动
        {
            Vector2 xiangliang1 = Input.GetTouch(0).position - oldPosition1;//记录相对于初始值的位移
            Vector2 xiangliang2 = Input.GetTouch(1).position - oldPosition2;
            float dir = Vector2.Dot(xiangliang1.normalized, xiangliang2.normalized);//点乘，判断第二个向量在第一个向量的什么位置
            if (dir <= 1 && dir >= 0)
            {//同向移动
                currentModel.transform.Translate(Input.GetTouch(0).deltaPosition.x * 0.01f, Input.GetTouch(1).deltaPosition.y * 0.01f, 0, Space.World);
            }
            else if (dir >= -1 && dir < 0)
            {//反向移动
                float currentTouchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                float tmpScale = currentTouchDistance / beginTouchDistance - 1f;
                tmpScale += currentScale.x;
                tmpScale = Mathf.Clamp(tmpScale, 0.5f, 3f);
                currentModel.transform.localScale = Vector3.one * tmpScale;

            }
        }
        isSingleFinger = false;

    }

    //public void ClickObject()
    //{
    //    Debug.Log("ClickObject");
    //    a = new List<A>();
    //    for (int i = 0; i < Input.touchCount; i++)
    //    {
    //        eventDataCurrentPosition.position = Input.GetTouch(i).position;
    //        List<RaycastResult> results = new List<RaycastResult>();
    //        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
    //        if (results.Count > 0)
    //        {
    //            if (results[0].gameObject.transform.parent.GetComponent<UIManager>() != null)
    //            {
    //                if (a.Count == 0)
    //                {
    //                    A a1 = new A();
    //                    a1.obj = results[0].gameObject.transform.parent.GetComponent<UIManager>();
    //                    a1.touch.Add(Input.GetTouch(i));
    //                    a.Add(a1);
    //                }
    //                else
    //                {
    //                    int aa = a.Count;
    //                    for (int j = 0; j < aa; j++)
    //                    {
    //                        if (a[j].obj == results[0].gameObject.transform.parent.GetComponent<UIManager>())
    //                        {
    //                            a[j].touch.Add(Input.GetTouch(i));
    //                        }
    //                        else
    //                        {
    //                            A a1 = new A();
    //                            a1.obj = results[0].gameObject.transform.parent.GetComponent<UIManager>();
    //                            a1.touch.Add(Input.GetTouch(i));
    //                            a.Add(a1);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}
public class A
{
    public UIManager obj;//自定义UI控制脚本
    public List<Touch> touch = new List<Touch>();
}