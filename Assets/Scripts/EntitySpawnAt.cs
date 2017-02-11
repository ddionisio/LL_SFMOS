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
    public bool spawnOnStart = true;
    public bool forceEnemyRegistry = false; //if true, add spawned entity to enemy registry in mission control regardless of flag (registerAsEnemy)

    [Header("Launch")]
    public EntityState launchState; //which state to set once Launch is called
    public Transform launchFollowTarget; //if you want the spawn to follow something upon launch, may not work on certain entities and certain states

    private EntityCommon mSpawned;

    //call by animator to release the spawn and do its own thing
    public void Launch() {
        if(mSpawned) {
            mSpawned.state = (int)launchState;

            if(launchFollowTarget)
                mSpawned.Follow(launchFollowTarget);
        }
    }

    /// <summary>
    /// Don't call this on certain states, e.g. Dead
    /// </summary>
    public void ChangeStateAndFollow(EntityState state, Transform t) {
        if(mSpawned) {
            mSpawned.state = (int)state;
            mSpawned.Follow(t);
        }
    }

    public void Unfollow() {
        if(mSpawned) {
            mSpawned.Follow(null);
        }
    }

    public void Spawn() {
        //shouldn't call this if there's already a spawn, so release the previous
        if(mSpawned) {
            if(preSpawned) {
                preSpawned.transform.position = transform.position;
                preSpawned.gameObject.SetActive(true);
            }
            else
                mSpawned.Release();
        }

        var parms = new M8.GenericParams();
        parms[Params.state] = (int)EntityState.Control;
        parms[Params.anchor] = transform;

        if(preSpawned) {
            mSpawned = preSpawned;
            mSpawned.Spawn(parms); //manually "spawn"
        }
        else {
            mSpawned = M8.PoolController.SpawnFromGroup<EntityCommon>(poolGroup, poolSpawnRef, poolSpawnRef, null, transform.position, parms);
            if(mSpawned)
                mSpawned.releaseCallback += OnSpawnedReleased;
        }
    }

    void OnDestroy() {
        if(mSpawned) {
            mSpawned.releaseCallback -= OnSpawnedReleased;
        }
    }

    void Awake() {
        if(preSpawned) {
            preSpawned.releaseCallback += OnSpawnedReleased;
        }
    }

    // Use this for initialization
    void Start () {
        if(spawnOnStart)
            Spawn();        
    }

    void OnSpawnedReleased(M8.EntityBase ent) {
        mSpawned.releaseCallback -= OnSpawnedReleased;
        mSpawned = null;
    }
}
