using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionScoreWidget : MonoBehaviour {
    public Text scoreLabel;
    
    void OnDisable() {
        if(HUD.instance)
            HUD.instance.scoreChangeCallback -= OnScoreUpdate;
    }

    void OnEnable() {
        HUD.instance.scoreChangeCallback += OnScoreUpdate;
    }
    
    void OnScoreUpdate(int score, int prevScore) {
        //do fancy stuff
        scoreLabel.text = score.ToString();
    }
}
