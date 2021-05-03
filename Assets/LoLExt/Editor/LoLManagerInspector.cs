using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace LoLExt {
    [CustomEditor(typeof(LoLManager), true)]
    public class LoLManagerInspector : Editor {
        private int mProgressInput;

        void OnEnable() {
            if(Application.isPlaying) {
                mProgressInput = ((LoLManager)target).curProgress;
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(Application.isPlaying) {
                GUILayout.BeginVertical(GUI.skin.box);

                var dat = (LoLManager)target;

                GUILayout.Label(string.Format("Current Progress: {0}/{1}", dat.curProgress, dat.progressMax));
                GUILayout.Label(string.Format("Current Score: {0}", dat.curScore));

                GUILayout.BeginHorizontal();
                mProgressInput = EditorGUILayout.IntField("Progress", mProgressInput);
                if(GUILayout.Button("Apply", GUILayout.Width(60f))) {
                    dat.ApplyProgress(mProgressInput);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }
    }
}