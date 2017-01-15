using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogen : M8.EntityBase {
    public StatEntityController stats { get { return mStats; } }

    private StatEntityController mStats;
    
    protected override void StateChanged() {

        switch((EntityState)state) {
            case EntityState.Normal:
                break;
            case EntityState.Dead:
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mStats)
            mStats.Reset();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
    }

    protected override void OnDestroy() {
        //dealloc here
        if(mStats) {
            mStats.HPChangedCallback -= OnStatHPChanged;
            mStats.StaminaChangedCallback -= OnStatStaminaChanged;
        }

        base.OnDestroy();
    }

    protected override void SpawnStart() {
        //start ai, player control, etc
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mStats = GetComponent<StatEntityController>();
        if(mStats) {
            mStats.HPChangedCallback += OnStatHPChanged;
            mStats.StaminaChangedCallback += OnStatStaminaChanged;            
        }
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
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
