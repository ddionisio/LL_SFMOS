using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJSON;

namespace LoLExt {
    public class LoLManagerMobile : LoLManager {
        //[Header("Data")]
        //public TextAsset localizeText;

        //public override bool isAutoSpeechEnabled { get { return false; } }

        protected override IEnumerator Start() {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            /*mLangCode = "en";
            mCurProgress = 0;

            ApplySettings();

            if(localizeText) {
                string json = localizeText.text;

                var langDefs = Json.Deserialize(json) as Dictionary<string, object>;
                ParseLanguage(Json.Serialize(langDefs[mLangCode]));
            }

            //ParseGameStart("");

            mIsReady = true;*/

            yield return base.Start();
        }
    }
}