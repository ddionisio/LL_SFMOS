using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using M8.UIModal.Interface;

public class ModalVictory : M8.UIModal.Controller, IPush {
    public const string modalRef = "victory";

    public Text totalScoreLabel;
    [M8.Localize]
    public string totalScoreStringRef; //assumes it has string format {0}
    
    public Text quizCorrectLabel;
    [M8.Localize]
    public string quizCorrectStringRef; //assumes it has string format {0} {1}

    public M8.SceneAssetPath toScene;

    public void ClickNextScene() {
        toScene.Load();
    }
    
    void IPush.Push(M8.GenericParams parms) {
        int totalScore = M8.SceneState.instance.global.GetValue(SceneStateVars.curScore);

        var questionAnsweredList = LoLManager.instance.questionAnsweredList;

        int correctCount = 0;

        for(int i = 0; i < questionAnsweredList.Count; i++) {
            if(questionAnsweredList[i].alternativeIndex == questionAnsweredList[i].correctAlternativeIndex)
                correctCount++;
        }

        totalScoreLabel.text = string.Format(M8.Localize.Get(totalScoreStringRef), totalScore);
        quizCorrectLabel.text = string.Format(M8.Localize.Get(quizCorrectStringRef), correctCount, questionAnsweredList.Count);
    }
}
