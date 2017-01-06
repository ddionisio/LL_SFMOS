using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M8.UIModal.Interface;

public class ModalLose : M8.UIModal.Controller, IPush, IPop {
    public const string modalRef = "lose";

    public M8.SceneAssetPath quitScene;

    public void Retry() {
        MissionManager.instance.Play();
    }

    public void Quit() {
        M8.SceneManager.instance.LoadScene(quitScene.name);
    }

    void IPush.Push(M8.GenericParams parms) {
    }

    void IPop.Pop() {
    }
}
