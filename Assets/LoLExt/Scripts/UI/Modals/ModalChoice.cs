using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class ModalChoice : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
        public const int choiceCapacity = 4;

        public const string parmTitleTextRef = "t";
        public const string parmDescTextRef = "d";
        public const string parmChoices = "c"; //array of ModalChoiceItemInfo
        public const string parmStartSelect = "i";
        public const string parmShuffle = "s";
        public const string parmDisplayPostSelected = "ps"; //bool, if true, display post selected after choosing an item
        public const string parmNextCallback = "n";

        [Header("Display")]        
        public Text titleLabel;
        public Text descLabel;

        [Header("Choice")]
        public ModalChoiceItem choiceTemplate; //put this in a hierarchy to spawn choice items.

        [Header("Confirm")]
        public Button confirmButton; //if not null, user must click on a choice and click on confirm. Otherwise, click on choice directly
        public GameObject confirmReadyGO; //if a choice has been made, activate this

        [Header("Text Speech")]
        public bool textSpeechAuto;

        private M8.CacheList<ModalChoiceItem> mItemActives = new M8.CacheList<ModalChoiceItem>(choiceCapacity);
        private M8.CacheList<ModalChoiceItem> mItemCache = new M8.CacheList<ModalChoiceItem>(choiceCapacity);

        private string mTitleTextRef;
        private string mDescTextRef;

        private ModalChoiceItem mCurChoiceItem;
        private System.Action<int> mNextCallback;
        private bool mIsDisplayPostSelected;

        public void ShowCorrectChoice(int correctIndex, bool enableConfirm) {
            //go through and show correct/wrong choices
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];

                if(itm.index == correctIndex) {
                    if(itm.correctGO) itm.correctGO.SetActive(true);
                    if(itm.wrongGO) itm.wrongGO.SetActive(false);
                }
                else {
                    if(itm.correctGO) itm.correctGO.SetActive(false);
                    if(itm.wrongGO) itm.wrongGO.SetActive(true);
                }

                if(itm.selectGO) itm.selectGO.SetActive(false);
            }

            if(enableConfirm) {
                //re-enable confirm
                if(confirmReadyGO)
                    confirmReadyGO.SetActive(true);

                if(confirmButton)
                    confirmButton.interactable = true;
            }
        }

        public void PlayDialogSpeech() {
            var grpName = name;
            int ind = 0;

            LoLManager.instance.StopSpeakQueue();

            if(!string.IsNullOrEmpty(mTitleTextRef)) {
                LoLManager.instance.SpeakTextQueue(mTitleTextRef, grpName, ind);
                ind++;
            }

            if(!string.IsNullOrEmpty(mDescTextRef)) {
                LoLManager.instance.SpeakTextQueue(mDescTextRef, grpName, ind);
                ind++;
            }

            for(int i = 0; i < mItemActives.Count; i++) {
                LoLManager.instance.SpeakTextQueue(mItemActives[i].textRef, grpName, ind + i);
            }
        }

        void M8.IModalActive.SetActive(bool aActive) {
            if(aActive) {
                if(textSpeechAuto)
                    PlayDialogSpeech();

                if(confirmReadyGO)
                    confirmReadyGO.SetActive(mCurChoiceItem != null);

                if(confirmButton)
                    confirmButton.interactable = mCurChoiceItem != null;
            }
            else {
                if(confirmReadyGO)
                    confirmReadyGO.SetActive(false);

                if(confirmButton)
                    confirmButton.interactable = false;
            }

            //apply selected
            //enable/disable choice interactions
            for(int i = 0; i < mItemActives.Count; i++) {
                if(aActive) {
                    mItemActives[i].selected = mItemActives[i] == mCurChoiceItem;
                    mItemActives[i].interactable = true;
                }
                else {
                    mItemActives[i].selected = false;
                    mItemActives[i].interactable = true;
                }
            }
        }
                
        void M8.IModalPush.Push(M8.GenericParams parms) {
            if(parms == null)
                return;

            int startIndex;
            bool shuffle;
            ModalChoiceItemInfo[] infos;

            //grab configuration
            parms.TryGetValue(parmTitleTextRef, out mTitleTextRef);

            parms.TryGetValue(parmDescTextRef, out mDescTextRef);
                        
            parms.TryGetValue(parmChoices, out infos);

            parms.TryGetValue(parmNextCallback, out mNextCallback);

            parms.TryGetValue(parmShuffle, out shuffle);

            parms.TryGetValue(parmDisplayPostSelected, out mIsDisplayPostSelected);

            startIndex = parms.ContainsKey(parmStartSelect) ? parms.GetValue<int>(parmStartSelect) : -1;

            //setup display
            if(titleLabel) titleLabel.text = !string.IsNullOrEmpty(mTitleTextRef) ? M8.Localize.Get(mTitleTextRef) : "";

            if(descLabel) {
                descLabel.text = !string.IsNullOrEmpty(mDescTextRef) ? M8.Localize.Get(mDescTextRef) : "";

                //resize to fit all of description
                //TODO: kind of hacky, also make sure not to use "stretch" for label's vertical
                Canvas.ForceUpdateCanvases();
                var size = descLabel.rectTransform.sizeDelta;
                size.y = descLabel.preferredHeight;
                descLabel.rectTransform.sizeDelta = size;
            }

            //setup choices
            ClearChoices();
            if(infos != null)
                GenerateChoices(infos, startIndex, shuffle);
                        
            //setup confirm
            if(confirmButton)
                confirmButton.interactable = false;

            if(confirmReadyGO)
                confirmReadyGO.SetActive(false);
        }

        void M8.IModalPop.Pop() {
            mCurChoiceItem = null;
            mNextCallback = null;
        }

        void Awake() {
            if(choiceTemplate)
                choiceTemplate.gameObject.SetActive(false);

            if(confirmButton)
                confirmButton.onClick.AddListener(OnConfirmClick);
        }

        void OnChoiceClick(ModalChoiceItem item) {
            //update selection
            var prevChoiceItem = mCurChoiceItem;
            mCurChoiceItem = item;

            if(prevChoiceItem)
                prevChoiceItem.selected = false;

            if(mCurChoiceItem)
                mCurChoiceItem.selected = true;

            if(confirmButton)
                confirmButton.interactable = true;

            //update confirm
            if(confirmReadyGO)
                confirmReadyGO.SetActive(true);

            if(!confirmButton) //call next if no confirm
                OnConfirmClick();
        }

        void OnConfirmClick() {
            if(confirmReadyGO)
                confirmReadyGO.SetActive(false);

            if(confirmButton)
                confirmButton.interactable = false;

            if(mCurChoiceItem) {
                if(mIsDisplayPostSelected && mCurChoiceItem.selectedPostGO)
                    mCurChoiceItem.selectedPostGO.SetActive(true);

                mNextCallback?.Invoke(mCurChoiceItem.index);
            }
        }

        private void GenerateChoices(ModalChoiceItemInfo[] infos, int startIndex, bool shuffle) {
            if(choiceTemplate) {
                var choiceRoot = choiceTemplate.transform.parent;

                for(int i = 0; i < infos.Length; i++) {
                    ModalChoiceItem itm;

                    if(mItemCache.Count > 0) {
                        itm = mItemCache.RemoveLast();
                    }
                    else { //add new
                        itm = Instantiate(choiceTemplate);
                        itm.transform.SetParent(choiceRoot, false);

                        itm.clickCallback += OnChoiceClick;
                    }

                    itm.Setup(i, infos[i]);
                    itm.selected = false;
                    itm.interactable = false;
                    itm.gameObject.SetActive(true);

                    mItemActives.Add(itm);
                }

                if(shuffle) {
                    mItemActives.Shuffle();

                    for(int i = 0; i < mItemActives.Count; i++) {
                        var itm = mItemActives[i];
                        itm.transform.SetAsLastSibling();
                    }
                }

                if(startIndex != -1 && startIndex < mItemActives.Count)
                    mCurChoiceItem = mItemActives[startIndex];
            }
        }

        private void ClearChoices() {
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];
                if(itm) {
                    itm.gameObject.SetActive(false);
                    mItemCache.Add(itm);
                }
            }

            mItemActives.Clear();
        }
    }
}