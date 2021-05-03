using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LoLExt {
    public class CameraShakeControl : MonoBehaviour {
        [Header("Camera")]
        [SerializeField]
        Camera _camera = null;
        [SerializeField]
        [M8.TagSelector]
        string _cameraTag = ""; //use this if you set camera to null
        [SerializeField]
        bool _cameraUseMain = true; //if true, camera is from Camera.main

        [Header("Params")]
        public float duration = 1f;
        public float strength = 3f;
        public int vibrate = 10;
        public float randomness = 90f;
        public bool fadeOut = true;

        public Camera cam {
            get {
                if(!_camera) {
                    if(_cameraUseMain)
                        _camera = Camera.main;
                    else {
                        var go = GameObject.FindGameObjectWithTag(_cameraTag);
                        if(go)
                            _camera = go.GetComponent<Camera>();
                    }
                }

                return _camera;
            }

            set {
                _camera = value;
            }
        }

        public void Shake() {
            var c = cam;
            var tween = c.DOShakePosition(duration, strength, vibrate, randomness, fadeOut);
            tween.SetAutoKill(true);
            tween.Play();
        }
    }
}