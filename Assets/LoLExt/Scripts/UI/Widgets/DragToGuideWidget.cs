using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class DragToGuideWidget : MonoBehaviour {

        public GameObject displayRootGO;
        public Transform dragRoot;
        public RectTransform lineRoot;

        [Header("Animation")]
        public Image cursorImage;
        public Sprite cursorIdleSprite;
        public Sprite cursorPressSprite;
        public float cursorFadeDelay = 0.3f;
        public float cursorIdleDelay = 0.3f;
        public float cursorMoveDelay = 0.5f;

        //[0, 1]
        public float dragPosition {
            get { return mDragPosition; }
            set {
                mDragPosition = value;

                if(!Application.isPlaying)
                    return;

                dragRoot.position = Vector2.Lerp(mDragStart, mDragEnd, mDragPosition);
            }
        }

        public bool isActive { get { return displayRootGO ? displayRootGO.activeSelf : false; } }

        public Vector2 dragStart { get { return mDragStart; } }
        public Vector2 dragEnd { get { return mDragEnd; } }

        private Vector2 mDragStart;
        private Vector2 mDragEnd;

        private float mDragPosition;
        private bool mIsPaused;

        public void UpdatePositions(Vector2 start, Vector2 end) {
            mDragStart = start;
            mDragEnd = end;

            mDragStart = start;
            mDragEnd = end;

            var dpos = mDragEnd - mDragStart;
            var dist = dpos.magnitude;

            //set line position, rotation, and size
            lineRoot.pivot = new Vector2(0.5f, 0f);
            lineRoot.position = mDragStart;

            if(dist > 0f)
                lineRoot.up = dpos / dist;

            //HACK: need a way to scale properly based on canvas resolution scaling
            lineRoot.sizeDelta = new Vector2(lineRoot.sizeDelta.x, dist * (576f / Screen.height));
            //

            dragRoot.position = Vector2.Lerp(mDragStart, mDragEnd, mDragPosition);
        }

        /// <summary>
        /// start and end in UI space
        /// </summary>
        public void Show(bool pause, Vector2 start, Vector2 end) {
            StopAllCoroutines();

            SetPause(pause);

            mDragPosition = 0f;

            UpdatePositions(start, end);

            if(displayRootGO) displayRootGO.SetActive(true);

            StartCoroutine(DoCursorMove());
        }

        public void Hide() {
            SetPause(false);

            if(displayRootGO) displayRootGO.SetActive(false);

            StopAllCoroutines();
        }

        void OnDisable() {
            Hide();
        }

        void Awake() {
            if(displayRootGO) displayRootGO.SetActive(false);
        }

        IEnumerator DoCursorMove() {
            var moveEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);

            while(true) {
                dragPosition = 0f;
                cursorImage.sprite = cursorIdleSprite;

                float curTime;
                float lastTime = Time.realtimeSinceStartup;

                //fade in            
                do {
                    curTime = Time.realtimeSinceStartup - lastTime;
                    float t = moveEaseFunc(curTime, cursorFadeDelay, 0f, 0f);

                    var clr = cursorImage.color;
                    clr.a = t;

                    cursorImage.color = clr;

                    yield return null;
                } while(curTime < cursorFadeDelay);
                //

                yield return new WaitForSecondsRealtime(cursorIdleDelay);

                cursorImage.sprite = cursorPressSprite;

                yield return new WaitForSecondsRealtime(cursorIdleDelay);

                //move
                lastTime = Time.realtimeSinceStartup;
                do {
                    curTime = Time.realtimeSinceStartup - lastTime;
                    float t = moveEaseFunc(curTime, cursorMoveDelay, 0f, 0f);

                    dragPosition = t;

                    yield return null;
                } while(curTime < cursorMoveDelay);
                //

                yield return new WaitForSecondsRealtime(cursorIdleDelay);

                cursorImage.sprite = cursorIdleSprite;

                yield return new WaitForSecondsRealtime(cursorIdleDelay);

                //fade out
                lastTime = Time.realtimeSinceStartup;
                do {
                    curTime = Time.realtimeSinceStartup - lastTime;
                    float t = moveEaseFunc(curTime, cursorFadeDelay, 0f, 0f);

                    var clr = cursorImage.color;
                    clr.a = 1.0f - t;

                    cursorImage.color = clr;

                    yield return null;
                } while(curTime < cursorFadeDelay);
                //

                yield return new WaitForSecondsRealtime(cursorIdleDelay);
            }
        }

        private void SetPause(bool pause) {
            if(mIsPaused != pause) {
                mIsPaused = pause;
                if(M8.SceneManager.isInstantiated) {
                    if(mIsPaused)
                        M8.SceneManager.instance.Pause();
                    else
                        M8.SceneManager.instance.Resume();
                }
            }
        }
    }
}