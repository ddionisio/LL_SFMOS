using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogen : M8.EntityBase {

    public FlockUnit flock;

    public StatEntityController stats { get { return mStats; } }
    public Rigidbody2D body { get { return mBody; } }

    private StatEntityController mStats;
    private Rigidbody2D mBody;

    private Coroutine mRout;
        
    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
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

        if(mStats)
            mStats.Reset();

        if(mBody)
            mBody.simulated = false;

        if(flock)
            flock.enabled = false;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        int toState = (int)EntityState.Normal;

        if(parms != null) {
            if(parms.ContainsKey(Params.state))
                toState = parms.GetValue<int>(Params.state);
        }

        //start ai, player control, etc
        state = toState;
    }

    protected override void OnDestroy() {
        //dealloc here
        if(mStats) {
            mStats.HPChangedCallback -= OnStatHPChanged;
            mStats.StaminaChangedCallback -= OnStatStaminaChanged;
        }

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mStats = GetComponent<StatEntityController>();
        if(mStats) {
            mStats.HPChangedCallback += OnStatHPChanged;
            mStats.StaminaChangedCallback += OnStatStaminaChanged;            
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
        yield return new WaitForSeconds(mStats.seekDelay);

        //request target from mission control

        //set flock move target

        //activate seek trigger (to latch on anything edible)
    }
    
    void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
    }

    void OnStatStaminaChanged(StatEntityController aStats, float prev) {
        //slow down
        //slow action rate
    }
}
