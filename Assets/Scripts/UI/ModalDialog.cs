using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalDialog : M8.UIModal.Controller, IPush, IActive {
    public const string parmStringRefs = "strRefs";

    public TextTypewriter textTypewriter;
    public GameObject readyGO;

    private string[] mStringRefs;
    private int mCurStringRefInd;

    public void Click() {
        if(textTypewriter.isTyping) {
            textTypewriter.Skip();
            return;
        }
        else if(mCurStringRefInd < mStringRefs.Length - 1) {
            mCurStringRefInd++;
            ApplyCurrentString();
            return;
        }

        M8.UIModal.Manager.instance.ModalCloseUpTo(name, true);
    }

    void OnDestroy() {
        if(textTypewriter)
            textTypewriter.doneCallback -= OnTypewriterFinish;
    }

    void Awake() {
        textTypewriter.doneCallback += OnTypewriterFinish;
    }

    void ApplyCurrentString() {
        readyGO.SetActive(false);

        textTypewriter.SetText(M8.Localize.Get(mStringRefs[mCurStringRefInd]));
    }

    void IPush.Push(M8.GenericParams parms) {
        mStringRefs = parms.GetValue<string[]>(parmStringRefs);

        readyGO.SetActive(false);

        mCurStringRefInd = 0;
    }

    void IActive.SetActive(bool aActive) {
        //start typin'
        if(aActive)
            ApplyCurrentString();
    }

    void OnTypewriterFinish() {
        readyGO.SetActive(true);
    }
}
