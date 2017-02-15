using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLSDK;

[M8.PrefabCore]
public class LoLManager : M8.SingletonBehaviour<LoLManager> {
    public class QuestionAnswered {
        public int questionIndex;
        public int alternativeIndex;
        public int correctAlternativeIndex;
        
        public MultipleChoiceAnswer answer;

        private bool mIsSubmitted;

        public QuestionAnswered(int aQuestionIndex, string questionId, int aAlternativeIndex, string alternativeId, int aCorrectAlternativeIndex) {
            questionIndex = aQuestionIndex;
            alternativeIndex = aAlternativeIndex;
            correctAlternativeIndex = aCorrectAlternativeIndex;

            answer = new MultipleChoiceAnswer();
            answer.questionId = questionId;
            answer.alternativeId = alternativeId;

            mIsSubmitted = false;
        }

        public void Submit() {
            if(!mIsSubmitted) {
                LOLSDK.Instance.SubmitAnswer(answer);
                mIsSubmitted = true;
            }
        }
    }

    public const string pauseModal = "options";

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

    public bool isQuestionsReceived { get { return mIsQuestionsReceived; } }

    public bool isQuestionsAllAnswered {
        get {
            if(mQuestionsList == null)
                return false;

            return mCurQuestionIndex >= mQuestionsList.questions.Length;
        }
    }

    public int questionCount {
        get {
            if(mQuestionsList == null)
                return 0;

            return mQuestionsList.questions.Length;
        }
    }

    public List<QuestionAnswered> questionAnsweredList {
        get { return mQuestionsAnsweredList; }
    }

    public int questionAnsweredCount {
        get {
            if(mQuestionsAnsweredList == null)
                return 0;

            return mQuestionsAnsweredList.Count;
        }
    }

    public int questionCurrentIndex {
        get {
            return mCurQuestionIndex;
        }
    }
    
    public event OnCallback progressCallback;
    public event OnCallback completeCallback;

    private float mMusicVolume;
    private float mSoundVolume;
    private float mFadeVolume;

    private bool mIsQuestionsReceived;
    private MultipleChoiceQuestionList mQuestionsList;
    private List<QuestionAnswered> mQuestionsAnsweredList;

    private int mCurQuestionIndex;

    private string mLastSoundBackgroundPath;

    public void PlaySound(string path, bool background, bool loop) {
        if(background && !string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            LOLSDK.Instance.StopSound(mLastSoundBackgroundPath);
        }

        LOLSDK.Instance.PlaySound(path, background, loop);

        mLastSoundBackgroundPath = path;
    }

    public void StopCurrentBackgroundSound() {
        if(!string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            LOLSDK.Instance.StopSound(mLastSoundBackgroundPath);
        }
    }
    
    public MultipleChoiceQuestion GetQuestion(int index) {
        if(mQuestionsList == null)
            return null;

        return mQuestionsList.questions[index];
    }

    public MultipleChoiceQuestion GetCurrentQuestion() {
        return GetQuestion(mCurQuestionIndex);
    }

    /// <summary>
    /// This will move the current question index by 1
    /// </summary>
    public QuestionAnswered AnswerCurrentQuestion(int alternativeIndex) {
        if(isQuestionsAllAnswered)
            return null;

        var curQuestion = GetCurrentQuestion();

        if(curQuestion == null) {
            Debug.LogWarning("No question found for index: "+mCurQuestionIndex);
            return null;
        }

        int correctAltIndex = -1;
        string correctAltId = curQuestion.correctAlternativeId;
        for(int i = 0; i < curQuestion.alternatives.Length; i++) {
            if(curQuestion.alternatives[i].alternativeId == correctAltId) {
                correctAltIndex = i;
                break;
            }
        }

        var newAnswered = new QuestionAnswered(mCurQuestionIndex, curQuestion.questionId, alternativeIndex, curQuestion.alternatives[alternativeIndex].alternativeId, correctAltIndex);

        //don't submit if it's already answered
        int questionInd = -1;
        for(int i = 0; i < mQuestionsAnsweredList.Count; i++) {
            if(mQuestionsAnsweredList[i].answer.questionId == newAnswered.answer.questionId) {
                questionInd = i;
                break;
            }
        }

        if(questionInd == -1) {
            newAnswered.Submit();

            mQuestionsAnsweredList.Add(newAnswered);
        }

        mCurQuestionIndex++;

        return newAnswered;
    }

    /// <summary>
    /// Call this if you want to cycle back
    /// </summary>
    /// <param name="ind"></param>
    public void ResetCurrentQuestionIndex() {
        mCurQuestionIndex = 0;
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
        LOLSDK.Init(_gameID);

        LOLSDK.Instance.QuestionsReceived += OnQuestionListReceive;
        LOLSDK.Instance.GameStateChanged += OnGameStateChanged;
                
        mCurProgress = 0;

        LOLSDK.Instance.SubmitProgress(0, 0, _progressMax);

        var settings = M8.UserData.GetInstance(userDataSettingsKey);

        mMusicVolume = settings.GetFloat(settingsMusicVolumeKey, 0.3f);
        mSoundVolume = settings.GetFloat(settingsSoundVolumeKey, 0.5f);
        mFadeVolume = settings.GetFloat(settingsFadeVolumeKey, 0.1f);

        ApplyVolumes();

#if UNITY_EDITOR
        CreateDummyQuestions();
#else
        LOLSDK.Instance.GetQuestions();
#endif
    }

    void OnQuestionListReceive(MultipleChoiceQuestionList questionList) {
        mIsQuestionsReceived = true;

        mQuestionsList = questionList;
        mQuestionsAnsweredList = new List<QuestionAnswered>(mQuestionsList.questions.Length);
    }

    void OnGameStateChanged(GameState state) {
        switch(state) {
            case GameState.Paused:
                /*if(M8.UIModal.Manager.instance) {
                    if(!M8.UIModal.Manager.instance.ModalIsInStack(pauseModal))
                        M8.UIModal.Manager.instance.ModalOpen(pauseModal);
                }
                else*/ if(!mPaused) {
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

    private void CreateDummyQuestions() {
        Alternative alternative1 = new Alternative();
        alternative1.text = "blue";
        alternative1.alternativeId = "1";

        Alternative alternative2 = new Alternative();
        alternative2.text = "red";
        alternative2.alternativeId = "2";

        Alternative alternative3 = new Alternative();
        alternative3.text = "yellow";
        alternative3.alternativeId = "3";

        Alternative alternative4 = new Alternative();
        alternative4.text = "green";
        alternative4.alternativeId = "4";

        MultipleChoiceQuestion question1 = new MultipleChoiceQuestion();
        question1.stem = "What is your favorite color? [IMAGE]";
        question1.questionId = "1";
        question1.correctAlternativeId = "1";
        question1.alternatives = new Alternative[] { alternative1, alternative2, alternative3, alternative4 };
        question1.imageURL = "http://s3.amazonaws.com/game-harness/images/red-green-and-blue-eye.jpg";

        Alternative alternative5 = new Alternative();
        alternative5.text = "Of course I like pie. All pie is good pie";
        alternative5.alternativeId = "5";

        Alternative alternative6 = new Alternative();
        alternative6.text = "No";
        alternative6.alternativeId = "6";

        Alternative alternative7 = new Alternative();
        alternative7.text = "Only apple pie";
        alternative7.alternativeId = "7";

        Alternative alternative8 = new Alternative();
        alternative8.text = "Yes, Pi is the best number ever. ";
        alternative8.alternativeId = "8";

        MultipleChoiceQuestion question2 = new MultipleChoiceQuestion();
        question2.stem = "This is a very very long question with an image in the middle of it. By long I mean it just goes on and on and on without reason. [IMAGE] The actual question is simply this: Do you like pie? Well, do you? If not, why on earth not? Who does't like pie?";
        question2.questionId = "2";
        question2.correctAlternativeId = "5";
        question2.alternatives = new Alternative[] { alternative5, alternative6, alternative7, alternative8 };
        question2.imageURL = "http://s3.amazonaws.com/game-harness/images/red-green-and-blue-eye.jpg";

        MultipleChoiceQuestion question3 = new MultipleChoiceQuestion();
        question3.stem = "This is a very very long question with no image in the middle of it. Also no image at the end. Or the beginning. By long I mean it just goes on and on and on without reason. The actual question is simply this: Do you like pie? Well, do you? If not, why on earth not? Who does't like pie?";
        question3.questionId = "3";
        question3.correctAlternativeId = "5";
        question3.alternatives = new Alternative[] { alternative5, alternative6, alternative7, alternative8 };
        question3.imageURL = null;

        MultipleChoiceQuestionList dummyList = new MultipleChoiceQuestionList();
        dummyList.questions = new MultipleChoiceQuestion[] { question1, question2, question3 };

        mIsQuestionsReceived = true;

        mQuestionsList = dummyList;
        mQuestionsAnsweredList = new List<QuestionAnswered>(mQuestionsList.questions.Length);
    }
}
