using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Renegadeware.LL_SFMOS {
    public class CardDeckWidget : MonoBehaviour {
        [Header("Template")]
        public GameObject template;
        public int templateCapacity = 4;

        [Header("Display")]
        public Transform layoutRoot;

        public event System.Action<CardWidget, PointerEventData> dragBeginCallback;
        public event System.Action<CardWidget, PointerEventData> dragCallback;
        public event System.Action<CardWidget, PointerEventData> dragEndCallback;

        private M8.CacheList<CardWidget> mCardActives;
        private M8.CacheList<CardWidget> mCardCache;

        public void Setup(CardData[] cards, Transform dragArea) {
            Clear();

            for(int i = 0; i < cards.Length; i++)
                AddCard(cards[i], dragArea);
        }

        public void Remove(CardWidget cardWidget) {
            if(mCardActives.Remove(cardWidget)) {
                cardWidget.Reset();
                cardWidget.gameObject.SetActive(false);
                mCardCache.Add(cardWidget);
            }
        }

        public void Clear() {
            for(int i = 0; i < mCardActives.Count; i++) {
                var card = mCardActives[i];
                card.Reset();

                card.gameObject.SetActive(false);

                mCardCache.Add(card);
            }

            mCardActives.Clear();
        }

        void Awake() {
            mCardActives = new M8.CacheList<CardWidget>(templateCapacity);
            mCardCache = new M8.CacheList<CardWidget>(templateCapacity);

            for(int i = 0; i < templateCapacity; i++) {
                var newGO = Instantiate(template, layoutRoot);
                var newCard = newGO.GetComponent<CardWidget>();

                newCard.dragBeginCallback += OnCardDragBegin;
                newCard.dragCallback += OnCardDrag;
                newCard.dragEndCallback += OnCardDragEnd;

                newGO.SetActive(false);

                mCardCache.Add(newCard);
            }
        }

        void OnCardDragBegin(CardWidget cardWidget, PointerEventData pointerEventData) {
            dragBeginCallback?.Invoke(cardWidget, pointerEventData);
        }

        void OnCardDrag(CardWidget cardWidget, PointerEventData pointerEventData) {
            dragCallback?.Invoke(cardWidget, pointerEventData);
        }

        void OnCardDragEnd(CardWidget cardWidget, PointerEventData pointerEventData) {
            dragEndCallback?.Invoke(cardWidget, pointerEventData);
        }

        private CardWidget AddCard(CardData cardData, Transform dragArea) {
            if(mCardCache.Count == 0) {
                Debug.LogWarning("Card cache is empty.");
                return null;
            }

            var cardWidget = mCardCache.RemoveLast();

            cardWidget.Setup(cardData, dragArea);

            cardWidget.transform.SetAsLastSibling();

            cardWidget.gameObject.SetActive(true);

            mCardActives.Add(cardWidget);

            return cardWidget;
        }
    }
}