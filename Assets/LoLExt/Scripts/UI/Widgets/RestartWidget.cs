using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    /// <summary>
    /// Requires M8.SceneState
    /// </summary>
    public class RestartWidget : MonoBehaviour, M8.IModalPush {
        public const string sceneVarName = "isRestart";

        [Header("Modal")]
        public M8.ModalManager modalManager;
        public string modalConfirm = "confirm";

        [Header("Text")]
        [M8.Localize]
        public string confirmTitleRef;
        [M8.Localize]
        public string confirmDescRef;

        [Header("UI")]
        public Button button;

        private M8.GenericParams mParms = new M8.GenericParams();

        public static bool allowRestart {
            get { return M8.SceneState.isInstantiated ? M8.SceneState.instance.local.GetValue(sceneVarName) != 0 : false; }
            set {
                if(M8.SceneState.isInstantiated)
                    M8.SceneState.instance.local.SetValue(sceneVarName, value ? 1 : 0, false);
            }
        }

        void Awake() {
            if(button)
                button.onClick.AddListener(OnClick);
        }

        void M8.IModalPush.Push(M8.GenericParams parms) {
            if(button)
                button.interactable = allowRestart;
        }

        void OnClick() {
            mParms[ModalConfirm.parmTitleTextRef] = confirmTitleRef;
            mParms[ModalConfirm.parmDescTextRef] = confirmDescRef;
            mParms[ModalConfirm.parmCallback] = (System.Action<bool>)OnRestartConfirm;

            if(modalManager)
                modalManager.Open(modalConfirm, mParms);
            else
                M8.ModalManager.main.Open(modalConfirm, mParms);
        }

        void OnRestartConfirm(bool confirm) {
            if(confirm)
                M8.SceneManager.instance.Reload();
        }

        void OnSceneVarChange(string var, M8.SceneState.StateValue val) {
            if(var == sceneVarName) {
                if(button)
                    button.interactable = val.ival != 0;
            }
        }
    }
}