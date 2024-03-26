using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using static VRTK.SDK_BaseController;

/// <summary>
/// 新手教程按键功能提示
/// </summary>
public class HandControllerTooltips : VRTK_ControllerTooltips
{

    protected override Transform GetTransform(Transform setTransform, SDK_BaseController.ControllerElements findElement)
    {
        Transform returnTransform = null;
        if (setTransform != null)
        {
            returnTransform = setTransform;
        }
        else if (controllerEvents != null)
        {
            GameObject modelController = VRTK_DeviceFinder.GetModelAliasController(controllerEvents.gameObject);

            if (modelController != null && modelController.activeInHierarchy)
            {
                SDK_BaseController.ControllerHand controllerHand = VRTK_DeviceFinder.GetControllerHand(controllerEvents.gameObject);
                string elementPath =GetControllerElementPath(findElement, controllerHand, true);
                returnTransform = (elementPath != null ? modelController.transform.Find(elementPath) : null);
            }
        }

        return returnTransform;
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
