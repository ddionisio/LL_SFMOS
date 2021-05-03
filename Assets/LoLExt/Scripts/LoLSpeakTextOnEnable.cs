using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLSpeakTextOnEnable : MonoBehaviour {
        [M8.Localize]
        public string key;
        public string playGroup;
        public int playIndex;

        void OnEnable() {
            if(string.IsNullOrEmpty(key))
                return;

            if(string.IsNullOrEmpty(playGroup))
                LoLManager.instance.SpeakText(key);
            else
                LoLManager.instance.SpeakTextQueue(key, playGroup, playIndex);
        }
    }
}