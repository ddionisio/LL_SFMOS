using System;
using System.Collections;
using System.Collections.Generic;
using M8.UIModal.Interface;
using UnityEngine;
using UnityEngine.UI;

public class ModalOptions : M8.UIModal.Controller, IClose, IPush, IPop {
    public const string parmShowGiveUp = "showGiveUp";

    public Slider musicSlider;
    public Slider soundSlider;

    public GameObject giveUpGO;
    public Button giveUpButton;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;

    [Header("Proceed To Result")]
    public string takeResultProceedShow;
    public string takeResultProceedHide;

    public float resultProceedHideDelay;

    public M8.SceneAssetPath resultScene;

    private bool mPaused;

    private Coroutine mRout;
    private bool mResultProceedActive;

    public void ClickNope() {
        if(animator.isPlaying || mResultProceedActive)
            return;

        giveUpButton.interactable = false;

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
        musicSlider.onValueChanged.AddListener(OnMusicSliderValue);
        soundSlider.onValueChanged.AddListener(OnSoundSliderValue);

        animator.takeCompleteCallback += OnAnimationComplete;
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            musicSlider.value = LoLManager.instance.musicVolume;
            soundSlider.value = LoLManager.instance.soundVolume;
        }
    }

    void IClose.Close() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        giveUpGO.SetActive(false);
    }
    
    void IPush.Push(M8.GenericParams parms) {
        if(!mPaused) {
            mPaused = true;
            M8.SceneManager.instance.Pause();            
        }

        bool showGiveUp = false;

        if(parms != null) {
            if(parms.ContainsKey(parmShowGiveUp))
                showGiveUp = parms.GetValue<bool>(parmShowGiveUp);
        }

        //show give up?
        if(showGiveUp) {
            giveUpGO.SetActive(true);
            giveUpButton.interactable = true;
        }
        else
            giveUpGO.SetActive(false);
    }
        
    void IPop.Pop() {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, true);

        if(mPaused) {
            mPaused = false;
            M8.SceneManager.instance.Resume();
        }

        giveUpGO.SetActive(false);
    }

    void OnMusicSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, false);
    }

    void OnSoundSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, false);
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
