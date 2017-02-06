using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPhagocyteTentacle : M8.EntityBase {
    public M8.Animator.AnimatorData animator;

    [Header("Gather Info")]
    public Transform pointer;
    public float speed;

    [Header("Head Info")]
    public SpriteRenderer headSpriteRender;
    [M8.SortingLayer]
    public string headPullBackSort;
    public int headPullBackSortOrder;

    [Header("Takes")]
    public string takeEat;

    public int score { get { return mScore; } }

    private string mHeadDefaultSort;
    private int mHeadDefaultOrder;

    private M8.EntityBase mTarget;
    private Vector2 mTargetDir;
    private float mDist;

    private int mScore;

    private Coroutine mRout;

    public void Gather(M8.EntityBase target, Vector2 dir, float dist, int aScore) {
        mTarget = target;
        mTargetDir = dir;
        mDist = dist;
        mScore = aScore;

        state = (int)EntityState.Gather;
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)state) {
            case EntityState.Gather:
                mRout = StartCoroutine(DoGather());
                break;
        }

        //Gather, Gathered
    }

    protected override void OnDespawned() {
        //reset stuff here
        mRout = null;

        if(mTarget) {
            mTarget.Release();
            mTarget = null;
        }

        headSpriteRender.gameObject.SetActive(false);
        headSpriteRender.sortingLayerName = mHeadDefaultSort;
        headSpriteRender.sortingOrder = mHeadDefaultOrder;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
    }

    /*protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }*/

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mHeadDefaultSort = headSpriteRender.sortingLayerName;
        mHeadDefaultOrder = headSpriteRender.sortingOrder;
    }
    
    IEnumerator DoGather() {
        if(mDist > 0f && mTarget) {
            Vector2 sPos = transform.position;
            Vector2 ePos = mTarget.transform.position;
            float delay = mDist/speed;
                        
            //go towards
            var ease = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);
            float curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Clamp01(ease(curTime, delay, 0f, 0f));

                if(!float.IsNaN(t))
                    pointer.position = Vector2.Lerp(sPos, ePos, t);
            }

            //eat
            pointer.up = mTargetDir;

            animator.Play(takeEat);
            while(animator.isPlaying)
                yield return null;

            mTarget.Release();
            mTarget = null;

            //pull back
            headSpriteRender.sortingLayerName = headPullBackSort;
            headSpriteRender.sortingOrder = headPullBackSortOrder;

            ease = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InCirc);
            curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Clamp01(ease(curTime, delay, 0f, 0f));

                if(!float.IsNaN(t))
                    pointer.position = Vector2.Lerp(ePos, sPos, t);
            }
        }
        
        mRout = null;

        state = (int)EntityState.Gathered;
    }
}
