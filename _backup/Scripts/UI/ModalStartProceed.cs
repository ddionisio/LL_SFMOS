using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalStartProceed : M8.UIModal.Controller, IClose, IClosing, IPush {
    [Header("Animation")]
    public M8.Animator.AnimatorData animator;

    [Header("Proceed To Result")]
    public string takeResultProceedShow;
    public string takeResultProceedHide;

    public float resultProceedHideDelay;

    public M8.SceneAssetPath resultScene;

    public M8.SceneAssetPath playScene;

    public Button nopeButton;

    private Coroutine mRout;
    private bool mResultProceedActive;

    public void ClickPlay() {
        Close();
        playScene.Load();
    }

    public void ClickNope() {
        if(animator.isPlaying || mResultProceedActive)
            return;

        nopeButton.interactable = false;

        animator.Play(takeResultProceedShow);
    }

    public void ClickProceedResult() {
        if(animator.isPlaying || !mResultProceedActive)
            return;

        Close();
        resultScene.Load();
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimationComplete;
    }

    void Awake() {
        animator.takeCompleteCallback += OnAnimationComplete;
    }

    IEnumerator DoHideResultProceed() {
        yield return new WaitForSeconds(resultProceedHideDelay);

        animator.Play(takeResultProceedHide);

        mResultProceedActive = false;

        nopeButton.interactable = true;

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

    void IPush.Push(M8.GenericParams parms) {
        nopeButton.interactable = true;
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
