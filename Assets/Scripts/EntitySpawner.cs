using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class EntitySpawner : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    public string poolGroup;
    public string entityRef;

    [Header("Info")]
    public int maxSpawn;

    public float spawnStartDelay;
    public float spawnDelay;

    public Transform spawnAt;
    public Transform spawnTo;

    public bool spawnIgnoreRotation;

    public bool activeAtStart;

    [Header("Data")]

    public float impulse = 0f; //set to 0 to ignore

    private bool mIsSpawning;
    private Coroutine mSpawningRout;

    private CacheList<EntityBase> mSpawnedEntities;

    public bool isSpawning {
        get {
            return mIsSpawning;
        }

        set {
            if(mIsSpawning != value) {
                mIsSpawning = value;

                if(mIsSpawning) {
                    if(mSpawningRout != null)
                        mSpawningRout = StartCoroutine(DoSpawning());
                }
                else {
                    if(mSpawningRout != null) {
                        StopCoroutine(mSpawningRout);
                        mSpawningRout = null;
                    }
                }
            }
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
    }

    // Use this for initialization
    void Start () {
        if(activeAtStart)
            isSpawning = true;
	}

    IEnumerator DoSpawning() {
        var pool = PoolController.GetPool(poolGroup);

        var wait = spawnDelay > 0f ? new WaitForSeconds(spawnDelay) : null;

        yield return new WaitForSeconds(spawnStartDelay);

        while(mIsSpawning) {
            if(mSpawnedEntities.Count < maxSpawn) {
                var spawnPos = spawnAt ? spawnAt.position : transform.position;
                var spawnRot = spawnIgnoreRotation ? Quaternion.identity : spawnAt ? spawnAt.rotation : transform.rotation;

                var spawned = pool.Spawn(entityRef, entityRef, spawnTo, spawnPos, spawnRot, null);

                var entity = spawned.GetComponent<EntityBase>();
                if(entity && !mSpawnedEntities.Exists(entity)) {
                    mSpawnedEntities.Add(entity);
                    entity.releaseCallback += OnEntityReleased;
                }
            }

            yield return wait;
        }

        mSpawningRout = null;
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
    }
}
