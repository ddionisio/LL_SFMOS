using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalDialogBase : M8.UIModal.Controller, IPop, IPush {
    public const string parmActionFinish = "cb";
    public const string parmPauseOverride = "pause";

    public TextTypewriter textTypewriter;
    public GameObject readyGO;

    public bool pause;
    public bool closeOnFinish = true;

    private bool mIsPaused;

    private System.Action mOnFinish;
    private bool mIsFinish;

    public void Click() {
        if(mIsFinish || M8.UIModal.Manager.instance.isBusy)
            return;

        if(textTypewriter.isTyping) {
            textTypewriter.Skip();
            return;
        }
        else if(Next()) {
            readyGO.SetActive(false);
            return;
        }

        mIsFinish = true;

        if(closeOnFinish)
            Close();
    }

    /// <summary>
    /// If returns true, don't close yet
    /// </summary>
    /// <returns></returns>
    protected virtual bool Next() {
        return true;
    }

    protected virtual void OnDestroy() {
        if(textTypewriter)
            textTypewriter.doneCallback -= OnTypewriterFinish;

        if(mIsPaused) {
            if(M8.SceneManager.instance)
                M8.SceneManager.instance.Resume();
            mIsPaused = false;
        }
    }

    protected virtual void Awake() {
        textTypewriter.doneCallback += OnTypewriterFinish;
    }

    public virtual void Push(M8.GenericParams parms) {
        readyGO.SetActive(false);

        mOnFinish = parms.GetValue<System.Action>(parmActionFinish);

        mIsFinish = false;

        bool doPause = pause;

        if(parms.ContainsKey(parmPauseOverride))
            doPause = parms.GetValue<bool>(parmPauseOverride);

        if(doPause) {
            if(!mIsPaused) {
                M8.SceneManager.instance.Pause();
                mIsPaused = true;
            }
        }
    }

    public virtual void Pop() {
        if(mIsPaused) {
            M8.SceneManager.instance.Resume();
            mIsPaused = false;
        }

        if(mOnFinish != null) {
            var call = mOnFinish;
            mOnFinish = null;

            call();
        }
    }
    
    void OnTypewriterFinish() {
        readyGO.SetActive(true);
    }
}
