using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "mucusForm", menuName = "Stats/Mucus Form")]
public class StatEntityMucusForm : ScriptableObject {
    [System.Serializable]
    public struct GrowData {
        [Header("Radius")]
        public float radius;
        public float scale;

        [Header("Launch")]
        public float launchForceGrowthDecayDelay;

        [Header("Stats")]
        public float HP;
        public float damage;
        public float damageStamina;
        public float impactForce;
        public int splitCount;
        public int splitGrowth;
    }
    
    [Header("Takes")]
    public string takeNormal;
    public string takeGrow;
    public string takeLaunch;
    public string takeBind;
    public string takeDeath;
    public string takeSelect;

    [Header("Growth")]
    public GrowData[] growths;
    
    public float growthDelay = 0.5f;

    [Header("Launch")]
    public float launchForceMin;
    public float launchForceMax;
    public float launchForceImpulse;
    public float launchForceMaxDistance;
    public AnimationCurve launchForceCurve;
    public float launchDuration;
    
    public AnimationCurve launchForceDecayCurve;
    
    [Header("Attack")]
    public string[] attackTagFilter;
    public LayerMask attackSplitLayerMask; //to determine which objects to split towards
    public float attackSplitScoreMultiplayerInc = 1.5f;

    public float excessRadius;
    
    public int growthMaxCount { get { return growths.Length; } }

    public float GetRadius(int curGrowth) {
        return growths[curGrowth].radius;
    }

    public float GetScale(int curGrowth) {
        return growths[curGrowth].scale;
    }

    public float GetDamage(int curGrowth) {
        return growths[curGrowth].damage;
    }

    public float GetDamageStamina(int curGrowth) {
        return growths[curGrowth].damageStamina;
    }

    public float GetImpactForce(int curGrowth) {
        return growths[curGrowth].impactForce;
    }

    public float GetHP(int curGrowth) {
        return growths[curGrowth].HP;
    }

    public float GetLaunchForceDecayDelay(int curGrowth) {
        return growths[curGrowth].launchForceGrowthDecayDelay;
    }

    public int GetSplitCount(int curGrowth) {
        return growths[curGrowth].splitCount;
    }

    public int GetSplitGrowth(int curGrowth) {
        return growths[curGrowth].splitGrowth;
    }

    public bool IsAttackValid(string tag) {
        for(int i = 0; i < attackTagFilter.Length; i++) {
            if(tag == attackTagFilter[i])
                return true;
        }

        return false;
    }
}
