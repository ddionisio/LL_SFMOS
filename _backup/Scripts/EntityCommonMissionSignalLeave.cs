using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allow for leaving when mission signals Leave
/// </summary>
public class EntityCommonMissionSignalLeave : MonoBehaviour {
    private EntityCommon mEntity;
    private bool mIsLeaving;

    void OnDestroy() {
        SetLeaving(false);

        if(mEntity) {
            mEntity.setStateCallback -= OnEntityChangeState;
            mEntity.releaseCallback -= OnEntityRelease;
        }

        if(MissionController.instance)
            MissionController.instance.signalCallback -= OnMissionControlSignal;
    }

    void Awake() {
        mEntity = GetComponent<EntityCommon>();
        mEntity.setStateCallback += OnEntityChangeState;
        mEntity.releaseCallback += OnEntityRelease;

        MissionController.instance.signalCallback += OnMissionControlSignal;
    }

    void SetLeaving(bool isLeaving) {
        if(mIsLeaving != isLeaving) {
            mIsLeaving = isLeaving;

            if(mIsLeaving) {
                if(MissionController.instance)
                    MissionController.instance.Signal(MissionController.SignalType.LeaveBegin, null);
            }
            else {
                if(MissionController.instance)
                    MissionController.instance.Signal(MissionController.SignalType.LeaveEnd, null);
            }
        }
    }

    void OnMissionControlSignal(MissionController.SignalType signal, object parm) {
        switch(signal) {
            case MissionController.SignalType.Leave:
                //make sure it's not released
                if(!mEntity.isReleased)
                    mEntity.Leave(parm as Transform);
                break;
        }
    }
    
    void OnEntityChangeState(M8.EntityBase ent) {
        switch((EntityState)ent.prevState) {
            case EntityState.Leave:
                SetLeaving(false);
                break;
        }

        switch((EntityState)ent.state) {
            case EntityState.Leave:
                SetLeaving(true);
                break;
        }
    }

    void OnEntityRelease(M8.EntityBase ent) {
        SetLeaving(false);
    }
}
