using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLProxy : MonoBehaviour {
        public void Complete() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.Complete();
        }

        public void ProgressIncrement() {
            if(LoLManager.isInstantiated) {
                int curProgress = LoLManager.instance.curProgress;
                LoLManager.instance.ApplyProgress(curProgress + 1);
            }
        }
    }
}