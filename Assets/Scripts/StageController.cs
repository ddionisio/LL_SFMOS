using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour {
    public enum ActionType {
        AnimPlayAndWait,
        AnimPlay,

        Dialog,
        DialogImage,
        DialogComposite,

        SelectArea,

        InputLock,
        InputUnlock,

        TimeLock,
        TimeUnlock,

        WaitEnemyCount,

        Wait,

        QuestionBonusDuration,
        QuestionBonusUpgradeMucus,
        QuestionBonusEnemyWipe,

        UpgradeReady,
    }
    
    [System.Serializable]
    public class Action {
        public string name;
        public ActionType type;
        public string id;
        public string lookup;

        [M8.Localize]
        public string stringRef;

        public Sprite sprite;

        public float fVal;
        public int iVal;
    }

    [Header("Modals")]
    public string modalSelect = "select";
    
    [Header("Look Ups")]
    public M8.Animator.AnimatorData[] animators; //make sure they are in their own game object, their name is used as lookup
    
    [Header("Enter")]
    public Action[] enterActions;
    [Header("Play")]
    public Action[] playActions;
    [Header("Exit")]
    public Action[] exitActions;

    public float duration; //duration before it goes game over, set to 0 for pure cutscene

    public bool isPlaying { get { return mPlayRout != null; } } //check during Play to see if we are finish

    private Dictionary<string, M8.Animator.AnimatorData> mAnimatorLookups;

    private M8.GenericParams mParmsDialog;

    private Coroutine mPlayRout;

    public IEnumerator Enter(MissionController missionCtrl) {
        yield return DoActions(missionCtrl, enterActions);
    }

    public IEnumerator Exit(MissionController missionCtrl) {
        yield return DoActions(missionCtrl, exitActions);
    }

    public void Play(MissionController missionCtrl) {
        mPlayRout = StartCoroutine(DoPlay(missionCtrl));
    }

    public void CancelPlay() {
        if(mPlayRout != null) {
            StopCoroutine(mPlayRout);
            mPlayRout = null;
        }
    }

    void OnDisable() {
        CancelPlay();
    }

    void Awake() {
        //look ups
        mAnimatorLookups = new Dictionary<string, M8.Animator.AnimatorData>();
        for(int i = 0; i < animators.Length; i++) {
            if(animators[i])
                mAnimatorLookups.Add(animators[i].name, animators[i]);
        }
        
        mParmsDialog = new M8.GenericParams();
    }

    IEnumerator DoActions(MissionController missionCtrl, Action[] actions) {
        for(int i = 0; i < actions.Length; i++) {
            var act = actions[i];

            bool isError = false;

            M8.Animator.AnimatorData anim;

            mParmsDialog.Clear();

            switch(act.type) {
                case ActionType.AnimPlay:                    
                    if(mAnimatorLookups.TryGetValue(act.lookup, out anim) && anim) {
                        anim.Play(act.id);
                    }
                    else
                        isError = true;
                    break;

                case ActionType.AnimPlayAndWait:
                    if(mAnimatorLookups.TryGetValue(act.lookup, out anim) && anim) {
                        anim.Play(act.id);
                        while(anim.isPlaying)
                            yield return null;
                    }
                    else
                        isError = true;
                    break;

                case ActionType.Dialog:
                    mParmsDialog[ModalDialog.parmStringRefs] = new string[] { act.stringRef };

                    M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                    while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                        yield return null;
                    break;

                case ActionType.DialogImage:
                    mParmsDialog[ModalDialogImage.parmPairRefs] = new ModalDialogImage.PairRef[] { new ModalDialogImage.PairRef() { stringRef=act.stringRef, sprite=act.sprite } };

                    M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                    while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                        yield return null;
                    break;

                case ActionType.DialogComposite:
                    mParmsDialog[ModalDialogComposite.parmPairRefs] = new ModalDialogComposite.PairRef[] { new ModalDialogComposite.PairRef() { stringRef=act.stringRef, compositeName=act.lookup } };

                    M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                    while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                        yield return null;
                    break;

                case ActionType.SelectArea:
                    mParmsDialog[ModalSelect.parmIndex] = act.iVal;

                    M8.UIModal.Manager.instance.ModalOpen(modalSelect, mParmsDialog);

                    while(M8.UIModal.Manager.instance.ModalIsInStack(modalSelect) || M8.UIModal.Manager.instance.isBusy)
                        yield return null;
                    break;

                case ActionType.InputLock:
                    missionCtrl.inputLock = true;
                    break;

                case ActionType.InputUnlock:
                    missionCtrl.inputLock = false;
                    break;

                case ActionType.TimeLock:
                    missionCtrl.isStageTimePause = true;
                    break;

                case ActionType.TimeUnlock:
                    missionCtrl.isStageTimePause = false;
                    break;

                case ActionType.WaitEnemyCount:
                    do {
                        yield return null;
                    } while(missionCtrl.enemyCount > 0);
                    break;

                case ActionType.Wait:
                    yield return new WaitForSeconds(act.fVal);
                    break;

                case ActionType.QuestionBonusDuration:
                case ActionType.QuestionBonusUpgradeMucus:
                case ActionType.QuestionBonusEnemyWipe:
                    yield return DoQuestion(act);
                    break;

                case ActionType.UpgradeReady:
                    missionCtrl.UpgradeReady();
                    break;
            }

            if(isError)
                Debug.LogWarning(string.Format("Error for id = {0}, lookup = {1}", act.id, act.lookup));
        }
    }

    IEnumerator DoQuestion(Action act) {

        bool isDone = false;
        bool isAnswerCorrect = false;

        ModalLoLQuestion.ResultCallback callback = delegate (bool isCorrect) {
            isAnswerCorrect = isCorrect;
            isDone = true;
        };

        mParmsDialog[ModalLoLQuestion.parmResultCallback] = callback;

        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

        while(!isDone)
            yield return null;

        mParmsDialog[ModalLoLQuestion.parmResultCallback] = null;

        if(isAnswerCorrect)
            Debug.Log("question answer correct");

        switch(act.type) {
            case ActionType.QuestionBonusDuration:
            case ActionType.QuestionBonusUpgradeMucus:
            case ActionType.QuestionBonusEnemyWipe:
                break;
        }

        //fail-safe if we needed more questions
        if(LoLManager.instance.isQuestionsAllAnswered)
            LoLManager.instance.ResetCurrentQuestionIndex();
    }

    IEnumerator DoPlay(MissionController missionCtrl) {

        yield return DoActions(missionCtrl, playActions);

        mPlayRout = null;
    }
}
