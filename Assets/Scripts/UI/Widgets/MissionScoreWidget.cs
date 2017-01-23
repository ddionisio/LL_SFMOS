using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionScoreWidget : MonoBehaviour {
    public Text scoreLabel;

    void OnDestroy() {
        if(MissionController.instance)
            MissionController.instance.scoreChangeCallback -= OnScoreUpdate;
    }

    void Awake() {
        
    }

	// Use this for initialization
	void Start () {
        MissionController.instance.scoreChangeCallback += OnScoreUpdate;

        //Apply initial score
        scoreLabel.text = MissionController.instance.score.ToString();
	}
	
    void OnScoreUpdate(int score, int prevScore) {
        //do fancy stuff
        scoreLabel.text = score.ToString();
    }
}
