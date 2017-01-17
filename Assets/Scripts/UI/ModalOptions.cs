using System;
using System.Collections;
using System.Collections.Generic;
using M8.UIModal.Interface;
using UnityEngine;

public class ModalOptions : M8.UIModal.Controller, IPush, IPop {
    private bool mPaused;

    void IPush.Push(M8.GenericParams parms) {
        if(!mPaused) {
            mPaused = true;
            M8.SceneManager.instance.Pause();            
        }

        //check params for mode: none (default), mission select (show "quit to title"), game (show "restart", "return to mission select")
    }

    void IPop.Pop() {
        if(mPaused) {
            mPaused = false;
            M8.SceneManager.instance.Resume();
        }
    }
}
