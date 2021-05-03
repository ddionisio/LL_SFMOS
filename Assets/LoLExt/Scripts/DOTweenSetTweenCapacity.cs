using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class DOTweenSetTweenCapacity : MonoBehaviour {
        public int tweenCapacity = 500;
        public int sequenceCapacity = 50;

        void Awake() {
            DG.Tweening.DOTween.SetTweensCapacity(tweenCapacity, sequenceCapacity);
        }
    }
}