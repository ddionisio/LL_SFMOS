using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class GameObjectSelectible : MonoBehaviour,
    IPointerDownHandler, 
    ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
    {
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

    public bool isLocked {
        get { return mIsLocked; }
        set {
            if(mIsLocked != value) {
                mIsLocked = value;
                if(mIsLocked) {
                    isSelected = false;

                    if(highlightGO)
                        highlightGO.SetActive(false);
                }

                if(mColl) mColl.enabled = !mIsLocked;
            }
        }
    }
    
    public System.Action<GameObjectSelectible> selectCallback; //when entity is selected/unselected
    public System.Action<GameObjectSelectible, PointerEventData> dragBeginCallback;
    public System.Action<GameObjectSelectible, PointerEventData> dragCallback;
    public System.Action<GameObjectSelectible, PointerEventData> dragEndCallback;

    private bool mIsSelected;
    private bool mIsLocked;

    private Collider2D mColl;

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

        mColl = GetComponent<Collider2D>();
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        eventData.selectedObject = gameObject;
    }
    
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

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(dragBeginCallback != null)
            dragBeginCallback(this, eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(dragCallback != null)
            dragCallback(this, eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(dragEndCallback != null)
            dragEndCallback(this, eventData);
    }
}
