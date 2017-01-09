using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventTest : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler {

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        Debug.LogWarning("Dick Down");
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        Debug.LogWarning("Dick Up");
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        Debug.LogWarning("Dick Enter");
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        Debug.LogWarning("Dick Exit");
    }

    void ISelectHandler.OnSelect(BaseEventData eventData) {
        Debug.LogWarning("Dick Select");
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData) {
        Debug.LogWarning("Dick Deselect");
    }
}
