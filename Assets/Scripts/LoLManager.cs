using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLSDK;

[M8.PrefabCore]
public class LoLManager : M8.SingletonBehaviour<LoLManager> {
    public delegate void OnChanged(LoLManager mgr, int delta);
    public delegate void OnCallback(LoLManager mgr);

    [SerializeField]
    string _gameID = "com.daviddionisio.LoLGame";
    [SerializeField]
    int _progressMax;

    private int mCurProgress;

    private bool mPaused;
    
    public string gameID { get { return _gameID; } }

    public int progressMax { get { return _progressMax; } }

    public int curProgress { get { return mCurProgress; } }
    
    public event OnCallback progressCallback;
    public event OnCallback completeCallback;

    protected override void OnInstanceInit() {
        LOLSDK.Init(_gameID);

        Restart();
    }

    protected override void OnInstanceDeinit() {
        
    }

    public void Restart() {
        mCurProgress = 0;

        LOLSDK.Instance.SubmitProgress(0, 0, _progressMax);
    }

    public void ApplyScore(int score) {
        LOLSDK.Instance.SubmitProgress(score, mCurProgress, _progressMax);
    }

    public void ApplyProgress(int progress, int score) {

        mCurProgress = Mathf.Clamp(progress, 0, _progressMax);

        ApplyScore(score);

        if(progressCallback != null)
            progressCallback(this);
    }

    /// <summary>
    /// Call this when player quits, or finishes
    /// </summary>
    public void Complete() {
        LOLSDK.Instance.CompleteGame();

        if(completeCallback != null)
            completeCallback(this);
    }

    void OnGameStateChanged(GameState state) {
        switch(state) {
            case GameState.Paused:
                if(!mPaused) {
                    mPaused = true;
                    M8.SceneManager.instance.Pause();
                }
                break;

            case GameState.Resumed:
                if(mPaused) {
                    mPaused = false;
                    M8.SceneManager.instance.Resume();
                }
                break;
        }
    }
}
