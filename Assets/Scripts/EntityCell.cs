using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCell : M8.EntityBase {
    public StatEntityController stats { get { return mStats; } }

    private StatEntityController mStats;

    protected override void StateChanged() {
        
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
        mStats = GetComponent<StatEntityController>();
        if(mStats) {
            mStats.HPChangedCallback += OnStatHPChanged;
            mStats.staminaChangedCallback += OnStatStaminaChanged;
        }
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
    }

    void OnStatStaminaChanged(StatEntityController aStats, float prev) {
        
    }
}
