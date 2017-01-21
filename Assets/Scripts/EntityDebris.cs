using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityDebris : M8.EntityBase {
    public const float roamTorqueMin = -1.5f;
    public const float roamTorqueMax = 1.5f;

    public StatEntityController stats { get { return mStats; } }
    public Rigidbody2D body { get { return mBody; } }

    public Transform anchor {
        get {
            return mAnchor;
        }

        set {
            if(mAnchor != value) {
                mAnchor = value;
                if(mAnchor) {
                    mBody.isKinematic = true; //force to kinematic

                    if(mAnchorRout == null)
                        mAnchorRout = StartCoroutine(DoAnchorUpdate());
                }
                else {
                    if(mAnchorRout != null) {
                        StopCoroutine(mAnchorRout);
                        mAnchorRout = null;
                    }
                }
            }
        }
    }

    private StatEntityController mStats;
    private Rigidbody2D mBody;

    private Coroutine mRout;

    private Transform mAnchor;
    private Coroutine mAnchorRout;

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Control:
                anchor = null; //force to detach anchor
                break;
        }

        switch((EntityState)state) {
            case EntityState.Control:
                if(mBody) {
                    mBody.isKinematic = true;
                    mBody.simulated = true;
                }
                break;

            case EntityState.Normal:
                if(mBody) {
                    mBody.isKinematic = false;
                    mBody.simulated = true;
                }

                mRout = StartCoroutine(DoRoam());
                break;

            case EntityState.Dead:
                //split if able
                if(mStats.data.canSplit) {
                    var pool = M8.PoolController.GetPool(mStats.data.splitSpawnPoolGroup);

                    Vector2 pos = transform.position;

                    //first
                    var splitDir = mStats.data.splitDir;
                    var splitType = mStats.data.splitEntityType;
                    var spawned = pool.Spawn(splitType, splitType, null, pos + splitDir*mStats.data.splitSpawnRadius, null);
                    var spawnedBody = spawned.GetComponent<Rigidbody2D>();
                    if(spawnedBody)
                        spawnedBody.AddForce(splitDir*mStats.data.splitImpulse, ForceMode2D.Impulse);

                    //second
                    splitDir = -splitDir;
                    splitType = mStats.data.splitEntityType;
                    spawned = pool.Spawn(splitType, splitType, null, pos + splitDir*mStats.data.splitSpawnRadius, null);
                    spawnedBody = spawned.GetComponent<Rigidbody2D>();
                    if(spawnedBody)
                        spawnedBody.AddForce(splitDir*mStats.data.splitImpulse, ForceMode2D.Impulse);
                }

                //animation

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

        if(mBody) {
            mBody.velocity = Vector2.zero;
            mBody.angularVelocity = 0f;
            mBody.simulated = false;
        }

        anchor = null;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        int toState = (int)EntityState.Normal;
        Transform toAnchor = null;

        if(parms != null) {
            if(parms.ContainsKey(Params.state))
                toState = parms.GetValue<int>(Params.state);

            if(parms.ContainsKey(Params.anchor))
                toAnchor = parms.GetValue<Transform>(Params.anchor);
        }

        //start ai, player control, etc
        state = toState;
        anchor = toAnchor;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mStats = GetComponent<StatEntityController>();
        if(mStats) {
            mStats.HPChangedCallback += OnStatHPChanged;
        }

        mBody = GetComponent<Rigidbody2D>();
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoRoam() {
        var wait = new WaitForFixedUpdate();

        //a bit of rotate
        mBody.AddTorque(Random.Range(roamTorqueMin, roamTorqueMax));

        //just bobble
        var dir = Random.Range(0, 2) == 0 ? Vector2.up : Vector2.down;
        var curTime = 0f;
        var delay = Random.Range(mStats.data.roamChangeDelayMin, mStats.data.roamChangeDelayMax);
        var force = Random.Range(mStats.data.roamForceMin, mStats.data.roamForceMax);

        while(true) {
            yield return wait;

            mBody.AddForce(dir*force);

            curTime += Time.fixedDeltaTime;
            if(curTime >= delay) {
                dir *= -1;
                curTime = 0f;
                delay = Random.Range(mStats.data.roamChangeDelayMin, mStats.data.roamChangeDelayMax);
                force = Random.Range(mStats.data.roamForceMin, mStats.data.roamForceMax);
            }
        }
    }

    IEnumerator DoAnchorUpdate() {
        var wait = new WaitForFixedUpdate();

        while(mAnchor) {
            mBody.MovePosition(mAnchor.position);

            yield return wait;
        }

        mAnchorRout = null;
    }

    void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP <= 0f)
            state = (int)EntityState.Dead;
    }
}
