using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CameraControllerToCanvas : MonoBehaviour {
    private Canvas mCanvas;

    void Awake() {
        mCanvas = GetComponent<Canvas>();

        if(mCanvas.worldCamera != CameraController.instance.activeCamera)
            mCanvas.worldCamera = CameraController.instance.activeCamera;
    }
}
