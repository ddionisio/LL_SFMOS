using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission1Controller : MissionController {
    public enum State {
        None,

        Begin,

        StageTransition,

        StagePlay,

        Finish,
    }

    [System.Serializable]
    public struct StageData {
        public GameObject activateGO;

        public EntityCommonSpawnLaunch.SpawnInfo[] spawnInfos;
    }

    public EntityCommonInputLaunchField launchField;

    [Header("Stage Transition Data")]
    public M8.Animator.AnimatorData transitionAnimator;

    public string takeStageTransitionBegin; //open up
    public string takeStageTransitionEnd; //after play starts, close up

    public float transitionBeginDelay = 0.3f;

    [Header("Stage Data")]
    public M8.Animator.AnimatorData animator;

    public float beginDelay = 1f;
    public GameObject beginActivateGO;

    public string takeBeginIntro;
    public string takeBeginOutro;
        
    public string takeFinish;

    public string stageSpawnLaunchPoolGroup;

    public StageData[] stages; //determines stage and sub stages

    public string stageSpawnCheckPoolGroup; //which group to check for spawns
    public string[] stageSpawnTagChecks; //which spawned entities to check to complete a stage

    public Transform[] leavePositions;
    
    public float stageDuration = 60; //the duration of the stage before it finishes

    public override int missionIndex { get { return 1; } }

    private bool mIsPointerActive;

    private int mCurStageInd;

    private State mCurState = State.None;
    private Coroutine mRout;
    
    private int mLeaveCounter;

    private List<M8.PoolDataController> mStageSpawnChecks; //if this reaches 0 after all sub stage finishes, go to next stage

    protected override void OnInstanceDeinit() {
        //unhook


        if(!string.IsNullOrEmpty(stageSpawnCheckPoolGroup)) {
            var pool = M8.PoolController.GetPool(stageSpawnCheckPoolGroup);
            if(pool) {
                pool.spawnCallback -= OnSpawnEntity;
                pool.despawnCallback -= OnDespawnEntity;
            }
        }

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        mStageSpawnChecks = new List<M8.PoolDataController>();
    }

    IEnumerator Start() {
        //hook stuff up after others init
        //hook stuff up after others init
        if(!string.IsNullOrEmpty(stageSpawnCheckPoolGroup)) {
            var pool = M8.PoolController.GetPool(stageSpawnCheckPoolGroup);
            if(pool) {
                pool.spawnCallback += OnSpawnEntity;
                pool.despawnCallback += OnDespawnEntity;
            }
        }

        yield return new WaitForSeconds(beginDelay);

        if(!animator)
            yield break;

        ApplyState(State.Begin);
    }

    public override void Signal(SignalType signal, object parms) {
        switch(signal) {
            case SignalType.LeaveBegin:
                mLeaveCounter++;
                break;
            case SignalType.LeaveEnd:
                mLeaveCounter--;
                break;
        }
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

                case State.Finish:
                    mRout = StartCoroutine(DoFinish());
                    break;
            }
        }
    }

    bool CheckSpawn(M8.PoolDataController pdc) {
        bool isValidSpawn = false;
        for(int i = 0; i < stageSpawnTagChecks.Length; i++) {
            if(pdc.CompareTag(stageSpawnTagChecks[i])) {
                isValidSpawn = true;
                break;
            }
        }

        return isValidSpawn;
    }

    IEnumerator DoBegin() {

        if(beginActivateGO)
            beginActivateGO.SetActive(true);

        //small intro to show stuff
        if(!string.IsNullOrEmpty(takeBeginIntro)) {
            animator.Play(takeBeginIntro);

            while(animator.isPlaying)
                yield return null;
        }
        
        //time to leave
        yield return DoLeaveProcess(takeBeginOutro);
        
        if(beginActivateGO)
            beginActivateGO.SetActive(false);

        mRout = null;

        EnterStage(0);
    }

    IEnumerator DoStageTransition() {

        //show incoming

        //apply progress animation to HUD

        if(transitionAnimator && !string.IsNullOrEmpty(takeStageTransitionBegin))
            transitionAnimator.Play(takeStageTransitionBegin);

        yield return new WaitForSeconds(transitionBeginDelay);

        mRout = null;

        ApplyState(State.StagePlay);
    }

    IEnumerator DoStagePlay() {

        //if for some reason we are suppose to be finished
        if(mCurStageInd >= stages.Length) {
            ApplyState(State.Finish);
            yield break;
        }
        
        var curStage = stages[mCurStageInd];

        //entry/spawn
        int takeInd = animator.GetTakeIndex("stage_"+mCurStageInd);
        if(takeInd != -1) {
            animator.Play(takeInd);

            while(animator.isPlaying)
                yield return null;
        }

        if(transitionAnimator && !string.IsNullOrEmpty(takeStageTransitionEnd))
            transitionAnimator.Play(takeStageTransitionEnd);

        //prep spawners and start it
        launchField.PopulateSpawners(stageSpawnLaunchPoolGroup, curStage.spawnInfos);

        launchField.StartSpawners();

        //ready to play

        //wait for duration of stage or if all pathogens get cleared
        var startTime = Time.time;

        while(Time.time - startTime < stageDuration && mStageSpawnChecks.Count > 0)
            yield return null;

        //time to leave
        yield return DoLeaveProcess("stage_"+mCurStageInd+"_exit");

        mRout = null;

        //go to next stage?
        if(mCurStageInd < stages.Length - 1) {
            EnterStage(mCurStageInd + 1);
        }
        else { //victory
            ApplyState(State.Finish);
        }
    }

    IEnumerator DoLeaveProcess(string takeLeave) {
        int takeLeaveInd = !string.IsNullOrEmpty(takeLeave) ? animator.GetTakeIndex(takeLeave) : -1;

        if(takeLeaveInd != -1)
            animator.Play(takeLeaveInd);

        mLeaveCounter = 0;

        SendSignal(SignalType.Leave, leavePositions[Random.Range(0, leavePositions.Length)]);

        do {
            yield return null;
        } while(mLeaveCounter > 0 || (takeLeaveInd != -1 && animator.isPlaying));
    }

    IEnumerator DoFinish() {
        yield return null;
    }

    void OnSpawnEntity(M8.PoolDataController pdc) {
        if(CheckSpawn(pdc))
            mStageSpawnChecks.Add(pdc);
    }

    void OnDespawnEntity(M8.PoolDataController pdc) {
        if(CheckSpawn(pdc))
            mStageSpawnChecks.Remove(pdc);
    }
}
