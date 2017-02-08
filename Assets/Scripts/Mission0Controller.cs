using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0Controller : MissionController {
    public const string takeStageFormat = "stage_{0}_{1}";

    public enum State {
        None,

        StagePlay,

        StageTransition,
                
        Victory,

        Defeat,
    }

    public enum ActionType {
        Cutscene,
        Dialog,
        InputLock,
        InputUnlock,
        Wait
    }

    [System.Serializable]
    public struct Action {
        public string id;
        public ActionType type;
        public string sVal;
        public float fVal;
    }

    [System.Serializable]
    public class StageData {
        public GameObject activateGO;

        public M8.Animator.AnimatorData anim;

        public Action[] enterActions;
        public Action[] exitActions;
        
        public float minDuration; //for stages with no pathogens

        public float stageNextDelay = 3; //how long before the next stage transition
    }

    [Header("Stage Data")]    
    public StageData[] stages; //determines stage and sub stages

    [Header("Main Animation")]
    public M8.Animator.AnimatorData mainAnimator; //general animator

    public string takeStageTransition; //do some woosh thing towards left
    public string takeDefeat;
    public string takeVictory;
                
    [Header("Mucus Gather")]
    public MucusGatherInputField mucusGatherInput;

    public Transform pointer;
    public GameObject pointerGO;
    public GameObject[] pointerDisplays;
    public M8.TransAttachTo pointerAttach;

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
        
    private bool mIsPointerActive;

    private int mCurStageInd = -1;

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

        for(int i = 0; i < pointerDisplays.Length; i++)
            pointerDisplays[i].SetActive(false);

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

        while(M8.SceneManager.instance.isLoading)
            yield return null;

        mucusGatherInput.isLocked = false;

        mCurStageInd = -1;

        EnterStage(0);
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

    void SetPointerDisplayActive(bool active) {
        for(int i = 0; i < pointerDisplays.Length; i++)
            pointerDisplays[i].SetActive(active);
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
    /// Call this during animation to start up the spawners (goblet mucus spawn)
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

    protected override void SetLock(bool aLock) {
        mucusGatherInput.isLocked = aLock;
    }

    void EnterStage(int stage) {
        var prevStageInd = mCurStageInd;

        if(prevStageInd >= 0 && prevStageInd < stages.Length) {
            if(stages[prevStageInd].activateGO)
                stages[prevStageInd].activateGO.SetActive(false);
        }
                
        mCurStageInd = stage;

        if(mCurStageInd >= 0 && mCurStageInd < stages.Length) {
            if(stages[mCurStageInd].activateGO)
                stages[mCurStageInd].activateGO.SetActive(true);
        }

        SendSignal(SignalType.NewStage, mCurStageInd);

        if(prevStageInd >= 0)
            ApplyState(State.StageTransition);
        else
            ApplyState(State.StagePlay);
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
    
    IEnumerator DoStageTransition() {
        mucusGatherInput.isLocked = false;

        //show incoming

        //apply progress animation to HUD

        if(!string.IsNullOrEmpty(takeStageTransition)) {
            mainAnimator.Play(takeStageTransition);

            while(mainAnimator.isPlaying)
                yield return null;
        }

        mRout = null;

        ApplyState(State.StagePlay);
    }

    IEnumerator DoActions(M8.Animator.AnimatorData anim, Action[] actions) {
        for(int i = 0; i < actions.Length; i++) {
            var act = actions[i];

            switch(act.type) {
                case ActionType.Cutscene:
                    anim.Play(act.id);
                    while(anim.isPlaying)
                        yield return null;
                    break;

                case ActionType.Dialog:
                    //open dialog
                    //wait for close
                    break;

                case ActionType.InputLock:
                    inputLock = true;
                    break;

                case ActionType.InputUnlock:
                    inputLock = false;
                    break;

                case ActionType.Wait:
                    yield return new WaitForSeconds(act.fVal);
                    break;
            }
        }
    }

    IEnumerator DoStagePlay() {
        //if for some reason we are suppose to be finished
        if(mCurStageInd >= stages.Length) {
            ApplyState(State.Victory);
            yield break;
        }

        var startTime = Time.time;

        var curStage = stages[mCurStageInd];

        yield return DoActions(curStage.anim, curStage.enterActions);
        
        //wait for all spawn checks to be released
        while(Time.time - startTime < curStage.minDuration || mEnemyCheckCount > 0)
            yield return null;

        //move specific entities to the left?

        if(curStage.stageNextDelay > 0f)
            yield return new WaitForSeconds(curStage.stageNextDelay);

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
            mainAnimator.Play(takeVictory);

            while(mainAnimator.isPlaying)
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
            mainAnimator.Play(takeDefeat);

            while(mainAnimator.isPlaying)
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

    bool IsValidLaunch(MucusGatherInputField input) {
        if(Vector2.Angle(Vector2.up, input.dir) > launchAngleLimit)
            return false;

        if(input.curLength < input.currentLaunchInput.radius)
            return false;

        return true;
    }
    
    void OnMucusFieldInputDown(MucusGatherInputField input) {
        pointerAttach.target = input.currentLaunchInput.transform;

        SetPointerActive(true);
    }

    void OnMucusFieldInputDrag(MucusGatherInputField input) {
        var origin = input.originPosition;
        var pos = input.currentPosition;

        bool pointerActive = IsValidLaunch(input);// !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

        if(pointerActive) {
            pointer.position = new Vector3(pos.x, pos.y, pointer.position.z);
        }

        SetPointerDisplayActive(pointerActive);
    }

    void OnMucusFieldInputUp(MucusGatherInputField input) {
        SetPointerActive(false);
        SetPointerDisplayActive(false);

        Vector2 pos = input.currentPosition;

        bool pointerActive = IsValidLaunch(input);

        if(pointerActive) {
            input.Launch(mucusFormBounds);
        }
        else
            input.Cancel();
    }

    void OnMucusFieldInputLockChange(MucusGatherInputField input) {
        if(input.isLocked) {
            SetPointerActive(false);
            SetPointerDisplayActive(false);
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
