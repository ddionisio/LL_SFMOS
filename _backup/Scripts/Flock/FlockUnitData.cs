﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "flockData", menuName = "Stats/FlockUnit")]
public class FlockUnitData : ScriptableObject {
    public LayerMask wallMask;

    public string[] avoidTags;

    public int group; //for cohesion and alignment

    public float maxForce = 120.0f; //N
    public float maxSpeed = 20f;

    public float pathRadius;
    public float wallRadius;

    public float separateDistance = 2.0f;
    public float avoidDistance = 0.0f;

    public float separateFactor = 1.5f;
    public float alignFactor = 1.0f;
    public float cohesionFactor = 1.0f;
    public float moveToFactor = 1.0f;
    public float catchUpFactor = 2.0f; //when we have a move target and there are no other flocks around
    public float pathFactor = 1.0f;
    public float wallFactor = 1.0f;
    public float avoidFactor = 1.0f;

    public float updateDelay = 1.0f;

    public float seekDelay = 1.0f;

    public float catchUpMinDistance; //min distance to use catchup factor

    public float wanderDelay;
    public bool wanderRestrict; //don't wander off the origin point (when changed to wander state)
    public float wanderRestrictRadius;

    public bool CheckAvoid(string tag) {
        for(int i = 0; i < avoidTags.Length; i++) {
            if(avoidTags[i] == tag)
                return true;
        }

        return false;
    }
}
