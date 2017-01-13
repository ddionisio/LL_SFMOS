using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucus : M8.EntityBase {

    private Transform mGatherTo;

    /// <summary>
    /// Set gather state, if towards != null, else revert state to idle
    /// </summary>
    public void SetGather(Transform towards) {
        if(towards) {
            state = (int)EntityState.Gather;
        }
        else {
            state = (int)EntityState.Normal;
        }
    }

    /// <summary>
    /// Called by MucusForm once it has processed
    /// </summary>
    public void Gathered() {
        state = (int)EntityState.Gathered;
    }

    protected override void StateChanged() {
        switch((EntityState)state) {
            case EntityState.Normal:
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
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
}
