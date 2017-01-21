using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogen : M8.EntityBase {

    public FlockUnit flock;

    public M8.Auxiliary.AuxTrigger2D seekTrigger;

    public StatEntityController stats { get { return mStats; } }
    public Rigidbody2D body { get { return mBody; } }

    public Transform anchor {
        get {
            return mAnchor;
        }

        set {
            if(mAnchor != value) {
                mAnchor = value;
                if(mAnchor) {
                    mBody.isKinematic = true; //force to kinematic

                    if(mAnchorRout == null)
                        mAnchorRout = StartCoroutine(DoAnchorUpdate());
                }
                else {
                    if(mAnchorRout != null) {
                        StopCoroutine(mAnchorRout);
                        mAnchorRout = null;
                    }
                }
            }
        }
    }

    private StatEntityController mStats;
    private Rigidbody2D mBody;

    private StatEntityController mSeekTriggeredStatCtrl;
    private Collider2D mSeekTriggeredColl;

    private Coroutine mRout;

    private Transform mAnchor;
    private Coroutine mAnchorRout;
                
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
                break;
        }

        switch((EntityState)state) {
            case EntityState.Control:
                if(mBody) {
                    mBody.isKinematic = true;
                    mBody.simulated = true;
                }

                if(flock) {
                    flock.Stop();
                    flock.enabled = false;
                }
                break;

            case EntityState.Normal:
                if(mBody) {
                    mBody.isKinematic = false;
                    mBody.simulated = true;
                }

                if(flock)
                    flock.enabled = true;
                break;

            case EntityState.Wander:
                if(mBody) {
                    mBody.isKinematic = false;
                    mBody.simulated = true;
                }

                if(flock) {
                    flock.enabled = true;
                    flock.wanderEnabled = true;
                    flock.Stop(); 
                }
                break;

            case EntityState.Seek:
                if(mBody) {
                    mBody.isKinematic = false;
                    mBody.simulated = true;
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

                if(mBody)
                    mBody.simulated = false;

                if(flock)
                    flock.enabled = false;
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mBody) {
            mBody.velocity = Vector2.zero;
            mBody.angularVelocity = 0f;
            mBody.simulated = false;
        }

        anchor = null;
        
        if(flock) {
            flock.enabled = false;
            flock.ResetData();
        }

        if(seekTrigger)
            seekTrigger.gameObject.SetActive(false);

        mSeekTriggeredStatCtrl = null;
        mSeekTriggeredColl = null;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        int toState = (int)EntityState.Normal;
        Transform toAnchor = null;

        if(parms != null) {
            if(parms.ContainsKey(Params.state))
                toState = parms.GetValue<int>(Params.state);

            if(parms.ContainsKey(Params.anchor))
                toAnchor = parms.GetValue<Transform>(Params.anchor);
        }

        //start ai, player control, etc
        state = toState;
        anchor = toAnchor;
    }

    protected override void OnDestroy() {
        //dealloc here
        if(mStats) {
            mStats.HPChangedCallback -= OnStatHPChanged;
            mStats.staminaChangedCallback -= OnStatStaminaChanged;
            mStats.signalCallback -= OnStatSignal;
        }

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mStats = GetComponent<StatEntityController>();
        if(mStats) {
            mStats.HPChangedCallback += OnStatHPChanged;
            mStats.staminaChangedCallback += OnStatStaminaChanged;
            mStats.signalCallback += OnStatSignal;
        }

        mBody = GetComponent<Rigidbody2D>();

        if(flock)
            flock.enabled = false;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoSeek() {
        yield return new WaitForSeconds(mStats.data.seekDelay);

        //request target from mission control
        var seekTarget = MissionController.instance.RequestTarget(transform);
                
        if(!(flock && seekTarget)) {
            state = (int)EntityState.Wander;
            yield break;
        }

        //set flock move target
        flock.moveTarget = seekTarget;
        flock.moveScale = mStats.data.seekFlockMoveToScale;

        mSeekTriggeredStatCtrl = null;
        mSeekTriggeredColl = null;

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

        var layerIndex = mSeekTriggeredColl.gameObject.layer;

        Vector2 dir = (Vector2)mSeekTriggeredColl.transform.position - pos;
        float dist = dir.magnitude;
        if(dist > 0f)
            dir /= dist;

        var coll = Physics2D.Raycast(pos, dir, dist, 1<<layerIndex);
        if(coll != mSeekTriggeredColl) {
            state = (int)EntityState.Wander;
            yield break;
        }

        if(mBody)
            mBody.isKinematic = true;

        if(flock)
            flock.enabled = false;

        Vector2 dest = coll.point;
        dist = coll.distance;

        var delay = dist/mStats.data.seekCloseInSpeed;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);

        var curTime = 0f;
        while(curTime < delay) {
            float t = easeFunc(curTime, delay, 0f, 0f);

            mBody.MovePosition(Vector2.Lerp(pos, dest, t));

            yield return null;

            curTime += Time.deltaTime;
        }

        transform.position = dest;
        //
                
        //eat away until it no longer has HP
        if(mSeekTriggeredStatCtrl) {
            var dmg = mStats.data.damage;
            var atkSpd = mStats.data.attackSpeed;

            //TODO: attack animation scale by attack speed

            curTime = 0f;

            while(mSeekTriggeredStatCtrl.isAlive) {
                yield return null;

                var staScale = Mathf.Max(0.1f, mStats.currentStamina/mStats.data.stamina);

                atkSpd = mStats.data.attackSpeed/staScale;

                curTime += Time.deltaTime;
                if(curTime >= atkSpd) {
                    mSeekTriggeredStatCtrl.currentHP -= dmg;

                    curTime = 0f;

                    //TODO: attack animation scale by attack speed
                }
            }
        }

        mRout = null;

        //wander? incubate? split?
        mStats.currentHP = 0f;
    }

    IEnumerator DoAnchorUpdate() {
        var wait = new WaitForFixedUpdate();

        while(mAnchor) {
            mBody.MovePosition(mAnchor.position);

            yield return wait;
        }

        mAnchorRout = null;
    }
    
    void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
    }

    void OnStatStaminaChanged(StatEntityController aStats, float prev) {
        //slow down
        //slow action rate
        float scale = aStats.currentStamina/aStats.data.stamina;

        if(flock)
            flock.moveScale = scale;
    }

    void OnStatSignal(StatEntityController aStats, GameObject sender, int signal, object data) {
        switch((StatEntitySignals)signal) {
            case StatEntitySignals.Bind:
                //bind animation
                break;

            case StatEntitySignals.Unbind:
                //stop bind animation
                break;
        }
    }
}
