using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
/// <summary>
/// 保存修改后的物体的刚体属性和父子关系
/// </summary>
public class InteractableObject : VRTK_InteractableObject
{


    public override void SaveCurrentState()
    {
        if (IsGrabbed() && !snappedInSnapDropZone)
        {
            previousParent = transform.parent;
            if (!IsSwappable())
            {
                previousIsGrabbable = isGrabbable;
            }

            if (interactableRigidbody != null)
            {
                previousKinematicState = interactableRigidbody.isKinematic;
            }
        }
    }
}
