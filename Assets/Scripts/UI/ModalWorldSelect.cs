using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalWorldSelect : M8.UIModal.Controller, IPush {
    public const string parmCamRefs = "cam";
    public const string parmBoundsRefs = "bounds";

    public RectTransform target;
        
    void IPush.Push(M8.GenericParams parms) {
        var cam = parms.GetValue<Camera>(parmCamRefs);
        var bounds = parms.GetValue<Bounds>(parmBoundsRefs);
        
        Vector3 
            min = RectTransformUtility.WorldToScreenPoint(cam, bounds.min), 
            max = RectTransformUtility.WorldToScreenPoint(cam, bounds.max);

        target.position = new Vector3(
            Mathf.Lerp(min.x, max.x, target.pivot.x),
            Mathf.Lerp(min.y, max.y, target.pivot.y),
            0f);

        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(max.x - min.x));
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(max.y - min.y));
    }
}
