using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoLExt {
    public class LoLSpeakTextClick : MonoBehaviour, IPointerClickHandler {
        [M8.Localize]
        public string key;
        public string playGroup;
        public int playIndex;

        public void Play() {
            if(string.IsNullOrEmpty(key))
                return;

            if(string.IsNullOrEmpty(playGroup))
                LoLManager.instance.SpeakText(key);
            else
                LoLManager.instance.SpeakTextQueue(key, playGroup, playIndex);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            Play();
        }
    }
}