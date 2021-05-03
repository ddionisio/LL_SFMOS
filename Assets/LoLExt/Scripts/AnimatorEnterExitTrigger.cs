using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class AnimatorEnterExitTrigger : MonoBehaviour {
        public GameObject activeGO;
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeEnter;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeExit;

        public float changeDelay = 2f;

        public bool isPlaying { get { return animator ? animator.isPlaying : false; } }

        private bool mIsTriggered;
        private float mLastTriggerTime;

        private Coroutine mRout;

        void OnTriggerEnter2D(Collider2D collision) {
            mIsTriggered = true;
            mLastTriggerTime = Time.time;

            if(mRout == null)
                mRout = StartCoroutine(DoEnter());
        }

        void OnTriggerExit2D(Collider2D collision) {
            mIsTriggered = false;
            mLastTriggerTime = Time.time;

            if(mRout == null)
                mRout = StartCoroutine(DoExit());
        }

        void OnEnable() {
            if(activeGO) activeGO.SetActive(false);
            mIsTriggered = false;
            mLastTriggerTime = 0f;
        }

        void OnDisable() {
            if(activeGO) activeGO.SetActive(false);
            mRout = null;
        }

        IEnumerator DoEnter() {
            if(activeGO) activeGO.SetActive(true);

            yield return animator.PlayWait(takeEnter);

            while(Time.time - mLastTriggerTime < changeDelay)
                yield return null;

            if(!mIsTriggered)
                mRout = StartCoroutine(DoExit());
            else
                mRout = null;
        }

        IEnumerator DoExit() {
            yield return animator.PlayWait(takeExit);

            if(activeGO) activeGO.SetActive(false);

            while(Time.time - mLastTriggerTime < changeDelay)
                yield return null;

            if(mIsTriggered)
                mRout = StartCoroutine(DoEnter());
            else
                mRout = null;
        }
    }
}