using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOptions : MonoBehaviour {

    public void Execute() {
        if(M8.UIModal.Manager.instance.isBusy)
            return;

        if(M8.UIModal.Manager.instance.ModalIsInStack("options"))
            M8.UIModal.Manager.instance.ModalCloseTop();
        else
            M8.UIModal.Manager.instance.ModalOpen("options");
    }
}
