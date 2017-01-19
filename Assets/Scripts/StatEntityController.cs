using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatEntityController : MonoBehaviour {
    public delegate void Callback(StatEntityController statCtrl, float prevValue);

    [SerializeField]
    StatEntity _data;

    public float HP { get { return _data.HP; } }
    public float stamina { get { return _data.stamina; } }

    public float attack { get { return _data.attack; } }
    public float attackPerSecond { get { return _data.attackPerSecond; } }

    public float attackStamina { get { return _data.attackStamina; } }
    public float attackStaminaPerSecond { get { return _data.attackStaminaPerSecond; } }

    public float seekDelay { get { return _data.seekDelay; } }
    public string seekTag { get { return _data.seekTag; } }

    public float currentHP {
        get { return mCurHP; }
        set {
            if(mCurHP != value) {
                var prev = mCurHP;
                mCurHP = Mathf.Clamp(value, 0f, HP);

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
                mCurStamina = Mathf.Clamp(value, 0f, stamina);

                if(StaminaChangedCallback != null)
                    StaminaChangedCallback(this, prev);
            }
        }
    }

    public event Callback HPChangedCallback;
    public event Callback StaminaChangedCallback;

    private float mCurHP;
    private float mCurStamina;

    public float GetDamage(float time) {
        return _data.attack*_data.attackPerSecond*time;
    }

    public float GetDamageStamina(float time) {
        return _data.attackStamina*_data.attackStaminaPerSecond*time;
    }

    public void Reset() {
        if(!_data)
            return;

        mCurHP = _data.HP;
        mCurStamina = _data.stamina;
    }

    void Awake() {
        Reset();
    }
}
