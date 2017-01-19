using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPathogenSpawnControl : MonoBehaviour {
    [Header("Launch")]
    public M8.Animator.AnimatorData animator;
    public string takeLaunch;
    public float launchDelay;
    public Transform launchFollow;

    [Header("Watch")]
    public M8.EntityBase entityDeathWatch; //if this dies, release spawned pathogens if we haven't launched
        
    [Header("Spawn")]
    public string poolGroup;
    public string poolSpawnRef;

    public Transform[] spawnAt;

    private Coroutine mRout;

    private bool mIsLaunched;

    private M8.CacheList<EntityPathogen> mSpawnedPathogens;

    public void Launch() {
        if(mIsLaunched || mRout != null) //can't launch anymore :(
            return;
                
        mRout = StartCoroutine(DoLaunch());
    }

    //called during animation to start following the launchFollow
    public void Follow() {
        for(int i = 0; i < mSpawnedPathogens.Count; i++) {
            var pathogen = mSpawnedPathogens[i];

            //start following if it's still valid, and not dead
            if(pathogen && pathogen.state != (int)EntityState.Dead) {
                pathogen.state = (int)EntityState.Normal;

                if(pathogen.flock)
                    pathogen.flock.moveTarget = launchFollow;
            }
        }
    }

    void OnDisable() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
        
        if(entityDeathWatch)
            entityDeathWatch.setStateCallback -= OnEntityDeathWatchChangeState;

        //clear out spawns, shouldn't be filled at this point
        for(int i = 0; i < mSpawnedPathogens.Count; i++) {
            if(mSpawnedPathogens[i]) {
                mSpawnedPathogens[i].releaseCallback -= OnSpawnedEntityRelease;
            }
        }

        mSpawnedPathogens.Clear();
    }

    void Awake() {
        mSpawnedPathogens = new M8.CacheList<EntityPathogen>(spawnAt.Length);

        if(entityDeathWatch)
            entityDeathWatch.setStateCallback += OnEntityDeathWatchChangeState;
    }
        
    void Start() {
        //spawn and hold
        var parms = new M8.GenericParams();
        parms[Params.state] = (int)EntityState.Control;

        for(int i = 0; i < spawnAt.Length; i++) {
            var spawned = M8.PoolController.SpawnFromGroup<EntityPathogen>(poolGroup, poolSpawnRef, poolSpawnRef, spawnAt[i], parms);
                        
            spawned.releaseCallback += OnSpawnedEntityRelease;

            mSpawnedPathogens.Add(spawned);
        }
    }

    IEnumerator DoLaunch() {
        yield return new WaitForSeconds(launchDelay);

        mIsLaunched = true;

        //play animator
        if(animator) {
            animator.Play(takeLaunch);

            while(animator.isPlaying)
                yield return null;
        }

        //start seeking, no longer need to track it
        for(int i = 0; i < mSpawnedPathogens.Count; i++) {
            if(mSpawnedPathogens[i]) {
                mSpawnedPathogens[i].state = (int)EntityState.Seek;

                mSpawnedPathogens[i].releaseCallback -= OnSpawnedEntityRelease; //no longer need to listen
            }
        }

        mSpawnedPathogens.Clear();

        mRout = null;
    }

    void OnSpawnedEntityRelease(M8.EntityBase ent) {
        ent.releaseCallback -= OnSpawnedEntityRelease;

        mSpawnedPathogens.Remove((EntityPathogen)ent);
    }

    void OnEntityDeathWatchChangeState(M8.EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Dead:
                //disperse spawned entities if we didn't get a chance to launch
                if(!mIsLaunched) {
                    mIsLaunched = true; //can no longer launch

                    //set spawned pathogens to wander
                    for(int i = 0; i < mSpawnedPathogens.Count; i++) {
                        if(mSpawnedPathogens[i]) {
                            if(mSpawnedPathogens[i].state != (int)EntityState.Dead)
                                mSpawnedPathogens[i].state = (int)EntityState.Wander;

                            mSpawnedPathogens[i].releaseCallback -= OnSpawnedEntityRelease; //no longer need to listen
                        }
                    }

                    mSpawnedPathogens.Clear();
                }

                ent.setStateCallback -= OnEntityDeathWatchChangeState;
                break;
        }
    }
}
