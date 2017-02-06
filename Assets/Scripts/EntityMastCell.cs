using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMastCell : M8.EntityBase {

    public EntityMastCellInput input;

    public M8.Animator.AnimatorData animator;

    [Header("Takes")]
    public string takeNormal;
    public string takeAlert;

    protected override void StateChanged() {
        
        switch((EntityState)state) {
            case EntityState.Normal:
                break;

            case EntityState.Alert:
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
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

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
