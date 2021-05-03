using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class GameStart : GameModeController<GameStart> {
        public static bool isStarted = false;

        [Header("Game Start")]
        public GameObject loadingGO;
        public GameObject readyGO;

        public GameObject titleGO;
        public Text titleText;
        [M8.Localize]
        public string titleStringRef;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            if(loadingGO) loadingGO.SetActive(true);
            if(readyGO) readyGO.SetActive(false);
            if(titleGO) titleGO.SetActive(false);
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            //wait for LoL to load/initialize
            while(!LoLManager.instance.isReady)
                yield return null;

            yield return new WaitForSeconds(0.5f);

            //start title
            if(titleText) titleText.text = LoLLocalize.Get(titleStringRef);
            if(titleGO) titleGO.SetActive(true);

            //show other stuff

            if(loadingGO) loadingGO.SetActive(false);
            if(readyGO) readyGO.SetActive(true);

            isStarted = true;
        }
    }
}