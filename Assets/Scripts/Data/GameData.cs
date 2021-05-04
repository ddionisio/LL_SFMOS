using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.LL_SFMOS {
    [CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
    public class GameData : M8.SingletonScriptableObject<GameData> {

        public const string userDataKeyRandomSeed = "seed";

        public enum CardSetType {
            Organs,
            Systems,
        }

        [System.Serializable]
        public class Question {
            [M8.Localize]
            public string questionTextRef;
            [M8.Localize]
            public string resultTextRef;

            public CardSetType cardSet;

            public CardData[] answers;
        }

        [Header("Modals")]
        public string modalQuestion = "question";

        [Header("Score")]
        public int scoreSingle = 100;
        public int scoreSingleMin = 10;
        public int scoreSinglePenalty = 25;

        [Header("Cards")]
        public CardData[] cardsOrgan;
        public CardData[] cardsSystem;

        [Header("Questions")]
        public int singleQuestionCount = 7;
        public Question[] singleQuestions;
        public int doubleQuestionCount = 3;
        public Question[] doubleQuestions;

        public bool isGameStarted { get; private set; } //true: we got through start normally, false: debug

        private int mRandomSeed;

        private Question[] mQuestionShuffle;
        private M8.CacheList<CardData> mCardDeck;

        /// <summary>
        /// Called in start scene
        /// </summary>
        public void Begin(bool isRestart) {
            isGameStarted = true;

            var lolMgr = LoLManager.instance;

            if(isRestart) {
                ClearAllUserData();

                GenerateRandomSeed();

                lolMgr.ApplyProgress(0, 0);
            }
            else
                ApplySavedSeed();

            //generate shuffled questions
        }

        /// <summary>
        /// Update progress, go to next level-scene
        /// </summary>
        public void Progress() {
            int curProgress;

            if (isGameStarted) {
                if (LoLManager.isInstantiated)
                    curProgress = LoLManager.instance.curProgress;
                else
                    curProgress = 0;
            }
            else {
                //determine our progress based on current scene
                curProgress = 0;
            }

            if (LoLManager.isInstantiated)
                LoLManager.instance.ApplyProgress(curProgress + 1);
        }

        /// <summary>
        /// Grab question based on current progress
        /// </summary>
        public Question GetCurrentQuestion() {
            return null;
        }

        public void GetCards(Question question, CardData[] cardsOutput) {
            if(cardsOutput == null || cardsOutput.Length == 0) {
                Debug.LogWarning("Cards Output is null or length == 0.");
                return;
            }

            int cardInd = 0, answerCount = question.answers.Length;

            if(answerCount > cardsOutput.Length) {
                Debug.LogWarning("There are more answers than card count.");
                return;
            }

            //put in answer cards
            for(int i = 0; i < answerCount; i++, cardInd++)
                cardsOutput[cardInd] = question.answers[i];

            //put in cards from shuffled deck
            CardDeckShuffle(question.cardSet);

            for(int i = 0; i < mCardDeck.Count && cardInd < cardsOutput.Length; i++) {
                var card = mCardDeck[i];
                if(M8.ArrayUtil.Contains(cardsOutput, 0, answerCount, card))
                    continue;

                cardsOutput[cardInd] = card;
                cardInd++;
            }

            //shuffle output
            M8.ArrayUtil.Shuffle(cardsOutput);
        }

        protected override void OnInstanceInit() {
            isGameStarted = false;
        }

        private void GenerateRandomSeed() {
            var newSeed = Random.Range(int.MinValue, int.MaxValue);

            LoLManager.instance.userData.SetInt(userDataKeyRandomSeed, newSeed);

            Random.InitState(newSeed);
        }

        private void ApplySavedSeed() {
            var usrData = LoLManager.instance.userData;

            if(usrData.HasKey(userDataKeyRandomSeed)) {
                var seed = usrData.GetInt(userDataKeyRandomSeed, 0);

                Random.InitState(seed);
            }
            else
                GenerateRandomSeed();
        }

        private void ClearAllUserData() {
            LoLManager.instance.userData.Remove(userDataKeyRandomSeed);
        }

        private void CardDeckShuffle(CardSetType setType) {
            if(mCardDeck == null) {
                int cardMaxCount = Mathf.Max(cardsOrgan.Length, cardsSystem.Length);
                mCardDeck = new M8.CacheList<CardData>(cardMaxCount);
            }
            else
                mCardDeck.Clear();

            switch(setType) {
                case CardSetType.Organs:
                    for(int i = 0; i < cardsOrgan.Length; i++)
                        mCardDeck.Add(cardsOrgan[i]);
                    break;

                case CardSetType.Systems:
                    for(int i = 0; i < cardsSystem.Length; i++)
                        mCardDeck.Add(cardsSystem[i]);
                    break;
            }

            mCardDeck.Shuffle();
        }
    }
}