using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCommon : M8.EntityBase {
    public M8.Animator.AnimatorData animator;
    public FlockUnit flock;

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

    private Transform mAnchor;
    private Coroutine mAnchorRout;

    public virtual void Follow(Transform follow) {
        if(flock)
            flock.moveTarget = follow;
    }

    protected override void OnDespawned() {
        if(flock) {
            flock.enabled = false;
            flock.ResetData();
        }

        if(mBody) {
            mBody.velocity = Vector2.zero;
            mBody.angularVelocity = 0f;
            mBody.simulated = false;
        }

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
