using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Renegadeware.LL_SFMOS {
    public class CardWidget : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [Header("Display")]
        public Image iconImage;
        public TMP_Text nameLabel;

        [Header("Drag")]
        public Transform dragRoot;

        public DG.Tweening.Ease dragReturnEase = DG.Tweening.Ease.InOutSine;
        public float dragReturnSpeed = 10f;

        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takePointerDown;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeDrag;

        public CardData cardData { get; private set; }

        public bool isDragging { get; private set; }

        public bool isBusy { get { return mRout != null; } }

        public event System.Action<CardWidget, PointerEventData> dragBeginCallback;
        public event System.Action<CardWidget, PointerEventData> dragCallback;
        public event System.Action<CardWidget, PointerEventData> dragEndCallback;

        private Transform mDragArea;
        
        private Coroutine mRout;

        private Vector3 mDragRootDefaultLocalPosition;

        private DG.Tweening.EaseFunction mDragReturnEaseFunc;

        private int mTakePointerDown = -1;
        private int mTakeDrag = -1;        

        public void Setup(CardData card, Transform dragArea) {
            cardData = card;

            if(iconImage) {
                iconImage.sprite = card.icon;
                //iconImage.SetNativeSize();
            }

            if(nameLabel)
                nameLabel.text = M8.Localize.Get(card.nameRef);

            mDragArea = dragArea;
        }

        public void Reset() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            if(dragRoot) {
                dragRoot.SetParent(transform, false);
                dragRoot.localPosition = mDragRootDefaultLocalPosition;
            }

            if(animator && animator.isPlaying) {
                animator.Stop();

                int lastTakeIndex = animator.lastPlayingTakeIndex;
                if(lastTakeIndex != -1)
                    animator.ResetTake(lastTakeIndex);
            }

            isDragging = false;
        }

        public void Return() {
            if(dragRoot.parent != mDragArea)
                return;

            if(mRout != null)
                StopCoroutine(mRout);

            mRout = StartCoroutine(DoDragReturn());
        }

        void OnApplicationFocus(bool focus) {
            if(!focus) {
                if(isDragging) {
                    Reset();

                    dragEndCallback?.Invoke(this, null);
                }
            }
        }

        void OnDisable() {
            mRout = null;
            isDragging = false;
        }

        void Awake() {
            if(dragRoot) {
                mDragRootDefaultLocalPosition = dragRoot.localPosition;
            }

            if(animator) {
                if(!string.IsNullOrEmpty(takeDrag))
                    mTakeDrag = animator.GetTakeIndex(takeDrag);

                if(!string.IsNullOrEmpty(takePointerDown))
                    mTakePointerDown = animator.GetTakeIndex(takePointerDown);
            }

            mDragReturnEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(dragReturnEase);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if(mTakePointerDown != -1)
                animator.Play(mTakePointerDown);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(isBusy)
                return;

            if(mTakeDrag != -1)
                animator.Play(mTakeDrag);

            dragRoot.SetParent(mDragArea, true);

            isDragging = true;

            dragBeginCallback?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            dragRoot.position = eventData.position;

            dragCallback?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            isDragging = false;

            dragEndCallback?.Invoke(this, eventData);
        }

        IEnumerator DoDragReturn() {
            var startPos = dragRoot.position;
            var endPos = transform.TransformPoint(mDragRootDefaultLocalPosition);

            var dist = (endPos - startPos).magnitude;
            if(dist > 0f) {
                var delay = dist / dragReturnSpeed;

                var curTime = 0f;
                while(curTime < delay) {
                    yield return null;

                    curTime += Time.deltaTime;

                    var t = mDragReturnEaseFunc(curTime, delay, 0f, 0f);

                    dragRoot.position = Vector3.Lerp(startPos, endPos, t);
                }
            }

            dragRoot.SetParent(transform, false);
            dragRoot.localPosition = mDragRootDefaultLocalPosition;

            mRout = null;
        }
    }
}