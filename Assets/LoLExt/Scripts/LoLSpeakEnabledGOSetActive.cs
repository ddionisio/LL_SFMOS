using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLSpeakEnabledGOSetActive : MonoBehaviour {
        public GameObject target;

        void OnEnable() {
            var go = target ? target : gameObject;
            if(LoLManager.isInstantiated)
                go.SetActive(LoLManager.instance.isAutoSpeechEnabled);
        }
    }
}