using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class TextFromAsset : MonoBehaviour {
        public Text textWidget;
        public TextAsset textAsset;

        private void Awake() {
            textWidget.text = textAsset.text;
        }
    }
}