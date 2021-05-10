using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using LoLExt;

namespace Renegadeware.LL_SFMOS {
    public class ModalQuestion : M8.ModalController, M8.IModalPush {
        public const string parmCurIndex = "i";
        public const string parmCount = "c";

        [System.Serializable]
        public class SlotInfo {
            public CardSlotWidget slot;
            public AnimatorEnterExit transition;

            public bool isFilled { get { return slot.isFilled; } }
            public bool isTransitionBusy { get { return transition.isPlaying; } }

            public bool isHighlight { get { return slot.highlight; } set { slot.highlight = value; } }

            public bool IsMatch(CardData cardData) {
                return slot.cardWidget.cardData == cardData;
            }

            public void Fill(CardData cardData) {
                if(cardData != null)
                    slot.SetCard(cardData);
                else
                    slot.Begin();
            }

            public void Hide() {
                transition.gameObject.SetActive(false);
            }

            public void Error() {
                slot.Error();
            }

            public void Enter() {
                slot.Begin();

                if(!transition.gameObject.activeSelf) {
                    transition.gameObject.SetActive(true);
                    transition.PlayEnter();
                }
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

        [Header("SFX")]
        [M8.SoundPlaylist]
        public string sfxCorrect;
        [M8.SoundPlaylist]
        public string sfxWrong;

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

            int startIndex = 0;
            int count = 0;

            if(parms != null) {
                if(parms.ContainsKey(parmCurIndex))
                    startIndex = parms.GetValue<int>(parmCurIndex);

                if(parms.ContainsKey(parmCount))
                    count = parms.GetValue<int>(parmCount);
            }

            StartCoroutine(DoQuestionSequence(startIndex, count));
        }

        IEnumerator DoQuestionSequence(int startIndex, int count) {
            var lolMgr = LoLManager.instance;
            var gameDat = GameData.instance;

            for(int i = startIndex; i < count; i++) {
                if(lolMgr.curProgress >= lolMgr.progressMax) //completed
                    break;

                mQuestion = gameDat.GetCurrentQuestion();
                if(mQuestion == null)
                    break;

                mSlotCount = mQuestion.answers.Length;

                if(gameDat.signalQuestion) gameDat.signalQuestion.Invoke();

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
                    slots[j].Hide();

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

                if(!string.IsNullOrEmpty(mQuestion.resultTextRef)) {
                    if(gameDat.signalResult) gameDat.signalResult.Invoke();

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
                else
                    yield return new WaitForSeconds(1.0f);
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
            SlotClearHighlights();

            var hit = pointerEventData.pointerCurrentRaycast;

            //highlight slot if pointer is inside
            if(hit.isValid && hit.gameObject) {
                var slot = GetSlot(hit.gameObject);
                if(slot != null)
                    slot.isHighlight = true;
            }   
        }

        void OnCardDragEnd(CardWidget cardWidget, PointerEventData pointerEventData) {
            SlotClearHighlights();

            //check if pointer is in slot
            if(pointerEventData != null) {
                SlotInfo slot = null;

                var hit = pointerEventData.pointerCurrentRaycast;
                if(hit.isValid && hit.gameObject)
                    slot = GetSlot(hit.gameObject);

                if(slot != null && !slot.isFilled) {
                    //can we fill it?
                    if(CanFill(cardWidget.cardData)) {
                        if(!string.IsNullOrEmpty(sfxCorrect))
                            M8.SoundPlaylist.instance.Play(sfxCorrect, false);

                        //fill slot and remove from deck
                        slot.Fill(cardWidget.cardData);

                        deckWidget.Remove(cardWidget);

                        mCurScore += GameData.instance.GetScore(mErrorCount, mSlotCount > 1);
                        mErrorCount = 0;
                    }
                    else { //error
                        if(!string.IsNullOrEmpty(sfxWrong))
                            M8.SoundPlaylist.instance.Play(sfxWrong, false);

                        slot.Error();

                        cardWidget.Return();

                        mErrorCount++;
                    }
                }
                else
                    cardWidget.Return();
            }
        }

        void OnQuestionTextEnd() {
            questionDialogEndGO.SetActive(true);
        }

        void OnResultTextEnd() {
            resultDialogEndGO.SetActive(true);
        }

        IEnumerator DoDialogEnter(M8.TextMeshPro.TextMeshProTypewriter dialog, AnimatorEnterExit transition, string textRef) {
            transition.gameObject.SetActive(true);

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

        private SlotInfo GetSlot(GameObject go) {
            for(int i = 0; i < mSlotCount; i++) {
                if(slots[i].slot.gameObject == go)
                    return slots[i];
            }

            return null;
        }

        private bool CanFill(CardData card) {
            //check if already filled
            for(int i = 0; i < mSlotCount; i++) {
                if(slots[i].IsMatch(card))
                    return false;
            }

            if(mQuestion == null)
                return false;

            //check if match answer
            return mQuestion.IsAnswerMatch(card);
        }

        private void SlotClearHighlights() {
            for(int i = 0; i < mSlotCount; i++)
                slots[i].isHighlight = false;
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