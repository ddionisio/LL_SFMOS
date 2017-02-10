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

        Wait
    }

    [System.Serializable]
    public class DialogLookup {
        public string id;

        [M8.Localize]
        public string[] stringRefs;
    }

    [System.Serializable]
    public class DialogImageLookup {
        public string id;
        
        public ModalDialogImage.PairRef[] pairs;
    }

    [System.Serializable]
    public class DialogCompositeLookup {
        public string id;

        public ModalDialogComposite.PairRef[] pairs;
    }

    [System.Serializable]
    public class SelectAreaLookup {
        public string id;
        public Bounds bounds;
    }

    [System.Serializable]
    public class Action {
        public string lookup;
        public string id;        
        public ActionType type;

        public float fVal;
    }
    
    [Header("Look Ups")]
    public M8.Animator.AnimatorData[] animators; //make sure they are in their own game object, their name is used as lookup
    public DialogLookup[] dialogs;
    public DialogImageLookup[] dialogImages;
    public DialogCompositeLookup[] dialogComposites;
    public SelectAreaLookup[] selects;
    
    [Header("Actions")]
    public Action[] enterActions;
    public Action[] playActions;
    public Action[] exitActions;

    public float duration; //duration before it goes game over, set to 0 for pure cutscene

    public bool isPlaying { get { return mPlayRout != null; } } //check during Play to see if we are finish

    private Dictionary<string, M8.Animator.AnimatorData> mAnimatorLookups;
    private Dictionary<string, DialogLookup> mDialogLookups;
    private Dictionary<string, DialogImageLookup> mDialogImageLookups;
    private Dictionary<string, DialogCompositeLookup> mDialogCompositeLookups;
    private Dictionary<string, SelectAreaLookup> mSelectAreaLookups;

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

        mDialogLookups = new Dictionary<string, DialogLookup>();
        for(int i = 0; i < dialogs.Length; i++) {
            mDialogLookups.Add(dialogs[i].id, dialogs[i]);
        }

        mDialogImageLookups = new Dictionary<string, DialogImageLookup>();
        for(int i = 0; i < dialogImages.Length; i++) {
            mDialogImageLookups.Add(dialogImages[i].id, dialogImages[i]);
        }

        mDialogCompositeLookups = new Dictionary<string, DialogCompositeLookup>();
        for(int i = 0; i < dialogComposites.Length; i++) {
            mDialogCompositeLookups.Add(dialogComposites[i].id, dialogComposites[i]);
        }

        mSelectAreaLookups = new Dictionary<string, SelectAreaLookup>();
        for(int i = 0; i < selects.Length; i++) {
            mSelectAreaLookups.Add(selects[i].id, selects[i]);
        }

        mParmsDialog = new M8.GenericParams();
    }

    IEnumerator DoActions(MissionController missionCtrl, Action[] actions) {
        for(int i = 0; i < actions.Length; i++) {
            var act = actions[i];

            bool isError = false;

            M8.Animator.AnimatorData anim;

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
                    DialogLookup dialog;
                    if(mDialogLookups.TryGetValue(act.lookup, out dialog)) {
                        mParmsDialog[ModalDialog.parmStringRefs] = dialog.stringRefs;

                        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                        while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                            yield return null;
                    }
                    else
                        isError = true;
                    break;

                case ActionType.DialogImage:
                    DialogImageLookup dialogImage;
                    if(mDialogImageLookups.TryGetValue(act.lookup, out dialogImage)) {
                        mParmsDialog[ModalDialogImage.parmPairRefs] = dialogImage.pairs;

                        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                        while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                            yield return null;
                    }
                    else
                        isError = true;
                    break;

                case ActionType.DialogComposite:
                    DialogCompositeLookup dialogComp;
                    if(mDialogCompositeLookups.TryGetValue(act.lookup, out dialogComp)) {
                        mParmsDialog[ModalDialogComposite.parmPairRefs] = dialogComp.pairs;

                        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                        while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                            yield return null;
                    }
                    else
                        isError = true;
                    break;

                case ActionType.SelectArea:
                    SelectAreaLookup selectArea;
                    if(mSelectAreaLookups.TryGetValue(act.lookup, out selectArea)) {
                        mParmsDialog[ModalWorldSelect.parmCamRefs] = Camera.main;
                        mParmsDialog[ModalWorldSelect.parmBoundsRefs] = selectArea.bounds;

                        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                        while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                            yield return null;
                    }
                    else
                        isError = true;
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
                    while(missionCtrl.enemyCount > 0)
                        yield return null;
                    break;

                case ActionType.Wait:
                    yield return new WaitForSeconds(act.fVal);
                    break;
            }

            if(isError)
                Debug.LogWarning(string.Format("Error for id = {0}, lookup = {1}", act.id, act.lookup));
        }
    }

    IEnumerator DoPlay(MissionController missionCtrl) {

        yield return DoActions(missionCtrl, playActions);

        mPlayRout = null;
    }
        
    void OnDrawGizmos() {
        if(selects == null)
            return;

        Gizmos.color = new Color(0.9568f, 0.52549f, 0.1058f);
        for(int i = 0; i < selects.Length; i++) {
            var bound = selects[i].bounds;
            Gizmos.DrawWireCube(bound.center, bound.size);
        }
    }
}
