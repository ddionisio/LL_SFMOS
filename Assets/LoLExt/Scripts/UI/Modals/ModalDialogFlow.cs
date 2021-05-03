using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    [System.Serializable]
    public class ModalDialogFlow {
        [M8.Localize]
        public string[] dialogTextRefs;

        private bool mIsNext;

        public IEnumerator Play() {
            yield return Play(ModalDialog.modalNameGeneric, null, null);
        }

        public IEnumerator Play(string modal, Sprite portrait, string nameTextRef) {
            for(int i = 0; i < dialogTextRefs.Length; i++) {
                string textRef = dialogTextRefs[i];
                if(string.IsNullOrEmpty(textRef))
                    continue;

                mIsNext = false;

                if(portrait)
                    ModalDialog.OpenApplyPortrait(modal, portrait, nameTextRef, textRef, OnDialogNext);
                else
                    ModalDialog.Open(modal, nameTextRef, textRef, OnDialogNext);

                while(!mIsNext)
                    yield return null;
            }

            if(M8.ModalManager.main.IsInStack(modal))
                M8.ModalManager.main.CloseUpTo(modal, true);

            //wait for dialog to close
            while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(modal))
                yield return null;
        }

        void OnDialogNext() {
            mIsNext = true;
        }
    }
}