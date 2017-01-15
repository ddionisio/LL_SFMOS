using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatEntityController : MonoBehaviour {
    public delegate void Callback(StatEntityController statCtrl, float prevValue);

    [SerializeField]
    StatEntity _data;

    public float HP { get { return _data.HP; } }
    public float stamina { get { return _data.stamina; } }

    public float currentHP {
        get { return mCurHP; }
        set {
            if(mCurHP != value) {
                var prev = mCurHP;
                mCurHP = value;

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
                mCurStamina = value;

                if(StaminaChangedCallback != null)
                    StaminaChangedCallback(this, prev);
            }
        }
    }

    public event Callback HPChangedCallback;
    public event Callback StaminaChangedCallback;

    private float mCurHP;
    private float mCurStamina;

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
