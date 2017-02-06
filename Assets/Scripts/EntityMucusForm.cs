﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucusForm : M8.EntityBase {
    public StatEntityMucusForm stats;

    public Rigidbody2D body;
    public CircleCollider2D circleCollider;
    public Transform root;

    public M8.Animator.AnimatorData animator;
 
    public float radius {
        get {
            return circleCollider.radius;
        }
    }

    public int currentGrowthCount { get { return mCurGrowthCount; } }
    
    private int mCurGrowthCount = 0;

    private float mGrowScale;
    
    private float mScoreMultiplier = 1f; //multiplier to apply to damaged target

    private Coroutine mRout;
    private Coroutine mGrowRout;

    private Vector2 mLaunchDir;
    private float mLaunchForce;
    private Bounds mLaunchBounds;

    //bind
    private StatEntityController mBoundEntityStat;
    private Transform mDefaultParent;

    public void Grow() {
        SetGrow(mCurGrowthCount + 1);
    }

    public void SetGrow(int val) {
        var prevGrowthCount = mCurGrowthCount;
        mCurGrowthCount = Mathf.Clamp(val, 0, stats.growthMaxCount);

        if(mCurGrowthCount != prevGrowthCount) {
            ApplyGrowthToCollider();
            
            //var growRadius = Mathf.Lerp(stats.radiusStart, stats.radiusEnd, (float)mCurGrowthCount/stats.growthMaxCount);
            mGrowScale = Mathf.Lerp(stats.radiusToRootScaleStart, stats.radiusToRootScaleEnd, (float)mCurGrowthCount/stats.growthMaxCount);

            if(mGrowRout == null)
                mGrowRout = StartCoroutine(DoGrow(prevGrowthCount));
        }
    }
        
    public void Launch(Vector2 dir, float length, Bounds bounds) {        
        if(dir.x + dir.y == 0f) {
            Cancel();
            return;
        }

        if(mCurGrowthCount <= 0) {
            Release();
            return;
        }

        mLaunchDir = dir;
        mLaunchForce = Mathf.Lerp(stats.launchForceMin, stats.launchForceMax, GetLaunchForceScale(length));
        mLaunchBounds = bounds;

        state = (int)EntityState.Launch;
    }

    public void Cancel() {
        state = (int)EntityState.Dead;
    }

    public float GetLaunchForceScale(float length) {
        return Mathf.Clamp01(stats.launchForceCurve.Evaluate(Mathf.Clamp01((length - radius)/stats.launchForceMaxDistance)));
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Bind:
                SetBound(null);
                break;
        }

        switch((EntityState)state) {
            case EntityState.Normal:
                body.simulated = false;

                if(mGrowRout == null)
                    animator.Play(stats.takeNormal);
                break;

            case EntityState.Launch:
                body.simulated = true;
                mRout = StartCoroutine(DoLaunch());
                break;

            case EntityState.Dead:
                if(mGrowRout != null) {
                    StopCoroutine(mGrowRout);
                    mGrowRout = null;

                    ApplyGrowthToScale();
                }

                body.simulated = false;

                animator.Play(stats.takeDeath);
                break;

            case EntityState.Bind:
                body.simulated = false;
                mRout = StartCoroutine(DoBound());
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mRout = null;
        mGrowRout = null;

        SetBound(null);
        
        mCurGrowthCount = 0;
        mScoreMultiplier = 1.0f;

        body.simulated = false;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;

        mDefaultParent = null;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        mDefaultParent = transform.parent;

        ApplyGrowthToCollider();
        ApplyGrowthToScale();

        state = (int)EntityState.Normal;
    }

    protected override void OnDestroy() {
        //dealloc here
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        animator.takeCompleteCallback += OnAnimatorComplete;

        body.simulated = false;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void SetBound(StatEntityController entStat) {
        if(mBoundEntityStat != entStat) {
            if(mBoundEntityStat != null) {
                mBoundEntityStat.SendSignal(gameObject, (int)StatEntitySignals.Unbind, null);
            }

            mBoundEntityStat = entStat;

            if(mBoundEntityStat != null) {
                transform.SetParent(mBoundEntityStat.transform, true);

                mBoundEntityStat.SendSignal(gameObject, (int)StatEntitySignals.Bind, null);
            }
            else
                transform.SetParent(mDefaultParent, true);
        }
    }

    void ApplyGrowthToCollider() {
        if(circleCollider) {
            var t = (float)mCurGrowthCount/stats.growthMaxCount;

            circleCollider.radius = Mathf.Clamp(Mathf.Lerp(stats.radiusStart, stats.radiusEnd, t), stats.radiusStart, stats.radiusEnd);
        }
    }

    void ApplyGrowthToScale() {
        if(root) {
            if(mCurGrowthCount > 0) {
                root.gameObject.SetActive(true);
                
                float scale = Mathf.Lerp(stats.radiusToRootScaleStart, stats.radiusToRootScaleEnd, (float)mCurGrowthCount/stats.growthMaxCount);

                root.localScale = new Vector3(scale, scale, 1f);
            }
            else
                root.gameObject.SetActive(false);
        }
    }

    IEnumerator DoGrow(float aGrowthCount) {
        root.gameObject.SetActive(true);

        animator.Play(stats.takeGrow);

        float sScale;
        if(aGrowthCount > 0) {
            sScale = Mathf.Lerp(stats.radiusToRootScaleStart, stats.radiusToRootScaleEnd, aGrowthCount/stats.growthMaxCount);
        }
        else
            sScale = 0f;

        float curTime = 0f;
        while(curTime < stats.growthDelay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = Mathf.Clamp01(curTime/stats.growthDelay);

            var scale = Mathf.Lerp(sScale, mGrowScale, t);

            root.localScale = new Vector3(scale, scale, 1f);
        }

        if(state == (int)EntityState.Normal)
            animator.Play(stats.takeNormal);

        mGrowRout = null;
    }

    IEnumerator DoLaunch() {
        if(stats.launchForceImpulse != 0f)
            body.AddForce(mLaunchDir*stats.launchForceImpulse, ForceMode2D.Impulse);

        //wait for growth to end
        while(mGrowRout != null)
            yield return null;

        var wait = new WaitForFixedUpdate();

        animator.Play(stats.takeLaunch);

        float decayDelay = Mathf.Lerp(stats.launchForceGrowthDecayMinDelay, stats.launchForceGrowthDecayMaxDelay, (float)mCurGrowthCount/stats.growthMaxCount);
                
        float curTime = 0f;
        float forceScale = 1f;
        while(curTime < stats.launchDuration && mLaunchBounds.Contains(body.position)) {
            body.AddForce(mLaunchDir*mLaunchForce*forceScale);

            yield return wait;

            curTime += Time.fixedDeltaTime;
            forceScale = Mathf.Clamp01(stats.launchForceDecayCurve.Evaluate(Mathf.Clamp01(curTime/decayDelay)));
        }

        mRout = null;

        state = (int)EntityState.Dead;
    }

    IEnumerator DoBound() {
        //wait for growth to end
        while(mGrowRout != null)
            yield return null;

        var wait = new WaitForFixedUpdate();
        
        var curHP = stats.GetHP(mCurGrowthCount);

        var sapDmg = mBoundEntityStat.data.damageStamina;
        var sapAttackSpd = mBoundEntityStat.data.attackStaminaSpeed;

        var sapCurTime = 0f;

        animator.Play(stats.takeBind);

        //sap by attachee
        while(curHP > 0f && mBoundEntityStat.isAlive) {
            yield return wait;

            var dt = Time.fixedDeltaTime;

            sapCurTime += dt;

            //attachee breaking off
            if(sapCurTime >= sapAttackSpd) {
                curHP -= sapDmg;

                sapCurTime -= sapAttackSpd;
            }
        }

        //restore stamina damage
        if(mBoundEntityStat.isAlive) {
            var dmgSta = stats.GetDamageStamina(mCurGrowthCount);
            mBoundEntityStat.currentStamina += dmgSta;
        }

        state = (int)EntityState.Dead;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(stats.IsAttackValid(other.tag)) {
            var otherBody = other.attachedRigidbody;

            var victimStats = otherBody.GetComponent<StatEntityController>();
            if(!victimStats) {
                //no stat, just die
                state = (int)EntityState.Dead;
                return;
            }

            var pathogenPrevHP = victimStats.currentHP;

            //pathogen already dead, ignore
            if(pathogenPrevHP <= 0f)
                return;

            var dmg = stats.GetDamage(mCurGrowthCount);
            var dmgSta = stats.GetDamageStamina(mCurGrowthCount);

            var wasAlive = victimStats.isAlive;

            victimStats.currentHP -= dmg;
            victimStats.currentStamina -= dmgSta;

            bool processDeath = true;

            //still alive?
            if(victimStats.isAlive) {
                //snap to pathogen
                Vector2 pos = transform.position;
                Vector2 collPos = other.bounds.center;

                Vector2 dir = collPos - pos;
                float dist = dir.magnitude;
                if(dist > 0f)
                    dir /= dist;

                var collBoundExt = other.bounds.extents;
                float collExtMin = Mathf.Min(collBoundExt.x, collBoundExt.y);

                dist -= collExtMin;

                Vector2 destPos = pos + dir*dist;

                transform.position = destPos;
                //

                //nudge the collided body
                
                if(!otherBody.isKinematic) {
                    var vel = body.velocity;
                    var impactDir = vel.normalized;
                    var impactForce = stats.GetImpactForce(mCurGrowthCount);
                    otherBody.AddForceAtPosition(impactDir*impactForce, destPos, ForceMode2D.Impulse);
                }
                //

                //bind if biological
                if(victimStats.data.type == StatEntity.Type.Biological) {
                    SetBound(victimStats);
                    state = (int)EntityState.Bind;

                    processDeath = false;
                }
            }
            
            if(processDeath) {
                //score
                if(wasAlive && !victimStats.isAlive) {
                    int score = Mathf.RoundToInt(victimStats.data.score * mScoreMultiplier);

                    if(victimStats.isActive)
                        MissionController.instance.ProcessKill(other, victimStats, score);
                    else
                        MissionController.instance.ScoreAt(victimStats.transform.position, score);
                }

                //check excess damage and split off to other pathogens
                var excessDmg = Mathf.Round(dmg - pathogenPrevHP);
                if(excessDmg > 0f) {
                    int splitCount = Mathf.CeilToInt(excessDmg);
                    int splitGrowth;
                    if(splitCount > stats.excessMaxSplitCount) {
                        splitGrowth = Mathf.Max(Mathf.CeilToInt((float)splitCount/stats.excessMaxSplitCount), 1);
                        splitCount = stats.excessMaxSplitCount;
                    }
                    else
                        splitGrowth = 1;

                    //split towards near pathogens
                    Vector2 pos = transform.position;
                    
                    var colls = Physics2D.OverlapCircleAll(other.transform.position, stats.excessRadius, stats.attackSplitLayerMask);
                                        
                    for(int i = 0; i < colls.Length && splitCount > 0; i++) {
                        if(!stats.IsAttackValid(colls[i].tag) || colls[i].attachedRigidbody == other.attachedRigidbody)
                            continue;

                        var dir = ((Vector2)colls[i].transform.position - pos).normalized;
                        
                        var splitMucusForm = M8.PoolController.SpawnFromGroup<EntityMucusForm>(poolData.group, poolData.factoryKey, name+"_split", null, pos, null);

                        splitMucusForm.mScoreMultiplier = mScoreMultiplier + 1.0f;
                        splitMucusForm.SetGrow(splitGrowth);
                        splitMucusForm.Launch(dir, stats.launchForceMaxDistance, mLaunchBounds);

                        splitCount--;
                    }

                    //random splits
                    for(int i = 0; i < splitCount; i++) {
                        var dir = Vector2.up;
                        dir = M8.MathUtil.Rotate(dir, 360f*(Random.Range(1, 65)/65.0f));
                        
                        var splitMucusForm = M8.PoolController.SpawnFromGroup<EntityMucusForm>(poolData.group, poolData.factoryKey, name+"_split", null, pos, null);

                        splitMucusForm.SetGrow(splitGrowth);
                        splitMucusForm.Launch(dir, stats.launchForceMaxDistance, mLaunchBounds);
                    }
                }

                state = (int)EntityState.Dead;
            }
        }
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == stats.takeDeath)
            Release();
    }

    void OnDrawGizmos() {
        if(!stats)
            return;

        Gizmos.color = Color.red;

        if(stats.radiusEnd > 0f)
            Gizmos.DrawWireSphere(transform.position, stats.radiusEnd);

        Gizmos.color *= 0.5f;

        if(stats.radiusStart > 0f)
            Gizmos.DrawWireSphere(transform.position, stats.radiusStart);

        Gizmos.color = Color.yellow;

        if(stats.excessRadius > 0f)
            Gizmos.DrawWireSphere(transform.position, stats.excessRadius);
    }
}
