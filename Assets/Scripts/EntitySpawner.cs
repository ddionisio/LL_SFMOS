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
    /// Called when spawn is deactivated, or when we are full
    /// </summary>
    void OnSpawnStop();
}

public class EntitySpawner : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    [Header("Spawn Info")]
    public string poolGroup;
    public string entityRef;

    [Header("Info")]
    public int maxSpawn;

    public float spawnStartDelay;
    public float spawnDelay;

    public Transform spawnAt;  //Note: use up vector as dir param during spawn
    public Transform spawnTo;

    public bool spawnIgnoreRotation;

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
                    for(int i = 0; i < mListeners.Length; i++)
                        mListeners[i].OnSpawnStart();

                    if(mSpawningRout != null)
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
        mSpawnedEntities = new CacheList<EntityBase>(maxSpawn);

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
        if(activeAtStart)
            isSpawning = true;

        GenerateInitialSpawns();
    }

    void GenerateInitialSpawns() {
        if(initialSpawnsAt != null) {
            var pool = PoolController.GetPool(poolGroup);

            for(int i = 0; i < initialSpawnsAt.Length && mSpawnedEntities.Count < maxSpawn; i++) {
                var pos = initialSpawnsAt[i].position; pos.z = 0f;

                Spawn(pool, pos, Quaternion.identity, null);
            }
        }
    }

    IEnumerator DoSpawning() {
        var pool = PoolController.GetPool(poolGroup);

        var wait = spawnDelay > 0f ? new WaitForSeconds(spawnDelay) : null;

        yield return new WaitForSeconds(spawnStartDelay);
        
        while(mIsSpawning) {
            bool isFull = mSpawnedEntities.Count >= maxSpawn;
            if(!isFull) {
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
                var spawnRot = spawnIgnoreRotation ? Quaternion.identity : spawnAt ? spawnAt.rotation : transform.rotation;

                Spawn(pool, spawnPos, spawnRot, mSpawnParms);

                isFull = mSpawnedEntities.Count >= maxSpawn;

                //spawn end
                if(isFull) {
                    for(int i = 0; i < mListeners.Length; i++)
                        mListeners[i].OnSpawnStop();
                }
                else {
                    for(int i = 0; i < mListeners.Length; i++)
                        mListeners[i].OnSpawnEnd();
                }
            }

            yield return wait;
        }

        mSpawningRout = null;
    }

    EntityBase Spawn(PoolController pool, Vector3 spawnPos, Quaternion spawnRot, GenericParams parms) {
        var spawned = pool.Spawn(entityRef, entityRef, spawnTo, spawnPos, spawnRot, null);

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
            for(int i = 0; i < initialSpawnsAt.Length; i++)
                Gizmos.DrawSphere(initialSpawnsAt[i].position, radius);
        }
    }
}
