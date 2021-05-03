using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class AttachPoint : MonoBehaviour {
        public float radius = 0.25f;
        public Color color = Color.white;

        private static Dictionary<string, AttachPoint> mAttachPoints = new Dictionary<string, AttachPoint>();

        public static AttachPoint Get(string name) {
            AttachPoint attachPoint;
            mAttachPoints.TryGetValue(name, out attachPoint);
            return attachPoint;
        }

        void OnDestroy() {
            if(mAttachPoints.ContainsKey(name))
                mAttachPoints.Remove(name);

        }

        void Awake() {
            if(mAttachPoints.ContainsKey(name))
                mAttachPoints[name] = this;
            else
                mAttachPoints.Add(name, this);
        }

        private void OnDrawGizmos() {
            if(radius > 0f) {
                Gizmos.color = color;
                Gizmos.DrawSphere(transform.position, radius);
            }
        }
    }
}