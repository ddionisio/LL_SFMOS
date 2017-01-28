using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCommonSpawnLaunch : MonoBehaviour {
    public M8.Animator.AnimatorData animator;

    public string takeEnter;

    public Transform spawnAt;
    public Transform launchAt; //point to launch

    public float moveToLaunchSpeed = 5f;
        
    public float respawnDelay; //delay after launching to respawn

    

    public EntityCommon spawnedEntity { get { return mSpawnedEntity; } }
    public GameObjectSelectible selectible { get { return mSpawnEntitySelectible; } }
    
    public event System.Action<EntityCommonSpawnLaunch, bool> launchReadyCallback; //called when launch ready status changes
    
    private string mPoolGroup;
    private string mEntityRef;

    private EntityCommon mSpawnedEntity;
    private GameObjectSelectible mSpawnEntitySelectible;

    private Coroutine mRout;
    
    /// <summary>
    /// Call by mission to start with a spawn
    /// </summary>
    public void Spawn(string poolGroup, string entityRef) {
        //stop current action
        if(mRout != null)
            StopCoroutine(mRout);

        animator.Stop();

        //clear out previous spawned
        ClearSpawned(true);
                
        mPoolGroup = poolGroup;
        mEntityRef = entityRef;

        mRout = StartCoroutine(DoSpawn());
    }

    public void Launch(Vector2 dir, float force) {
        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoLaunch(dir, force));
    }

    void OnDestroy() {
        ClearSpawned(false);
    }
    
    void ClearSpawned(bool release) {

        if(mSpawnEntitySelectible) {
            //unhook listener
            mSpawnEntitySelectible.selectCallback -= OnEntitySelect;

            mSpawnEntitySelectible = null;
        }

        if(mSpawnedEntity) {
            mSpawnedEntity.releaseCallback -= OnEntityRelease;

            if(release)
                mSpawnedEntity.Release();

            mSpawnedEntity = null;
        }
    }

    IEnumerator DoSpawn() {
        animator.Play(takeEnter);

        //wait one frame for spawnAt to be setup
        yield return null;

        //spawn at the position
        var spawnParms = new M8.GenericParams();
        spawnParms[Params.state] = (int)EntityState.Control;
        spawnParms[Params.anchor] = spawnAt;

        mSpawnedEntity = M8.PoolController.SpawnFromGroup<EntityCommon>(mPoolGroup, mEntityRef, mEntityRef, null, spawnAt.position, spawnParms);

        mSpawnedEntity.releaseCallback += OnEntityRelease; //fail-safe

        mSpawnEntitySelectible = mSpawnedEntity.GetComponentInChildren<GameObjectSelectible>();

        //wait for play to finish
        while(animator.isPlaying)
            yield return null;

        mSpawnedEntity.state = (int)EntityState.Normal;
        mSpawnedEntity.Follow(spawnAt);

        //listen for select, launch
        mSpawnEntitySelectible.selectCallback += OnEntitySelect;


        mRout = null;
    }

    IEnumerator DoLaunchReady() {
        mSpawnedEntity.state = (int)EntityState.Control;
        mSpawnedEntity.anchor = null;

        //manually move towards the launch site
        Vector2 sPos = mSpawnedEntity.transform.position;
        Vector2 ePos = launchAt.position;
        Vector2 dPos = ePos - sPos;

        float dist = dPos.magnitude;
        
        float delay = dist/moveToLaunchSpeed;

        var wait = new WaitForFixedUpdate();

        float curTime = 0f;

        var ease = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);
                
        while(curTime < delay) {
            yield return wait;

            curTime += Time.fixedDeltaTime;

            float t = ease(curTime, delay, 0f, 0f);

            mSpawnedEntity.body.MovePosition(Vector2.Lerp(sPos, ePos, t));
        }

        mSpawnedEntity.body.MovePosition(ePos);

        mRout = null;
        
        if(launchReadyCallback != null)
            launchReadyCallback(this, true);
    }

    IEnumerator DoLaunch(Vector2 dir, float force) {
        var spawnedEntity = mSpawnedEntity;

        ClearSpawned(false);

        spawnedEntity.Launch(dir, force);

        //wait a bit to respawn
        yield return new WaitForSeconds(respawnDelay);

        mRout = null;

        Spawn(mPoolGroup, mEntityRef);
    }

    void OnEntityRelease(M8.EntityBase ent) {
        //shouldn't happen here
        ClearSpawned(false);

        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
    
    void OnEntitySelect(GameObjectSelectible entLaunch) {
        if(entLaunch.isSelected) {
            if(mRout == null)
                mRout = StartCoroutine(DoLaunchReady());
        }
        else {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            mSpawnedEntity.anchor = null;

            mSpawnedEntity.state = (int)EntityState.Normal;
            mSpawnedEntity.Follow(spawnAt);

            if(launchReadyCallback != null)
                launchReadyCallback(this, false);
        }
    }
}
