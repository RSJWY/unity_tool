using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Leap;
using Leap.Unity;
using Newtonsoft.Json;
using UnityEngine;
using YooAsset;
using IngameDebugConsole;

public class Manager_Hotcode : MonoBehaviour
{
    ResourcePackage prefabPackage;
    Dictionary<string, CommandEnum> command_dic = new();
    ResourcePackage rawFilePackage;
    RawFileHandle configRH;
    AssetHandle bookAH;
    AssetHandle gestureAH;
    
    [SerializeField]
    LeapMotionPrefab handPrefab;

    [SerializeField]
    BookPro book;
    [SerializeField]
    AutoFlip autoFlip;

    [Tooltip("手部的最小移动速度")]
    public float smallestVelocity = 2f;
    [Tooltip("姿势判断的速度")]
    public float deltaVelocity = 0.5f;

    /// <summary>
    /// leapmotion控制器
    /// </summary>
    public LeapProvider leapMotion;
    /// <summary>
    /// 手的信息
    /// </summary>

    private Frame mFrame;
    /// <summary>
    /// 手的物理特征
    /// </summary>
    private Hand mHand;


    /// <summary>
    /// 姿势判断锁
    /// </summary>
    bool flipBookLock;
    /// <summary>
    /// 手进入锁
    /// </summary>
    bool judgmentLock;


    void Awake()
    {
        prefabPackage = YooAssets.GetPackage(PackageNameEnum.DataAndPrefab.ToString());
        rawFilePackage = YooAssets.GetPackage(PackageNameEnum.RawFileAndHotCode.ToString());
    }
    // Start is called before the first frame update
    async void Start()
    {
        await LoadJsonConfigAndRunUDPListen();
        await LaodBook();
        MsgContorl.instance.AddEvent(BookPageEvent.NowPage,(int a)=>{
            Debug.Log($"当前面数：{a}");
        });
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            handPrefab.handModle.SetActive(!handPrefab.handModle.activeSelf);
        }
    }

    #region 手势判断
    /// <summary>
    /// 翻书手势
    /// </summary>
    void OnPoseEvent()
    {
        //翻书事件回调
        handPrefab.fanShuPose.WhilePoseDetected.AddListener(() =>
            {
                mFrame = leapMotion.CurrentFrame;//当前帧的手识别
                mHand = mFrame.Hands[0];//获取第一个识别到的手
                if (IsMoveLeft(mHand))
                {
                    PreviousPage();
                    //Debug.Log("向左" + _hand.PalmVelocity.x);
                }
                else if (IsMoveRight(mHand))
                {
                    NextPage();
                    //Debug.Log("向右" + _hand.PalmVelocity.x);
                }
            });
            //握拳事件回调
        handPrefab.woQuanPose.OnPoseDetected.AddListener(() =>
            {
                ResetToStart();
            });
    }

     /// <summary>
    /// 手滑向左边
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    bool IsMoveLeft(Hand hand)   // 手划向左边
    {
        return hand.PalmVelocity.x < -deltaVelocity;
    }

    /// <summary>
    /// 手滑向右边
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    bool IsMoveRight(Hand hand)
    {
        return hand.PalmVelocity.x > deltaVelocity;
    }
    /// <summary>
    /// 判断是不是握拳
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    bool IsCloseHand(Hand hand)
    {
        List<Finger> listOfFingers = hand.Fingers;//获取手指列表
        int count = 0;
        for (int f = 0; f < listOfFingers.Count; f++)
        {
            Finger finger = listOfFingers[f];
            if ((finger.TipPosition - hand.PalmPosition).magnitude < 0.05f)//指尖和掌心的位置检测
            {
                count++;
            }
        }
        return (count >= 4);//当有四个指尖靠近中心，返回真，
    }


    void leapMotionIdentify()
    {
        if (leapMotion == null)
            return;
        mFrame = leapMotion.CurrentFrame;
        GestureJudgment();
        HandCount();
    }
    /// <summary>
    /// 手势判断
    /// </summary>
    void GestureJudgment()
    {
        //获取当前帧,获得手的个数

        if (mFrame.Hands.Count > 0 && judgmentLock == true && flipBookLock == false)
        {
            Hand _hand = mFrame.Hands[0];//获取识别到的手
            if (_hand.GrabStrength == 0)//手是打开状态
            {
                //float movePos= _hand.PalmPosition.x;//获取
                if (IsCloseHand(_hand))
                {
                    return;//确定不是握拳状态
                }
                else if (IsMoveLeft(_hand))
                {
                    PreviousPage();
                    //Debug.Log("向左" + _hand.PalmVelocity.x);
                }
                else if (IsMoveRight(_hand))
                {
                    NextPage();
                    //Debug.Log("向右" + _hand.PalmVelocity.x);
                }
            }

        }
    }

    bool StartAndEndPages()
    {
        //if (bookPro.CurrentPaper >= bookPro.papers.Length - 1) return;
        //if (bookPro.CurrentPaper <= 1) return;

        return false;
    }

    /// <summary>
    /// 手进入可以开始判断
    /// </summary>
    void HandCount()
    {
        if (mFrame.Hands.Count > 0)
        {
            Invoke(nameof(JudgmentStart), 0.5f);
        }
        else
        {
            CancelInvoke(nameof(JudgmentStart));
            judgmentLock = false;
        }
    }

    /// <summary>
    /// 等待恢复可翻页状态
    /// </summary>
    /// <param name="_time"></param>
    void WaitingInterval()
    {
        flipBookLock = false;
    }
    /// <summary>
    /// 开始识别
    /// </summary>
    void JudgmentStart()
    {
        judgmentLock = true;
    }

    /// <summary>
    /// 是否静止状态
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    public bool isStationary(Hand hand)
    {
        return hand.PalmVelocity.magnitude < smallestVelocity;      //Vector3.Magnitude返回向量的长度
    }
    #endregion
    #region 翻书事件

    /// <summary>
    /// 下一页
    /// </summary>
    public void NextPage()
    {
        if (!autoFlip._isPageFlipping)
            autoFlip.FlipLeftPage();
    }
    /// <summary>
    /// 上一页
    /// </summary>
    public void PreviousPage()
    {
        if (!autoFlip._isPageFlipping)
            autoFlip.FlipRightPage();
    }
    /// <summary>
    /// 复位
    /// </summary>
    public void ResetToStart()
    {
        ToPages(book.StartFlippingPaper);
    }
    //指定跳转到某一个页面
    public void ToPages(int pagenumber)
    {
        if (!autoFlip._isPageFlipping)
        {
            int newpaper = Mathf.Clamp(pagenumber, book.StartFlippingPaper, book.EndFlippingPaper);
            book.CurrentPaper = newpaper;
            book.UpdatePages();
            MsgContorl.instance.SendEvent(BookPageEvent.NowPage, book.currentPaper);
            Debug.Log($"跳转到：{pagenumber}");
        }
    }
    #endregion
    #region UDP监听
    void CommandEvent(string command)
    {
        Debug.Log($"指令为：{command}");
        //相关指令
        if (command_dic.ContainsKey(command))
        {
            switch (command_dic[command])
            {
                case CommandEnum.LeftToRight:
                    NextPage();
                    break;
                case CommandEnum.RightToLeft:
                    PreviousPage();
                    break;
                case CommandEnum.ResetToStart:
                    ResetToStart();
                    break;
            }
        }
        else
        {
            //判断跳转页指令
            string[] _strarr = command.Split(" ");
            if (_strarr[0] == "AB" && _strarr[1] == "DE")
            {
                int result;
                if (int.TryParse(_strarr[2], out result))
                {
                    ToPages(result);
                }
                else
                {
                    Debug.LogWarning($"{command} 指令不存在");
                }
            }
            else
            {
                Debug.LogWarning($"{command} 指令不存在");
            }
        }
    }
    #endregion
    /// <summary>
    /// 加载书相关内容
    /// </summary>
    /// <returns></returns>
    async UniTask LaodBook()
    {
        //加载
        bookAH = prefabPackage.LoadAssetAsync("Prefab_BookPro");
        await bookAH.Task;
        gestureAH = prefabPackage.LoadAssetAsync("Prefab_Gesture");
        await gestureAH.Task;
        //提交初始化
        await Instantiate();

    }
    /// <summary>
    /// 初始化整体
    /// </summary>
    /// <returns></returns>
    async UniTask Instantiate()
    {
        //异步初始化书
        InstantiateOperation bookIO = bookAH.InstantiateAsync(this.transform.parent);
        await bookIO.Task;
        InstantiateOperation gIO = gestureAH.InstantiateAsync();
        await gIO.Task;
        //获取相关组件
        book = bookIO.Result.GetComponent<BookPro>();
        autoFlip = bookIO.Result.GetComponent<AutoFlip>();
        book.CurrentPaper = 1;
        book.UpdatePages();
        book.OnFlip.AddListener(() =>
        {
            //通知当前翻书的页数
            MsgContorl.instance.SendEvent(BookPageEvent.NowPage, book.currentPaper);
        });
        //初始化手势识别
        handPrefab = gIO.Result.GetComponent<LeapMotionPrefab>();
        leapMotion = handPrefab.leapmotionServer;
        //handPose = gIO.Result.GetComponentInChildren<HandPoseDetector>();
        OnPoseEvent();
    }
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns></returns>
    async UniTask LoadJsonConfigAndRunUDPListen()
    {
        configRH = rawFilePackage.LoadRawFileAsync("Config_Config");
        await configRH.Task;
        ConfigRoot configjson = JsonConvert.DeserializeObject<ConfigRoot>(configRH.GetRawFileText());

        command_dic.Add(configjson.BookCommand.LeftToRight, CommandEnum.LeftToRight);
        command_dic.Add(configjson.BookCommand.RightToLeft, CommandEnum.RightToLeft);
        command_dic.Add(configjson.BookCommand.ResetToStart, CommandEnum.ResetToStart);

        UDPController.instance.Init(configjson.UDPListenSetting.IP, configjson.UDPListenSetting.Port);
        UDPController.instance.MsgActionEvent = CommandEvent;
    }

}
