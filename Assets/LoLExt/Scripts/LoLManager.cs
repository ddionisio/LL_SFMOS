using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using MiniJSON;

using LoLSDK;

namespace LoLExt {
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

        public struct SpeakQueueData {
            public int index;
            public string key;
        }

        public const string userDataSettingsKey = "settings";

        public const string settingsSpeechMuteKey = "sp";

        private const string questionsJSONFilePath = "questions.json";
        private const string startGameJSONFilePath = "startGame.json";

        public delegate void OnChanged(LoLManager mgr, int delta);
        public delegate void OnCallback(LoLManager mgr);
        public delegate void OnSpeakCallback(LoLManager mgr, string key);

        [SerializeField]
        string _gameID = "com.daviddionisio.LoLGame";
        [SerializeField]
        int _progressMax;
        [SerializeField]
        LoLSaveData _userData = null;
        [SerializeField]
        M8.UserData _settingsData = null;
        [SerializeField]
        float _speakQueueStartDelay = 0.3f;

        [Header("Audio")]
        public GameObject musicPlaylistRootGO;
        public GameObject soundPlaylistRootGO;

        [Header("Signals")]
        public M8.Signal signalProgressUpdate;

        protected int mCurProgress;
        protected int mCurScore;

        protected string mLangCode = "en";

        private bool mIsFocus;

        public string gameID { get { return _gameID; } }

        public bool isReady { get { return mIsReady; } }

        public int progressMax { get { return _progressMax; } set { _progressMax = value; } }

        public int curProgress { get { return mCurProgress; } }
        public int curScore { 
            get { return mCurScore; } 
            set {
                if(mCurScore != value) {
                    mCurScore = value;

                    scoreUpdateCallback?.Invoke(this);
                }
            } 
        }

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

        public LoLSaveData userData { get { return _userData; } }
        public M8.UserData settingsData { get { return _settingsData; } }

        public virtual bool isAutoSpeechEnabled { get { return true; } }

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

        public bool isSpeechMute {
            get {
                return mIsSpeechMute;
            }
        }

        public event OnCallback scoreUpdateCallback;
        public event OnCallback progressCallback;
        public event OnCallback completeCallback;
        public event OnSpeakCallback speakCallback;

        protected bool mIsSpeechMute;

        protected bool mIsQuestionsReceived;
        protected MultipleChoiceQuestionList mQuestionsList;
        protected List<QuestionAnswered> mQuestionsAnsweredList;

        protected int mCurQuestionIndex;

        protected bool mIsReady;

        //loading data, wait for true, then parse the jsons
        private bool mIsGameStartHandled;
        private string mGameStartJson;

        private bool mIsLanguageHandled;
        private string mLanguageJson;

        private Coroutine mSpeakQueueRout;
        private string mSpeakQueueGroup;
        private LinkedList<SpeakQueueData> mSpeakQueues = new LinkedList<SpeakQueueData>();

        protected virtual void _SpeakText(string key) {
            //Debug.Log("Speaking: " + key);
            if(!mIsSpeechMute)
                LOLSDK.Instance.SpeakText(key);
        }

        public void SpeakText(string key) {
            //cancel speak queue
            StopSpeakQueue();

            _SpeakText(key);

            if(speakCallback != null)
                speakCallback(this, key);
        }

        public void SpeakTextQueue(string key, string group, int index) {
            //cancel speak queue if we are in a different group
            if(mSpeakQueueRout != null && group != mSpeakQueueGroup)
                StopSpeakQueue();

            mSpeakQueueGroup = group;

            //don't add to queue if it already exists
            //add to queue based on index
            LinkedListNode<SpeakQueueData> nodeToAddBefore = null;

            for(var node = mSpeakQueues.First; node != null; node = node.Next) {
                var nodeData = node.Value;

                if(key == nodeData.key) {
                    //already exists
                    return;
                }

                if(nodeToAddBefore == null && index < nodeData.index) {
                    nodeToAddBefore = node;
                }
            }

            var newQueue = new SpeakQueueData() { key = key, index = index };

            if(nodeToAddBefore != null)
                mSpeakQueues.AddBefore(nodeToAddBefore, newQueue);
            else
                mSpeakQueues.AddLast(newQueue);

            //start up routine if not yet started
            if(mSpeakQueueRout == null)
                mSpeakQueueRout = StartCoroutine(DoSpeakQueue());
        }

        public void StopSpeakQueue() {
            if(mSpeakQueueRout != null) {
                StopCoroutine(mSpeakQueueRout);
                mSpeakQueueRout = null;
            }

            mSpeakQueueGroup = null;
            mSpeakQueues.Clear();
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
        public virtual QuestionAnswered AnswerCurrentQuestion(int alternativeIndex) {
            if(isQuestionsAllAnswered)
                return null;

            var curQuestion = GetCurrentQuestion();

            if(curQuestion == null) {
                Debug.LogWarning("No question found for index: " + mCurQuestionIndex);
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

        protected void ProgressCallback() {
            if(progressCallback != null)
                progressCallback(this);

            if(signalProgressUpdate)
                signalProgressUpdate.Invoke();
        }

        public void ApplyCurrentProgress() {
            ApplyProgress(mCurProgress, mCurScore);
        }

        public void ApplyProgress(int progress) {

            ApplyProgress(progress, mCurScore);
        }

        public virtual void ApplyProgress(int progress, int score) {

            mCurProgress = Mathf.Clamp(progress, 0, _progressMax);            

            LOLSDK.Instance.SubmitProgress(score, mCurProgress, _progressMax);

            curScore = score;

            _userData.Save();

            ProgressCallback();
        }

        public void ApplySpeechMute(bool isMute) {
            if(mIsSpeechMute != isMute) {
                mIsSpeechMute = isMute;

                if(_settingsData)
                    _settingsData.SetInt(settingsSpeechMuteKey, mIsSpeechMute ? 1 : 0);
            }
        }

        /// <summary>
        /// Call this when player quits, or finishes
        /// </summary>
        public virtual void Complete() {
            LOLSDK.Instance.CompleteGame();

            if(completeCallback != null)
                completeCallback(this);
        }

        void OnApplicationFocus(bool focus) {
            mIsFocus = focus;
        }

        protected virtual IEnumerator Start() {
            mLangCode = "en";
            mIsReady = false;

            mIsGameStartHandled = false;
            mIsLanguageHandled = false;

            mIsFocus = Application.isFocused;

            //force run in background as requirement
            Application.runInBackground = false;

            // Create the WebGL (or mock) object
#if UNITY_EDITOR
            ILOLSDK webGL = new LoLSDK.MockWebGL();
#elif UNITY_WEBGL
            ILOLSDK webGL = new LoLSDK.WebGL();
#else
            ILOLSDK webGL = new LoLSDK.MockWebGL();
#endif

            // Initialize the object, passing in the WebGL
            LOLSDK.Init(webGL, _gameID);

            // Register event handlers
#if !UNITY_EDITOR
        LOLSDK.Instance.StartGameReceived += new StartGameReceivedHandler(this.HandleStartGame);
        LOLSDK.Instance.QuestionsReceived += new QuestionListReceivedHandler(this.HandleQuestions);
        LOLSDK.Instance.LanguageDefsReceived += new LanguageDefsReceivedHandler(this.HandleLanguageDefs);
#endif

            mCurProgress = 0;

            // Mock the platform-to-game messages when in the Unity editor.
#if UNITY_EDITOR
            LoadMockData();
#endif

            // Then, tell the platform the game is ready.
            LOLSDK.Instance.GameIsReady();

            //wait for start and language to be handled
            while(!(mIsGameStartHandled && mIsLanguageHandled))
                yield return null;

            //parse start
            if(!string.IsNullOrEmpty(mGameStartJson)) {
                ParseGameStart(mGameStartJson);
                mGameStartJson = null;
            }

            //parse language
            if(!string.IsNullOrEmpty(mLanguageJson)) {
                ParseLanguage(mLanguageJson);
                mLanguageJson = null;
            }

            const float timeOutDelay = 20.0f;

            float lastTime = Time.time;

            //load user data
            if(_userData) {
                _userData.Load();
                while(!_userData.isLoaded) {
                    //time-out?
                    if(Time.time - lastTime >= timeOutDelay) {
                        Debug.LogWarning("User Data loading timed-out.");
                        break;
                    }

                    yield return null;
                }

                mCurScore = _userData.score;
                mCurProgress = _userData.currentProgress;
            }

            lastTime = Time.time;

            //load settings data
            if(_settingsData && _settingsData != _userData) {
                _settingsData.Load();
                while(!_settingsData.isLoaded) {
                    //time-out?
                    if(Time.time - lastTime >= timeOutDelay) {
                        Debug.LogWarning("Settings Data loading timed-out.");
                        break;
                    }

                    yield return null;
                }
            }

            yield return null;

            //initialize audio
            if(M8.MusicPlaylist.instance)
                M8.MusicPlaylist.instance.SetupSourceProxy(musicPlaylistRootGO);

            if(M8.SoundPlaylist.instance)
                M8.SoundPlaylist.instance.SetupSourceRoot(soundPlaylistRootGO);

            ApplySettings();

            mIsReady = true;
        }

        protected void ParseGameStart(string json) {
            //TODO: this is giving out an error in Test Harness, will uncomment later if it finally works
            var startGamePayload = Json.Deserialize(json) as Dictionary<string, object>;

            // Capture the language code from the start payload. Use this to switch fonts
            object languageCodeObj;
            if(startGamePayload.TryGetValue("languageCode", out languageCodeObj))
                mLangCode = languageCodeObj.ToString();
            else
                mLangCode = "en"; //default

            object scoreObj;
            if(startGamePayload.TryGetValue("score", out scoreObj))
                mCurScore = System.Convert.ToInt32(scoreObj);
            else
                mCurScore = 0;

            object curProgressObj;
            if(startGamePayload.TryGetValue("currentProgress", out curProgressObj))
                mCurProgress = System.Convert.ToInt32(curProgressObj);
            else
                mCurProgress = 0;
        }

        protected void ParseLanguage(string json) {
            ((LoLLocalize)M8.Localize.instance).Load(mLangCode, json);
        }

        protected void ApplySettings() {
            if(_settingsData) {
                mIsSpeechMute = _settingsData.GetInt(settingsSpeechMuteKey) != 0;

                var audioSettings = GetComponent<M8.UserSettingAudio>();
                if(audioSettings)
                    audioSettings.Load();
            }
        }

        // Start the game here
        protected void HandleStartGame(string json) {
            if(!mIsGameStartHandled) {
                mGameStartJson = json;
                mIsGameStartHandled = true;
            }
        }

        // Store the questions and show them in order based on your game flow.
        protected void HandleQuestions(MultipleChoiceQuestionList questionList) {
            mIsQuestionsReceived = true;

            mQuestionsAnsweredList = new List<QuestionAnswered>();
        }

        // Use language to populate UI
        protected void HandleLanguageDefs(string json) {
            if(!mIsLanguageHandled) {
                mLanguageJson = json;
                mIsLanguageHandled = true;
            }
        }

        private IEnumerator DoWait(float delay) {
            float lastTime = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - lastTime < delay) {
                //focus lost
                if(!mIsFocus) {
                    float timePassed = Time.realtimeSinceStartup - lastTime;

                    //wait for focus to return
                    while(!mIsFocus)
                        yield return null;

                    //refresh lastTime
                    lastTime = Time.realtimeSinceStartup - timePassed;
                }

                yield return null;
            }
        }

        private IEnumerator DoSpeakQueue() {
            if(_speakQueueStartDelay > 0f)
                yield return DoWait(_speakQueueStartDelay);

            while(mSpeakQueues.Count > 0) {
                string key = mSpeakQueues.First.Value.key;
                mSpeakQueues.RemoveFirst();

                //play
                _SpeakText(key);

                //wait a bit
                float playDelay = 1.0f;

                var extraInfo = ((LoLLocalize)M8.Localize.instance).GetExtraInfo(key);
                if(extraInfo != null)
                    playDelay = extraInfo.voiceDuration;

                yield return DoWait(playDelay);
            }

            mSpeakQueueRout = null;
        }

#if UNITY_EDITOR
        void LoadMockData() {
            mLangCode = ((LoLLocalize)M8.Localize.instance).editorLanguageCode;

            //apply start data
            string startDataFilePath = Path.Combine(Application.streamingAssetsPath, startGameJSONFilePath);
            if(File.Exists(startDataFilePath)) {
                mGameStartJson = File.ReadAllText(startDataFilePath);
            }
            else
                mGameStartJson = "";
            //

            //apply language
            string langFilePath = ((LoLLocalize)M8.Localize.instance).editorLanguagePath;
            if(File.Exists(langFilePath)) {
                string json = File.ReadAllText(langFilePath);

                var langDefs = Json.Deserialize(json) as Dictionary<string, object>;
                mLanguageJson = Json.Serialize(langDefs[mLangCode]);
            }
            else
                mLanguageJson = "";
            //

            //apply questions
            string questionsFilePath = Path.Combine(Application.streamingAssetsPath, questionsJSONFilePath);
            if(File.Exists(questionsFilePath)) {
                string questionsDataAsJson = File.ReadAllText(questionsFilePath);
                MultipleChoiceQuestionList qs = MultipleChoiceQuestionList.CreateFromJSON(questionsDataAsJson);
                HandleQuestions(qs);
            }
            //

            mIsGameStartHandled = true;
            mIsLanguageHandled = true;
        }
#endif
    }
}