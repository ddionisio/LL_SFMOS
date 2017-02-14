using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalDialogComposite : ModalDialogBase {
    public const string parmPairRefs = "strCompPairs";

    [System.Serializable]
    public struct PairRef {
        [M8.Localize]
        public string stringRef;
        public bool clear;
        public string compositeName;        
    }

    public GameObject[] composites; //use name as reference
    
    private PairRef[] mRefs;
    private int mCurRefInd;
    
    private Dictionary<string, GameObject> mComposites;
    private GameObject mCurComposite;

    protected override bool Next() {
        if(mCurRefInd < mRefs.Length - 1) {
            mCurRefInd++;
            ApplyCurrent();
            return true;
        }

        return false;
    }
    
    protected override void Awake() {
        base.Awake();

        mComposites = new Dictionary<string, GameObject>();
        for(int i = 0; i < composites.Length; i++) {
            if(composites[i]) {
                mComposites.Add(composites[i].name, composites[i]);
                composites[i].SetActive(false);
            }
        }
    }
    
    void ApplyCurrent() {        
        if(mRefs[mCurRefInd].clear) {
            if(mCurComposite) {
                mCurComposite.SetActive(false);
                mCurComposite = null;
            }
        }
        else {
            var compName = mRefs[mCurRefInd].compositeName;

            if(!string.IsNullOrEmpty(compName)) {
                GameObject go;
                if(mComposites.TryGetValue(compName, out go) && go) {
                    if(mCurComposite)
                        mCurComposite.SetActive(false);

                    go.SetActive(true);

                    mCurComposite = go;
                }
            }
        }

        textTypewriter.SetText(M8.Localize.Get(mRefs[mCurRefInd].stringRef));
    }

    public override void Push(M8.GenericParams parms) {
        base.Push(parms);

        mRefs = parms.GetValue<PairRef[]>(parmPairRefs);

        var parmPairs = parms.GetValue<PairRef[]>(parmPairRefs);

        List<PairRef> parsedRefs = new List<PairRef>();

        for(int i = 0; i < parmPairs.Length; i++) {
            var parsed = Utility.GrabLocalizeGroup(parmPairs[i].stringRef);
            for(int j = 0; j < parsed.Length; j++) {
                var newRef = new PairRef() { compositeName = parmPairs[i].compositeName, clear = parmPairs[i].clear, stringRef = parsed[j] };
                parsedRefs.Add(newRef);
            }
        }

        mRefs = parsedRefs.ToArray();
        mCurRefInd = 0;
    }

    public override void Pop() {
        base.Pop();

        if(mCurComposite) {
            mCurComposite.SetActive(false);
            mCurComposite = null;
        }
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        //start typin'
        if(aActive)
            ApplyCurrent();
    }
}
