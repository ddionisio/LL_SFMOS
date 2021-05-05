using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using LoLExt;

namespace Renegadeware.LL_SFMOS {
    public class ModalQuestion : M8.ModalController, M8.IModalPush {
        public const string parmCount = "c";

        [System.Serializable]
        public class SlotInfo {
            public CardSlotWidget slot;
            public AnimatorEnterExit transition;

            public bool isFilled { get { return slot.isFilled; } }
            public bool isTransitionBusy { get { return transition.isPlaying; } }

            public void Fill(CardData cardData) {
                slot.SetCard(cardData);
            }

            public void Hide() {
                transition.gameObject.SetActive(false);
            }

            public void Enter() {
                if(!transition.gameObject.activeSelf) {
                    transition.gameObject.SetActive(true);
                    transition.PlayEnter();
                }

                slot.Begin();
            }

            public void Exit() {
                if(transition.gameObject.activeSelf)
                    transition.PlayExit();
            }
        }

        [Header("Display")]
        public Transform cardDragArea;

        [Header("Deck")]
        public CardDeckWidget deckWidget;
        public AnimatorEnterExit deckTransition;

        [Header("Slots")]
        public SlotInfo[] slots;

        [Header("Dialogs")]
        public M8.TextMeshPro.TextMeshProTypewriter questionDialog;
        public GameObject questionDialogEndGO;
        public AnimatorEnterExit questionDialogTransition;

        public M8.TextMeshPro.TextMeshProTypewriter resultDialog;
        public GameObject resultDialogEndGO;
        public GameObject resultInteractGO;
        public AnimatorEnterExit resultDialogTransition;

        private CardData[] mAnswers;

        private GameData.Question mQuestion;

        private bool mIsResultDone;
        private int mSlotCount;

        private int mCurScore;
        private int mErrorCount;

        public void ResultClick() {
            if(resultDialog.isBusy)
                resultDialog.Skip();
            else
                mIsResultDone = true;
        }

        void M8.IModalPush.Push(M8.GenericParams parms) {
            HideAll();

            int count = 0;

            if(parms != null) {
                if(parms.ContainsKey(parmCount))
                    count = parms.GetValue<int>(parmCount);
            }

            StartCoroutine(DoQuestionSequence(count));
        }

        IEnumerator DoQuestionSequence(int count) {
            var lolMgr = LoLManager.instance;
            var gameDat = GameData.instance;

            for(int i = 0; i < count; i++) {
                if(lolMgr.curProgress >= lolMgr.progressMax) //completed
                    break;

                mQuestion = gameDat.GetCurrentQuestion();
                if(mQuestion == null)
                    break;

                mSlotCount = mQuestion.answers.Length;

                //question dialog
                StartCoroutine(DoDialogEnter(questionDialog, questionDialogTransition, mQuestion.questionTextRef));

                //deck
                gameDat.GetCards(mQuestion, mAnswers);

                deckWidget.Setup(mAnswers, cardDragArea);

                deckTransition.gameObject.SetActive(true);
                deckTransition.PlayEnter();

                //show/reset slots
                for(int j = 0; j < mSlotCount; j++)
                    slots[j].Enter();

                //fail-safe: fewer slots than before
                for(int j = mSlotCount; j < slots.Length; j++)
                    slots[j].Exit();

                //wait for slots to be filled
                mCurScore = 0;
                mErrorCount = 0;

                while(!IsSlotsFilled())
                    yield return null;

                //update score and progress
                lolMgr.ApplyProgress(lolMgr.curProgress + 1, lolMgr.curScore + mCurScore);

                //hide deck and question
                deckTransition.PlayExit();
                questionDialogTransition.PlayExit();

                while(deckTransition.isPlaying || questionDialogTransition.isPlaying)
                    yield return null;

                deckTransition.gameObject.SetActive(false);

                questionDialog.gameObject.SetActive(false);
                questionDialogEndGO.SetActive(false);
                questionDialogTransition.gameObject.SetActive(false);

                //result
                yield return DoDialogEnter(resultDialog, resultDialogTransition, mQuestion.resultTextRef);

                //wait for result click
                mIsResultDone = false;

                resultInteractGO.SetActive(true);

                while(!mIsResultDone)
                    yield return null;

                resultInteractGO.SetActive(false);

                //hide result dialog
                yield return resultDialogTransition.PlayExitWait();

                resultDialog.gameObject.SetActive(false);
                resultDialogEndGO.SetActive(false);                
                resultDialogTransition.gameObject.SetActive(false);
            }

            //hide slots
            for(int i = 0; i < mSlotCount; i++)
                slots[i].Exit();

            while(IsSlotsTransitioning())
                yield return null;

            for(int i = 0; i < mSlotCount; i++)
                slots[i].Hide();

            Close();
        }

        void Awake() {
            deckWidget.dragCallback += OnCardDrag;
            deckWidget.dragEndCallback += OnCardDragEnd;

            questionDialog.doneCallback += OnQuestionTextEnd;
            resultDialog.doneCallback += OnResultTextEnd;

            mAnswers = new CardData[deckWidget.templateCapacity];
        }

        void OnCardDrag(CardWidget cardWidget, PointerEventData pointerEventData) {
            //highlight slot if pointer is inside
        }

        void OnCardDragEnd(CardWidget cardWidget, PointerEventData pointerEventData) {
            //check if pointer is in slot
            bool isDropped = false;

            if(isDropped) {
                //matching card data?
                bool isMatch = false;

                if(isMatch) {
                    //fill slot

                    deckWidget.Remove(cardWidget);

                    mCurScore += GameData.instance.GetScore(mErrorCount, mSlotCount > 1);

                    mErrorCount = 0;
                }
                else {
                    //slot error animate

                    cardWidget.Return();

                    mErrorCount++;
                }
            }
            else
                cardWidget.Return();
        }

        void OnQuestionTextEnd() {
            questionDialogEndGO.SetActive(true);
        }

        void OnResultTextEnd() {
            resultDialogEndGO.SetActive(true);
        }

        IEnumerator DoDialogEnter(M8.TextMeshPro.TextMeshProTypewriter dialog, AnimatorEnterExit transition, string textRef) {
            dialog.gameObject.SetActive(true);

            yield return transition.PlayEnterWait();

            dialog.text = M8.Localize.Get(textRef);
            dialog.gameObject.SetActive(true);

            LoLManager.instance.StopSpeakQueue();
            LoLManager.instance.SpeakText(textRef);
        }

        private void HideAll() {
            deckTransition.gameObject.SetActive(false);

            for(int i = 0; i < slots.Length; i++)
                slots[i].Hide();

            questionDialog.gameObject.SetActive(false);
            questionDialogEndGO.SetActive(false);
            questionDialogTransition.gameObject.SetActive(false);

            resultDialog.gameObject.SetActive(false);
            resultDialogEndGO.SetActive(false);
            resultInteractGO.SetActive(false);
            resultDialogTransition.gameObject.SetActive(false);
        }

        private bool IsSlotsFilled() {
            int slotFilledCount = 0;

            for(int i = 0; i < mSlotCount; i++) {
                if(slots[i].isFilled)
                    slotFilledCount++;
            }

            return slotFilledCount >= mSlotCount;
        }

        private bool IsSlotsTransitioning() {
            int slotTransitionCount = 0;

            for(int i = 0; i < mSlotCount; i++) {
                if(slots[i].isTransitionBusy)
                    slotTransitionCount++;
            }

            return slotTransitionCount > 0;
        }
    }
}