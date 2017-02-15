using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using M8.UIModal.Interface;

public class ModalLose : M8.UIModal.Controller, IClose, IPush, IPop {
    public const string modalRef = "lose";

    public GameObject resultProceedButtonGO;
    public Button giveUpButton;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;

    [Header("Proceed To Result")]
    public string takeResultProceedShow;
    public string takeResultProceedHide;

    public float resultProceedHideDelay;

    public M8.SceneAssetPath resultScene;

    private Coroutine mRout;
    private bool mResultProceedActive;

    public void Retry() {
        MissionController.instance.Retry();
    }

    public void Quit() {
        if(animator.isPlaying || mResultProceedActive)
            return;

        giveUpButton.interactable = false;

        resultProceedButtonGO.SetActive(true);

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

        giveUpButton.interactable = true;

        mRout = null;
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimationComplete;
    }

    void Awake() {
        animator.takeCompleteCallback += OnAnimationComplete;
    }

    void IClose.Close() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        resultProceedButtonGO.SetActive(false);
    }
    
    void IPush.Push(M8.GenericParams parms) {
        resultProceedButtonGO.SetActive(false);
        giveUpButton.interactable = true;

        ButtonOptions.instance.showGiveUp = false;
    }

    void IPop.Pop() {
        ButtonOptions.instance.showGiveUp = true;
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
