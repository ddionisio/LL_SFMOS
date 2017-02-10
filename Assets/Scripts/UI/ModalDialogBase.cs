using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalDialogBase : M8.UIModal.Controller, IPop, IPush {
    public TextTypewriter textTypewriter;
    public GameObject readyGO;

    public bool pause;

    private bool mIsPaused;

    public void Click() {
        if(textTypewriter.isTyping) {
            textTypewriter.Skip();
            return;
        }
        else if(Next()) {
            readyGO.SetActive(false);
            return;
        }

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

        if(pause) {
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
    }
    
    void OnTypewriterFinish() {
        readyGO.SetActive(true);
    }
}
