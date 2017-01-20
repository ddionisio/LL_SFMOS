using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatEntityController : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public delegate void Callback(StatEntityController statCtrl, float prevValue);
    public delegate void CallbackSignal(StatEntityController statCtrl, GameObject sender, int signal, object data);

    [SerializeField]
    StatEntity _data;

    public StatEntity data { get { return _data; } }

    public float currentHP {
        get { return mCurHP; }
        set {
            if(mCurHP != value) {
                var prev = mCurHP;
                mCurHP = Mathf.Clamp(value, 0f, _data.HP);

                if(HPChangedCallback != null)
                    HPChangedCallback(this, prev);
            }
        }
    }

    public float currentStamina {
        get { return mCurStamina; }
        set {
            if(mCurStamina != value) {
                var prev = mCurStamina;
                mCurStamina = Mathf.Clamp(value, 0f, _data.stamina);

                if(staminaChangedCallback != null)
                    staminaChangedCallback(this, prev);
            }
        }
    }

    public bool isAlive {
        get { return mActive && mCurHP > 0f; }
    }
    
    public event Callback HPChangedCallback;
    public event Callback staminaChangedCallback;
    public event CallbackSignal signalCallback;

    private float mCurHP;
    private float mCurStamina;

    private bool mActive;
        
    public void Reset() {
        if(!_data)
            return;

        mCurHP = _data.HP;
        mCurStamina = _data.stamina;
    }

    public void SendSignal(GameObject sender, int signal, object data) {
        if(signalCallback != null)
            signalCallback(this, sender, signal, data);
    }

    void Awake() {
        Reset();
    }

    void Start() {
        mActive = true;
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        mActive = true;
    }

    void M8.IPoolDespawn.OnDespawned() {
        mActive = false;
    }
}
