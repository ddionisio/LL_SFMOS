using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCommon : M8.EntityBase {
    public M8.Animator.AnimatorData animator;
    public FlockUnit flock;

    public StatEntityController stats { get { return mStats; } }
    public CellBindController cellBind { get { return mCellBind; } }
    public Rigidbody2D body { get { return mBody; } }
    public Collider2D coll { get { return mColl; } }

    public Transform anchor {
        get {
            return mAnchor;
        }

        set {
            if(mAnchor != value) {
                //var prevAnchor = mAnchor;
                mAnchor = value;
                if(mAnchor) {
                    //Debug.Log(name+" anchor to "+mAnchor.name);
                    mBody.isKinematic = true; //force to kinematic

                    if(mAnchorRout == null)
                        mAnchorRout = StartCoroutine(DoAnchorUpdate());
                }
                else {
                    //Debug.Log(name+" anchor unset from: "+prevAnchor.name);
                    if(mAnchorRout != null) {
                        StopCoroutine(mAnchorRout);
                        mAnchorRout = null;
                    }
                }
            }
        }
    }

    private StatEntityController mStats;
    private CellBindController mCellBind;
    private Rigidbody2D mBody;
    private Collider2D mColl;

    private Transform mAnchor;
    private Coroutine mAnchorRout;

    public virtual void Follow(Transform follow) {
        if(flock)
            flock.moveTarget = follow;
    }

    public virtual void Leave(Transform leaveDest) {
        if(flock)
            flock.moveTarget = leaveDest;

        if(mColl)
            mColl.enabled = false;

        state = (int)EntityState.Leave;
    }

    public virtual void Launch(Vector2 dir, float force) {
        state = (int)EntityState.Launch;

        if(flock) {
            flock.Stop();
            flock.enabled = false;
        }

        if(mBody) {
            mBody.isKinematic = false;
            mBody.simulated = true;
            mBody.AddForce(dir*force, ForceMode2D.Impulse);
        }
    }

    protected override void OnDespawned() {
        if(mStats.data.registerAsEnemy)
            MissionController.instance.Signal(MissionController.SignalType.EnemyUnregister, this);

        if(flock) {
            flock.enabled = false;
            flock.ResetData();
        }

        if(mBody) {
            mBody.velocity = Vector2.zero;
            mBody.angularVelocity = 0f;
            mBody.simulated = false;
        }

        if(mColl)
            mColl.enabled = true;

        if(mCellBind)
            mCellBind.Deinit();

        anchor = null;
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
        anchor = toAnchor;
        state = toState;

        if(mStats.data.registerAsEnemy)
            MissionController.instance.Signal(MissionController.SignalType.EnemyRegister, this);
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

        mCellBind = GetComponent<CellBindController>();

        mBody = GetComponent<Rigidbody2D>();

        if(flock) {
            flock.enabled = false;
            mColl = flock.coll;
        }
        else
            mColl = GetComponent<Collider2D>();
    }

    protected virtual void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
    }

    protected virtual void OnStatStaminaChanged(StatEntityController aStats, float prev) {
        
    }

    protected virtual void OnStatSignal(StatEntityController aStats, GameObject sender, int signal, object data) {
        switch((StatEntitySignals)signal) {
            case StatEntitySignals.Bind:
                //bind animation
                break;

            case StatEntitySignals.Unbind:
                //stop bind animation
                break;
        }
    }

    IEnumerator DoAnchorUpdate() {
        var wait = new WaitForFixedUpdate();

        while(mAnchor) {
            mBody.MovePosition(mAnchor.position);

            yield return wait;
        }

        mAnchorRout = null;
    }
}
