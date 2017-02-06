using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0Controller : MissionController {
    public const string takeStageFormat = "stage_{0}_{1}";

    public enum State {
        None,

        Begin,

        StageTransition,

        StagePlay,

        Victory,

        Defeat,
    }

    [System.Serializable]
    public struct StageData {
        public int subStageCount;
        public float subStageNextDelay;
        public float minDuration; //for stages with no pathogens
        public GameObject activateGO;
    }

    [Header("Mucus Gather")]
    public MucusGatherInputField mucusGatherInput;
    public MucusGather mucusGather;

    public Transform pointer;
    public GameObject pointerGO;

    public Bounds mucusFormBounds;

    public EntitySpawner[] spawnerActivates; //activated upon calling ActivateSpawners

    public float launchAngleLimit = 75f;

    [Header("Health")]
    public EntityCommon[] cellWalls; //when all these die, game over, man

    [Header("Macrophage")]
    public EntityPhagocyte macrophage;
    public float processDeathWaitDelay;
    public float processDeathPositionY; //relative to mucusFormBounds
    public float processDeathPickUpXMin; //relative to mucusFormBounds, and processDeathPositionY
    public float processDeathPickUpXMax; //relative to mucusFormBounds, and processDeathPositionY
    public float processDeathFallSpeed;
    public float processMoveSpeedMin;
    public float processMoveSpeedMax;
    public float processMoveWaveHeight;
    public float processMoveWaveRate; //Y position wave while moving

    [Header("Stage Data")]
    public M8.Animator.AnimatorData animator;

    public float beginDelay = 1f;
    public GameObject beginActivateGO;

    public string takeBeginIntro;
    public string takeBeginOutro;

    public string takeStageTransition; //do some woosh thing towards left

    public string takeDefeat;
    public string takeVictory;

    public StageData[] stages; //determines stage and sub stages
    
    public float stageNextDelay = 3; //how long before the next stage transition

    private bool mIsPointerActive;

    private int mCurStageInd;

    private State mCurState = State.None;
    private Coroutine mRout;
    
    private int mEnemyCheckCount; //if this reaches 0 after all sub stage finishes, go to next stage
    private int mVictimCount;

    private M8.CacheList<EntityCommon> mCellWallsAlive;

    public override int missionIndex { get { return 0; } }

    public bool isProcessingVictims {
        get {
            return mVictimCount > 0 || macrophage.isEating;
        }
    }

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(mucusGatherInput) {
            mucusGatherInput.pointerDownCallback -= OnMucusFieldInputDown;
            mucusGatherInput.pointerDragCallback -= OnMucusFieldInputDrag;
            mucusGatherInput.pointerUpCallback -= OnMucusFieldInputUp;
            mucusGatherInput.lockChangeCallback -= OnMucusFieldInputLockChange;
        }

        for(int i = 0; i < cellWalls.Length; i++) {
            if(cellWalls[i])
                cellWalls[i].setStateCallback -= OnCellWallStateChanged;
        }
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        mIsPointerActive = false;

        if(pointerGO)
            pointerGO.SetActive(false);

        if(pointer)
            pointer.gameObject.SetActive(false);

        for(int i = 0; i < stages.Length; i++) {
            if(stages[i].activateGO)
                stages[i].activateGO.SetActive(false);
        }

        mEnemyCheckCount = 0;
        mVictimCount = 0;

        mCellWallsAlive = new M8.CacheList<EntityCommon>(cellWalls.Length);
    }

    IEnumerator Start() {
        //hook stuff up after others init
        
        mucusGatherInput.pointerDownCallback += OnMucusFieldInputDown;
        mucusGatherInput.pointerDragCallback += OnMucusFieldInputDrag;
        mucusGatherInput.pointerUpCallback += OnMucusFieldInputUp;
        mucusGatherInput.lockChangeCallback += OnMucusFieldInputLockChange;

        for(int i = 0; i < cellWalls.Length; i++) {
            if(cellWalls[i]) {
                cellWalls[i].setStateCallback += OnCellWallStateChanged;
                mCellWallsAlive.Add(cellWalls[i]);
            }
        }

        mucusGatherInput.isLocked = true;

        yield return new WaitForSeconds(beginDelay);

        if(!animator)
            yield break;

        ApplyState(State.Begin);
    }

    void SetPointerActive(bool active) {
        if(mIsPointerActive != active) {
            mIsPointerActive = active;

            if(pointerGO)
                pointerGO.SetActive(mIsPointerActive);

            if(pointer)
                pointer.gameObject.SetActive(mIsPointerActive);
        }
    }

    public override void Signal(SignalType signal, object parms) {
        switch(signal) {
            case SignalType.EnemyRegister:
                mEnemyCheckCount++;
                break;

            case SignalType.EnemyUnregister:
                mEnemyCheckCount--;
                break;
        }
    }

    /// <summary>
    /// Call this during begin animation to start up the spawners (goblet mucus spawn)
    /// </summary>           
    public void ActivateSpawners() {
        for(int i = 0; i < spawnerActivates.Length; i++) {
            if(spawnerActivates[i])
                spawnerActivates[i].isSpawning = true;
        }
    }

    public override Transform RequestTarget(Transform requestor) {
        if(mCellWallsAlive.Count <= 0)
            return null;

        var cell = mCellWallsAlive[Random.Range(0, mCellWallsAlive.Count)];

        return cell ? cell.transform : null;
    }

    public override void ProcessKill(Collider2D victimColl, StatEntityController victimStatCtrl, int score) {
        //default behaviour
        //ScoreAt(victimStatCtrl.transform.position, score);
        StartCoroutine(DoVictimProcess(victimColl, victimStatCtrl, score));
    }

    void EnterStage(int stage) {
        if(mCurStageInd >= 0 && mCurStageInd < stages.Length) {
            if(stages[mCurStageInd].activateGO)
                stages[mCurStageInd].activateGO.SetActive(false);
        }

        mCurStageInd = stage;

        if(mCurStageInd >= 0 && mCurStageInd < stages.Length) {
            if(stages[mCurStageInd].activateGO)
                stages[mCurStageInd].activateGO.SetActive(true);
        }

        SendSignal(SignalType.NewStage, mCurStageInd);

        ApplyState(State.StageTransition);
    }

    void ApplyState(State state) {
        if(mCurState != state) {
            //var prevState = mCurState;
            mCurState = state;

            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            switch(state) {
                case State.Begin:
                    mRout = StartCoroutine(DoBegin());
                    break;

                case State.StageTransition:
                    mRout = StartCoroutine(DoStageTransition());
                    break;

                case State.StagePlay:
                    mRout = StartCoroutine(DoStagePlay());
                    break;

                case State.Victory:
                    mRout = StartCoroutine(DoVictory());
                    break;

                case State.Defeat:
                    mRout = StartCoroutine(DoDefeat());
                    break;
            }
        }
    }

    IEnumerator DoBegin() {
        mucusGatherInput.isLocked = false;

        if(beginActivateGO)
            beginActivateGO.SetActive(true);

        //small intro to show stuff
        if(!string.IsNullOrEmpty(takeBeginIntro)) {
            animator.Play(takeBeginIntro);

            while(animator.isPlaying)
                yield return null;
        }

        //mucusGatherInput.isLocked = false;

        //free form, wait for pathogens to die
        while(mEnemyCheckCount > 0)
            yield return null;
        
        //mucusGatherInput.isLocked = true;

        if(!string.IsNullOrEmpty(takeBeginOutro)) {
            animator.Play(takeBeginOutro);

            while(animator.isPlaying)
                yield return null;
        }

        if(beginActivateGO)
            beginActivateGO.SetActive(false);

        mRout = null;

        EnterStage(0);
    }

    IEnumerator DoStageTransition() {
        mucusGatherInput.isLocked = false;

        //show incoming

        //apply progress animation to HUD

        if(!string.IsNullOrEmpty(takeStageTransition)) {
            animator.Play(takeStageTransition);

            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        ApplyState(State.StagePlay);
    }

    IEnumerator DoStagePlay() {
        mucusGatherInput.isLocked = false;

        //if for some reason we are suppose to be finished
        if(mCurStageInd >= stages.Length) {
            ApplyState(State.Victory);
            yield break;
        }

        var startTime = Time.time;

        var curStage = stages[mCurStageInd];

        if(curStage.subStageCount > 0) {
            var waitNextSubStage = new WaitForSeconds(curStage.subStageNextDelay);

            for(int i = 0; i < curStage.subStageCount; i++) {
                var take = string.Format("stage_{0}_{1}", mCurStageInd, i);
                animator.Play(take);

                while(animator.isPlaying)
                    yield return null;

                yield return waitNextSubStage;
            }
        }
        else {
            var take = "stage_"+mCurStageInd;
            animator.Play(take);
        }

        //wait for all spawn checks to be released      

        while(Time.time - startTime < curStage.minDuration || mEnemyCheckCount > 0)
            yield return null;

        //move specific entities to the left?

        yield return new WaitForSeconds(stageNextDelay);

        mRout = null;

        //go to next stage?
        if(mCurStageInd < stages.Length - 1) {
            EnterStage(mCurStageInd + 1);
        }
        else { //victory
            ApplyState(State.Victory);
        }
    }

    IEnumerator DoVictory() {
        mucusGatherInput.isLocked = true;

        SendSignal(SignalType.Complete, 0);

        if(!string.IsNullOrEmpty(takeVictory)) {
            animator.Play(takeVictory);

            while(animator.isPlaying)
                yield return null;
        }

        //wait for eating process
        while(isProcessingVictims)
            yield return null;

        mRout = null;

        ProcessVictory();
    }

    IEnumerator DoDefeat() {
        mucusGatherInput.isLocked = true;

        SendSignal(SignalType.Defeat, 0);

        if(!string.IsNullOrEmpty(takeDefeat)) {
            animator.Play(takeDefeat);

            while(animator.isPlaying)
                yield return null;
        }

        //wait for eating process
        while(isProcessingVictims)
            yield return null;

        mRout = null;

        ProcessLose();
    }

    IEnumerator DoVictimProcess(Collider2D coll, StatEntityController statCtrl, int score) {
        yield return new WaitForSeconds(processDeathWaitDelay);

        mVictimCount++;
        
        float deathProcessX = mucusFormBounds.center.x + Random.Range(processDeathPickUpXMin, processDeathPickUpXMax);
        float deathY = mucusFormBounds.center.y + processDeathPositionY;

        Transform trans = statCtrl.transform;

        //fall down
        Vector2 pos = trans.position;

        float yStart = pos.y;

        if(yStart != deathY) {
            var ease = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InSine);

            float dist = Mathf.Abs(deathY - yStart);

            float delay = dist/processDeathFallSpeed;
            float curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Clamp01(ease(curTime, delay, 0f, 0f));

                trans.position = new Vector2(pos.x, Mathf.Lerp(yStart, deathY, t));
            }
        }

        //move towards phagocyte
        float xStart = pos.x;

        if(xStart != deathProcessX) {
            float dist = Mathf.Abs(deathProcessX - xStart);

            float spd = Random.Range(processMoveSpeedMin, processMoveSpeedMax);
            float delay = dist/spd;
            float curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Clamp01(curTime/delay);

                trans.position = new Vector2(Mathf.Lerp(xStart, deathProcessX, t), deathY - Mathf.Sin(curTime*processMoveWaveRate)*processMoveWaveHeight);
            }
        }

        //let it be eaten
        macrophage.Eat(statCtrl.GetComponent<M8.EntityBase>(), score);

        mVictimCount--;
    }

    bool IsValidLaunch(Vector2 pos) {
        if(mucusGather.Contains(pos))
            return false;

        Vector2 dir = (pos - (Vector2)mucusGather.transform.position).normalized;
        if(Vector2.Angle(Vector2.up, dir) > launchAngleLimit)
            return false;

        return true;
    }

    void OnMucusFieldInputDown(MucusGatherInputField input) {
        if(input.currentAreaType == MucusGatherInputField.AreaType.Bottom) {
            mucusGather.transform.position = new Vector3(input.originPosition.x, input.originPosition.y, 0f);
            mucusGather.Activate();
        }
    }

    void OnMucusFieldInputDrag(MucusGatherInputField input) {
        if(mucusGather.isActive) {
            var pos = input.currentPosition;

            bool pointerActive = IsValidLaunch(pos);// !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

            if(pointerActive) {
                pointer.position = new Vector3(pos.x, pos.y, pointer.position.z);
            }

            SetPointerActive(pointerActive);
        }
    }

    void OnMucusFieldInputUp(MucusGatherInputField input) {
        SetPointerActive(false);

        if(mucusGather.isActive) {
            Vector2 pos = input.currentPosition;

            bool pointerActive = IsValidLaunch(pos);// !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

            if(pointerActive) {
                Vector2 sPos = mucusGather.mucusFormSpawnAt.position;

                var dir = pos - sPos;
                var dist = dir.magnitude;
                if(dist > 0f)
                    dir /= dist;

                mucusGather.Release(dir, dist, mucusFormBounds);
            }
            else {
                mucusGather.Cancel();
            }
        }
    }

    void OnMucusFieldInputLockChange(MucusGatherInputField input) {
        if(input.isLocked) {
            SetPointerActive(false);
            mucusGather.Cancel();
        }
    }

    void OnCellWallStateChanged(M8.EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Dead:
                //remove from cache
                mCellWallsAlive.Remove((EntityCommon)ent);

                Debug.Log("cell wall dead: "+ent.name);

                if(mCellWallsAlive.Count <= 0)
                    ApplyState(State.Defeat);
                break;
        }
    }
    
    void OnDrawGizmos() {

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(mucusFormBounds.center, mucusFormBounds.size);

        Gizmos.color *= 0.5f;

        float y = mucusFormBounds.center.y + processDeathPositionY;

        Vector3 lPosS = new Vector3(mucusFormBounds.min.x, y, 0f);
        Vector3 lPosE = new Vector3(mucusFormBounds.max.x, y, 0f);

        Gizmos.DrawLine(lPosS, lPosE);

        float processDeathRadius = Mathf.Abs(processDeathPickUpXMax - processDeathPickUpXMin);

        Gizmos.DrawSphere(new Vector3(mucusFormBounds.center.x + Mathf.Lerp(processDeathPickUpXMin, processDeathPickUpXMax, 0.5f), y, 0f), processDeathRadius);
    }
}
