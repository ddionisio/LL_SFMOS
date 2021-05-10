using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.LL_SFMOS {
    public class CardSlotWidget : MonoBehaviour {
        [Header("Display")]
        public CardWidget cardWidget;
        public GameObject highlightGO;

        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeNormal;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeError;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeAccept;

        public bool highlight { 
            get { return highlightGO.activeSelf; } 
            set { highlightGO.SetActive(value); }
        }

        public bool isFilled {
            get { return cardWidget.gameObject.activeSelf; }
        }

        private int mTakeNormalInd = -1;
        private int mTakeErrorInd = -1;
        private int mTakeAcceptInd = -1;

        public void Begin() {
            cardWidget.Setup(null, null);
            cardWidget.gameObject.SetActive(false);
            highlightGO.SetActive(false);

            if(mTakeNormalInd != -1)
                animator.Play(mTakeNormalInd);
        }

        public void SetCard(CardData card) {
            highlightGO.SetActive(false);

            cardWidget.Setup(card, null);
            cardWidget.gameObject.SetActive(true);

            if(mTakeAcceptInd != -1)
                animator.Play(mTakeAcceptInd);
        }

        public void Error() {
            highlightGO.SetActive(false);

            if(mTakeErrorInd != -1)
                animator.Play(mTakeErrorInd);
        }

        void Awake() {
            if(animator) {
                if(!string.IsNullOrEmpty(takeNormal))
                    mTakeNormalInd = animator.GetTakeIndex(takeNormal);

                if(!string.IsNullOrEmpty(takeError))
                    mTakeErrorInd = animator.GetTakeIndex(takeError);

                if(!string.IsNullOrEmpty(takeAccept))
                    mTakeAcceptInd = animator.GetTakeIndex(takeAccept);
            }

            cardWidget.gameObject.SetActive(false);
            highlightGO.SetActive(false);
        }
    }
}