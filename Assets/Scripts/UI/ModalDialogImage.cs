using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalDialogImage : ModalDialogBase {
    public const string parmPairRefs = "strImgPairs";

    [System.Serializable]
    public struct PairRef {
        public Sprite sprite;
        [M8.Localize]
        public string stringRef;
    }

    public Image imageDisplay;
    
    private PairRef[] mRefs;
    private int mCurRefInd;

    private bool mIsImageShown;
    
    protected override bool Next() {
        if(mCurRefInd < mRefs.Length - 1) {
            ApplyCurrent();
            return true;
        }

        return false;
    }
    
    void ApplyCurrent() {
        if(mRefs[mCurRefInd].sprite) {
            if(!mIsImageShown) {
                imageDisplay.gameObject.SetActive(true);
                mIsImageShown = true;
            }

            imageDisplay.sprite = mRefs[mCurRefInd].sprite;
            imageDisplay.SetNativeSize();
        }

        textTypewriter.SetText(M8.Localize.Get(mRefs[mCurRefInd].stringRef));
    }

    public override void Push(M8.GenericParams parms) {
        base.Push(parms);

        mRefs = parms.GetValue<PairRef[]>(parmPairRefs);
        
        mCurRefInd = 0;

        imageDisplay.gameObject.SetActive(false);
        mIsImageShown = false;        
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        //start typin'
        if(aActive)
            ApplyCurrent();
    }
}
