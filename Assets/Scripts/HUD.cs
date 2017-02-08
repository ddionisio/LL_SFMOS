using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : M8.SingletonBehaviour<HUD> {
    public GameObject missionGO;

    public event System.Action<int, int> scoreChangeCallback;

    public void UpdateScore(int score, int prevScore) {
        if(scoreChangeCallback != null)
            scoreChangeCallback(score, prevScore);
    }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        
    }
}
