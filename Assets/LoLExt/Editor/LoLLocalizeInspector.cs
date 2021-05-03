using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LoLExt {
    [CustomEditor(typeof(LoLLocalize))]
    public class LoLLocalizeInspector : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            M8.EditorExt.Utility.DrawSeparator();

            var dat = target as LoLLocalize;

            var lastEnabled = GUI.enabled;
            GUI.enabled = !string.IsNullOrEmpty(dat.editorExcelPath) && System.IO.File.Exists(dat.editorExcelPath);

            if(GUILayout.Button("Import")) {
                string error = "";
                if(!LoLLocalizePostProcess.ApplyAsset(dat, ref error))
                    EditorUtility.DisplayDialog("Error", error, "OK");
            }

            GUI.enabled = lastEnabled;
        }
    }
}