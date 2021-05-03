using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EntityCommonSpawnLaunch : MonoBehaviour {
    [System.Serializable]
    public struct SpawnInfo {
        public string entityRef;
        public CellBindData bindData;
    }

    public class SpawnInfoGenerator {
        private string mSpawnPoolGroup;
        private SpawnInfo[] mSpawnInfos;
        private int mCurIndex;

        public string poolGroup { get { return mSpawnPoolGroup; } }

        public SpawnInfo next {
            get {
                var ret = mSpawnInfos[mCurIndex];
                mCurIndex++;
                if(mCurIndex == mSpawnInfos.Length)
                    mCurIndex = 0;
                return ret;
            }
        }

        public void Init(string aPoolGroup, SpawnInfo[] infos) {
            mSpawnPoolGroup = aPoolGroup;
            mSpawnInfos = infos;
            mCurIndex = 0;
        }
    }

    public M8.Animator.AnimatorData animator;

    public string takeEnter;

    public Transform spawnAt;
    public Transform launchAt; //point to launch

    public float moveToLaunchSpeed = 5f;
        
    public float respawnDelay; //delay after launching to respawn

    public event System.Action<EntityCommonSpawnLaunch, PointerEventData> dragBeginCallback;
    public event System.Action<EntityCommonSpawnLaunch, PointerEventData> dragCallback;
    public event System.Action<EntityCommonSpawnLaunch, PointerEventData> dragEndCallback;

    public EntityCommon spawnedEntity { get { return mSpawnedEntity; } }
    public GameObjectSelectible selectible { get { return mSpawnEntitySelectible; } }
    
    private EntityCommon mSpawnedEntity;
    private GameObjectSelectible mSpawnEntitySelectible;

    private SpawnInfoGenerator mSpawnInfoGen;

    private Coroutine mRout;
    private Coroutine mLaunchReadyRout;

    public void SetSpawnGenerator(SpawnInfoGenerator gen) {
        mSpawnInfoGen = gen;
    }
    
    /// <summary>
    /// Call by mission to start with a spawn, make sure to populate first
    /// </summary>
    public void StartSpawn() {
        //stop current action
        if(mLaunchReadyRout != null) {
            StopCoroutine(mLaunchReadyRout);
            mLaunchReadyRout = null;
        }

        if(mRout != null)
            StopCoroutine(mRout);
                
        animator.Stop();

        //clear out previous spawned
        ClearSpawned(true);

        if(mSpawnInfoGen == null) {
            Debug.LogWarning("No Spawn Info Generator for: "+name);
            return;
        }

        mRout = StartCoroutine(DoSpawn());
    }

    public void Launch(Vector2 dir, float force) {
        if(mLaunchReadyRout != null) {
            StopCoroutine(mLaunchReadyRout);
            mLaunchReadyRout = null;
        }

        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoLaunch(dir, force));
    }

    void OnDestroy() {
        ClearSpawned(false);
    }
    
    void ClearSpawned(bool release) {

        if(mSpawnEntitySelectible) {
            mSpawnEntitySelectible.isLocked = false;

            //unhook listener
            mSpawnEntitySelectible.selectCallback -= OnEntitySelect;
            mSpawnEntitySelectible.dragBeginCallback -= OnEntityDragBegin;
            mSpawnEntitySelectible.dragCallback -= OnEntityDrag;
            mSpawnEntitySelectible.dragEndCallback -= OnEntityDragEnd;

            mSpawnEntitySelectible = null;
        }

        if(mSpawnedEntity) {
            mSpawnedEntity.releaseCallback -= OnEntityRelease;

            if(release)
                mSpawnedEntity.Release();

            mSpawnedEntity = null;
        }
    }
    
    void SetLaunchReady(bool ready) {
        if(ready) {
            if(mLaunchReadyRout == null)
                mLaunchReadyRout = StartCoroutine(DoLaunchReady());
        }
        else {
            if(mLaunchReadyRout != null) {
                StopCoroutine(mLaunchReadyRout);
                mLaunchReadyRout = null;
            }

            mSpawnedEntity.anchor = null;
            mSpawnedEntity.state = (int)EntityState.Normal;
            mSpawnedEntity.Follow(spawnAt);
        }
    }

    IEnumerator DoSpawn() {
        var spawnInfo = mSpawnInfoGen.next;
        
        animator.Play(takeEnter);

        //wait one frame for spawnAt to be setup
        yield return null;

        //spawn at the position
        var spawnParms = new M8.GenericParams();
        spawnParms[Params.state] = (int)EntityState.Control;
        spawnParms[Params.anchor] = spawnAt;

        mSpawnedEntity = M8.PoolController.SpawnFromGroup<EntityCommon>(mSpawnInfoGen.poolGroup, spawnInfo.entityRef, spawnInfo.entityRef, null, spawnAt.position, spawnParms);

        if(mSpawnedEntity.cellBind)
            mSpawnedEntity.cellBind.Populate(spawnInfo.bindData);

        mSpawnedEntity.releaseCallback += OnEntityRelease; //fail-safe

        mSpawnEntitySelectible = mSpawnedEntity.GetComponentInChildren<GameObjectSelectible>();

        //wait for play to finish
        while(animator.isPlaying)
            yield return null;

        SetLaunchReady(false);

        //listen for select, drag
        mSpawnEntitySelectible.selectCallback += OnEntitySelect;
        mSpawnEntitySelectible.dragBeginCallback += OnEntityDragBegin;
        mSpawnEntitySelectible.dragCallback += OnEntityDrag;
        mSpawnEntitySelectible.dragEndCallback += OnEntityDragEnd;

        mRout = null;
    }

    IEnumerator DoLaunchReady() {
        var spawnedEntity = mSpawnedEntity;

        spawnedEntity.state = (int)EntityState.Control;
        spawnedEntity.anchor = null;

        //manually move towards the launch site
        Vector2 sPos = spawnedEntity.transform.position;
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

            spawnedEntity.body.MovePosition(Vector2.Lerp(sPos, ePos, t));
        }

        spawnedEntity.body.MovePosition(ePos);

        mLaunchReadyRout = null;
    }

    IEnumerator DoLaunch(Vector2 dir, float force) {
        var spawnedEntity = mSpawnedEntity;

        ClearSpawned(false);

        //wait for launch ready to end
        while(mLaunchReadyRout != null)
            yield return null;
                
        spawnedEntity.Launch(dir, force);

        //wait a bit to respawn
        yield return new WaitForSeconds(respawnDelay);

        mRout = null;

        StartSpawn();
    }

    void OnEntityRelease(M8.EntityBase ent) {
        //shouldn't happen here
        ClearSpawned(false);

        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mLaunchReadyRout != null) {
            StopCoroutine(mLaunchReadyRout);
            mLaunchReadyRout = null;
        }
    }
    
    void OnEntitySelect(GameObjectSelectible entLaunch) {
        if(!entLaunch.isSelected) {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }

        SetLaunchReady(entLaunch.isSelected);
    }

    void OnEntityDragBegin(GameObjectSelectible entLaunch, PointerEventData dat) {
        if(dragBeginCallback != null)
            dragBeginCallback(this, dat);
    }

    void OnEntityDrag(GameObjectSelectible entLaunch, PointerEventData dat) {
        if(dragCallback != null)
            dragCallback(this, dat);
    }

    void OnEntityDragEnd(GameObjectSelectible entLaunch, PointerEventData dat) {
        if(dragEndCallback != null)
            dragEndCallback(this, dat);
    }
}
