using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPhagocyte : M8.EntityBase {
    public const int targetCapacity = 16;

    public M8.Animator.AnimatorData animator;

    [Header("Attack Info")]
    public string spawnArmPoolGroup;
    public string spawnArmEntityRef;
    public float spawnArmRadius;
    
    [Header("Takes")]
    public string takeNormal;
    public string takeSeek;
    public string takeEat;
    public string takeGameover;

    public bool isEating {
        get {
            return mArms.Count > 0;
        }
    }
    
    private Coroutine mRout;
    
    private M8.CacheList<EntityPhagocyteTentacle> mArms;

    public void Eat(M8.EntityBase victim, int score) {
        Vector2 pos = transform.position;
        Vector2 dest = victim.transform.position;

        Vector2 dir = dest - pos;
        float dist = dir.magnitude;
        if(dist > 0f) dir /= dist;

        var spawnPos = pos + dir*spawnArmRadius;
        
        var arm = M8.PoolController.SpawnFromGroup<EntityPhagocyteTentacle>(spawnArmPoolGroup, spawnArmEntityRef, spawnArmEntityRef, null, spawnPos, null);

        arm.setStateCallback += OnArmChangeState;
        arm.releaseCallback += OnArmRelease;

        arm.Gather(victim, dir, dist, score);

        mArms.Add(arm);

        if(animator && animator.currentPlayingTakeName == takeNormal)
            animator.Play(takeSeek);
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)state) {
            case EntityState.Normal:
                if(animator && !string.IsNullOrEmpty(takeNormal))
                    animator.Play(takeNormal);
                break;

            case EntityState.Alert: //game over
                if(animator && !string.IsNullOrEmpty(takeGameover))
                    animator.Play(takeGameover);
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(MissionController.instance)
            MissionController.instance.signalCallback += OnMissionSignal;

        mRout = null;

        //release any active arms
        for(int i = 0; i < mArms.Count; i++) {
            if(mArms[i] && !mArms[i].isReleased) {
                mArms[i].setStateCallback -= OnArmChangeState;
                mArms[i].releaseCallback -= OnArmRelease;

                mArms[i].Release();
            }
        }

        mArms.Clear();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        MissionController.instance.signalCallback += OnMissionSignal;

        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    protected override void OnDestroy() {
        //dealloc here
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();
        
        //initialize data/variables
        mArms = new M8.CacheList<EntityPhagocyteTentacle>(targetCapacity);

        if(animator)
            animator.takeCompleteCallback += OnAnimatorComplete;
    }
    
    void OnArmChangeState(M8.EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Gathered:
                ReleaseArm((EntityPhagocyteTentacle)ent);
                break;
        }
    }

    void OnArmRelease(M8.EntityBase ent) {
        ReleaseArm((EntityPhagocyteTentacle)ent);
    }

    void ReleaseArm(EntityPhagocyteTentacle arm) {
        mArms.Remove(arm);

        arm.setStateCallback -= OnArmChangeState;
        arm.releaseCallback -= OnArmRelease;

        //score
        MissionController.instance.ScoreAt(transform.position, arm.score);

        arm.Release();

        if(animator && animator.currentPlayingTakeName != takeEat)
            animator.Play(takeEat);
        
        //antigen presentation?
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takeEat) {
            if(mArms.Count > 0)
                anim.Play(takeSeek);
            else
                anim.Play(takeNormal);
        }
    }

    void OnMissionSignal(MissionController.SignalType signal, object parm) {
        if(signal == MissionController.SignalType.Defeat) {
            state = (int)EntityState.Alert;
        }
    }

    void OnDrawGizmos() {
        if(spawnArmRadius > 0.0f) {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, spawnArmRadius);
        }
    }
}
