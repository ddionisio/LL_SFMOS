using System.Collections;
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
            return mRadius;
        }

        private set {
            if(mRadius != value) {
                mRadius = Mathf.Clamp(value, stats.radiusStart, stats.radiusEnd);

                if(circleCollider) circleCollider.radius = mRadius;
                
                float scale = stats.radiusStart > 0f ? mRadius/stats.radiusStart : 0f;

                if(root)
                    root.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }

    public int currentGrowthCount { get { return mCurGrowthCount; } }
    
    private float mRadius = 0f;
    private int mCurGrowthCount = 0;

    private Coroutine mRout;

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
            //TODO: interpolate
            float t = (float)mCurGrowthCount/stats.growthMaxCount;

            radius = Mathf.Lerp(stats.radiusStart, stats.radiusEnd, t);
        }
    }

    public void Launch(Vector2 dir, float length, Bounds bounds) {        
        if(dir.x + dir.y == 0f) {
            Cancel();
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
                break;

            case EntityState.Launch:
                body.simulated = true;
                mRout = StartCoroutine(DoLaunch());
                break;

            case EntityState.Dead:
                body.simulated = false;

                //TODO: animation
                Release();
                break;

            case EntityState.Bind:
                body.simulated = false;
                mRout = StartCoroutine(DoBound());
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        SetBound(null);

        radius = stats.radiusStart;

        mCurGrowthCount = 0;

        body.simulated = false;

        mDefaultParent = null;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        mDefaultParent = transform.parent;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        radius = stats.radiusStart;
        
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

    IEnumerator DoLaunch() {
        var wait = new WaitForFixedUpdate();

        float decayDelay = Mathf.Lerp(stats.launchForceGrowthDecayMinDelay, stats.launchForceGrowthDecayMaxDelay, (float)mCurGrowthCount/stats.growthMaxCount);

        if(stats.launchForceImpulse != 0f)
            body.AddForce(mLaunchDir*stats.launchForceImpulse, ForceMode2D.Impulse);

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
        var wait = new WaitForFixedUpdate();
        
        var curHP = stats.GetHP(mCurGrowthCount);

        var sapDmg = mBoundEntityStat.data.damageStamina;
        var sapAttackSpd = mBoundEntityStat.data.attackStaminaSpeed;

        var sapCurTime = 0f;
        
        //TODO: animation

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

        //restore stamina damage (if biological)
        if(mBoundEntityStat.isAlive && mBoundEntityStat.data.type == StatEntity.Type.Biological) {
            var dmgSta = stats.GetDamageStamina(mCurGrowthCount);
            mBoundEntityStat.currentStamina += dmgSta;
        }

        state = (int)EntityState.Dead;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(stats.IsAttackValid(other.tag)) {
            var pathogenStats = other.attachedRigidbody.GetComponent<StatEntityController>();
            if(!pathogenStats) {
                //no stat, just die
                state = (int)EntityState.Dead;
                return;
            }

            var pathogenPrevHP = pathogenStats.currentHP;

            //pathogen already dead, ignore
            if(pathogenPrevHP <= 0f)
                return;

            var dmg = stats.GetDamage(mCurGrowthCount);
            var dmgSta = stats.GetDamageStamina(mCurGrowthCount);
                        
            pathogenStats.currentHP -= dmg;
            pathogenStats.currentStamina -= dmgSta;

            //if pathogen still alive, bind
            if(pathogenStats.isAlive) {
                //snap to pathogen
                Vector2 pos = transform.position;

                var layerIndex = other.gameObject.layer;

                Vector2 dir = (Vector2)other.transform.position - pos;
                float dist = dir.magnitude;
                if(dist > 0f)
                    dir /= dist;

                var coll = Physics2D.Raycast(pos, dir, dist, 1<<layerIndex);
                if(coll.rigidbody == other.attachedRigidbody) { //should be the same collider
                    transform.position = coll.point;
                    SetBound(pathogenStats);
                    state = (int)EntityState.Bind;
                }
                else //edge case, just die
                    state = (int)EntityState.Dead;
            }
            else {
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
