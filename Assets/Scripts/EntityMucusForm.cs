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

    public void Grow() {
        if(mCurGrowthCount < growthMaxCount) {
            mCurGrowthCount++;

            //TODO: interpolate
            float t = (float)mCurGrowthCount/growthMaxCount;

            radius = Mathf.Lerp(radiusStart, radiusEnd, t);
        }
    }

    public void Launch(Vector2 dir, float force) {

    }

    public void Cancel() {
        state = (int)EntityState.Dead;
    }

    protected override void StateChanged() {
        switch((EntityState)state) {
            case EntityState.Normal:
                body.simulated = false;
                break;

            case EntityState.Launch:
                body.simulated = true;
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

    void OnDrawGizmos() {

        Gizmos.color = Color.red;

        if(radiusEnd > 0f)
            Gizmos.DrawWireSphere(transform.position, radiusEnd);

        Gizmos.color *= 0.5f;

        if(radiusStart > 0f)
            Gizmos.DrawWireSphere(transform.position, radiusStart);
    }
}
