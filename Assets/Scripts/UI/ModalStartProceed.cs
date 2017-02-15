using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalStartProceed : M8.UIModal.Controller, IClose, IClosing {
    [Header("Animation")]
    public M8.Animator.AnimatorData animator;

    [Header("Proceed To Result")]
    public string takeResultProceedShow;
    public string takeResultProceedHide;

    public float resultProceedHideDelay;

    public M8.SceneAssetPath resultScene;

    public M8.SceneAssetPath playScene;

    private Coroutine mRout;
    private bool mResultProceedActive;

    public void ClickPlay() {
        Close();
        playScene.Load();
    }

    public void ClickNope() {
        if(animator.isPlaying || mResultProceedActive)
            return;
                
        animator.Play(takeResultProceedShow);        
    }

    public void ClickProceedResult() {
        if(animator.isPlaying || !mResultProceedActive)
            return;

        Close();
        resultScene.Load();
    }

    IEnumerator DoHideResultProceed() {
        yield return new WaitForSeconds(resultProceedHideDelay);

        animator.Play(takeResultProceedHide);

        mResultProceedActive = false;

        mRout = null;
    }
    
    void IClose.Close() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mResultProceedActive) {
            animator.Play(takeResultProceedHide);
            mResultProceedActive = false;
        }
    }

    IEnumerator IClosing.Closing() {
        while(animator.isPlaying)
            yield return null;
    }

    void OnAnimationComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takeResultProceedShow) {
            mResultProceedActive = true;

            if(mRout != null)
                StopCoroutine(mRout);

            mRout = StartCoroutine(DoHideResultProceed());
        }
    }
}
