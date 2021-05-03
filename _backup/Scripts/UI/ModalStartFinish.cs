using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalStartFinish : M8.UIModal.Controller, IPush, IPop, IClose {
    public const string parmMaxScore = "maxScore";

    public Text scoreLabel;
    [M8.Localize]
    public string scoreStringRef;

    public M8.Animator.AnimatorData animator;
    public string takeResultShow;
    public string takeResultHide;

    public GameObject resultsGO;

    private bool mIsResultShow;

    public void ClickResults() {
        if(M8.UIModal.Manager.instance.isBusy || animator.isPlaying)
            return;

        if(mIsResultShow)
            animator.Play(takeResultHide);
        else
            animator.Play(takeResultShow);

        mIsResultShow = !mIsResultShow;
    }

    public void ClickComplete() {
        if(M8.UIModal.Manager.instance.isBusy)
            return;

        LoLManager.instance.Complete();
        Application.Quit();
    }

    void IPush.Push(M8.GenericParams parms) {
        int maxScore = parms.GetValue<int>(parmMaxScore);
        scoreLabel.text = string.Format(M8.Localize.Get(scoreStringRef), M8.SceneState.instance.global.GetValue(SceneStateVars.curScore), maxScore);
    }

    void IPop.Pop() {
    }

    void IClose.Close() {
        resultsGO.SetActive(false);
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimationComplete;
    }

    void Awake() {
        animator.takeCompleteCallback += OnAnimationComplete;
    }

    void OnAnimationComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {

    }
}
