using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMucusForm : M8.EntityBase {
    public Rigidbody2D body;
    public CircleCollider2D circleCollider;
    public Transform root;

    public M8.Animator.AnimatorData animator;
    
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

    public float radius {
        get {
            return mRadius;
        }

        private set {
            if(mRadius != value) {
                mRadius = Mathf.Clamp(value, radiusStart, radiusEnd);

                if(circleCollider) circleCollider.radius = mRadius;
                
                float scale = radiusStart > 0f ? mRadius/radiusStart : 0f;

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
        if(mCurGrowthCount < growthMaxCount) {
            mCurGrowthCount++;

            //TODO: interpolate
            float t = (float)mCurGrowthCount/growthMaxCount;

            radius = Mathf.Lerp(radiusStart, radiusEnd, t);
        }
    }

    public void Launch(Vector2 dir, float length, Bounds bounds) {
        if(dir.x + dir.y == 0f) {
            Cancel();
            return;
        }

        mLaunchDir = dir;
        mLaunchForce = Mathf.Lerp(launchForceMin, launchForceMax, GetLaunchForceScale(length));
        mLaunchBounds = bounds;

        state = (int)EntityState.Launch;
    }

    public void Cancel() {
        state = (int)EntityState.Dead;
    }

    public float GetLaunchForceScale(float length) {
        return Mathf.Clamp01(launchForceCurve.Evaluate(Mathf.Clamp01((length - radius)/launchForceMaxDistance)));
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
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        radius = radiusStart;

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
        radius = radiusStart;
        
        body.simulated = false;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoLaunch() {
        var wait = new WaitForFixedUpdate();

        float decayDelay = Mathf.Lerp(launchForceGrowthDecayMinDelay, launchForceGrowthDecayMaxDelay, (float)mCurGrowthCount/growthMaxCount);

        if(launchForceImpulse != 0f)
            body.AddForce(mLaunchDir*launchForceImpulse, ForceMode2D.Impulse);

        float curTime = 0f;
        float forceScale = 1f;
        while(curTime < launchDuration && mLaunchBounds.Contains(body.position)) {
            body.AddForce(mLaunchDir*mLaunchForce*forceScale);

            yield return wait;

            curTime += Time.fixedDeltaTime;
            forceScale = Mathf.Clamp01(launchForceDecayCurve.Evaluate(Mathf.Clamp01(curTime/decayDelay)));
        }

        mRout = null;

        state = (int)EntityState.Dead;
    }

    void OnDrawGizmos() {

        Gizmos.color = Color.red;

        if(radiusEnd > 0f)
            Gizmos.DrawWireSphere(transform.position, radiusEnd);

        Gizmos.color *= 0.5f;

        if(radiusStart > 0f)
            Gizmos.DrawWireSphere(transform.position, radiusStart);
    }
}
