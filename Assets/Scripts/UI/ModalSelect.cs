using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalSelect : M8.UIModal.Controller, IPush, IPop {
    public const string parmIndex = "index";

    public GameObject[] selectGOs;

    public float autoCloseDelay = 1.0f;

    private int mCurInd;

    void Awake() {
        mCurInd = -1;

        for(int i = 0; i < selectGOs.Length; i++) {
            if(selectGOs[i])
                selectGOs[i].SetActive(false);
        }
    }

    void IPush.Push(M8.GenericParams parms) {
        mCurInd = parms.GetValue<int>(parmIndex);

        selectGOs[mCurInd].SetActive(true);
    }

    void IPop.Pop() {
        if(mCurInd >= 0) {
            selectGOs[mCurInd].SetActive(false);
            mCurInd = -1;
        }
    }

    public override void SetActive(bool aActive) {
        if(aActive)
            StartCoroutine(DoAutoClose());
        else
            StopAllCoroutines();
    }

    IEnumerator DoAutoClose() {
        yield return new WaitForSeconds(autoCloseDelay);

        Close();
    }
}