﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogen : M8.EntityBase {
    public StatEntityController stats { get { return mStats; } }

    private StatEntityController mStats;
    private Rigidbody2D mBody;
    
    protected override void StateChanged() {

        switch((EntityState)state) {
            case EntityState.Normal:
                if(mBody)
                    mBody.simulated = true;
                break;
            case EntityState.Dead:
                Debug.Log("dead: "+name);

                if(mBody)
                    mBody.simulated = false;
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mStats)
            mStats.Reset();

        if(mBody)
            mBody.simulated = true;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
        state = (int)EntityState.Normal;
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
