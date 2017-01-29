using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLymphocyte : EntityCommon {

    public M8.Auxiliary.AuxTrigger2D bindTrigger; //check for antigen match
        
    [Header("Launch Info")]
    public GameObject launchActiveGO; //activate upon launch

    public float launchEndSpeed; //what speed to go back to normal

    private Coroutine mRout;

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Control:
                anchor = null; //force to detach anchor                
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

                break;

            case EntityState.Normal:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = true;
                }

                if(flock)
                    flock.enabled = true;

                break;

            case EntityState.Launch:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = false;
                }

                if(flock)
                    flock.enabled = false;

                if(launchActiveGO)
                    launchActiveGO.SetActive(true);

                mRout = StartCoroutine(DoLaunch());
                break;

            case EntityState.Dead:
                Debug.Log("dead: "+name);

                if(body)
                    body.simulated = false;

                if(flock)
                    flock.enabled = false;

                if(launchActiveGO)
                    launchActiveGO.SetActive(false);

                //animate

                Release();
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(launchActiveGO)
            launchActiveGO.SetActive(false);

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here        
        if(bindTrigger) {
            bindTrigger.enterCallback -= OnBindTriggerEnter;
            bindTrigger.exitCallback -= OnBindTriggerExit;
        }

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        if(bindTrigger) {
            bindTrigger.enterCallback += OnBindTriggerEnter;
            bindTrigger.exitCallback += OnBindTriggerExit;
        }

        if(launchActiveGO)
            launchActiveGO.SetActive(false);
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoLaunch() {
        var wait = new WaitForFixedUpdate();

        var minSpdSqr = launchEndSpeed*launchEndSpeed;
        float curSpdSqr;

        //check speed until it minimizes
        do {
            yield return wait;
            curSpdSqr = body.velocity.sqrMagnitude;
        } while(curSpdSqr > minSpdSqr && state == (int)EntityState.Launch);

        mRout = null;

        state = (int)EntityState.Normal;
    }

    void OnBindTriggerEnter(Collider2D coll) {
    }

    void OnBindTriggerExit(Collider2D coll) {
    }
}
