using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJSON;

namespace LoLExt {
    public class LoLManagerMockup : LoLManager {
        [Header("Mockup")]
        public TextAsset localizeText;

        public override bool isAutoSpeechEnabled { get { return false; } }

        protected override void _SpeakText(string key) {

        }

        public override void ApplyProgress(int progress, int score) {

            mCurProgress = Mathf.Clamp(progress, 0, progressMax);

            ProgressCallback();
        }

        public override void Complete() {
            Debug.Log("COMPLETE");
        }

        protected override IEnumerator Start() {
            mLangCode = "en";
            mCurProgress = 0;

            ApplySettings();

            if(localizeText) {
                string json = localizeText.text;

                var langDefs = Json.Deserialize(json) as Dictionary<string, object>;
                ParseLanguage(Json.Serialize(langDefs[mLangCode]));
            }

            //ParseGameStart("");

            mIsReady = true;

            yield return null;
        }
    }
}