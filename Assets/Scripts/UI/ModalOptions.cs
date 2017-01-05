using System;
using System.Collections;
using System.Collections.Generic;
using M8.UIModal.Interface;
using UnityEngine;

public class ModalOptions : M8.UIModal.Controller, IPush, IPop {
    void IPush.Push(M8.GenericParams parms) {
        M8.SceneManager.instance.Pause();
    }

    void IPop.Pop() {
        M8.SceneManager.instance.Resume();
    }
}
