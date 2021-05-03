using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Determines gather position, and launch vector.  Requires BoxCollider2D
/// </summary>
public class MucusGatherInputField : M8.SingletonBehaviour<MucusGatherInputField>,
    IPointerDownHandler, IPointerUpHandler, IDragHandler {
    public const int registeredInputActiveCapacity = 8;

    public enum AreaType {
        None,
        Top,
        Bottom
    }

    public Collider2D coll;

    public float split = 0.5f; //the split between launch and drag area
    
    public Vector2 originPosition { get { return mOrigPos; } }

    public EntityMucusFormInput currentLaunchInput { get { return mCurLaunchInput; } }

    public AreaType currentAreaType { get { return mCurArea; } }
    public Vector2 currentPosition { get { return mCurPos; } }

    public Vector2 dir { get { return mDir; } }
    public float curLength { get { return mCurLength; } }

    public bool isLocked {
        get { return mIsLocked; }

        set {
            if(mIsLocked != value) {
                mIsLocked = value;

                if(lockChangeCallback != null)
                    lockChangeCallback(this);

                Cancel();
            }
        }
    }

    public event Action<MucusGatherInputField> pointerDownCallback;
    public event Action<MucusGatherInputField> pointerDragCallback;
    public event Action<MucusGatherInputField> pointerUpCallback;
    public event Action<MucusGatherInputField> lockChangeCallback;
    
    private AreaType mCurArea = AreaType.None;
    
    private Vector2 mOrigPos;
    private Vector2 mCurPos;
    private Vector2 mDir;
    private float mCurLength;

    private bool mIsLocked;

    private EntityMucusFormInput mCurLaunchInput;
    private M8.CacheList<EntityMucusFormInput> mRegisteredInputs;

    public void SetRegisterInput(EntityMucusFormInput input, bool add) {
        if(add)
            mRegisteredInputs.Add(input);
        else
            mRegisteredInputs.Remove(input);
    }

    public void Launch(Bounds bounds) {
        if(mCurLaunchInput) {
            var ent = mCurLaunchInput.entity;

            if(ent is EntityMucusForm)
                ((EntityMucusForm)ent).Launch(mDir, mCurLength, bounds);
        }
    }

    public void Cancel() {
        if(mCurLaunchInput) {
            mCurLaunchInput.Cancel();
            mCurLaunchInput = null;
        }
    }

    EntityMucusFormInput GetInput(Vector2 pos) {
        for(int i = 0; i < mRegisteredInputs.Count; i++) {
            Vector2 epos = mRegisteredInputs[i].transform.position;
            var dpos = epos - pos;
            var sqrDist = dpos.sqrMagnitude;
            if(sqrDist <= mRegisteredInputs[i].radius)
                return mRegisteredInputs[i];
        }

        return null;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if(mIsLocked)
            return;

        ComputePosition(eventData.pointerCurrentRaycast.worldPosition, out mOrigPos, out mCurArea);

        var lastInput = mCurLaunchInput;
        mCurLaunchInput = GetInput(mOrigPos);
        if(!mCurLaunchInput)
            return;

        if(lastInput && lastInput != mCurLaunchInput)
            lastInput.Cancel();

        mCurLaunchInput.Select();

        mCurPos = mOrigPos;
        mDir = Vector2.zero;
        mCurLength = 0f;

        //Debug.Log("input down: "+eventData.pointerCurrentRaycast.worldPosition.ToString()+" "+eventData.pointerCurrentRaycast.screenPosition.ToString());
           
        if(pointerDownCallback != null)
            pointerDownCallback(this);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        if(mIsLocked || !mCurLaunchInput)
            return;
        
        //Debug.Log("input up: "+eventData.pointerCurrentRaycast.worldPosition.ToString()+" "+eventData.pointerCurrentRaycast.screenPosition.ToString());
        ComputePosition(eventData.pointerCurrentRaycast.worldPosition, out mCurPos, out mCurArea);

        mDir = mCurPos - mOrigPos;
        mCurLength = mDir.magnitude;
        if(mCurLength > 0f)
            mDir /= mCurLength;

        if(pointerUpCallback != null)
            pointerUpCallback(this);

        if(mCurLaunchInput) {
            mRegisteredInputs.Remove(mCurLaunchInput);
            mCurLaunchInput = null;
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(mIsLocked || !mCurLaunchInput)
            return;

        if(mCurLaunchInput.isLocked) {
            mCurLaunchInput.Cancel();
            mCurLaunchInput = null;
            return;
        }

        if(!eventData.pointerCurrentRaycast.isValid)
            return;

        ComputePosition(eventData.pointerCurrentRaycast.worldPosition, out mCurPos, out mCurArea);

        mDir = mCurPos - mOrigPos;
        mCurLength = mDir.magnitude;
        if(mCurLength > 0f)
            mDir /= mCurLength;
        
        if(pointerDragCallback != null)
            pointerDragCallback(this);
    }

    protected override void OnInstanceInit() {
        mRegisteredInputs = new M8.CacheList<EntityMucusFormInput>(registeredInputActiveCapacity);
    }
    
    void ComputePosition(Vector2 pos, out Vector2 resultPos, out AreaType resultAreaType) {
        //clamp drag position
        var bounds = coll.bounds;
        var boundMin = bounds.min;
        var boundMax = bounds.max;

        resultPos = new Vector2(Mathf.Clamp(pos.x, boundMin.x, boundMax.x), Mathf.Clamp(pos.y, boundMin.y, boundMax.y));

        float splitY = Mathf.Lerp(boundMin.y, boundMax.y, Mathf.Clamp01(split));

        if(resultPos.y > splitY)
            resultAreaType = AreaType.Top;
        else
            resultAreaType = AreaType.Bottom;
    }

    void OnDrawGizmos() {
        //
        if(!coll)
            return;

        Bounds b = coll.bounds;

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
