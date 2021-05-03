using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace LoLExt {
    [CustomEditor(typeof(GameBounds2D))]
    public class GameBounds2DEditor : Editor {
        BoxBoundsHandle mBoxHandle = new BoxBoundsHandle();

        void OnSceneGUI() {
            var dat = target as GameBounds2D;
            if(dat == null)
                return;

            using(new Handles.DrawingScope(dat.editRectColor)) {
                mBoxHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;

                Bounds b = new Bounds(dat.rect.center, dat.rect.size);

                mBoxHandle.center = new Vector3(b.center.x, b.center.y, 0f);
                mBoxHandle.size = new Vector3(b.size.x, b.size.y, 0f);

                EditorGUI.BeginChangeCheck();
                mBoxHandle.DrawHandle();
                if(EditorGUI.EndChangeCheck()) {
                    Vector2 min = mBoxHandle.center - mBoxHandle.size * 0.5f;

                    float _minX = Mathf.Round(min.x / dat.editRectSteps.x);
                    float _minY = Mathf.Round(min.y / dat.editRectSteps.y);

                    min.x = _minX * dat.editRectSteps.x;
                    min.y = _minY * dat.editRectSteps.y;

                    Vector2 max = mBoxHandle.center + mBoxHandle.size * 0.5f;

                    float _maxX = Mathf.Round(max.x / dat.editRectSteps.x);
                    float _maxY = Mathf.Round(max.y / dat.editRectSteps.y);

                    max.x = _maxX * dat.editRectSteps.x;
                    max.y = _maxY * dat.editRectSteps.y;

                    b.center = Vector2.Lerp(min, max, 0.5f);
                    b.size = max - min;

                    Undo.RecordObject(dat, "Change GameBounds2D");
                    dat.rect = new Rect(b.min, b.size);

                    if(dat.editSyncBoxCollider) {
                        BoxCollider2D boxColl = dat.GetComponent<BoxCollider2D>();
                        if(boxColl) {
                            Undo.RecordObject(boxColl, "Change GameBounds2D");
                            boxColl.offset = dat.transform.worldToLocalMatrix.MultiplyPoint3x4(dat.rect.center);
                            boxColl.size = dat.rect.size;
                        }
                    }
                }
            }
        }
    }
}