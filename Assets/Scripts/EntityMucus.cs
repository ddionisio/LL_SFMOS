using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucus : M8.EntityBase {
    public StatEntityMucus stats;

    public Rigidbody2D body;
            
    private Transform mGatherTo;

    private Coroutine mRout;

    private Vector2 mSpawnPos;
    private Vector2 mSpawnImpulseDir;

    /// <summary>
    /// Set gather state, if towards != null, else revert state to idle
    /// </summary>
    public void SetGather(Transform towards) {
        mGatherTo = towards;

        if(towards) {
            state = (int)EntityState.Gather;
        }
        else {
            state = (int)EntityState.Normal;
        }
    }
    
    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Gather:
                mGatherTo = null;
                break;

            case EntityState.Gathered:
                gameObject.SetActive(true);
                break;
        }
        
        switch((EntityState)state) {
            case EntityState.Normal:
                body.simulated = true;
                body.velocity = Vector2.zero;

                mRout = StartCoroutine(DoWander());
                break;

            case EntityState.Gather:
                body.simulated = false;

                mRout = StartCoroutine(DoGather());
                break;

            case EntityState.Gathered:                
                gameObject.SetActive(false);
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        mGatherTo = null;
        
        body.simulated = true;
        body.velocity = Vector2.zero;

        mSpawnImpulseDir = Vector2.zero;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        if(parms != null) {
            parms.TryGetValue(Params.dir, out mSpawnImpulseDir);
        }

        //start ai, player control, etc
        if(mSpawnImpulseDir != Vector2.zero)
            body.AddForce(mSpawnImpulseDir*stats.spawnImpulse);

        mSpawnPos = transform.position;

        state = (int)EntityState.Normal;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoGather() {
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;

        bool active = true;        
        float delay = 0f;

        if(mGatherTo) {
            Vector2 startPos = transform.position;
            Vector2 endPos = mGatherTo.position;

            float dist = (endPos - startPos).magnitude;

            if(dist > 0f) {
                delay = dist/stats.gatherSpeed;
            }
            else
                active = false;
        }

        Vector2 curVel = Vector2.zero;
        float curTime = 0f;

        while(active) {
            curTime += Time.deltaTime;

            Vector2 curPos = transform.position;
            Vector2 endPos = mGatherTo.position;

            transform.position = Vector2.SmoothDamp(curPos, endPos, ref curVel, delay, float.MaxValue, Time.deltaTime);

            yield return null;

            active = curTime < delay && mGatherTo;
        }

        mRout = null;

        state = (int)EntityState.Gathered;
    }

    bool IsWanderPositionOutOfBounds(float dir) {
        float x = body.position.x;
        if(dir < 0f)
            return x < mSpawnPos.x - stats.wanderExtent;
        else if(dir > 0f)
            return x > mSpawnPos.x + stats.wanderExtent;
        return false;
    }

    void OnCollisionEnter(Collision collision) {
        var contact = collision.contacts[0];
        Vector2 normal = contact.normal;

        Vector2 dir = body.velocity.normalized;

        var refl = Vector2.Reflect(dir, normal);

        mDirX = Mathf.Sign(refl.x);
    }

    float mDirX;

    IEnumerator DoWander() {
        var wait = new WaitForFixedUpdate();
        
        float curTurnTime = 0f;
        float turnDelay = Random.Range(stats.wanderTurnDelayMin, stats.wanderTurnDelayMax);
        float curForce = 0f;
        mDirX = Random.Range(0, 2) == 0 ? 1.0f : -1.0f;
        
        while(true) {
            Vector2 curVel = body.velocity;
            if(curVel.sqrMagnitude < stats.wanderVelocityLimit*stats.wanderVelocityLimit) {                
                Vector2 forceDir = new Vector2(mDirX, 0f);
                body.AddForce(forceDir*curForce);
            }

            if(curTurnTime >= turnDelay || IsWanderPositionOutOfBounds(mDirX)) {
                curTurnTime = 0f;
                turnDelay = Random.Range(stats.wanderTurnDelayMin, stats.wanderTurnDelayMax);
                curForce = 0f;
                mDirX *= -1;
            }
            
            yield return wait;

            curTurnTime += Time.fixedDeltaTime;

            if(curForce < stats.wanderForceMax) {
                curForce += stats.wanderForceOverTime*Time.fixedDeltaTime;
                if(curForce > stats.wanderForceMax)
                    curForce = stats.wanderForceMax;
            }
        }
    }
}
