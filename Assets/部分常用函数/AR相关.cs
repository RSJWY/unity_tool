using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR相关 : MonoBehaviour
{
    #region 触摸
    Touch oldTouch1;//上次触摸点1(手指1)
    Touch oldTouch2;//上次触摸点2(手指2)
    Touch newTouch1;//新触摸点1
    Touch newTouch2;//新触摸点2
    Touch touch;//触摸判断
    void Touch()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }
        touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, 1000, 1 << 8))
                {
                    //从屏幕发射一道射线，碰到物体
                }
                //单击事件
                break;
            case TouchPhase.Moved:
                TouchMove();
                //移动事件
                break;
            case TouchPhase.Ended:
                //离开事件
                break;
        }
    }
    void TouchMove()
    {

        if (Input.touchCount == 1)
        {
            //单点触摸， 水平上下旋转
            if (1 == Input.touchCount)
            {
                touch = Input.GetTouch(0);
                Vector2 deltaPos = touch.deltaPosition;
                transform.Rotate(Vector3.down * deltaPos.x, Space.World);//绕Y轴进行旋转
                //  transform.Rotate(Vector3.right * deltaPos.y, Space.World);//绕X轴进行旋转，下面我们还可以写绕Z轴进行旋转
            }
        }
        if (Input.touchCount == 2)
        {
            //多点触摸, 放大缩小
            newTouch1 = Input.GetTouch(0);
            newTouch2 = Input.GetTouch(1);
            //第2点刚开始接触屏幕, 只记录，不做处理
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                return;
            }
            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
            //两个距离之差，为正表示放大手势， 为负表示缩小手势
            float offset = newDistance - oldDistance;
            //放大因子， 一个像素按 0.01倍来算(100可调整)
            float scaleFactor = offset / 100f;
            Vector3 localScale = transform.localScale;
            Vector3 scale = new Vector3(localScale.x + scaleFactor,
                                        localScale.y + scaleFactor,
                                        localScale.z + scaleFactor);
            //在什么情况下进行缩放
            if (scale.x >= 0.5f && scale.y <= 2f)
            {
                transform.localScale = scale;
            }
            //记住最新的触摸点，下次使用
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;
        }
    }
    #endregion

}
