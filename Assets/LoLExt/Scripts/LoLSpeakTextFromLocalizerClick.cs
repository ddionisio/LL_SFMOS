using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoLExt {
    public class LoLSpeakTextFromLocalizerClick : LoLSpeakTextFromLocalizer, IPointerClickHandler {
        public GameObject hideOnClick;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            Play();

            if(hideOnClick)
                hideOnClick.SetActive(false);
        }
    }
}