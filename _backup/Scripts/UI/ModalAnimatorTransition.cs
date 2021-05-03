using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8.UIModal.Interface;

public class ModalAnimatorTransition : MonoBehaviour, IOpening, IClosing {
    public M8.Animator.AnimatorData animator;

    public string takeOpen;
    public string takeClose;

    IEnumerator IOpening.Opening() {
        if(animator && !string.IsNullOrEmpty(takeOpen)) {
            animator.Play(takeOpen);
            while(animator.isPlaying)
                yield return null;
        }
    }

    IEnumerator IClosing.Closing() {
        if(animator && !string.IsNullOrEmpty(takeClose)) {
            animator.Play(takeClose);
            while(animator.isPlaying)
                yield return null;
        }
    }
}
