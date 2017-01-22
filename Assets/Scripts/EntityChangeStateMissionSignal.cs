using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityChangeStateMissionSignal : MonoBehaviour {
    [Header("State")]
    public EntityState state;
    public bool once;

    [Header("Signal")]
    public MissionController.SignalType signal;
    public int counter;

    private M8.EntityBase mEnt;

    void OnDestroy() {
        if(mEnt)
            mEnt.setStateCallback -= OnEntityStateChanged;
    }

    void Awake() {
        mEnt = GetComponent<M8.EntityBase>();
        if(mEnt)
            mEnt.setStateCallback += OnEntityStateChanged;
    }

    void OnEntityStateChanged(M8.EntityBase ent) {
        if(mEnt.state == (int)state) {
            MissionController.instance.Signal(signal, counter);

            if(once) {
                mEnt.setStateCallback -= OnEntityStateChanged;
                mEnt = null;
            }
        }
    }
}
