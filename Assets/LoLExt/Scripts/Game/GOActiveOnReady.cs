using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    /// <summary>
    /// Wait until LoLManager is ready
    /// </summary>
    public class GOActiveOnReady : MonoBehaviour {
        public GameObject targetGO;

        void OnEnable() {
            if(IsReady())
                targetGO.SetActive(true);
            else {
                targetGO.SetActive(false);

                StartCoroutine(OnWaitReady());
            }
        }

        IEnumerator OnWaitReady() {
            while(!IsReady())
                yield return null;

            targetGO.SetActive(true);
        }

        private bool IsReady() {
            return LoLManager.isInstantiated && LoLManager.instance.isReady;
        }
    }
}