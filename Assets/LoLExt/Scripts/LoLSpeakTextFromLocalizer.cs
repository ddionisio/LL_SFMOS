using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLSpeakTextFromLocalizer : MonoBehaviour {

        M8.TextMeshPro.LocalizerTextMeshPro localizer;

        public string playGroup = "default";
        public int playIndex = -1;

        public bool autoPlay;

        public void Play() {
            if(string.IsNullOrEmpty(localizer.key))
                return;

            if(string.IsNullOrEmpty(playGroup))
                LoLManager.instance.SpeakText(localizer.key);
            else
                LoLManager.instance.SpeakTextQueue(localizer.key, playGroup, playIndex);
        }

        void OnEnable() {
            if(autoPlay)
                Play();
        }

        void Awake() {
            if(!localizer)
                localizer = GetComponent<M8.TextMeshPro.LocalizerTextMeshPro>();
        }
    }
}