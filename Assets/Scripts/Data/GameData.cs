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

            public bool IsAnswerMatch(CardData card) {
                for(int i = 0; i < answers.Length; i++) {
                    if(answers[i] == card)
                        return true;
                }

                return false;
            }
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

        public int maxScore { get { return (scoreSingle * singleQuestionCount) + (scoreSingle * doubleQuestionCount * 4); } } //double score per answer

        private Question[] mQuestionShuffle;
        private M8.CacheList<CardData> mCardDeck;

        public int GetScore(int errorCount, bool isDouble) {
            int score = scoreSingle;
            if(isDouble)
                score *= 2;

            int errorPenalty = scoreSinglePenalty;
            if(isDouble)
                errorPenalty *= 2;

            score -= errorPenalty * errorCount;

            int minScore = scoreSingleMin;
            if(isDouble)
                minScore *= 2;

            if(score < minScore)
                score = minScore;

            return score;
        }

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
            var questionList = new List<Question>(singleQuestionCount + doubleQuestionCount);

            var singleQuestionShuffle = new Question[singleQuestions.Length];
            System.Array.Copy(singleQuestions, singleQuestionShuffle, singleQuestionShuffle.Length);
            M8.ArrayUtil.Shuffle(singleQuestionShuffle);

            int singleQuestionShuffleCount = Mathf.Min(singleQuestionShuffle.Length, singleQuestionCount);
            for(int i = 0; i < singleQuestionShuffleCount; i++)
                questionList.Add(singleQuestionShuffle[i]);

            var doubleQuestionShuffle = new Question[doubleQuestions.Length];
            System.Array.Copy(doubleQuestions, doubleQuestionShuffle, doubleQuestionShuffle.Length);
            M8.ArrayUtil.Shuffle(doubleQuestionShuffle);

            int doubleQuestionShuffleCount = Mathf.Min(doubleQuestionShuffle.Length, doubleQuestionCount);
            for(int i = 0; i < doubleQuestionShuffleCount; i++)
                questionList.Add(doubleQuestionShuffle[i]);

            mQuestionShuffle = questionList.ToArray();
        }

        /// <summary>
        /// Grab question based on current progress
        /// </summary>
        public Question GetCurrentQuestion() {
            if(mQuestionShuffle == null || mQuestionShuffle.Length == 0) {
                Debug.LogWarning("Questions array is null or empty.");
            }
            else if(LoLManager.isInstantiated) {
                int index = Mathf.Clamp(LoLManager.instance.curProgress, 0, mQuestionShuffle.Length - 1);
                return mQuestionShuffle[index];
            }

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

            if(LoLManager.isInstantiated)
                LoLManager.instance.progressMax = singleQuestionCount + doubleQuestionCount;
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