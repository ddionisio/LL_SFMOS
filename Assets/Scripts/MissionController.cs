using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : M8.SingletonBehaviour<MissionController> {

    public delegate void OnValueChangeCallback(int prev, int cur);

    public event OnValueChangeCallback scoreChangeCallback;

    private int mCurScore;

    public int score {
        get { return mCurScore; }

        set {
            if(mCurScore != value) {
                var prev = mCurScore;
                mCurScore = value;

                if(scoreChangeCallback != null)
                    scoreChangeCallback(prev, mCurScore);
            }
        }
    }


    public void ProcessVictory() {
        MissionManager.instance.Complete(mCurScore);

        MissionManager.instance.Victory();
    }

    public void ProcessLose() {
        M8.UIModal.Manager.instance.ModalOpen(ModalLose.modalRef);
    }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        
    }
}
