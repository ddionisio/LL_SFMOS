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

    public void Grow() {
        if(mCurGrowthCount < stats.growthMaxCount) {
            mCurGrowthCount++;

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
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        radius = stats.radiusStart;

        mCurGrowthCount = 0;

        body.simulated = false;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void SpawnStart() {
        //start ai, player control, etc
        state = (int)EntityState.Normal;
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

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag(Tags.pathogen)) {
            //var pathogenStats = other.GetComponent<StatEntityController>();


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
    }
}
