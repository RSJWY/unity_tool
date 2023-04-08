using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Networking;
/// <summary>
/// 工具组
/// </summary>
public class MyTool
{
    private static MyTool _inst;
    public static MyTool inst
    {
        get
        {
            return _inst;
        }
    }

    /// <summary>
    /// 击飞
    /// </summary>
    /// <param name="cc">被击飞物体控制器</param>
    /// <param name="totalY">高低</param>
    /// <param name="horVec">方向</param>
    /// <param name="duration">时间</param>
    /// <param name="_func">结束时执行的函数</param>
    /// <returns></returns>
    public void DOHit(CharacterController cc, float totalY, Vector3 horVec, float duration, Action _func)
    {
        Transform tf = cc.transform;
        if (totalY == 0)
        {
            totalY = 0.1f;
        }
        float time = 0;
        float lastT = 0, deltaT = 0;
        //0 ->1的数值动画
        Tween tween = DOTween.To(x => time = x, 0, 1, duration);
        tween.SetEase(Ease.OutQuad);
        //tween.SetLoops(2,LoopType.Yoyo);

        Vector3 targetMove = horVec;
        targetMove.y += totalY; //目标移动量
        tween.onComplete = () =>
        {
            _func.Invoke();
        };
        tween.OnUpdate(() =>
        {
            deltaT = time - lastT;
            lastT = time;

            Vector3 delta = targetMove * deltaT;
            cc.Move(delta);
        });
    }

    /// <summary>
    /// 在DOTween时，延时物体显隐
    /// </summary>
    /// <param name="_obj">要控制的物体</param>
    /// <param name="_time">时间</param>
    /// <returns></returns>
    public IEnumerator DWEActive(GameObject _obj, float _time, bool _bool)
    {
        yield return new WaitForSeconds(_time);
        _obj.gameObject.SetActive(_bool);
    }

    /// <summary>
    /// 设置DOTween动画是否受时间影响
    /// </summary>
    /// <param name="_dwt">动画组</param>
    /// <param name="_bool">布尔值</param>
    public void SetDWTUpdata(Sequence _dwt, bool _bool = true)
    {
        _dwt.SetUpdate(_bool);
    }

    /// <summary>
    /// 读取物品图片
    /// </summary>
    /// <param name="_url">文件地址</param>
    /// <param name="_img">显示图片的位置</param>
    /// <returns></returns>
    public IEnumerator ToolGetPic(string _url, Image _img)
    {
        UnityWebRequest q = UnityWebRequestTexture.GetTexture(_url);
        yield return q.SendWebRequest();//等待读取结束
        if (q.isDone)
        {
            Texture2D t = DownloadHandlerTexture.GetContent(q);
            Sprite s = Sprite.Create(t,
                new Rect(0, 0,
                t.width, t.height),
               Vector2.zero);//设置图片格式
            _img.overrideSprite = s;//图片设置
        }
        _img.gameObject.SetActive(true);
    }

    /// <summary>
    /// 加载AssetBundle文件
    /// </summary>
    /// <param name="fileName">名字</param>
    /// <param name="type">格式</param>
    /// <param name="target_obj">要挂载的目标物体</param>
    /// <returns></returns>
    public IEnumerator LoadAB(string fileName, string type,GameObject target_obj=null)
    {
        //拼接路径
        string url = Application.streamingAssetsPath + "/" + fileName + "." + type + ".assetbundle";
        UnityWebRequest q = UnityWebRequestAssetBundle.GetAssetBundle(url);//加载
        yield return q.SendWebRequest();
        if (q.isDone)
        {
            AssetBundle _a = DownloadHandlerAssetBundle.GetContent(q);//转换格式
            UnityEngine.Object _obj = _a.LoadAsset(fileName);//加载
            GameObject _ob = GameObject.Instantiate(_obj) as GameObject;//复制
            _ob.transform.SetParent(target_obj.transform);
            _ob.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// 系统的时间
    /// </summary>
    /// <returns></returns>
    public string NowSystemTime()
    {
        return string.Format("{0:D2}:{1:D2}:{2:D2} ", 
            DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
    }



    #region Transform相关设置
    /// <summary>
    /// 快速设置Transform位置信息
    /// <para>默认位置与旋转皆为0，scale为1</para>
    /// </summary>
    /// <param name="islocal"></param>
    /// <param name="target"></param>
    /// <param name="position"></param>
    /// <param name="EulerAngle"></param>
    /// <param name="localscale"></param>
    public void SetNewTransformInfo(bool islocal, Transform target)
    {
        //判断转换相对还是世界
        if (islocal)
        {
            target.transform.localPosition = Vector3.zero;
            target.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            target.transform.position =Vector3.zero;
            target.transform.eulerAngles = Vector3.zero;
        }
        target.transform.localScale= Vector3.one;
    }

    /// <summary>
    /// 快速设置Transform位置信息
    /// <para>default 默认为 Vector3(0,0,0)</para>
    /// </summary>
    /// <param name="islocal">世界坐标还是相对坐标，</param>
    /// <param name="target">目标的Transform组件</param>
    /// <param name="position">POS值</param>
    /// <param name="EulerAngle">Euler值</param>
    /// <param name="Rotation">Euler值</param>
    /// <param name="localscale"></param>
    public void SetNewTransformInfo(bool islocal, Transform target,
        Vector3 position=default, Vector3 EulerAngle = default, Vector3 localscale = default)
    {
        //判断转换相对还是世界
        if (islocal)
        {
            target.transform.localPosition = position == default ? Vector3.zero : position;
            target.transform.localEulerAngles = EulerAngle == default ? Vector3.zero : EulerAngle;
        }
        else
        {
            target.transform.position = position == default ? Vector3.zero : position;
            target.transform.eulerAngles = EulerAngle == default ? Vector3.zero : EulerAngle;
        }
        target.transform.localScale = localscale == default ? Vector3.one : localscale;
    }

    /// <summary>
    /// 快速设置Transform位置信息
    /// <para>default 默认为 Vector3(0,0,0)</para>
    /// <para>如果四元数值为空，则赋值操作将由欧拉角转为四元数进行操作</para>
    /// </summary>
    /// <param name="islocal">世界坐标还是相对坐标，</param>
    /// <param name="target">目标的Transform组件</param>
    /// <param name="position">POS值</param>
    /// <param name="Rotation">四元数值</param>
    /// <param name="Rotation">Euler值</param>
    /// <param name="localscale"></param>
    public void SetNewTransformInfo(bool islocal, Transform target,
        Vector3 position = default, Quaternion Rotation = default, Vector3 localscale = default)
    {
        //判断四元数信息
        if (Rotation == default)
        {
            Rotation = Quaternion.Euler(Vector3.zero);
        }

        //判断转换相对还是世界
        if (islocal)
        {
            target.transform.localPosition = position == default ? Vector3.zero : position;
            target.transform.localRotation = Rotation;
        }
        else
        {
            target.transform.position = position == default ? Vector3.zero : position;
            target.transform.rotation = Rotation;
        }
        target.transform.localScale = localscale == default ? Vector3.zero : localscale;
    }
    #endregion

    #region 废弃
    //废弃

    AsyncOperation asy;
    #region 系统时间

    private int hour;
    private int minute;
    private int second;
    private int year;
    private int month;
    private int day;

    /// <summary>
    /// 控制台输出时间
    /// </summary>
    /// <returns>时间字符串</returns>
    public string aNowSyetemTime()
    {
        return null;

        ////获取当前时间
        //hour = DateTime.Now.Hour;
        //minute = DateTime.Now.Minute;
        //second = DateTime.Now.Second;
        //year = DateTime.Now.Year;
        //month = DateTime.Now.Month;
        //day = DateTime.Now.Day;

        ////格式化显示当前时间
        ////return string.Format("{0:D2}:{1:D2}:{2:D2} " + "{3:D4}/{4:D2}/{5:D2}", hour, minute, second, year, month, day);
        //return string.Format("{0:D2}:{1:D2}:{2:D2} " , hour, minute, second);
    }
    #endregion
    /// <summary>
    /// 切换场景
    /// </summary>
    /// <param name="_next_name">下一个场景名字</param>
    /// <param name="_img">进图条图片</param>
    /// <param name="_text">进度条显示</param>
    /// <param name="_false">假占比</param>
    /// <param name="_true">真占比</param>
    /// <returns></returns>
    /// 
    public IEnumerator SceneLoad(string _next_name, Image _img, Text _text = null, float _false = 0.7f, float _false_time = 3f, float _true = 0.3f)
    {
        yield return 1f;
        _img.DOFillAmount(_false, _false_time).OnUpdate(() => {
            if (_text != null)
            {
                _text.text = (_img.fillAmount * 100).ToString("0.00") + "%";
            }
        });
        yield return new WaitForSeconds(_false_time + 0.2f);
        asy = SceneManager.LoadSceneAsync(_next_name);
        asy.allowSceneActivation = false;
        float f = 0;
        while (true)
        {
            f = _false + _true / 0.9f * asy.progress;
            _img.fillAmount = f;
            _text.text = (f * 100).ToString("0.00") + "%";
            yield return new WaitForSeconds(0.1f);
            if (asy.progress >= 0.9f)
            {
                break;
            }
        }
        _img.fillAmount = 1f;
        _text.text = "100.00%";
        asy.allowSceneActivation = true;
    }

    #endregion



#if UNITY_EDITOR
    /// <summary>
    /// 检查路径下是否有重命名的文件，并重新指定一个没有任何冲突的名字
    /// 规则为：类型名字+_+编号+后缀
    /// </summary>
    /// <param name="_url">路径</param>
    /// <param name="_suffix">后缀名，默认为 "*.asset" </param>
    /// <returns></returns>
    public static string TryGetName<T>(string _url, string _suffix = ".asset")
    {
        //新建初始参数
        string str = "";
        int index = 0;
        UnityEngine.Object obj = null;
        do
        {
            str = _url + "/" + typeof(T).Name + "_" + index + _suffix;//拼接
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(str, typeof(T));//尝试加载
            index++;
        } while (obj);//没有要找的，退出
        return str;//文件名不冲突，返回名字
    }
#endif
}