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

    private int mCurScore;
    private int mCurProgress;

    public string gameID { get { return _gameID; } }

    public int progressMax { get { return _progressMax; } }

    public int curScore {
        get { return mCurScore; }
        set {
            if(mCurScore != value) {
                var prevScore = mCurScore;

                mCurScore = value;

                var delta = mCurScore - prevScore;

                if(scoreChangedCallback != null)
                    scoreChangedCallback(this, delta);
            }
        }
    }

    public int curPogress { get { return mCurProgress; } }

    public event OnChanged scoreChangedCallback;
    public event OnCallback progressCallback;
    public event OnCallback completeCallback;

    protected override void OnInstanceInit() {
        LOLSDK.Init(_gameID);

        Restart();
    }

    protected override void OnInstanceDeinit() {
        
    }

    public void Restart() {
        mCurScore = 0;
        mCurProgress = 0;

        LOLSDK.Instance.SubmitProgress(0, 0, _progressMax);
    }

    public void Progress() {
        if(mCurProgress >= _progressMax)
            return;

        mCurProgress++;
                
        LOLSDK.Instance.SubmitProgress(mCurScore, mCurProgress, _progressMax);
        
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
                M8.SceneManager.instance.Pause();
                break;

            case GameState.Resumed:
                M8.SceneManager.instance.Resume();
                break;
        }
    }
}
