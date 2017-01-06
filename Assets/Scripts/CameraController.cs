using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : M8.SingletonBehaviour<CameraController> {
    [SerializeField]
    Camera _camera;

    public Camera activeCamera { get { return _camera; } }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        
    }
}
