using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOptions : M8.SingletonBehaviour<ButtonOptions> {
    public string modalRef;

    public bool showGiveUp { get { return mShowGiveUp; } set { mShowGiveUp = value; } }

    private bool mShowGiveUp;

    private M8.GenericParams mParms;

    public void Execute() {
        if(M8.UIModal.Manager.instance.isBusy || M8.SceneManager.instance.isLoading)
            return;

        if(M8.UIModal.Manager.instance.ModalIsInStack(modalRef)) {
            M8.UIModal.Manager.instance.ModalCloseUpTo(modalRef, true);
        }
        else {
            mParms[ModalOptions.parmShowGiveUp] = mShowGiveUp;

            M8.UIModal.Manager.instance.ModalOpen(modalRef, mParms);
        }
    }

    void Awake() {
        mParms = new M8.GenericParams();
    }
}
