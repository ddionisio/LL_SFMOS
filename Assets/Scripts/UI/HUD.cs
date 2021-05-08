using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.LL_SFMOS {
    public class HUD : M8.SingletonBehaviour<HUD> {
        public M8.TextMeshPro.TextMeshProCounter scoreCounter;
        public AnimatorEnterExit scoreTransition;

        protected override void OnInstanceInit() {
            scoreTransition.gameObject.SetActive(false);
        }

        public void ScoreEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.scoreUpdateCallback += OnScoreUpdate;

            StartCoroutine(DoScoreEnter());
        }

        public void ScoreExit() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.scoreUpdateCallback -= OnScoreUpdate;

            StartCoroutine(DoScoreExit());
        }

        IEnumerator DoScoreEnter() {
            if(LoLManager.isInstantiated)
                scoreCounter.SetCountImmediate(LoLManager.instance.curScore);
            else
                scoreCounter.SetCountImmediate(0);

            scoreTransition.gameObject.SetActive(true);
            yield return scoreTransition.PlayEnterWait();
        }

        IEnumerator DoScoreExit() {
            yield return scoreTransition.PlayExitWait();

            scoreTransition.gameObject.SetActive(false);
        }

        void OnScoreUpdate(LoLManager lolMgr) {
            scoreCounter.count = lolMgr.curScore;
        }
    }
}