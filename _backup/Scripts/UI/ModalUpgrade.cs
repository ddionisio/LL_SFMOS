using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalUpgrade : M8.UIModal.Controller, IPop, IPush {
    public const string parmCallback = "cb";

    public delegate void UpgradeChosenCallback(UpgradeType upgrade);

    [System.Serializable]
    public class Choice {
        public UpgradeType type;
        public Button button;        
        public GameObject maxGO;
        public GameObject selectGO;
        [M8.Localize]
        public string descriptionRef;
    }

    public Choice[] choices;
    public Text descriptionLabel;
    public GameObject readyGO;

    private int mCurChoiceIndex;

    private UpgradeChosenCallback mUpgradeCallback;

    private bool mPaused;

    public void ClickReady() {
        if(mUpgradeCallback != null) {
            var cb = mUpgradeCallback;
            mUpgradeCallback = null;
            cb(choices[mCurChoiceIndex].type);
        }

        Close();
    }

    void OnDestroy() {
        if(mPaused) {
            if(M8.SceneManager.instance)
                M8.SceneManager.instance.Resume();
            mPaused = false;
        }
    }

    void Awake() {
        //setup choices
        for(int i = 0; i < choices.Length; i++) {
            int ind = i;
            choices[i].button.onClick.AddListener(delegate () {
                OnButtonSelect(ind);
            });
        }
    }

    void IPush.Push(M8.GenericParams parms) {
        //reset
        mCurChoiceIndex = 0;

        readyGO.SetActive(false);

        for(int i = 0; i < choices.Length; i++) {
            var choice = choices[i];

            bool isMaxed = false;

            switch(choice.type) {
                case UpgradeType.Time:
                    break;
                case UpgradeType.Mucus:
                    break;
                case UpgradeType.Neutrophil:
                    break;
            }

            choice.button.interactable = !isMaxed;
            choice.maxGO.SetActive(isMaxed);
            choice.selectGO.SetActive(false);
        }

        if(parms != null) {
            mUpgradeCallback = parms.GetValue<UpgradeChosenCallback>(parmCallback);
        }
        else {
            mUpgradeCallback = null;
        }

        if(!mPaused) {
            M8.SceneManager.instance.Pause();
            mPaused = true;
        }
    }

    void IPop.Pop() {
        if(mPaused) {
            M8.SceneManager.instance.Resume();
            mPaused = false;
        }
    }

    void OnButtonSelect(int ind) {
        if(mCurChoiceIndex >= 0 && mCurChoiceIndex < choices.Length)
            choices[mCurChoiceIndex].selectGO.SetActive(false);

        mCurChoiceIndex = ind;

        choices[mCurChoiceIndex].selectGO.SetActive(true);

        descriptionLabel.text = M8.Localize.Get(choices[mCurChoiceIndex].descriptionRef);

        readyGO.SetActive(true);
    }
}
