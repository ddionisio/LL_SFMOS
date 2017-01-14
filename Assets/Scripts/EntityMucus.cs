using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucus : M8.EntityBase {
    public Rigidbody2D body;

    public float gatherSpeed;

    private Transform mGatherTo;

    private Coroutine mRout;

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
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void SpawnStart() {
        //start ai, player control, etc
        state = (int)EntityState.Normal;
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
                delay = dist/gatherSpeed;
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
}
