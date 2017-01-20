using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : M8.SingletonBehaviour<MissionController> {
    public enum SignalType {
        Waiting,
        Proceed,
    }

    public delegate void OnValueChangeCallback(int cur, int prev);
    
    public event OnValueChangeCallback scoreChangeCallback;
    public event System.Action<SignalType, int> signalCallback; //listen to signals from mission control

    private int mCurScore;
    private M8.StatsController mStats;
        
    public int score {
        get { return mCurScore; }

        set {
            if(mCurScore != value) {
                var prev = mCurScore;
                mCurScore = value;

                if(scoreChangeCallback != null)
                    scoreChangeCallback(mCurScore, prev);
            }
        }
    }

    public M8.StatsController stats { get { return mStats; } }

    public void ProcessVictory() {
        MissionManager.instance.Complete(mCurScore);

        MissionManager.instance.Victory();
    }

    public void ProcessLose() {
        M8.UIModal.Manager.instance.ModalOpen(ModalLose.modalRef);
    }

    public virtual void Signal(SignalType signal, int counter) {

    }

    public virtual Transform RequestTarget(Transform requestor) {
        return null;
    }

    protected void SendSignal(SignalType signal, int counter) {
        if(signalCallback != null)
            signalCallback(signal, counter);
    }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        mStats = GetComponent<M8.StatsController>();
    }
}
