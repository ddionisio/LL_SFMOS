using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalLoLQuestion : M8.UIModal.Controller, IPop, IPush {
    public const string parmResultCallback = "cb";

    public delegate void ResultCallback(bool isAnswerCorrect);

    [Header("Pool")]
    public M8.PoolController pool;
    public string textPoolRef;
    public string imagePoolRef;

    [Header("Widgets")]
    public Color correctColor = Color.white;
    public Color wrongColor = Color.white;

    public Transform content;
    public Transform choicesRoot;

    public Text[] choiceTexts;
    public Button[] choiceButtons;
    public Image[] choiceHighlights;
    public GameObject[] choiceSelects;
    public GameObject resultCorrect;
    public GameObject resultWrong;
    public GameObject finishGO; //once results appear

    public float imageMaxWidth = 128f;

    private ResultCallback mResultCallback;
    private bool mIsAnswered;
    private bool mIsAnswerCorrect;
    private LoLSDK.MultipleChoiceQuestion mQuestionData;

    private Texture mLoadedTexture;

    public void ClickChoice(int index) {
        if(mIsAnswered || !isActive)
            return;

        //disable
        for(int i = 0; i < choiceButtons.Length; i++) {
            if(choiceButtons[i])
                choiceButtons[i].interactable = false;
        }

        //visualize player's selection
        for(int i = 0; i < choiceSelects.Length; i++) {
            if(choiceSelects[i])
                choiceSelects[i].SetActive(index == i);
        }

        //grab correct answer
        mIsAnswerCorrect = false;
        int answerIndex = 0;

        var answered = LoLManager.instance.AnswerCurrentQuestion(index);
        if(answered != null) {
            mIsAnswerCorrect = answered.answer.alternativeId == mQuestionData.correctAlternativeId;
            answerIndex = answered.correctAlternativeIndex;
        }

        for(int i = 0; i < choiceHighlights.Length; i++) {
            if(choiceHighlights[i])
                choiceHighlights[i].color = answerIndex == i ? correctColor : wrongColor;
        }

        if(resultCorrect) resultCorrect.SetActive(mIsAnswerCorrect);
        if(resultWrong) resultWrong.SetActive(!mIsAnswerCorrect);

        mIsAnswered = true;

        if(finishGO) finishGO.SetActive(true);
    }
    
    public override void SetActive(bool aActive) {
        base.SetActive(aActive);
        
    }

    void IPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            mResultCallback = parms.GetValue<ResultCallback>(parmResultCallback);
        }

        //reset
        for(int i = 0; i < choiceButtons.Length; i++) {
            if(choiceButtons[i])
                choiceButtons[i].interactable = true;
        }

        for(int i = 0; i < choiceSelects.Length; i++) {
            if(choiceSelects[i])
                choiceSelects[i].SetActive(false);
        }

        for(int i = 0; i < choiceHighlights.Length; i++) {
            if(choiceHighlights[i])
                choiceHighlights[i].color = Color.white;
        }

        if(resultCorrect) resultCorrect.SetActive(false);
        if(resultWrong) resultWrong.SetActive(false);

        if(finishGO) finishGO.SetActive(false);

        mIsAnswerCorrect = false;
        mIsAnswered = false;

        mQuestionData = LoLManager.instance.GetCurrentQuestion();

        //populate
        if(mQuestionData != null)
            Populate();
        else { //failsafe            
            if(finishGO) finishGO.SetActive(true);
        }
    }

    void Populate() {
        string questionStr = mQuestionData.stem;

        //TODO: multi images?
        const string imageTag = "[IMAGE]";
        int imageTagSize = imageTag.Length;

        int imageInd = questionStr.IndexOf("[IMAGE]");

        if(imageInd == -1) {
            Text txt = pool.Spawn<Text>(textPoolRef, textPoolRef, content, null);
            txt.transform.SetSiblingIndex(0);
            txt.text = questionStr;
        }
        else {
            string line1 = "";
            if(imageInd > 0)
                line1 = questionStr.Substring(0, imageInd).TrimEnd();

            string line2 = "";
            if(imageInd + imageTagSize < questionStr.Length)
                line2 = questionStr.Substring(imageInd + imageTagSize).Trim();

            int ind = 0;

            if(line1.Length > 0) {
                Text txt = pool.Spawn<Text>(textPoolRef, textPoolRef, content, null);
                txt.transform.SetSiblingIndex(ind); ind++;
                txt.text = line1;
            }

            if(!string.IsNullOrEmpty(mQuestionData.imageURL)) {
                RawImage img = pool.Spawn<RawImage>(imagePoolRef, imagePoolRef, content, null);                
                img.transform.SetSiblingIndex(ind); ind++;

                StartCoroutine(DoLoadTexture(mQuestionData.imageURL, img));
            }
            if(line2.Length > 0) {
                Text txt = pool.Spawn<Text>(textPoolRef, textPoolRef, content, null);
                txt.transform.SetSiblingIndex(ind); ind++;
                txt.text = line2;
            }
        }

        //choices
        int c = Mathf.Min(choiceTexts.Length, mQuestionData.alternatives.Length);
        for(int i = 0; i < c; i++) {
            if(choiceTexts[i]) {
                choiceTexts[i].gameObject.SetActive(true);
                choiceTexts[i].text = mQuestionData.alternatives[i].text;
            }
        }

        //fail-safe for cheeky questions with < 4 choices
        for(int i = c; i < choiceTexts.Length; i++) {
            if(choiceTexts[i])
                choiceTexts[i].gameObject.SetActive(false);
        }
    }

    void IPop.Pop() {
        pool.ReleaseAll();

        if(mLoadedTexture) {
            Destroy(mLoadedTexture);
            mLoadedTexture = null;
        }

        if(mResultCallback != null)
            mResultCallback(mIsAnswerCorrect);
    }

    IEnumerator DoLoadTexture(string url, RawImage toImage) {
        var rectT = toImage.GetComponent<RectTransform>();

        //TODO: loading icon?
        toImage.color = Color.clear;
        rectT.sizeDelta = new Vector2(32f, 32f);

        var www = new WWW(url);
        yield return www;

        mLoadedTexture = www.texture;

        toImage.texture = mLoadedTexture;

        //apply proper size
        var ratio = (float)mLoadedTexture.height/mLoadedTexture.width;
        
        float w = Mathf.Min(imageMaxWidth, mLoadedTexture.width);
        float h = w * ratio;

        rectT.sizeDelta = new Vector2(w, h);

        toImage.color = Color.white;
    }
}
