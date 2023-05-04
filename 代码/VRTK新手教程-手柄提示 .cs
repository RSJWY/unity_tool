using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
public class MyGuide : MonoBehaviour
{
    public bool pulseTriggerHighlightColor = false;
    VRTK_ControllerEvents events;
    private VRTK_ControllerTooltips tooltips;
    private VRTK_ControllerHighlighter highligher;
    private float pulseTimer = 0.75f;
    private float dimOpacity = 0.8f;
    private Color highlightColor = Color.yellow;
    private float highlightTimer = 0.5f;
    private Color pulseColor = Color.black;
    private Color currentPulseColor;
    SDK_BaseController.ControllerElements state;


    // Start is called before the first frame update
    void Awake()
    {
        events = GetComponent<VRTK_ControllerEvents>();//事件委托
        highligher = GetComponent<HandHighlighter>();//闪烁提示
        tooltips = GetComponentInChildren<HandToolTips>();//提示工具
        tooltips.ToggleTips(false);//关闭
        highligher.ConfigureControllerPaths();
    }

    private void OnEnable()
    {
        events.TriggerPressed += TriggerPressedEvent;
        events.TouchpadPressed+= TouchPadPressedEvent;
        events.ButtonTwoPressed += ButtonTwoPressedEvent;
        events.ButtonOnePressed += ButtonOnePressedEvent;
        events.GripPressed += GripPressedEvent;
    }

    private void OnDisable()
    {
        events.TriggerPressed -= TriggerPressedEvent;
        events.TouchpadPressed -= TouchPadPressedEvent;
        events.ButtonTwoPressed -= ButtonTwoPressedEvent;
        events.ButtonOnePressed -= ButtonOnePressedEvent;
        events.GripPressed -= GripPressedEvent;
    }



    /// <summary>
    /// 扳机
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TriggerPressedEvent(object sender, ControllerInteractionEventArgs e)
    {
        CancelInvoke();//关闭协程
        state = SDK_BaseController.ControllerElements.Trigger;//更新循环提醒物体枚举
        tooltips.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip);
        highligher.HighlightElement(SDK_BaseController.ControllerElements.Trigger, pulseColor, (pulseTriggerHighlightColor ? pulseTimer : highlightTimer));
        if (pulseTriggerHighlightColor)
        {
            InvokeRepeating(nameof(PulseTrigger), pulseTimer, pulseTimer);
        }
    }
    /// <summary>
    /// 遥杆
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TouchPadPressedEvent(object sender, ControllerInteractionEventArgs e)
    {
        CancelInvoke();//关闭协程
        state = SDK_BaseController.ControllerElements.Touchpad;//更新循环提醒物体枚举
        tooltips.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip);
        highligher.HighlightElement(SDK_BaseController.ControllerElements.Touchpad, pulseColor, (pulseTriggerHighlightColor ? pulseTimer : highlightTimer));
        if (pulseTriggerHighlightColor)
        {
            InvokeRepeating(nameof(PulseTrigger), pulseTimer, pulseTimer);
        }
    }
    /// <summary>
    /// 扳机
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonOnePressedEvent(object sender, ControllerInteractionEventArgs e)
    {
        CancelInvoke();//关闭协程
        state = SDK_BaseController.ControllerElements.ButtonOne;//更新循环提醒物体枚举
        tooltips.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.ButtonOneTooltip);
        highligher.HighlightElement(SDK_BaseController.ControllerElements.ButtonOne, pulseColor, (pulseTriggerHighlightColor ? pulseTimer : highlightTimer));
        if (pulseTriggerHighlightColor)
        {
            InvokeRepeating(nameof(PulseTrigger), pulseTimer, pulseTimer);
        }
    }
    /// <summary>
    /// 扳机
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonTwoPressedEvent(object sender, ControllerInteractionEventArgs e)
    {
        CancelInvoke();//关闭协程
        state = SDK_BaseController.ControllerElements.ButtonTwo;//更新循环提醒物体枚举
        tooltips.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.ButtonTwoTooltip);
        highligher.HighlightElement(SDK_BaseController.ControllerElements.ButtonTwo, pulseColor, (pulseTriggerHighlightColor ? pulseTimer : highlightTimer));
        if (pulseTriggerHighlightColor)
        {
            InvokeRepeating(nameof(PulseTrigger), pulseTimer, pulseTimer);
        }
    }
    /// <summary>
    /// 扳机
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GripPressedEvent(object sender, ControllerInteractionEventArgs e)
    {
        CancelInvoke();//关闭协程
        state = SDK_BaseController.ControllerElements.GripLeft;//更新循环提醒物体枚举
        tooltips.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.GripTooltip);
        highligher.HighlightElement(SDK_BaseController.ControllerElements.GripLeft, pulseColor, (pulseTriggerHighlightColor ? pulseTimer : highlightTimer));
        if (pulseTriggerHighlightColor)
        {
            InvokeRepeating(nameof(PulseTrigger), pulseTimer, pulseTimer);
        }
    }






    /// <summary>
    /// 循环提醒
    /// </summary>
    private void PulseTrigger()
    {
        highligher.HighlightElement(state, currentPulseColor, pulseTimer);
        currentPulseColor = (currentPulseColor == pulseColor ? highlightColor : pulseColor);
    }
    // Update is called once per frame
    void Update()
    {

        
    }
}
