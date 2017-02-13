using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LessonInputField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    public Collider2D coll;

    public Vector2 curPos {
        get {
            return mCurPos;
        }
    }

    public bool isLocked {
        get { return mIsLocked; }

        set {
            if(mIsLocked != value) {
                mIsLocked = value;
                
                if(mIsLocked)
                    Cancel();
            }
        }
    }

    public LessonCard curCard {
        get {
            if(mCurCardIndex == -1) return null;

            return mCards[mCurCardIndex];
        }
    }

    public event Action<LessonInputField> pointerUpCallback;
    public event Action<LessonInputField> pointerDownCallback;
    public event Action<LessonInputField> pointerDragCallback;

    private Vector2 mOrigPos;
    private Vector2 mCurPos;

    private bool mIsLocked;

    private LessonCard[] mCards;
    private int mCurCardIndex; //card held

    public void Populate(LessonCard[] cards) {
        mCards = cards;
        mCurCardIndex = -1;
    }

    public void Cancel() {
        if(mCurCardIndex != -1) {
            //revert card
            mCards[mCurCardIndex].Return();

            mCurCardIndex = -1;
        }
    }

    public void ClearCurrent() {
        mCurCardIndex = -1;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if(mIsLocked)
            return;

        mOrigPos = ComputePosition(eventData.pointerCurrentRaycast.worldPosition);
        mCurPos = mOrigPos;

        //check which card is within point
        int cardInd = GetCardIndex(mCurPos);
        if(mCurCardIndex != -1 && mCurCardIndex != cardInd) {
            Cancel();
        }

        mCurCardIndex = cardInd;
        if(mCurCardIndex != -1) {
            mCards[mCurCardIndex].PointerDown(mCurPos);
        }

        if(pointerDownCallback != null)
            pointerDownCallback(this);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        if(mIsLocked)
            return;

        mCurPos = ComputePosition(eventData.pointerCurrentRaycast.worldPosition);

        //assume this will take care of the card to either dock it, or return
        if(pointerUpCallback != null)
            pointerUpCallback(this);
        
        //if it's not processed, just return it
        Cancel();
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(mIsLocked)
            return;

        mCurPos = ComputePosition(eventData.pointerCurrentRaycast.worldPosition);

        if(mCurCardIndex != -1) {
            mCards[mCurCardIndex].PointerDragUpdate(mCurPos);
        }

        if(pointerDragCallback != null)
            pointerDragCallback(this);
    }

    Vector2 ComputePosition(Vector2 pos) {
        //clamp drag position
        var bounds = coll.bounds;
        var boundMin = bounds.min;
        var boundMax = bounds.max;

        return new Vector2(Mathf.Clamp(pos.x, boundMin.x, boundMax.x), Mathf.Clamp(pos.y, boundMin.y, boundMax.y));
    }

    int GetCardIndex(Vector2 pos) {
        for(int i = 0; i < mCards.Length; i++) {
            if(!mCards[i].isDocked && !mCards[i].isMoving && mCards[i].worldBounds.Contains(pos))
                return i;
        }

        return -1;
    }
}
