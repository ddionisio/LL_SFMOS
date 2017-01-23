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
            var newVal = Mathf.Clamp(value, 0f, _data.HP);

            if(mCurHP != newVal) {
                var prev = mCurHP;
                mCurHP = newVal;

                if(HPChangedCallback != null)
                    HPChangedCallback(this, prev);
            }
        }
    }

    public float currentStamina {
        get { return mCurStamina; }
        set {
            var newVal = Mathf.Clamp(value, 0f, _data.stamina);

            if(mCurStamina != newVal) {
                var prev = mCurStamina;
                mCurStamina = newVal;

                if(staminaChangedCallback != null)
                    staminaChangedCallback(this, prev);
            }
        }
    }
    
    /// <summary>
    /// Use this instead of checking HP to check if entity is actually alive
    /// </summary>
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

        Reset();
    }

    void OnDrawGizmos() {
        if(data == null)
            return;

        if(data.splitSpawnRadius > 0f) {
            Gizmos.color = Color.yellow;
            Gizmos.color *= 0.3f;
            Gizmos.DrawWireSphere(transform.position, data.splitSpawnRadius);
        }

        /*if(data.roamRadius > 0f) {
            Gizmos.color = Color.magenta;
            Gizmos.color *= 0.3f;
            Gizmos.DrawWireSphere(transform.position, data.roamRadius);
        }*/
    }
}
