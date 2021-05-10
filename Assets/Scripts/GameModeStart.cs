using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using LoLExt;

namespace Renegadeware.LL_SFMOS {
    public class GameModeStart : GameModeController<GameModeStart> {
        [Header("Screen")]
        public GameObject loadingGO;
        public GameObject readyGO;

        [Header("Title")]
        [M8.Localize]
        public string titleRef;
        public TMP_Text titleText;

        [Header("Play")]
        public Button newButton;

        [M8.Localize]
        public string continueConfirmTitleRef;
        [M8.Localize]
        public string continueConfirmTextRef;

        public GameObject continueRootGO;
        public Button continueButton;

        [Header("Ready")]
        public M8.Animator.Animate readyAnimator;
        [M8.Animator.TakeSelector(animatorField = "readyAnimator")]
        public string readyTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "readyAnimator")]
        public string readyTakeExit;

        [Header("Game")]
        public GameObject gameRootGO;
        public M8.Animator.Animate gameAnimator;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeIntro;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeIntroToQuestion;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeContinue;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeQuestionToResult;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeResultToQuestion;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeQuestionToVictory;
        [M8.Animator.TakeSelector(animatorField = "gameAnimator")]
        public string gameTakeVictory;

        [Header("Intro")]
        public ModalDialogFlow introDialog;
        public GameObject introCardsGO;
        public ModalDialogFlow introCardsDialog;
        public GameObject introCardsCirculatoryGO;
        public ModalDialogFlow introCirculatoryDialog;
        public GameObject introCardsHeartGO;
        public ModalDialogFlow introHeartDialog;
        public ModalDialogFlow introReadyDialog;

        [Header("Double")]
        public ModalDialogFlow doubleDialog;

        [Header("End")]
        public ModalDialogFlow endDialog;
        public GameObject endRootGO;
        public GameObject endRootUIGO;
        [M8.Localize]
        public string endScoreTextRef;
        public TMP_Text endScoreText;

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;
        [M8.MusicPlaylist]
        public string musicEnd;

        private M8.GenericParams mModalParms = new M8.GenericParams();

        private bool mIsQuestionReady;

        protected override void OnInstanceDeinit() {
            if(GameData.isInstantiated) {
                var gameDat = GameData.instance;

                if(gameDat.signalQuestion) gameDat.signalQuestion.callback -= OnSignalQuestion;
                if(gameDat.signalResult) gameDat.signalResult.callback -= OnSignalResult;
            }

            base.OnInstanceDeinit();
        }

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            if(loadingGO) loadingGO.SetActive(true);
            if(readyGO) readyGO.SetActive(false);
            if(gameRootGO) gameRootGO.SetActive(false);

            if(introCardsGO) introCardsGO.SetActive(false);
            if(introCardsCirculatoryGO) introCardsCirculatoryGO.SetActive(false);
            if(introCardsHeartGO) introCardsHeartGO.SetActive(false);

            if(endRootGO) endRootGO.SetActive(false);
            if(endRootUIGO) endRootUIGO.SetActive(false);

            //Setup Play
            newButton.onClick.AddListener(OnPlayNew);
            continueButton.onClick.AddListener(OnPlayContinue);

            var gameDat = GameData.instance;

            if(gameDat.signalQuestion) gameDat.signalQuestion.callback += OnSignalQuestion;
            if(gameDat.signalResult) gameDat.signalResult.callback += OnSignalResult;
        }

        protected override IEnumerator Start() {
            //Loading
            yield return base.Start();

            while(!LoLManager.instance.isReady)
                yield return null;

            if(loadingGO) loadingGO.SetActive(false);

            //Title/Ready

            if(!string.IsNullOrEmpty(music))
                M8.MusicPlaylist.instance.Play(music, true, true);

            //Setup Title
            if(titleText) titleText.text = M8.Localize.Get(titleRef);

            if(readyGO) readyGO.SetActive(true);

            if(continueRootGO) continueRootGO.SetActive(LoLManager.instance.curProgress > 0);

            if(readyAnimator)
                yield return readyAnimator.PlayWait(readyTakeEnter);
        }

        void OnPlayNew() {
            if(LoLManager.instance.curProgress > 0) {
                ModalConfirm.Open(continueConfirmTitleRef, continueConfirmTextRef, (confirm) => {
                    if (confirm)
                        StartCoroutine(DoPlay(true));
                });
            }
            else
                StartCoroutine(DoPlay(true));
        }

        void OnPlayContinue() {
            StartCoroutine(DoPlay(false));
        }

        void OnSignalQuestion() {
            if(!mIsQuestionReady) {
                if(gameAnimator && !string.IsNullOrEmpty(gameTakeResultToQuestion))
                    gameAnimator.Play(gameTakeResultToQuestion);

                mIsQuestionReady = true;
            }
        }

        void OnSignalResult() {
            if(mIsQuestionReady) {
                if(gameAnimator && !string.IsNullOrEmpty(gameTakeQuestionToResult))
                    gameAnimator.Play(gameTakeQuestionToResult);

                mIsQuestionReady = false;
            }
        }

        IEnumerator DoPlay(bool isRestart) {
            if(readyAnimator)
                yield return readyAnimator.PlayWait(readyTakeExit);

            if(readyGO) readyGO.SetActive(false);

            if(gameRootGO) gameRootGO.SetActive(true);

            var lolMgr = LoLManager.instance;
            var gameDat = GameData.instance;

            int questionStartIndex = 0;

            //start new
            if(lolMgr.curProgress <= 0 || isRestart) {
                if(gameAnimator && !string.IsNullOrEmpty(gameTakeIntro))
                    yield return gameAnimator.PlayWait(gameTakeIntro);

                yield return introDialog.Play();

                if(introCardsGO) introCardsGO.SetActive(true);

                yield return introCardsDialog.Play();

                if(introCardsGO) introCardsGO.SetActive(false);
                if(introCardsCirculatoryGO) introCardsCirculatoryGO.SetActive(true);

                yield return introCirculatoryDialog.Play();

                if(introCardsCirculatoryGO) introCardsCirculatoryGO.SetActive(false);
                if(introCardsHeartGO) introCardsHeartGO.SetActive(true);

                yield return introHeartDialog.Play();

                if(introCardsHeartGO) introCardsHeartGO.SetActive(false);

                yield return introReadyDialog.Play();

                if(gameAnimator && !string.IsNullOrEmpty(gameTakeIntroToQuestion))
                    yield return gameAnimator.PlayWait(gameTakeIntroToQuestion);

                mIsQuestionReady = true;

                GameData.instance.Begin(isRestart);
            }
            else { //continue
                //different entrance to announcer
                if(gameAnimator && !string.IsNullOrEmpty(gameTakeContinue))
                    yield return gameAnimator.PlayWait(gameTakeContinue);

                mIsQuestionReady = true;

                GameData.instance.Begin(false);

                //determine current index
                if(lolMgr.curProgress < gameDat.singleQuestionCount)
                    questionStartIndex = lolMgr.curProgress;
                else
                    questionStartIndex = lolMgr.curProgress - gameDat.singleQuestionCount;
            }

            do {
                yield return null;
            } while(M8.ModalManager.main.isBusy);

            HUD.instance.ScoreEnter();

            //single questions
            if(lolMgr.curProgress < gameDat.singleQuestionCount) {
                mModalParms[ModalQuestion.parmCurIndex] = questionStartIndex;
                mModalParms[ModalQuestion.parmCount] = gameDat.singleQuestionCount;

                M8.ModalManager.main.Open(gameDat.modalQuestion, mModalParms);

                //wait for questions to be done
                do {
                    yield return null;
                } while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(gameDat.modalQuestion));

                questionStartIndex = 0;
            }

            //double questions intro
            if(lolMgr.curProgress == gameDat.singleQuestionCount) {
                //explain double questions
                yield return doubleDialog.Play();
            }

            //double questions
            if(lolMgr.curProgress < gameDat.singleQuestionCount + gameDat.doubleQuestionCount) {
                mModalParms[ModalQuestion.parmCurIndex] = questionStartIndex;
                mModalParms[ModalQuestion.parmCount] = gameDat.doubleQuestionCount;

                M8.ModalManager.main.Open(gameDat.modalQuestion, mModalParms);

                //wait for questions to be done
                do {
                    yield return null;
                } while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(gameDat.modalQuestion));
            }

            HUD.instance.ScoreExit();

            //end
            if(gameAnimator && !string.IsNullOrEmpty(gameTakeQuestionToVictory))
                yield return gameAnimator.PlayWait(gameTakeQuestionToVictory);

            yield return endDialog.Play();

            if(!string.IsNullOrEmpty(musicEnd))
                M8.MusicPlaylist.instance.Play(musicEnd, true, true);

            if(endScoreText)
                endScoreText.text = string.Format(M8.Localize.Get(endScoreTextRef), lolMgr.curScore, gameDat.maxScore);

            if(endRootGO) endRootGO.SetActive(true);
            if(endRootUIGO) endRootUIGO.SetActive(true);

            if(gameAnimator && !string.IsNullOrEmpty(gameTakeVictory))
                yield return gameAnimator.PlayWait(gameTakeVictory);

            //complete
            lolMgr.Complete();
        }
    }
}