using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "mucus", menuName = "Stats/Mucus")]
public class StatEntityMucus : ScriptableObject {
    public float gatherSpeed;

    public float spawnImpulse;

    [Header("Wander")]
    public float wanderExtent;
    public float wanderTurnDelayMin;
    public float wanderTurnDelayMax;
    public float wanderForceOverTime;
    public float wanderForceMax;
    public float wanderVelocityLimit;
}
