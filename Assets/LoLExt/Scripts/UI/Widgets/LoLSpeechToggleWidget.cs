using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class LoLSpeechToggleWidget : MonoBehaviour {
        [Header("Display")]
        public GameObject onActiveGO;
        public GameObject offActiveGO;

        public void ToggleSound() {
            bool isOn = LoLManager.instance.isSpeechMute;

            if(isOn) { //turn off
                LoLManager.instance.ApplySpeechMute(false);
            }
            else { //turn on
                LoLManager.instance.ApplySpeechMute(true);
            }

            UpdateToggleStates();
        }

        void OnEnable() {
            UpdateToggleStates();
        }

        private void UpdateToggleStates() {
            bool isMute = LoLManager.instance.isSpeechMute;

            if(onActiveGO) onActiveGO.SetActive(!isMute);
            if(offActiveGO) offActiveGO.SetActive(isMute);
        }
    }
}