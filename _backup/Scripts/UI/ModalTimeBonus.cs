using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalTimeBonus : M8.UIModal.Controller, IPush, IPop {
    public const string parmTime = "time";
    public const string parmMult = "scoreMult";

    public Text timeText;
    public Text scoreMultText;
    public Text scoreResultText;

    public float processSpeed = 10f; //time per second

    public float closeAutoDelay = 1.5f; //how long before closing after process finished

    private float mCurTime;
    private float mTotalTime;

    private float mScoreMult;
    
    private float mTotalScore;

    private Coroutine mRout;

    public void Click() {
        if(M8.UIModal.Manager.instance.isBusy)
            return;

        if(mCurTime > 0f)
            mCurTime = 0f;
        else {
            EndProcess();

            Close();
        }
    }
        
    void IPush.Push(M8.GenericParams parms) {
        //throw new NotImplementedException();
        mTotalTime = parms.GetValue<float>(parmTime);
        mScoreMult = parms.GetValue<float>(parmMult);

        mTotalScore = mTotalTime*mScoreMult;

        timeText.text = Utility.TimerFormat(mTotalTime);
        scoreMultText.text = "x"+mScoreMult;
        scoreResultText.text = "+0";
    }

    void IPop.Pop() {
        //throw new NotImplementedException();
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            mRout = StartCoroutine(DoProcess());
        }
        else {
            EndProcess();
        }
    }

    void EndProcess() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoProcess() {
        if(mTotalTime > 0f) {
            mCurTime = mTotalTime;

            while(mCurTime > 0f) {
                yield return null;

                mCurTime -= processSpeed*Time.deltaTime;
                if(mCurTime < 0f)
                    mCurTime = 0f;

                timeText.text = Utility.TimerFormat(mCurTime);

                int score = Mathf.RoundToInt(Mathf.Lerp(0, mTotalScore, 1.0f - mCurTime/mTotalTime));

                scoreResultText.text = "+"+score;
            }
        }

        timeText.text = "0.00";
        scoreResultText.text = "+"+Mathf.RoundToInt(mTotalScore);

        yield return new WaitForSeconds(closeAutoDelay);
        
        mRout = null;

        Close();
    }
}
