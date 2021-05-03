using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class MusicVolumeSliderWidget : MonoBehaviour {
        [Header("Display")]
        public Slider slider;

        void OnDisable() {
            slider.onValueChanged.RemoveListener(ApplyValue);
        }

        void OnEnable() {
            if(M8.UserSettingAudio.isInstantiated) {
                slider.value = M8.UserSettingAudio.instance.musicVolume;
            }

            slider.onValueChanged.AddListener(ApplyValue);
        }

        void ApplyValue(float val) {
            if(M8.UserSettingAudio.isInstantiated)
                M8.UserSettingAudio.instance.musicVolume = val;
        }
    }
}