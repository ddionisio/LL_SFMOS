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

        var parmRefs = parms.GetValue<string[]>(parmStringRefs);

        List<string> parsedRefs = new List<string>();

        for(int i = 0; i < parmRefs.Length; i++) {
            var parsed = Utility.GrabLocalizeGroup(parmRefs[i]);
            for(int j = 0; j < parsed.Length; j++)
                parsedRefs.Add(parsed[j]);
        }

        mStringRefs = parsedRefs.ToArray();
        mCurStringRefInd = 0;
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        //start typin'
        if(aActive)
            ApplyCurrentString();
    }
}
