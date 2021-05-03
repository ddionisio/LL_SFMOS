using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorScreenTransition : MonoBehaviour, M8.SceneManager.ITransition {
    public M8.Animator.AnimatorData animator;

    public GameObject activeGO;

    public string takeOut;
    public string takeIn;

    private int mTakeOutInd;
    private int mTakeInInd;

    void OnDestroy() {
        if(M8.SceneManager.instance)
            M8.SceneManager.instance.RemoveTransition(this);
    }

    void Awake() {
        M8.SceneManager.instance.AddTransition(this);

        mTakeOutInd = animator.GetTakeIndex(takeOut);
        mTakeInInd = animator.GetTakeIndex(takeIn);

        activeGO.SetActive(false);
    }

    int M8.SceneManager.ITransition.priority { get { return 0; } }

    IEnumerator M8.SceneManager.ITransition.Out() {
        activeGO.SetActive(true);

        animator.Play(mTakeOutInd);
        while(animator.isPlaying)
            yield return null;
    }

    IEnumerator M8.SceneManager.ITransition.In() {
        animator.Play(mTakeInInd);
        while(animator.isPlaying)
            yield return null;
        
        activeGO.SetActive(false);

        animator.ResetTake(mTakeOutInd);
    }
}
