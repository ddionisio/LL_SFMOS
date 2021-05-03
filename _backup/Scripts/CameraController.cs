using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : M8.SingletonBehaviour<CameraController> {
    [SerializeField]
    Camera _camera;
    [SerializeField]
    M8.Auxiliary.AuxTrigger2D _cameraTrigger;

    public Camera activeCamera { get { return _camera; } }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        
    }
}
