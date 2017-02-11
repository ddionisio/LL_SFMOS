using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsCount : MonoBehaviour {
    public Text text;

	// Use this for initialization
	IEnumerator Start () {
        while(!LoLManager.instance.isQuestionsReceived)
            yield return null;

        text.text = LoLManager.instance.questionCount.ToString();
	}
}
