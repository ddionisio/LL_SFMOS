using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucus : M8.EntityBase {
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
