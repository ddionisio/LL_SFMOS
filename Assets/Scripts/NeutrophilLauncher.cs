using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutrophilLauncher : MonoBehaviour {
    public const int launchCapacity = 32;

    [System.Serializable]
    public class SpawnPoint {
        public Transform point;
        public M8.Animator.AnimatorData animator; //make sure this animates the point
        public string takeSpawn = "spawn";
        public string takeLaunch = "launch";
        
        public Neutrophil entity { get { return mEntity; } }
        public EntityCommon target { get { return mTarget; } }
        public bool isSpawning { get { return mIsSpawning; } }
        
        private Neutrophil mEntity;
        private EntityCommon mTarget;

        private bool mIsSpawning;

        public void Init() {
        }

        public void Reset() {
            mEntity = null;
            mTarget = null;

            mIsSpawning = false;

            if(animator) animator.Stop();
        }
                
        public void Spawn(string poolRef,  string entityRef) {
            if(mEntity) //shouldn't get here
                return;

            if(animator) {
                mEntity = M8.PoolController.GetPool(poolRef).Spawn<Neutrophil>(entityRef, entityRef, null, point.position, null);
                mEntity.Follow(point);

                mIsSpawning = true;

                animator.Play(takeSpawn);
            }
        }

        public void Launch(EntityCommon target) {
            if(animator) {
                if(mEntity) {
                    mTarget = target;
                    animator.Play(takeLaunch);
                }
            }
        }

        void OnAnimationFinish(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
            if(take.name == takeSpawn) {
                if(mEntity)
                    mEntity.Follow(null);

                mIsSpawning = false;
            }
            else if(take.name == takeLaunch) {
                //set to launch
                if(mEntity)
                    mEntity.Launch(mTarget);

                mEntity = null;
                mTarget = null;
            }
        }
    }

    [Header("Spawn")]
    public string poolRef;
    public string entityRef;

    public SpawnPoint[] spawnPts;

    [Header("Launch")]
    public M8.Auxiliary.AuxTrigger2D detectTrigger;
    
    [Header("Animation")]
    public M8.Animator.AnimatorData animator;
    public string takeActivate;
    public string takeDeactivate;
    
    private int mSpawnCount; //when it reaches 0, deactivate

    private Coroutine mRout;
    private Coroutine mLaunchRout;

    private M8.CacheList<EntityCommon> mTargetPotentialEntities;
    
    public void Activate(int addSpawnCount) {
        mSpawnCount += addSpawnCount;

        if(mRout == null) {
            mRout = StartCoroutine(DoSpawns());
        }
    }

    void OnDisable() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mLaunchRout != null) {
            StopCoroutine(mLaunchRout);
            mLaunchRout = null;
        }

        mSpawnCount = 0;

        var pool = M8.PoolController.GetPool(poolRef);
        if(pool)
            pool.ReleaseAllByType(entityRef);

        for(int i = 0; i < spawnPts.Length; i++)
            spawnPts[i].Reset();

        mTargetPotentialEntities.Clear();

        if(detectTrigger)
            detectTrigger.gameObject.SetActive(false);
    }

    void Awake() {
        for(int i = 0; i < spawnPts.Length; i++)
            spawnPts[i].Init();

        detectTrigger.enterCallback += OnDetectTriggerEnter;

        mTargetPotentialEntities = new M8.CacheList<EntityCommon>(launchCapacity);

        detectTrigger.gameObject.SetActive(false);
    }

    void OnDetectTriggerEnter(Collider2D coll) {
        EntityCommon ent = coll.GetComponent<EntityCommon>();

        //make sure it's alive and is't already added
        if(ent.stats.isAlive && !mTargetPotentialEntities.IsFull && !mTargetPotentialEntities.Exists(ent)) {
            mTargetPotentialEntities.Add(ent);

            if(mLaunchRout == null)
                mLaunchRout = StartCoroutine(DoLaunch());
        }
    }

    IEnumerator DoLaunch() {
        while(true) {
            if(mTargetPotentialEntities.Count == 0)
                break;

            //check if potential target is still valid
            var potentialTarget = mTargetPotentialEntities[0];
            if(potentialTarget == null || !potentialTarget.stats.isAlive) {
                mTargetPotentialEntities.Remove();
                continue;
            }
                        
            //grab launch ready
            SpawnPoint spawnPt = null;
            for(int i = 0; i < spawnPts.Length; i++) {
                if(spawnPts[i].entity && !spawnPts[i].isSpawning && !spawnPts[i].target) {
                    spawnPt = spawnPts[i];
                    break;
                }
            }

            if(spawnPt != null) {
                //launch at target
                spawnPt.Launch(potentialTarget);

                mTargetPotentialEntities.Remove();
            }

            yield return null;
        }

        mLaunchRout = null;
    }
    
    IEnumerator DoSpawns() {
        detectTrigger.gameObject.SetActive(true);

        animator.Play(takeActivate);
        while(animator.isPlaying)
            yield return null;

        while(mSpawnCount > 0) {
            //grab available
            SpawnPoint spawnPt = null;
            for(int i = 0; i < spawnPts.Length; i++) {
                if(!spawnPts[i].entity) {
                    spawnPt = spawnPts[i];
                    break;
                }
            }

            if(spawnPt != null) {
                //spawn
                spawnPt.Spawn(poolRef, entityRef);
            }

            yield return null;
        }

        //check if all pts are now available
        while(true) {
            int spawnAvailableCount = 0;
            for(int i = 0; i < spawnPts.Length; i++) {
                if(!spawnPts[i].entity)
                    spawnAvailableCount++;
            }

            if(spawnAvailableCount == spawnPts.Length)
                break;

            yield return null;
        }

        //deactivate
        detectTrigger.gameObject.SetActive(false);

        if(mLaunchRout != null) {
            StopCoroutine(mLaunchRout);
            mLaunchRout = null;
        }

        mTargetPotentialEntities.Clear();

        animator.Play(takeDeactivate);

        mRout = null;
    }

    void SpawnHookUpCallback(M8.EntityBase ent, bool hookup) {
        if(hookup) {
            ent.setStateCallback += OnSpawnChangeState;
            ent.releaseCallback += OnSpawnRelease;
        }
        else {
            ent.setStateCallback -= OnSpawnChangeState;
            ent.releaseCallback -= OnSpawnRelease;
        }
    }

    void OnSpawnChangeState(M8.EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Dead:
            case EntityState.DeadInstant:
                mSpawnCount--;
                if(mSpawnCount < 0) //shouldn't happen
                    mSpawnCount = 0;

                SpawnHookUpCallback(ent, false);
                break;

            case EntityState.Leave: //doesn't count
                SpawnHookUpCallback(ent, false);
                break;
        }
    }

    void OnSpawnRelease(M8.EntityBase ent) {
        mSpawnCount--;
        if(mSpawnCount < 0) //shouldn't happen
            mSpawnCount = 0;

        //check if this is in the spawns, then reset it
        for(int i = 0; i < spawnPts.Length; i++) {
            if(spawnPts[i].entity == ent) {
                spawnPts[i].Reset();
                break;
            }
        }

        SpawnHookUpCallback(ent, false);
    }

    void OnDrawGizmos() {
        if(spawnPts == null)
            return;

        Gizmos.color = Color.green;

        const float r = 0.15f;

        for(int i = 0; i < spawnPts.Length; i++) {
            if(spawnPts[i].point)
                Gizmos.DrawWireSphere(spawnPts[i].point.position, r);
        }
    }
}
