using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : M8.SingletonBehaviour<HUD> {
    [SerializeField]
    GameObject missionGO;
    [SerializeField]
    GameObject timeGO;

    public event System.Action<int, int> scoreChangeCallback;
    public event System.Action<float> timeUpdateCallback;

    public void SetMissionActive(bool aActive) {
        if(missionGO)
            missionGO.SetActive(aActive);
    }

    public void SetTimeActive(bool aActive) {
        if(timeGO)
            timeGO.SetActive(aActive);
    }
    
    public void UpdateScore(int score, int prevScore) {
        if(scoreChangeCallback != null)
            scoreChangeCallback(score, prevScore);
    }

    public void UpdateTime(float time) {
        if(timeUpdateCallback != null)
            timeUpdateCallback(time);
    }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        SetMissionActive(false);
    }
}
