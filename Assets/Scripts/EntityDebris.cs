using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityDebris : EntityCommon {
    public const float roamTorqueMin = -1.5f;
    public const float roamTorqueMax = 1.5f;

    private int mTakeHurtInd;

    private Coroutine mRout;

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
                if(body) {
                    body.isKinematic = true;
                    body.simulated = true;
                }
                break;

            case EntityState.Normal:
                if(body) {
                    body.isKinematic = false;
                    body.simulated = true;
                }

                mRout = StartCoroutine(DoRoam());
                break;

            case EntityState.Dead:
                if(body) {
                    body.simulated = false;
                }

                //split if able
                if(stats.data.canSplit) {
                    var pool = M8.PoolController.GetPool(stats.data.splitSpawnPoolGroup);

                    Vector2 pos = transform.position;

                    //first
                    var splitDir = stats.data.splitDir;
                    var splitType = stats.data.splitEntityType;
                    var spawned = pool.Spawn(splitType, splitType, null, pos + splitDir*stats.data.splitSpawnRadius, null);
                    var spawnedBody = spawned.GetComponent<Rigidbody2D>();
                    if(spawnedBody)
                        spawnedBody.AddForce(splitDir*stats.data.splitImpulse, ForceMode2D.Impulse);

                    //second
                    splitDir = -splitDir;
                    splitType = stats.data.splitEntityType;
                    spawned = pool.Spawn(splitType, splitType, null, pos + splitDir*stats.data.splitSpawnRadius, null);
                    spawnedBody = spawned.GetComponent<Rigidbody2D>();
                    if(spawnedBody)
                        spawnedBody.AddForce(splitDir*stats.data.splitImpulse, ForceMode2D.Impulse);
                }

                //animation
                if(animator && !string.IsNullOrEmpty(stats.data.takeDeath))
                    animator.Play(stats.data.takeDeath);
                else if(stats.data.releaseOnDeath)
                    Release();

                //Release();
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        base.OnDespawned();
    }

    /*protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
    }*/

    protected override void OnDestroy() {
        //dealloc here
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        mTakeHurtInd = -1;

        if(animator) {
            if(!string.IsNullOrEmpty(stats.data.takeHurt))
                mTakeHurtInd = animator.GetTakeIndex(stats.data.takeHurt);

            animator.takeCompleteCallback += OnAnimatorComplete;
        }
    }

    // Use this for one-time initialization
    /*protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }*/

    protected override void OnStatHPChanged(StatEntityController aStats, float prev) {
        if(aStats.currentHP > 0f) {
            if(aStats.currentHP < prev) {
                if(mTakeHurtInd != -1)
                    animator.Play(mTakeHurtInd);
            }
            //healed?

            return;
        }

        base.OnStatHPChanged(aStats, prev);
    }

    IEnumerator DoRoam() {
        var wait = new WaitForFixedUpdate();

        //a bit of rotate
        body.AddTorque(Random.Range(roamTorqueMin, roamTorqueMax));

        //just bobble
        var dir = Random.Range(0, 2) == 0 ? Vector2.up : Vector2.down;
        var curTime = 0f;
        var delay = Random.Range(stats.data.roamChangeDelayMin, stats.data.roamChangeDelayMax);
        var force = Random.Range(stats.data.roamForceMin, stats.data.roamForceMax);

        while(true) {
            yield return wait;

            body.AddForce(dir*force);

            curTime += Time.fixedDeltaTime;
            if(curTime >= delay) {
                dir *= -1;
                curTime = 0f;
                delay = Random.Range(stats.data.roamChangeDelayMin, stats.data.roamChangeDelayMax);
                force = Random.Range(stats.data.roamForceMin, stats.data.roamForceMax);
            }
        }
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == stats.data.takeSpawn) {
            if(!string.IsNullOrEmpty(stats.data.takeNormal))
                animator.Play(stats.data.takeNormal);
        }
        else if(take.name == stats.data.takeDeath) {
            if(stats.data.releaseOnDeath)
                Release();
        }
    }
}