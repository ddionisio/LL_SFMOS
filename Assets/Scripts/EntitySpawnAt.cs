using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note: spawns with param "state"=Control, "anchor"=transform
/// </summary>
public class EntitySpawnAt : MonoBehaviour {
    [Header("Spawn")]
    public string poolGroup;
    public string poolSpawnRef;
    public M8.EntityBase preSpawned; //if you want something from the scene, rather than the pool

    [Header("Launch")]
    public EntityState launchState; //which state to set once Launch is called

    private M8.EntityBase mSpawned;

    //call by animator to release the spawn and do its own thing
    public void Launch() {
        if(mSpawned)
            mSpawned.state = (int)launchState;
    }

    void OnDestroy() {
        if(mSpawned) {
            mSpawned.releaseCallback -= OnSpawnedReleased;
        }
    }

    void Awake() {
        if(preSpawned) {
            preSpawned.releaseCallback += OnSpawnedReleased;
            mSpawned = preSpawned;
        }
    }

    // Use this for initialization
    void Start () {
        if(mSpawned) {
            //mSpawned
        }
        else {
            var parms = new M8.GenericParams();
            parms[Params.state] = (int)EntityState.Control;
            parms[Params.anchor] = transform;

            mSpawned = M8.PoolController.SpawnFromGroup<M8.EntityBase>(poolGroup, poolSpawnRef, poolSpawnRef, null, parms);
            if(mSpawned)
                mSpawned.releaseCallback += OnSpawnedReleased;
        }
    }

    void OnSpawnedReleased(M8.EntityBase ent) {
        mSpawned.releaseCallback -= OnSpawnedReleased;
        mSpawned = null;
    }
}
