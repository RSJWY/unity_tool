using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;
using static VRTK.SDK_BaseController;
/// <summary>
/// 高亮闪烁组件
/// </summary>
public class HandControllerHighlighter : VRTK_ControllerHighlighter
{
   
    /// <summary>
    /// The ConfigureControllerPaths method is used to set up the model element paths.
    /// </summary>
    public override void ConfigureControllerPaths()
    {
        cachedElements.Clear();
        modelElementPaths.bodyModelPath = GetElementPath(modelElementPaths.bodyModelPath, SDK_BaseController.ControllerElements.Body);
        modelElementPaths.triggerModelPath = GetElementPath(modelElementPaths.triggerModelPath, SDK_BaseController.ControllerElements.Trigger);
        modelElementPaths.leftGripModelPath = GetElementPath(modelElementPaths.leftGripModelPath, SDK_BaseController.ControllerElements.GripLeft);
        modelElementPaths.rightGripModelPath = GetElementPath(modelElementPaths.rightGripModelPath, SDK_BaseController.ControllerElements.GripRight);
        modelElementPaths.touchpadModelPath = GetElementPath(modelElementPaths.touchpadModelPath, SDK_BaseController.ControllerElements.Touchpad);
        modelElementPaths.touchpadTwoModelPath = GetElementPath(modelElementPaths.touchpadTwoModelPath, SDK_BaseController.ControllerElements.TouchpadTwo);
        modelElementPaths.buttonOneModelPath = GetElementPath(modelElementPaths.buttonOneModelPath, SDK_BaseController.ControllerElements.ButtonOne);
        modelElementPaths.buttonTwoModelPath = GetElementPath(modelElementPaths.buttonTwoModelPath, SDK_BaseController.ControllerElements.ButtonTwo);
        modelElementPaths.systemMenuModelPath = GetElementPath(modelElementPaths.systemMenuModelPath, SDK_BaseController.ControllerElements.SystemMenu);
        modelElementPaths.startMenuModelPath = GetElementPath(modelElementPaths.startMenuModelPath, SDK_BaseController.ControllerElements.StartMenu);
    }
    protected override string GetElementPath(string currentPath, SDK_BaseController.ControllerElements elementType)
    {
        SDK_BaseController.ControllerHand currentHand = VRTK_DeviceFinder.GetControllerHand(actualController);
        string foundElementPath = GetControllerElementPath(elementType, currentHand);
        return (currentPath.Trim() == "" && foundElementPath != null ? foundElementPath : currentPath.Trim());
    }
    public override void PopulateHighlighters()
    {
        if (actualController != null)
        {
            highlighterOptions.Clear();
            VRTK_SharedMethods.AddDictionaryValue(highlighterOptions, "resetMainTexture", true, true);

            autoHighlighter = false;
            baseHighlighter = GetValidHighlighter();
            if (baseHighlighter == null)
            {
                autoHighlighter = true;
                baseHighlighter = actualController.AddComponent<VRTK_MaterialColorSwapHighlighter>();
            }

            SDK_BaseController.ControllerHand currentHand = VRTK_DeviceFinder.GetControllerHand(actualController);

            baseHighlighter.Initialise(null, actualController, highlighterOptions);

            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.ButtonOne, currentHand)), baseHighlighter, elementHighlighterOverrides.buttonOne);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.ButtonTwo, currentHand)), baseHighlighter, elementHighlighterOverrides.buttonTwo);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.Body, currentHand)), baseHighlighter, elementHighlighterOverrides.body);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.GripLeft, currentHand)), baseHighlighter, elementHighlighterOverrides.gripLeft);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.GripRight, currentHand)), baseHighlighter, elementHighlighterOverrides.gripRight);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.StartMenu, currentHand)), baseHighlighter, elementHighlighterOverrides.startMenu);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.SystemMenu, currentHand)), baseHighlighter, elementHighlighterOverrides.systemMenu);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.Touchpad, currentHand)), baseHighlighter, elementHighlighterOverrides.touchpad);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.TouchpadTwo, currentHand)), baseHighlighter, elementHighlighterOverrides.touchpadTwo);
            AddHighlighterToElement(GetElementTransform(GetControllerElementPath(SDK_BaseController.ControllerElements.Trigger, currentHand)), baseHighlighter, elementHighlighterOverrides.trigger);
        }
    }


    /// <summary>
    /// 暂时这么用着，投机取巧方案
    /// </summary>
    /// <param name="element"></param>
    /// <param name="hand"></param>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    string GetControllerElementPath(ControllerElements element, ControllerHand hand, bool fullPath = false)
    {
        string suffix = (fullPath ? "/attach" : "");
        switch (element)
        {
            case ControllerElements.AttachPoint:
                return "tip/attach";
            case ControllerElements.Trigger:
                return "trigger" + suffix;
            case ControllerElements.GripLeft:
                return "button_grip" + suffix;
            case ControllerElements.GripRight:
                return "button_grip" + suffix;
            case ControllerElements.Touchpad:
                return "thumbstick" + suffix;
            case ControllerElements.ButtonOne:
                return "button_a" + suffix;
            case ControllerElements.ButtonTwo:
                return "button_b" + suffix;
            //case ControllerElements.SystemMenu:
            //    return GetControllerSystemMenuPath(hand, suffix);
            case ControllerElements.StartMenu:
                return "button_enter" + suffix;
            case ControllerElements.Body:
                return "body";
        }
        return "";
    }
}
