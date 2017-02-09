using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour {
    public enum ActionType {
        Cutscene,
        Dialog,
        InputLock,
        InputUnlock,
        Wait
    }

    [System.Serializable]
    public struct DialogLookup {
        public string id;

        //[M8.]
        public string[] stringRefs;
    }

    [System.Serializable]
    public struct Action {
        public string id;
        public ActionType type;
        public string sVal;
        public float fVal;
    }

    public M8.Animator.AnimatorData anim;

    [Header("Look Ups")]
    public DialogLookup[] dialogs;

    [Header("Actions")]
    public Action[] enterActions;
    public Action[] exitActions;

    public float minDuration; //for stages with no pathogens

    public float stageNextDelay = 3; //how long before the next stage transition

    private Dictionary<string, DialogLookup> mDialogLookups;

    private M8.GenericParams mParmsDialog;

    void Awake() {
        //look ups
        mDialogLookups = new Dictionary<string, DialogLookup>();
        for(int i = 0; i < dialogs.Length; i++) {
            mDialogLookups.Add(dialogs[i].id, dialogs[i]);
        }

        mParmsDialog = new M8.GenericParams();
    }

    IEnumerator DoActions(Action[] actions) {
        for(int i = 0; i < actions.Length; i++) {
            var act = actions[i];

            switch(act.type) {
                case ActionType.Cutscene:
                    anim.Play(act.id);
                    while(anim.isPlaying)
                        yield return null;
                    break;

                case ActionType.Dialog:
                    DialogLookup dat;
                    if(mDialogLookups.TryGetValue(act.sVal, out dat)) {
                        mParmsDialog[ModalDialog.parmStringRefs] = dat.stringRefs;

                        M8.UIModal.Manager.instance.ModalOpen(act.id, mParmsDialog);

                        while(M8.UIModal.Manager.instance.ModalIsInStack(act.id) || M8.UIModal.Manager.instance.isBusy)
                            yield return null;
                    }
                    else
                        Debug.LogWarning("Unable to find lookup: "+act.sVal);
                    break;

                case ActionType.InputLock:
                    MissionController.instance.inputLock = true;
                    break;

                case ActionType.InputUnlock:
                    MissionController.instance.inputLock = false;
                    break;

                case ActionType.Wait:
                    yield return new WaitForSeconds(act.fVal);
                    break;
            }
        }
    }

    public IEnumerator Enter() {
        yield return DoActions(enterActions);
    }

    public IEnumerator Exit() {
        yield return DoActions(exitActions);
    }
}
