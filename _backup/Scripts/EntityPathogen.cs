using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogen : EntityCommon {

    public M8.Auxiliary.AuxTrigger2D seekTrigger;
    
    [Header("Hurt")]
    public GameObject hurtActiveGO;

    private EntitySpawner mSpawner; //for carriers

    private StatEntityController mSeekTriggeredStatCtrl;
    private Collider2D mSeekTriggeredColl;

    private float mCurHurtDuration;

    private Coroutine mRout;
    private Coroutine mHurtRout;

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Control:
                anchor = null; //force to detach anchor                
                break;

            case EntityState.Seek:
                if(seekTrigger)
                    seekTrigger.gameObject.SetActive(false);

                if(flock) {
                    flock.moveScale = 1.0f;
                }

                ClearSeek();
                break;
        }

        switch((EntityState)state) {
            case EntityState.Control:
                if(body) {
                    body.isKinematic = true;
                    body.simulated = true;
                }

                if(flock) {
                    flock.Stop();
                    flock.enabled = false;
                }

                if(mSpawner)
                    mSpawner.isSpawning = false;
                break;

            case EntityState.Normal:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = true;
                }

                if(flock)
                    flock.enabled = true;

                if(mSpawner)
                    mSpawner.isSpawning = true;

                if(animator && !string.IsNullOrEmpty(stats.data.takeNormal))
                    animator.Play(stats.data.takeNormal);
                break;

            case EntityState.Wander:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = true;
                }

                if(flock) {
                    flock.enabled = true;
                    flock.wanderEnabled = true;
                    flock.Stop();
                }

                if(mSpawner)
                    mSpawner.isSpawning = true;

                if(animator && !string.IsNullOrEmpty(stats.data.takeNormal))
                    animator.Play(stats.data.takeNormal);
                break;

            case EntityState.Seek:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = true;
                }

                if(flock) {
                    flock.enabled = true;
                    flock.wanderEnabled = true;
                    flock.Stop();
                }

                mRout = StartCoroutine(DoSeek());
                break;

            case EntityState.Dead:
                Debug.Log("dead: "+name);

                if(body) {
                    body.simulated = false;
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }

                if(flock)
                    flock.enabled = false;

                if(mSpawner)
                    mSpawner.isSpawning = false;

                if(animator && !string.IsNullOrEmpty(stats.data.takeDeath))
                    animator.Play(stats.data.takeDeath);
                break;

            case EntityState.DeadInstant:
                Debug.Log("dead instant: "+name);

                if(body) {
                    body.simulated = false;
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }

                if(flock)
                    flock.enabled = false;

                if(mSpawner)
                    mSpawner.isSpawning = false;

                if(animator && !string.IsNullOrEmpty(stats.data.takeDeathInstant))
                    animator.Play(stats.data.takeDeathInstant);
                else
                    Release();
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mRout = null;
        mHurtRout = null;

        if(seekTrigger)
            seekTrigger.gameObject.SetActive(false);

        if(hurtActiveGO)
            hurtActiveGO.SetActive(false);

        ClearSeek();

        base.OnDespawned();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
        if(animator) {
            if(!string.IsNullOrEmpty(stats.data.takeSpawn))
                animator.Play(stats.data.takeSpawn);
        }
    }

    protected override void OnDestroy() {
        //dealloc here        
        if(seekTrigger) {
            seekTrigger.enterCallback -= OnSeekTriggerEnter;
        }

        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        if(seekTrigger) {
            seekTrigger.enterCallback += OnSeekTriggerEnter;
        }

        if(animator)
            animator.takeCompleteCallback += OnAnimatorComplete;

        if(hurtActiveGO)
            hurtActiveGO.SetActive(false);

        mSpawner = GetComponent<EntitySpawner>();
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void ApplySeekFromCollider(Collider2D coll) {
        if(coll && coll != mSeekTriggeredColl) {
            if(stats.data.IsSeekValid(coll.tag)) {
                //unbind prev.
                if(mSeekTriggeredStatCtrl)
                    mSeekTriggeredStatCtrl.SendSignal(gameObject, (int)StatEntitySignals.Unbind, null);

                mSeekTriggeredColl = coll;
                mSeekTriggeredStatCtrl = coll.GetComponent<StatEntityController>();
            }
        }
    }

    void ClearSeek() {
        if(mSeekTriggeredStatCtrl) {
            if(mSeekTriggeredStatCtrl.isAlive)
                mSeekTriggeredStatCtrl.SendSignal(gameObject, (int)StatEntitySignals.Unbind, null);

            mSeekTriggeredStatCtrl = null;
        }

        mSeekTriggeredColl = null;
    }

    IEnumerator DoSeek() {
        yield return new WaitForSeconds(stats.data.seekDelay);

        //request target from mission control
        var seekTarget = MissionController.instance.RequestTarget();

        if(!(flock && seekTarget)) {
            Debug.LogWarning("Nothing to seek");
            state = (int)EntityState.Wander;
            yield break;
        }

        if(animator && !string.IsNullOrEmpty(stats.data.takeSeek))
            animator.Play(stats.data.takeSeek);

        //set flock move target
        flock.moveTarget = seekTarget;
        flock.moveScale = stats.data.seekFlockMoveToScale;

        ClearSeek();

        //activate seek trigger (to latch on anything edible)
        if(seekTrigger)
            seekTrigger.gameObject.SetActive(true);

        //wait for triggered
        while(!mSeekTriggeredColl)
            yield return null;

        if(seekTrigger)
            seekTrigger.gameObject.SetActive(false);

        //
        //close in
        Vector2 pos = transform.position;
        Vector2 collPos = mSeekTriggeredColl.bounds.center;

        Vector2 dir = collPos - pos;
        float dist = dir.magnitude;
        if(dist > 0f)
            dir /= dist;

        var collBoundExt = mSeekTriggeredColl.bounds.extents;
        float collExtMin = Mathf.Min(collBoundExt.x, collBoundExt.y);

        dist -= collExtMin;

        Vector2 dest = pos + dir*dist;
        //

        if(body)
            body.isKinematic = true;

        if(flock)
            flock.enabled = false;

        var delay = Mathf.Abs(dist)/stats.data.seekCloseInSpeed;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);

        var curTime = 0f;
        while(curTime < delay) {
            float t = easeFunc(curTime, delay, 0f, 0f);

            body.MovePosition(Vector2.Lerp(pos, dest, t));

            yield return null;

            curTime += Time.deltaTime;
        }

        body.MovePosition(dest);
        body.velocity = Vector2.zero;
        //

        //eat away until it no longer has HP
        if(mSeekTriggeredStatCtrl) {
            //bind signal
            mSeekTriggeredStatCtrl.SendSignal(gameObject, (int)StatEntitySignals.Bind, null);

            var dmg = stats.data.damage;
            var atkSpd = stats.data.attackSpeed;

            //TODO: attack animation scale by attack speed
            if(animator && !string.IsNullOrEmpty(stats.data.takeAttack))
                animator.Play(stats.data.takeAttack);

            curTime = 0f;

            while(mSeekTriggeredStatCtrl.isAlive) {
                yield return null;

                var staScale = Mathf.Max(0.1f, stats.currentStamina/stats.data.stamina);

                atkSpd = stats.data.attackSpeed/staScale;

                curTime += Time.deltaTime;
                if(curTime >= atkSpd) {
                    mSeekTriggeredStatCtrl.currentHP -= dmg;

                    curTime = 0f;

                    //TODO: attack animation scale by attack speed
                }
            }
        }

        //seek again
        //RestartState();
        //start repro?
        state = (int)EntityState.Normal;
    }

    IEnumerator DoHurt() {
        if(hurtActiveGO) hurtActiveGO.SetActive(true);

        while(mCurHurtDuration > 0f) {
            yield return null;
            mCurHurtDuration -= Time.deltaTime;
        }

        if(hurtActiveGO) hurtActiveGO.SetActive(false);

        mHurtRout = null;
    }

    protected override void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP > 0f) {
            if(aStats.currentHP < prev) {
                mCurHurtDuration = stats.data.hurtDuration;
                if(mHurtRout == null)
                    mHurtRout = StartCoroutine(DoHurt());
            }
            //healed?

            return;
        }

        base.OnStatHPChanged(aStats, prev);
    }

    protected override void OnStatStaminaChanged(StatEntityController aStats, float prev) {
        //slow down
        //slow action rate
        float scale = aStats.currentStamina/aStats.data.stamina;

        if(flock)
            flock.moveScale = scale;
    }

    protected override void OnStatSignal(StatEntityController aStats, GameObject sender, int signal, object data) {
        switch((StatEntitySignals)signal) {
            case StatEntitySignals.Bind:
                //bind animation
                break;

            case StatEntitySignals.Unbind:
                //stop bind animation
                break;
        }
    }

    void OnSeekTriggerEnter(Collider2D coll) {
        //only apply if there's no current seek
        if(!mSeekTriggeredColl)
            ApplySeekFromCollider(coll);
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == stats.data.takeSpawn) {
            animator.Play(stats.data.takeNormal);
        }
        else if(take.name == stats.data.takeDeath) {
            if(stats.data.releaseOnDeath)
                Release();
        }
        else if(take.name == stats.data.takeDeathInstant) {
            Release();
        }
    }
}