﻿using System.Collections;
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

    [Header("Stats")]
    public float HPMin;
    public float HPMax;

    [Header("Attack")]
    public float damageMin;
    public float damageMax;

    public float damageStaminaMin;
    public float damageStaminaMax;
    
    public float excessRadius;
    public int excessMaxSplitCount;

    public float GetDamage(int curGrowth) {
        float t = (float)curGrowth/growthMaxCount;

        return Mathf.Lerp(damageMin, damageMax, t);
    }

    public float GetDamageStamina(int curGrowth) {
        float t = (float)curGrowth/growthMaxCount;

        return Mathf.Lerp(damageStaminaMin, damageStaminaMax, t);
    }

    public float GetHP(int curGrowth) {
        float t = (float)curGrowth/growthMaxCount;

        return Mathf.Lerp(HPMin, HPMax, t);
    }
}
