using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalDialog : ModalDialogBase {
    public const string parmStringRefs = "strRefs";
        
    private string[] mStringRefs;
    private int mCurStringRefInd;

    protected override bool Next() {
        if(mCurStringRefInd < mStringRefs.Length - 1) {
            mCurStringRefInd++;
            ApplyCurrentString();
            return true;
        }

        return false;
    }
    
    void ApplyCurrentString() {
        textTypewriter.SetText(M8.Localize.Get(mStringRefs[mCurStringRefInd]));
    }

    public override void Push(M8.GenericParams parms) {
        base.Push(parms);

        mStringRefs = parms.GetValue<string[]>(parmStringRefs);
        
        mCurStringRefInd = 0;
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        //start typin'
        if(aActive)
            ApplyCurrentString();
    }
}
