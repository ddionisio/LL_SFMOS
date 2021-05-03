using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neutrophil : M8.EntityBase {

    [Header("Info")]
    public float speed = 4f;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;
    public string takeNormal;
    public string takeLaunch;
    public string takeExplode;
    public string takeLeave;

    private Coroutine mRout;
    private EntityCommon mTarget;
    private Transform mFollow;

    public void Follow(Transform follow) {
        mFollow = follow;

        if(mFollow)
            state = (int)EntityState.Seek;
        else
            state = (int)EntityState.Normal;
    }

    public void Launch(EntityCommon launch) {
        mTarget = launch;

        if(mTarget)
            state = (int)EntityState.Launch;
        else
            state = (int)EntityState.Normal;
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Seek:
                mFollow = null;
                break;
            case EntityState.Launch:
                mTarget = null;
                break;
        }

        switch((EntityState)state) {
            case EntityState.Normal:
                animator.Play(takeNormal);
                break;

            case EntityState.Seek:
                animator.Play(takeNormal);
                mRout = StartCoroutine(DoFollow());
                break;

            case EntityState.Launch:
                animator.Play(takeLaunch);
                mRout = StartCoroutine(DoLaunch());                
                break;

            case EntityState.Leave:
                animator.Play(takeLeave);
                break;

            case EntityState.Dead:
                animator.Play(takeExplode);
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        mTarget = null;
        mFollow = null;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        state = (int)EntityState.Normal;
    }

    protected override void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimationComplete;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        animator.takeCompleteCallback += OnAnimationComplete;
    }

    void OnAnimationComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takeLeave || take.name == takeExplode)
            Release();
    }

    IEnumerator DoFollow() {
        while(mFollow) {
            transform.position = mFollow.position;
            yield return null;
        }

        mRout = null;

        state = (int)EntityState.Normal;
    }

    IEnumerator DoLaunch() {
        yield return null;

        if(mTarget) {
            float delay = 0;

            Vector2 sPos = transform.position;
            Vector2 ePos = mTarget.transform.position; //this is just for initial setup of duration
            Vector2 dpos = ePos - sPos;
            float distSqr = dpos.sqrMagnitude;
            if(distSqr > 0f) {
                delay = Mathf.Sqrt(distSqr)/speed;
            }

            var tween = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);
            float curTime = 0f;

            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;
                if(curTime > delay)
                    curTime = delay;

                float t = tween(curTime, delay, 0f, 0f);

                ePos = mTarget.transform.position;

                transform.position = Vector2.Lerp(sPos, ePos, t);

                //check if enemy is still alive
                if(!mTarget.stats.isAlive)
                    break;
            }

            mRout = null;

            if(mTarget && mTarget.stats.isAlive) {
                //score
                MissionController.instance.ScoreAt(ePos, mTarget.stats.data.score);

                mTarget.state = (int)EntityState.DeadInstant;
                mTarget.stats.currentHP = 0f;

                state = (int)EntityState.Dead;
            }
            else
                state = (int)EntityState.Leave;
        }
        else { //failsafe
            mRout = null;
            state = (int)EntityState.Leave;
        }
    }
}
