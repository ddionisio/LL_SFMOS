using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : M8.SingletonBehaviour<MissionController> {
    public enum SignalType {
        Waiting, //tell listeners the mission is waiting (waiting for Proceed for most cases)

        Proceed, //tell mission to proceed (used during begin)

        NewStage, //tell listeners that a new stage is starting

        Complete, //tell listeners the mission is over

        Defeat, //tell listeners we are defeated

        Leave, //tell listeners to leave, responders are expected to signal back LeaveBegin on the same frame

        LeaveBegin, //tells mission it is leaving
        LeaveEnd, //tells mission it has left

        EnemyRegister, //parms = M8.EntityBase, for mission to check for enemy count before stage can proceed
        EnemyUnregister, //parms = M8.EntityBase, make sure to call this during release, so stage can proceed
    }

    public delegate void OnValueChangeCallback(int cur, int prev);
    public delegate void OnValueAmountAtCallback(Vector2 worldPos, int amount);

    public virtual int missionIndex { get { return -1; } }

    public int score {
        get { return mCurScore; }

        set {
            if(mCurScore != value) {
                var prev = mCurScore;
                mCurScore = value;

                HUD.instance.UpdateScore(mCurScore, prev);
            }
        }
    }

    public M8.StatsController stats { get { return mStats; } }

    public bool inputLock {
        get { return mInputLock; }
        set {
            if(mInputLock != value) {
                mInputLock = value;
                SetInputLock(mInputLock);
            }
        }
    }

    public virtual bool isStageTimePause {
        get { return false; }
        set {  }
    }

    public virtual int enemyCount {
        get { return 0; }
    }

    public event OnValueAmountAtCallback scoreAtCallback;
    public event System.Action<SignalType, object> signalCallback; //listen to signals from mission control

    private int mCurScore;
    private M8.StatsController mStats;
    private bool mInputLock;
        
    public void ScoreAt(Vector2 worldPos, int scoreAmt) {
        score += scoreAmt;

        if(scoreAtCallback != null)
            scoreAtCallback(worldPos, scoreAmt);
    }

    public void ProcessVictory() {
        MissionManager.instance.Complete(mCurScore);

        MissionManager.instance.Quiz();
    }

    public void ProcessLose() {
        M8.UIModal.Manager.instance.ModalOpen(ModalLose.modalRef);
    }

    public virtual void ProcessKill(Collider2D victimColl, StatEntityController victimStatCtrl, int score) {
        //default behaviour
        ScoreAt(victimStatCtrl.transform.position, score);
    }

    public virtual void Signal(SignalType signal, object parms) {

    }

    public virtual Transform RequestTarget(Transform requestor) {
        return null;
    }

    protected virtual void SetInputLock(bool aLock) {

    }

    protected void SendSignal(SignalType signal, object parms) {
        if(signalCallback != null)
            signalCallback(signal, parms);
    }

    protected override void OnInstanceDeinit() {
    }

    protected override void OnInstanceInit() {
        //for debug purpose to play mission scenes directly
        MissionManager.instance.SetMission(missionIndex);

        mStats = GetComponent<M8.StatsController>();
    }
}
