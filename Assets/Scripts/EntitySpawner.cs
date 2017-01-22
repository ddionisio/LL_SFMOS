using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public interface IEntitySpawnerListener {
    /// <summary>
    /// Called upon activation, or while active and no longer full
    /// </summary>
    void OnSpawnStart();

    /// <summary>
    /// Called when about to spawn
    /// </summary>
    void OnSpawnBegin();
    
    /// <summary>
    /// Called until returns true, then spawns
    /// </summary>
    /// <returns></returns>
    bool OnSpawning();

    /// <summary>
    /// Called after spawning
    /// </summary>
    void OnSpawnEnd();

    /// <summary>
    /// Called when spawn is deactivated
    /// </summary>
    void OnSpawnStop();

    /// <summary>
    /// Called when spawn becomes full, then when we are no longer full
    /// </summary>
    void OnSpawnFull(bool full);
}

public class EntitySpawner : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    public EntitySpawnerData data;
    
    public Transform spawnAt;  //Note: use up vector as dir param during spawn
    public Transform spawnTo;
    
    public bool activeAtStart;

    //List of spawn locations to initially spawn things at start
    public Transform[] initialSpawnsAt;
    
    private bool mIsSpawning;
    private Coroutine mSpawningRout;

    private CacheList<EntityBase> mSpawnedEntities;

    private IEntitySpawnerListener[] mListeners;

    private GenericParams mSpawnParms;

    public bool isSpawning {
        get {
            return mIsSpawning;
        }

        set {
            if(mIsSpawning != value) {
                mIsSpawning = value;

                if(mIsSpawning) {
                    if(mSpawningRout == null)
                        mSpawningRout = StartCoroutine(DoSpawning());
                }
                else {
                    if(mSpawningRout != null) {
                        StopCoroutine(mSpawningRout);
                        mSpawningRout = null;
                    }

                    for(int i = 0; i < mListeners.Length; i++)
                        mListeners[i].OnSpawnStop();
                }
            }
        }
    }

    public int spawnCount {
        get {
            return mSpawnedEntities.Count;
        }
    }

    public void ApplyParam(string arg, object val) {
        if(val != null)
            mSpawnParms[arg] = val;
        else
            mSpawnParms.Remove(arg);
    }

    void OnEnable() {
        if(mIsSpawning) {
            if(mSpawningRout == null)
                mSpawningRout = StartCoroutine(DoSpawning());
        }
    }

    void OnDisable() {
        if(mSpawningRout != null) {
            StopCoroutine(mSpawningRout);
            mSpawningRout = null;
        }
    }

    void Awake() {
        mSpawnedEntities = new CacheList<EntityBase>(data.maxSpawn);

        //grab listeners
        var comps = GetComponentsInChildren<MonoBehaviour>(true);

        var listeners = new List<IEntitySpawnerListener>();

        for(int i = 0; i < comps.Length; i++) {
            var comp = comps[i];

            var listener = comp as IEntitySpawnerListener;
            if(listener != null) listeners.Add(listener);
        }

        mListeners = listeners.ToArray();

        mSpawnParms = new GenericParams();
    }

    // Use this for initialization
    void Start() {
        GenerateInitialSpawns();

        if(activeAtStart)
            isSpawning = true;
    }

    void GenerateInitialSpawns() {
        if(initialSpawnsAt != null) {
            var pool = PoolController.GetPool(data.poolGroup);

            for(int i = 0; i < initialSpawnsAt.Length && mSpawnedEntities.Count < data.maxSpawn; i++) {
                var pos = initialSpawnsAt[i].position; pos.z = 0f;

                Spawn(pool, pos, Quaternion.identity, null);
            }
        }
    }

    IEnumerator DoSpawning() {
        var pool = PoolController.GetPool(data.poolGroup);
        
        var wait = data.spawnDelay > 0f ? new WaitForSeconds(data.spawnDelay) : null;

        for(int i = 0; i < mListeners.Length; i++)
            mListeners[i].OnSpawnStart();

        yield return new WaitForSeconds(data.spawnStartDelay);
        
        while(mIsSpawning) {
            bool isFull = mSpawnedEntities.Count >= data.maxSpawn;
            if(isFull) {
                for(int i = 0; i < mListeners.Length; i++)
                    mListeners[i].OnSpawnFull(true);

                while(mSpawnedEntities.Count >= data.maxSpawn)
                    yield return null;

                //wait for a bit
                yield return new WaitForSeconds(data.spawnFullDelay);

                for(int i = 0; i < mListeners.Length; i++)
                    mListeners[i].OnSpawnFull(false);
            }

            //spawn begin
            for(int i = 0; i < mListeners.Length; i++)
                mListeners[i].OnSpawnBegin();

            //spawning
            while(true) {
                int spawnReadyCount = 0;
                for(int i = 0; i < mListeners.Length; i++) {
                    if(mListeners[i].OnSpawning())
                        spawnReadyCount++;
                }

                if(spawnReadyCount >= mListeners.Length)
                    break;

                yield return null;
            }

            //spawn
            var spawnPos = spawnAt ? spawnAt.position : transform.position; spawnPos.z = 0f;
            var spawnRot = data.spawnIgnoreRotation ? Quaternion.identity : spawnAt ? spawnAt.rotation : transform.rotation;

            Spawn(pool, spawnPos, spawnRot, mSpawnParms);

            isFull = mSpawnedEntities.Count >= data.maxSpawn;

            //spawn end
            for(int i = 0; i < mListeners.Length; i++)
                mListeners[i].OnSpawnEnd();

            yield return wait;
        }

        mSpawningRout = null;
    }

    EntityBase Spawn(PoolController pool, Vector3 spawnPos, Quaternion spawnRot, GenericParams parms) {
        var spawned = pool.Spawn(data.entityRef, data.entityRef, spawnTo, spawnPos, spawnRot, null);

        var entity = spawned.GetComponent<EntityBase>();
        if(entity && !mSpawnedEntities.Exists(entity)) {
            mSpawnedEntities.Add(entity);
            entity.releaseCallback += OnEntityReleased;
        }

        return entity;
    }

    void IPoolSpawn.OnSpawned(GenericParams parms) {
        if(activeAtStart)
            isSpawning = true;
    }

    void IPoolDespawn.OnDespawned() {
        isSpawning = false;

        foreach(var ent in mSpawnedEntities) {
            if(ent)
                ent.releaseCallback -= OnEntityReleased;
        }

        mSpawnedEntities.Clear();
    }

    void OnEntityReleased(EntityBase ent) {
        mSpawnedEntities.Remove(ent);
        ent.releaseCallback -= OnEntityReleased;

        if(mSpawningRout != null) {
            for(int i = 0; i < mListeners.Length; i++)
                mListeners[i].OnSpawnStart();
        }
    }

    void OnDrawGizmos() {
        const float radius = 0.1f;

        //Spawn At
        var arrowTrans = spawnAt ? spawnAt : transform;

        Gizmos.color = Color.green;

        Gizmos.DrawSphere(arrowTrans.position, radius);

        Gizmo.Arrow(arrowTrans.position, arrowTrans.up, 0.2f);

        //Initial spawners
        if(initialSpawnsAt != null) {
            for(int i = 0; i < initialSpawnsAt.Length; i++) {
                if(initialSpawnsAt[i])
                    Gizmos.DrawSphere(initialSpawnsAt[i].position, radius);
            }
        }
    }
}
