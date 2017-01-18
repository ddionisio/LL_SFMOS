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
    }
    
    [Header("Mucus Gather")]
    public MucusGatherInputField mucusGatherInput;
    public MucusGather mucusGather;

    public Transform pointer;
    public GameObject pointerGO;

    public Bounds mucusFormBounds;

    [Header("Health")]
    public EntityCell[] cellWalls; //when all these die, game over, man
    
    [Header("Stage Data")]
    public M8.Animator.AnimatorData animator;

    public float beginDelay = 1f;

    public string takeBeginIntro;
    public string takeBeginOutro;

    public string takeStageTransition; //do some woosh thing towards left

    public string takeDefeat;
    public string takeVictory;

    public StageData[] stages; //determines stage and sub stages

    public float stagePlayDuration = 60; //in seconds
    public float stageNextDelay = 3; //how long before the next stage transition

    private bool mIsPointerActive;

    private int mCurStageInd;

    private State mCurState = State.None;
    private Coroutine mRout;

    private bool mIsWaiting;

    public bool isWaiting { get { return mIsWaiting; } }
        
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
                cellWalls[i].stats.HPChangedCallback -= OnCellWallHPChanged;
        }
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        mucusGatherInput.pointerDownCallback += OnMucusFieldInputDown;
        mucusGatherInput.pointerDragCallback += OnMucusFieldInputDrag;
        mucusGatherInput.pointerUpCallback += OnMucusFieldInputUp;
        mucusGatherInput.lockChangeCallback += OnMucusFieldInputLockChange;

        mIsPointerActive = false;

        if(pointerGO)
            pointerGO.SetActive(false);

        if(pointer)
            pointer.gameObject.SetActive(false);

        for(int i = 0; i < cellWalls.Length; i++) {
            if(cellWalls[i])
                cellWalls[i].stats.HPChangedCallback += OnCellWallHPChanged;
        }
    }

    IEnumerator Start() {
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

    /// <summary>
    /// Call this when you want to progress in current state, when it is waiting
    /// </summary>
    public void Progress() {
        mIsWaiting = false;
    }

    public void EnterStage(int stage) {
        mCurStageInd = stage;

        ApplyState(State.StageTransition);
    }

    void ApplyState(State state) {
        if(mCurState != state) {
            var prevState = mCurState;
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
        mucusGatherInput.isLocked = true;

        //small intro to show stuff
        if(!string.IsNullOrEmpty(takeBeginIntro)) {
            animator.Play(takeBeginIntro);

            while(animator.isPlaying)
                yield return null;
        }

        mucusGatherInput.isLocked = false;

        //free form, wait for signal via call to Progress
        mIsWaiting = true;

        while(mIsWaiting)
            yield return null;

        mucusGatherInput.isLocked = true;

        if(!string.IsNullOrEmpty(takeBeginOutro)) {
            animator.Play(takeBeginOutro);

            while(animator.isPlaying)
                yield return null;
        }

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

        var curStage = stages[mCurStageInd];

        var startTime = Time.time;

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

        //left over wait to get to new stage
        while(Time.time - startTime < stagePlayDuration)
            yield return null;

        //move current entities to the left?

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

        if(!string.IsNullOrEmpty(takeVictory)) {
            animator.Play(takeVictory);

            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        ProcessVictory();
    }

    IEnumerator DoDefeat() {
        mucusGatherInput.isLocked = true;

        if(!string.IsNullOrEmpty(takeDefeat)) {
            animator.Play(takeDefeat);

            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        ProcessLose();
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

            bool pointerActive = !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

            SetPointerActive(pointerActive);

            if(pointerActive) {
                pointer.position = new Vector3(pos.x, pos.y, pointer.position.z);
            }
        }
    }

    void OnMucusFieldInputUp(MucusGatherInputField input) {
        SetPointerActive(false);

        if(mucusGather.isActive) {
            Vector2 pos = input.currentPosition;

            bool pointerActive = !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

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

    void OnCellWallHPChanged(StatEntityController statCtrl, float prev) {
        //check if they are all dead
        int numDead = 0;
        for(int i = 0; i < cellWalls.Length; i++) {
            if(cellWalls[i]) {
                if(cellWalls[i].stats.currentHP <= 0f)
                    numDead++;
            }
            else
                numDead++;
        }

        if(numDead >= cellWalls.Length)
            ApplyState(State.Defeat);
    }

    void OnDrawGizmos() {

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(mucusFormBounds.center, mucusFormBounds.size);
    }
}
