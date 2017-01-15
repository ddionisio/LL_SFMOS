using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "mucusForm", menuName = "Stats/Mucus Form")]
public class StatEntityMucusForm : ScriptableObject {
    [Header("Radius")]
    public float radiusStart;
    public float radiusEnd;

    [Header("Growth")]
    public int growthMaxCount;

    [Header("Launch")]
    public float launchForceMin;
    public float launchForceMax;
    public float launchForceImpulse;
    public float launchForceMaxDistance;
    public AnimationCurve launchForceCurve;
    public float launchDuration;

    public float launchForceGrowthDecayMinDelay = 3f;
    public float launchForceGrowthDecayMaxDelay = 1f;
    public AnimationCurve launchForceDecayCurve;

    [Header("Attack")]
    public float damage;
    public float damageStamina;
}
