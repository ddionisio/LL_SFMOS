using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LoLExt {
    [System.Serializable]
    public class ModalChoiceItemInfo {
        public Sprite iconRef;
        public string textRef;
    }

    public class ModalChoiceItem : MonoBehaviour, IPointerClickHandler {
        [Header("Display")]
        public Image iconImage;
        public Text label;
        public GameObject selectGO;
        public GameObject selectedGO;
        public GameObject selectedPostGO;
        public GameObject correctGO;
        public GameObject wrongGO;

        public event System.Action<ModalChoiceItem> clickCallback;

        public int index { get; private set; }        
        public bool interactable { 
            get { return selectable ? selectable.interactable : false; } 
            set {
                if(selectable)
                    selectable.interactable = value;
            }
        }

        public bool selected {
            get { return selectedGO ? selectedGO.activeSelf : false; }
            set {
                if(selectedGO)
                    selectedGO.SetActive(value);
            }
        }

        public string textRef { get; private set; }

        public Selectable selectable {
            get {
                if(!mSelectable)
                    mSelectable = GetComponent<Selectable>();
                return mSelectable;
            }
        }

        private Selectable mSelectable;

        public void Setup(int index, ModalChoiceItemInfo info) {
            this.index = index;

            if(iconImage)
                iconImage.sprite = info.iconRef;

            textRef = info.textRef;

            if(label)
                label.text = M8.Localize.Get(textRef);

            if(selectGO) selectGO.SetActive(true);
            if(correctGO) correctGO.SetActive(false);
            if(wrongGO) wrongGO.SetActive(false);
            if(selectedPostGO) selectedPostGO.SetActive(false);
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            if(!interactable)
                return;

            clickCallback?.Invoke(this);
        }
    }
}