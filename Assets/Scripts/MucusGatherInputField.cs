using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Determines gather position, and launch vector.  Requires BoxCollider2D
/// </summary>
public class MucusGatherInputField : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler {
    public enum AreaType {
        None,
        Top,
        Bottom
    }

    public float split = 0.5f; //the split between launch and drag area

    public bool isDown { get { return mIsDown; } }
    
    public Vector2 originPosition { get { return mOrigPos; } }

    public AreaType currentAreaType { get { return mCurArea; } }
    public Vector2 currentPosition { get { return mCurPos; } }

    public event Action<MucusGatherInputField> pointerDownCallback;
    public event Action<MucusGatherInputField> pointerDragCallback;
    public event Action<MucusGatherInputField> pointerUpCallback;

    private BoxCollider2D mColl;

    private AreaType mCurArea = AreaType.None;

    private bool mIsDown; //is held

    private RaycastResult mOrigResult;
    private RaycastResult mCurResult;

    private Vector2 mOrigPos;
    private Vector2 mCurPos;

    public void ApplyCurrentToOrigin() {
        mOrigResult = mCurResult;
        mOrigPos = mCurPos;
    }
    
    void Awake() {
        mColl = GetComponent<BoxCollider2D>();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        //Debug.Log("input down: "+eventData.pointerCurrentRaycast.worldPosition.ToString()+" "+eventData.pointerCurrentRaycast.screenPosition.ToString());

        mIsDown = true;

        mOrigResult = eventData.pointerCurrentRaycast;
        
        ComputePosition(mOrigResult.worldPosition, out mOrigPos, out mCurArea);

        mCurPos = mOrigPos;

        if(pointerDownCallback != null)
            pointerDownCallback(this);
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        //Debug.Log("input up: "+eventData.pointerCurrentRaycast.worldPosition.ToString()+" "+eventData.pointerCurrentRaycast.screenPosition.ToString());

        mIsDown = false;

        ComputePosition(eventData.pointerCurrentRaycast.worldPosition, out mCurPos, out mCurArea);

        if(pointerUpCallback != null)
            pointerUpCallback(this);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        mCurResult = eventData.pointerCurrentRaycast;

        if(!mCurResult.isValid || mCurResult.gameObject != gameObject)
            return;
        
        ComputePosition(mCurResult.worldPosition, out mCurPos, out mCurArea);

        if(pointerDragCallback != null)
            pointerDragCallback(this);
    }

    void ComputePosition(Vector2 pos, out Vector2 resultPos, out AreaType resultAreaType) {
        //clamp drag position
        var bounds = mColl.bounds;
        var boundMin = bounds.min;
        var boundMax = bounds.max;

        resultPos.x = Mathf.Clamp(pos.x, boundMin.x, boundMax.x);
        resultPos.y = Mathf.Clamp(pos.y, boundMin.y, boundMax.y);

        float splitY = Mathf.Lerp(boundMin.y, boundMax.y, Mathf.Clamp01(split));

        if(resultPos.y > splitY)
            resultAreaType = AreaType.Top;
        else
            resultAreaType = AreaType.Bottom;
    }

    void OnDrawGizmos() {
        var coll = GetComponent<BoxCollider2D>();
        if(coll) {
            //
            Bounds b = new Bounds(
                new Vector3(transform.position.x + coll.offset.x, transform.position.y + coll.offset.y, 0f),
                new Vector3(coll.size.x*transform.localScale.x, coll.size.y*transform.localScale.y, 0f));

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(b.center, b.size);

            //
            float splitY = Mathf.Lerp(b.min.y, b.max.y, Mathf.Clamp01(split));

            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                new Vector3(b.min.x, splitY, 0f),
                new Vector3(b.max.x, splitY, 0f));
        }
    }
}
