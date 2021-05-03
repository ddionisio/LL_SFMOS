using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class RectTransformGroupSizeDeltaRange : MonoBehaviour {
        public RectTransform targetRoot; //if null, use self

        public float range {
            get { return mRange; }
            set {
                if(!targetRoot)
                    targetRoot = transform as RectTransform;

                mRange = Mathf.Clamp01(value);

                var size = Vector2.Lerp(startSize, endSize, mRange);

                for(int i = 0; i < targetRoot.childCount; i++) {
                    RectTransform rt = (RectTransform)targetRoot.GetChild(i);
                    rt.sizeDelta = size;
                }
            }
        }

        public Vector2 startSize;
        public Vector2 endSize;

        private float mRange;
    }
}