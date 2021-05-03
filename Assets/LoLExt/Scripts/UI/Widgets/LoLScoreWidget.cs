using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class LoLScoreWidget : MonoBehaviour {
        public Text target;

        public bool useCountAnimation;
        public float countDelay = 1f;

        void OnEnable() {
            if(useCountAnimation)
                StartCoroutine(DoCount());
            else
                target.text = LoLManager.instance.curScore.ToString();
        }

        void Awake() {
            if(!target)
                target = GetComponent<Text>();
        }

        IEnumerator DoCount() {
            float startCount = 0f;
            int curCount;

            target.text = "0";

            var curTime = 0f;
            while(curTime < countDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = Mathf.Clamp01(curTime / countDelay);

                float toCount = LoLManager.instance.curScore;

                curCount = Mathf.RoundToInt(Mathf.Lerp(startCount, toCount, t));

                target.text = curCount.ToString();
            }

            target.text = LoLManager.instance.curScore.ToString();
        }
    }
}