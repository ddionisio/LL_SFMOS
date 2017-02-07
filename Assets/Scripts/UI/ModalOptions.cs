using System;
using System.Collections;
using System.Collections.Generic;
using M8.UIModal.Interface;
using UnityEngine;
using UnityEngine.UI;

public class ModalOptions : M8.UIModal.Controller, IActive, IPush, IPop {
    public Slider musicSlider;
    public Slider soundSlider;

    private bool mPaused;

    void Awake() {
        musicSlider.onValueChanged.AddListener(OnMusicSliderValue);
        soundSlider.onValueChanged.AddListener(OnSoundSliderValue);
    }

    void IActive.SetActive(bool aActive) {
        if(aActive) {
            musicSlider.value = LoLManager.instance.musicVolume;
            soundSlider.value = LoLManager.instance.soundVolume;
        }
    }

    void IPush.Push(M8.GenericParams parms) {
        if(!mPaused) {
            mPaused = true;
            M8.SceneManager.instance.Pause();            
        }

        //check params for mode: none (default), mission select (show "quit to title"), game (show "restart", "return to mission select")
    }
        
    void IPop.Pop() {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, true);

        if(mPaused) {
            mPaused = false;
            M8.SceneManager.instance.Resume();
        }
    }

    void OnMusicSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, false);
    }

    void OnSoundSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.value, musicSlider.value, false);
    }
}
