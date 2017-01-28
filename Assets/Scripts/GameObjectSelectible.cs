using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class GameObjectSelectible : MonoBehaviour,
    IPointerDownHandler, //IPointerUpHandler, 
    ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler {
    public GameObject highlightGO; //when highlighted
    public GameObject selectGO; //when selected

    public bool isSelected {
        get { return mIsSelected; }

        private set {
            if(mIsSelected != value) {
                mIsSelected = value;

                if(selectGO)
                    selectGO.SetActive(mIsSelected);

                if(selectCallback != null)
                    selectCallback(this);
            }
        }
    }

    /// <summary>
    /// This is the last pointer event received from IPointerDownHandler.OnPointerDown
    /// </summary>
    public PointerEventData lastPointerEventData { get { return mLastPointerEventData; } }
    
    public System.Action<GameObjectSelectible> selectCallback; //when entity is selected/unselected

    private bool mIsSelected;
    private PointerEventData mLastPointerEventData;

    void OnDisable() {
        isSelected = false;

        if(highlightGO)
            highlightGO.SetActive(false);
    }
    
    void Awake() {
        if(highlightGO)
            highlightGO.SetActive(false);

        if(selectGO)
            selectGO.SetActive(false);
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        mLastPointerEventData = eventData;

        eventData.selectedObject = gameObject;
    }

    //void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
    //}

    void ISelectHandler.OnSelect(BaseEventData eventData) {
        isSelected = true;
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData) {
        isSelected = false;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(highlightGO)
            highlightGO.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO)
            highlightGO.SetActive(false);
    }
}
