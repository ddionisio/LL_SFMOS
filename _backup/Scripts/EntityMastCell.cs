using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMastCell : M8.EntityBase {

    public GameObject inputGO;

    public M8.Animator.AnimatorData animator;

    [Header("Takes")]
    public string takeNormal;
    public string takeActive;
    public string takeAlert;
    public string takeSleep;

    [Header("Info")]
    public float alertDelay = 3f;
    public float sleepDelay = 8f;

    [Header("UI")]
    public string modalUpgrade = "upgrade";
    public string modalDialog = "dialogBottom";
    public string modalQuiz = "quiz";

    [Header("UI Texts")]
    [M8.Localize]
    public string dialogPreQuiz;
    [M8.Localize]
    public string dialogPostQuizFail;

    private Coroutine mRout;
    private bool mIsClicked;

    public void Activate() {
        state = (int)EntityState.Active;
    }

    public void Click() {
        if(state != (int)EntityState.Active || mIsClicked)
            return;

        //show upgrade dialog
        mIsClicked = true;

        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoActive());
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)prevState) {
            case EntityState.Active:
                mIsClicked = false;
                break;
        }

        switch((EntityState)state) {
            case EntityState.Normal:
                animator.Play(takeNormal);
                inputGO.SetActive(false);
                break;

            case EntityState.Alert:
                mRout = StartCoroutine(DoPlayAnim(takeAlert, alertDelay, EntityState.Sleep));
                inputGO.SetActive(false);
                break;

            case EntityState.Active:
                animator.Play(takeActive);
                inputGO.SetActive(true);
                break;

            case EntityState.Sleep:
                mRout = StartCoroutine(DoPlayAnim(takeSleep, sleepDelay, EntityState.Normal));
                inputGO.SetActive(false);
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        mIsClicked = false;

        if(inputGO)
            inputGO.SetActive(false);
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
                
        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    /*protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }*/

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        inputGO.SetActive(false);
    }

    // Use this for one-time initialization
    /*protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }*/

    IEnumerator DoPlayAnim(string take, float delay, EntityState afterState) {
        animator.Play(take);

        yield return new WaitForSeconds(delay);

        mRout = null;

        state = (int)afterState;
    }

    IEnumerator DoActive() {
        var parms = new M8.GenericParams();

        //upgrade dialog
        UpgradeType upgrade = UpgradeType.None;

        ModalUpgrade.UpgradeChosenCallback cb = delegate (UpgradeType toUpgrade) {
            upgrade = toUpgrade;
        };

        parms[ModalUpgrade.parmCallback] = cb;

        M8.UIModal.Manager.instance.ModalOpen(modalUpgrade, parms);

        while(upgrade == UpgradeType.None)
            yield return null;

        parms.Clear();
        //

        //dialog to say there's a quiz
        bool isDlgClosed = false;

        System.Action dialogCb = delegate () {
            isDlgClosed = true;
        };

        parms[ModalDialog.parmStringRefs] = new string[] { dialogPreQuiz };
        parms[ModalDialogBase.parmPauseOverride] = true;
        parms[ModalDialogBase.parmActionFinish] = dialogCb;

        M8.UIModal.Manager.instance.ModalOpen(modalDialog, parms);

        while(!isDlgClosed)
            yield return null;

        parms.Clear();
        //

        //quiz time
        isDlgClosed = false;
        bool isAnsweredCorrectly = false;

        ModalLoLQuestion.ResultCallback quizResultCb = delegate (bool correct) {
            isDlgClosed = true;
            isAnsweredCorrectly = correct;
        };

        parms[ModalLoLQuestion.parmResultCallback] = quizResultCb;

        M8.UIModal.Manager.instance.ModalOpen(modalQuiz, parms);

        while(!isDlgClosed)
            yield return null;

        parms.Clear();

        //fail-safe if we needed more questions
        if(LoLManager.instance.isQuestionsAllAnswered)
            LoLManager.instance.ResetCurrentQuestionIndex();
        //

        //upgrade?
        if(isAnsweredCorrectly) {
            MissionController.instance.Upgrade(upgrade);

            mRout = null;
            state = (int)EntityState.Alert;
        }
        else {
            //wrong :(
            isDlgClosed = false;

            parms[ModalDialog.parmStringRefs] = new string[] { dialogPostQuizFail };
            parms[ModalDialogBase.parmPauseOverride] = true;
            parms[ModalDialogBase.parmActionFinish] = dialogCb;

            M8.UIModal.Manager.instance.ModalOpen(modalDialog, parms);

            while(!isDlgClosed)
                yield return null;

            parms.Clear();
            //

            mRout = null;
            state = (int)EntityState.Sleep;
        }
    }
}
