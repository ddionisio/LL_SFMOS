using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public interface IEntitySpawnerListener {
    /// <summary>
    /// Called upon activation, waiting for spawn ready
    /// </summary>
    void OnSpawnStart();

    /// <summary>
    /// Called when ready to spawn
    /// </summary>
    void OnSpawnReady();

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
    /// Callled until returns true, then ready to spawn again
    /// </summary>
    /// <returns></returns>
    bool OnSpawningFinish();

    /// <summary>
    /// Called when full
    /// </summary>
    void OnSpawnEnd();

    /// <summary>
    /// Called when spawn is deactivated
    /// </summary>
    void OnSpawnStop();
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

    public event Action<EntitySpawner, M8.EntityBase> spawnCallback;

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
        
        var startDelay = UnityEngine.Random.Range(data.spawnStartDelayMin, data.spawnStartDelayMax);

        if(startDelay > 0f) {
            for(int i = 0; i < mListeners.Length; i++)
                mListeners[i].OnSpawnStart();

            yield return new WaitForSeconds(UnityEngine.Random.Range(data.spawnStartDelayMin, data.spawnStartDelayMax));
        }
        
        while(mIsSpawning) {
            //spawn begin
            for(int i = 0; i < mListeners.Length; i++)
                mListeners[i].OnSpawnBegin();

            //wait for spawn ready
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

            //spawning finished?
            while(true) {
                int spawnFinishCount = 0;
                for(int i = 0; i < mListeners.Length; i++) {
                    if(mListeners[i].OnSpawningFinish())
                        spawnFinishCount++;
                }

                if(spawnFinishCount >= mListeners.Length)
                    break;

                yield return null;
            }

            bool isFull = mSpawnedEntities.Count >= data.maxSpawn;
            if(isFull) {
                //spawn end
                for(int i = 0; i < mListeners.Length; i++)
                    mListeners[i].OnSpawnEnd();

                while(mSpawnedEntities.Count >= data.maxSpawn)
                    yield return null;

                //wait for a bit
                yield return new WaitForSeconds(data.spawnFullDelay);
            }
            else {
                //ready up again
                for(int i = 0; i < mListeners.Length; i++)
                    mListeners[i].OnSpawnReady();

                yield return new WaitForSeconds(UnityEngine.Random.Range(data.spawnDelayMin, data.spawnDelayMax));
            }
        }

        mSpawningRout = null;
    }

    void EntityRegisterCallbacks(EntityBase entity, bool register) {
        if(register) {
            entity.releaseCallback += OnEntityReleased;

            if(data.spawnRemoveFromCheckState != EntityState.Invalid)
                entity.setStateCallback += OnEntityChangedState;
        }
        else {
            entity.releaseCallback -= OnEntityReleased;

            if(data.spawnRemoveFromCheckState != EntityState.Invalid)
                entity.setStateCallback -= OnEntityChangedState;
        }
    }

    EntityBase Spawn(PoolController pool, Vector3 spawnPos, Quaternion spawnRot, GenericParams parms) {
        var spawned = pool.Spawn(data.entityRef, data.entityRef, spawnTo, spawnPos, spawnRot, null);

        var entity = spawned.GetComponent<EntityBase>();
        if(entity && !mSpawnedEntities.Exists(entity)) {
            mSpawnedEntities.Add(entity);
            EntityRegisterCallbacks(entity, true);
        }

        if(spawnCallback != null)
            spawnCallback(this, entity);

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
                EntityRegisterCallbacks(ent, false);
        }

        mSpawnedEntities.Clear();
    }

    void OnEntityChangedState(EntityBase ent) {
        if((EntityState)ent.state == data.spawnRemoveFromCheckState) {
            mSpawnedEntities.Remove(ent);
            EntityRegisterCallbacks(ent, false);
        }
    }

    void OnEntityReleased(EntityBase ent) {
        mSpawnedEntities.Remove(ent);
        EntityRegisterCallbacks(ent, false);
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
