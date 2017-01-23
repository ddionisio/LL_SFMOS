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
    public EntityCommon preSpawned; //if you want something from the scene, rather than the pool

    [Header("Launch")]
    public EntityState launchState; //which state to set once Launch is called

    private EntityCommon mSpawned;

    //call by animator to release the spawn and do its own thing
    public void Launch() {
        if(mSpawned)
            mSpawned.state = (int)launchState;
    }

    public void Cancel() {

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
        var parms = new M8.GenericParams();
        parms[Params.state] = (int)EntityState.Control;
        parms[Params.anchor] = transform;

        if(mSpawned) {
            mSpawned.Spawn(parms); //manually "spawn"
        }
        else {
            mSpawned = M8.PoolController.SpawnFromGroup<EntityCommon>(poolGroup, poolSpawnRef, poolSpawnRef, null, transform.position, parms);
            if(mSpawned)
                mSpawned.releaseCallback += OnSpawnedReleased;
        }
    }

    void OnSpawnedReleased(M8.EntityBase ent) {
        mSpawned.releaseCallback -= OnSpawnedReleased;
        mSpawned = null;
    }
}
