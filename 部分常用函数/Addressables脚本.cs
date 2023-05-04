using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadUI : BaseUI
{
    public Button bachang_bth;
    public Button start_bth;


    protected override void Awake()
    {
        base.Awake();
        bachang_bth.onClick.AddListener(() =>
        {
            MyMsg.inst.Send(MsgEventName.UIEndShow, MsgUIEventName.DangQianXianShi, transform);
            MyMsg.inst.Send(MsgEventName.GuangBO,MsgUIEventName.ScenecLoad);
            StartCoroutine(LoadBaChang());
        });
        start_bth.onClick.AddListener(() =>
        {
            MyMsg.inst.Send(MsgEventName.UIEndShow, MsgUIEventName.DangQianXianShi, transform);
            MyMsg.inst.Send(MsgEventName.GuangBO, MsgUIEventName.ScenecLoad);
            SceneManager.LoadScene("zhanchang");
        });
    }
    /// <summary>
    /// 加载场景
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadBaChang()
    {
        Text txt = bachang_bth.transform.GetChild(0).GetComponent<Text>();
        // 异步加载场景(如果场景资源没有下载，会自动下载)，
        var handle = Addressables.LoadAssetAsync<GameObject>("靶场");
        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("场景加载异常: " + handle.OperationException.ToString());
            yield break;
        }
        txt.fontSize = 36;
        while (!handle.IsDone)
        {
            // 进度（0~1）
            float percentage = handle.PercentComplete;
            txt.text = "正在加载"+ percentage.ToString("0.00");
            yield return null;
        }
        txt.text = "正在跳转";
        yield return new WaitForSeconds(0.5f);
        Addressables.LoadSceneAsync("Assets/Scenes/BaChang.unity").Completed += (SceneInstance) =>
        {
            SceneManager.LoadScene("BaChang");
        };

    }
    // Start is called before the first frame update
    protected override void Start()
    {
        Invoke(nameof(Init),1f);
    }

    void Init()
    {
        UIadmin.startAppear();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }