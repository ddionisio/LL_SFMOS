using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "mucusForm", menuName = "Stats/Mucus Form")]
public class StatEntityMucusForm : ScriptableObject {
    [Header("Radius")]
    public float radiusStart;
    public float radiusEnd;

    public float radiusToRootScaleStart;
    public float radiusToRootScaleEnd;

    [Header("Takes")]
    public string takeNormal;
    public string takeGrow;
    public string takeLaunch;
    public string takeBind;
    public string takeDeath;

    [Header("Growth")]
    public int growthMaxCount;
    public float growthDelay = 0.5f;

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
    public string[] attackTagFilter;
    public LayerMask attackSplitLayerMask; //to determine which objects to split towards

    public float damageMin;
    public float damageMax;

    public float damageStaminaMin;
    public float damageStaminaMax;

    public float impactForceMin;
    public float impactForceMax;

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

    public float GetImpactForce(int curGrowth) {
        float t = (float)curGrowth/growthMaxCount;

        return Mathf.Lerp(impactForceMin, impactForceMax, t);
    }

    public float GetHP(int curGrowth) {
        float t = (float)curGrowth/growthMaxCount;

        return Mathf.Lerp(HPMin, HPMax, t);
    }

    public bool IsAttackValid(string tag) {
        for(int i = 0; i < attackTagFilter.Length; i++) {
            if(tag == attackTagFilter[i])
                return true;
        }

        return false;
    }
}
