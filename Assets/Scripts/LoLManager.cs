using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLSDK;

[M8.PrefabCore]
public class LoLManager : M8.SingletonBehaviour<LoLManager> {
    public const string userDataSettingsKey = "settings";

    public const string settingsMusicVolumeKey = "mv";
    public const string settingsSoundVolumeKey = "sv";
    public const string settingsFadeVolumeKey = "fv";

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

    public float musicVolume { get { return mMusicVolume; } }
    public float soundVolume { get { return mSoundVolume; } }
    public float fadeVolume { get { return mFadeVolume; } }
    
    public event OnCallback progressCallback;
    public event OnCallback completeCallback;

    private float mMusicVolume;
    private float mSoundVolume;
    private float mFadeVolume;

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
    
    public void ApplyVolumes() {
        LOLSDK.Instance.ConfigureSound(mSoundVolume, mMusicVolume, mFadeVolume);
    }

    public void ApplyVolumes(float sound, float music, bool save) {
        ApplyVolumes(sound, music, mFadeVolume, save);
    }

    public void ApplyVolumes(float sound, float music, float fade, bool save) {
        LOLSDK.Instance.ConfigureSound(sound, music, fade);

        if(save) {
            mSoundVolume = sound;
            mMusicVolume = music;
            mFadeVolume = fade;

            var settings = M8.UserData.GetInstance(userDataSettingsKey);
            settings.SetFloat(settingsSoundVolumeKey, mSoundVolume);
            settings.SetFloat(settingsMusicVolumeKey, mMusicVolume);
            settings.SetFloat(settingsFadeVolumeKey, mFadeVolume);
        }
    }

    /// <summary>
    /// Call this when player quits, or finishes
    /// </summary>
    public void Complete() {
        LOLSDK.Instance.CompleteGame();

        if(completeCallback != null)
            completeCallback(this);
    }

    void Start() {
        var settings = M8.UserData.GetInstance(userDataSettingsKey);

        mMusicVolume = settings.GetFloat(settingsMusicVolumeKey, 0.3f);
        mSoundVolume = settings.GetFloat(settingsSoundVolumeKey, 0.5f);
        mFadeVolume = settings.GetFloat(settingsFadeVolumeKey, 0.1f);

        ApplyVolumes();
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
