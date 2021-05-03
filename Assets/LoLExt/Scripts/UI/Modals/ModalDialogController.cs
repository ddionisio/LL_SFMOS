using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    /// <summary>
    /// Use this as a convenience to open dialog in a sequence
    /// </summary>
    public class ModalDialogController : MonoBehaviour {
        public string modal = "dialog";

        public Sprite portrait;
        public bool applyPortrait;

        [M8.Localize]
        public string nameTextRef;
        [M8.Localize]
        public string[] dialogTextRefs;

        public bool isPlaying { get { return mPlayRout != null; } }

        private Coroutine mPlayRout;
        private bool mIsNext;

        public IEnumerator PlayWait() {
            for(int i = 0; i < dialogTextRefs.Length; i++) {
                string textRef = dialogTextRefs[i];
                if(string.IsNullOrEmpty(textRef))
                    continue;

                mIsNext = false;

                if(applyPortrait)
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

            mPlayRout = null;
        }

        public void Play() {
            if(mPlayRout == null)
                mPlayRout = StartCoroutine(PlayWait());
        }

        public void Stop() {
            if(mPlayRout != null) {
                StopCoroutine(mPlayRout);
                mPlayRout = null;
            }

            if(M8.ModalManager.main && M8.ModalManager.main.IsInStack(modal))
                M8.ModalManager.main.CloseUpTo(modal, true);

            mIsNext = false;
        }

        void OnDisable() {
            Stop();
        }

        void OnDialogNext() {
            mIsNext = true;
        }
    }
}