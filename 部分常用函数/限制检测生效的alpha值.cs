using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 改写Image，限制射线检测生效的alpha值
/// </summary>
public class MyImage : MonoBehaviour
{
    Image image;
    private void Start()
    {
        image = transform.GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = 0.1f;
    }
}
