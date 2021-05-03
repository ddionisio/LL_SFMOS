using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class GameBounds2D : MonoBehaviour {
        public Rect rect = new Rect(0f, 0f, 100f, 100f);

        //editor info
        public Vector2 editRectSteps = Vector2.one;
        public Color editRectColor = Color.cyan;
        public bool editSyncBoxCollider = false; //sync bound position and size to box collider

        public Vector2 Clamp(Vector2 center, Vector2 ext) {
            Vector2 min = (Vector2)rect.min + ext;
            Vector2 max = (Vector2)rect.max - ext;

            float extX = rect.width * 0.5f;
            float extY = rect.height * 0.5f;

            if(extX > ext.x)
                center.x = Mathf.Clamp(center.x, min.x, max.x);
            else
                center.x = rect.center.x;

            if(extY > ext.y)
                center.y = Mathf.Clamp(center.y, min.y, max.y);
            else
                center.y = rect.center.y;

            return center;
        }

        void OnDrawGizmos() {
            Gizmos.color = editRectColor;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}