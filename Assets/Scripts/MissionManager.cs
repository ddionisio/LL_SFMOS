using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : M8.SingletonBehaviour<MissionManager> {
    public const string sceneNamePrefix = "mission";

    [System.Serializable]
    public class MissionData {
        [SerializeField]
        int _maxScore;

        private int mScore = 0;
        private int mScoreQuiz = 0;

        private bool mCompleted = false;
        private bool mCompletedQuiz = false;
        
        public int curGrade {
            get {
                float t = Mathf.Clamp01(curScore/(float)_maxScore);

                int gradeCount = MissionManager.instance.grades.Length;
                if(gradeCount == 0)
                    return 0;

                return Mathf.RoundToInt(Mathf.Lerp(0,  gradeCount - 1, t));
            }
        }

        public int curScore {
            get { return mScore + mScoreQuiz; }
        }

        public bool completed { get { return mCompleted; } }

        public bool completedQuiz { get { return mCompletedQuiz; } }

        public void Complete(int score) {
            mScore = score;
            mCompleted = true;
        }

        public void CompleteQuiz(int scoreQuiz) {
            mScoreQuiz = scoreQuiz;
            mCompletedQuiz = true;
        }
    }

    [System.Serializable]
    public class GradeData {
        public string locRef;
        public GameObject widgetSmallTemplate;
    }

    [SerializeField]
    MissionData[] _missions;

    [SerializeField]
    GradeData[] _grades;

    [SerializeField]
    int _quizScorePerAnswer = 100;

    private int mCurMission = -1;
    
    public GradeData[] grades { get { return _grades; } }

    public MissionData curMission {
        get {
            if(mCurMission == -1)
                return null;

            return _missions[mCurMission];
        }
    }

    public int totalScore {
        get {
            int score = 0;

            for(int i = 0; i < _missions.Length; i++)
                score += _missions[i].curScore;

            return score;
        }
    }

    public int totalComplete {
        get {
            int count = 0;
            for(int i = 0; i < _missions.Length; i++) {
                if(_missions[i].completed)
                    count++;
                if(_missions[i].completedQuiz)
                    count++;
            }

            return count;
        }
    }

    public void Begin(int mission) {
        mCurMission = mission;

        M8.SceneManager.instance.LoadScene(sceneNamePrefix+mCurMission+"_intro");
    }

    public void Play() {
        if(mCurMission == -1)
            return;

        M8.SceneManager.instance.LoadScene(sceneNamePrefix+mCurMission+"_play");
    }

    public void Victory() {
        if(mCurMission == -1)
            return;
                
        M8.SceneManager.instance.LoadScene(sceneNamePrefix+mCurMission+"_victory");
    }
    
    public void Quiz() {
        if(mCurMission == -1)
            return;

        M8.SceneManager.instance.LoadScene(sceneNamePrefix+mCurMission+"_quiz");
    }

    public void Complete(int score) {
        var _mission = curMission;

        _mission.Complete(score);

        LoLManager.instance.ApplyProgress(totalComplete, totalScore);
    }

    public void CompleteQuiz(int correctCount) {
        var _mission = curMission;

        _mission.CompleteQuiz(correctCount*_quizScorePerAnswer);

        LoLManager.instance.ApplyProgress(totalComplete, totalScore);
    }
    
    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        
    }
}
