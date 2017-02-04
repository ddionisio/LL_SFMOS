using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCellStationary : EntityCommon {
    [Header("Idle Info")]
    public string[] takeIdles;
    public string takeIdleWeak;
    public float idleChangeDelayMin = 0.5f;
    public float idleChangeDelayMax = 1.5f;
    public float idleSadnessHPScale = 0.4f;
    
    [Header("Main Takes")]
    public string takeDead;
    public string takeDanger;

    [Header("Body Takes")]
    public string takeBodyNormal;
    public string takeBodyWeak;
    public string takeBodyDanger;
    public string takeBodyDead;

    [Header("Body")]
    public M8.Animator.AnimatorData bodyAnimator;

    public SpriteRenderer bodySpriteRender;

    public Color bodyColorNormal = Color.white;
    public Color bodyColorDead = Color.gray;

    private bool mIsWeak;
    private Coroutine mNormalRout;

    protected override void StateChanged() {
        switch((EntityState)prevState) {
            case EntityState.Normal:
                if(mNormalRout != null) {
                    StopCoroutine(mNormalRout);
                    mNormalRout = null;
                }

                mIsWeak = false;
                break;
        }
                
        switch((EntityState)state) {
            case EntityState.Normal:
                bodySpriteRender.color = bodyColorNormal;

                ApplyNormalState();
                break;

            case EntityState.Bind:
                bodySpriteRender.color = bodyColorNormal;

                animator.Play(takeDanger);
                bodyAnimator.Play(takeBodyDanger);
                break;

            case EntityState.Dead:
                Debug.Log("dead: "+name);

                bodySpriteRender.color = bodyColorDead;

                //animate
                animator.Play(takeDead);
                bodyAnimator.Play(takeBodyDead);

                if(body)
                    body.simulated = false;

                if(coll)
                    coll.enabled = false;
                break;
        }
    }

    protected override void OnDespawned() {
        mNormalRout = null;

        base.OnDespawned();
    }
        
    protected override void OnStatSignal(StatEntityController aStats, GameObject sender, int signal, object data) {
        if(!stats.isAlive)
            return;

        switch((StatEntitySignals)signal) {
            case StatEntitySignals.Bind:
                state = (int)EntityState.Bind;
                break;

            case StatEntitySignals.Unbind:
                //determine previous state
                state = (int)EntityState.Normal;
                break;
        }
    }

    protected override void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
        else {
            ApplyNormalState();
        }
    }

    void ApplyNormalState() {
        //check if sad
        float hpScale = stats.currentHP/stats.data.HP;

        if(hpScale > idleSadnessHPScale) {
            if(mNormalRout == null)
                mNormalRout = StartCoroutine(DoNormal());

            mIsWeak = false;
        }
        else {
            if(!mIsWeak) {
                animator.Play(takeIdleWeak);
                bodyAnimator.Play(takeBodyWeak);

                mIsWeak = true;
            }
        }
    }

    IEnumerator DoNormal() {
        bodyAnimator.Play(takeBodyNormal);

        while(true) {
            yield return new WaitForSeconds(Random.Range(idleChangeDelayMin, idleChangeDelayMax));

            animator.Play(takeIdles[Random.Range(0, takeIdles.Length)]);
            while(animator.isPlaying)
                yield return null;
        }
    }
}
