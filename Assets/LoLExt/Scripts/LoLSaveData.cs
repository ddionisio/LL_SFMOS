using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLSDK;

namespace LoLExt {
    [System.Serializable]
    public class LoLSaveState {
        public string encodedData;

        public byte[] DecodeRawData() {
            return System.Convert.FromBase64String(encodedData);
        }

        public void EncodeRawData(byte[] rawData) {
            if(rawData != null)
                encodedData = System.Convert.ToBase64String(rawData);
            else
                encodedData = "";
        }
    }

    [CreateAssetMenu(fileName = "userData", menuName = "LoL/userData")]
    public class LoLSaveData : M8.UserData {
        public int score { get; private set; }
        public int currentProgress { get; private set; }
        public int maximumProgress { get; private set; }

        private LoLSaveState mSaveState;

        /// <summary>
        /// Note: This must be called after LoL has instantiated and ready
        /// </summary>
        public override void Load() {
            isLoaded = false;

            LOLSDK.Instance.LoadState<LoLSaveState>(OnLoaded);
        }

        protected override byte[] LoadRawData() {
            return mSaveState != null ? mSaveState.DecodeRawData() : new byte[0];
        }

        protected override void SaveRawData(byte[] dat) {
            if(mSaveState == null) //fail-safe
                mSaveState = new LoLSaveState();

            mSaveState.EncodeRawData(dat);

            LOLSDK.Instance.SaveState(mSaveState);
        }

        protected override void DeleteRawData() {
            if(mSaveState == null) //fail-safe
                mSaveState = new LoLSaveState();

            mSaveState.EncodeRawData(new byte[0]);

            LOLSDK.Instance.SaveState(mSaveState);
        }

        void OnLoaded(State<LoLSaveState> state) {
            if(state != null) {
                score = state.score;
                currentProgress = state.currentProgress;
                maximumProgress = state.maximumProgress;

                mSaveState = state.data;
            }
            else {
                score = 0;
                currentProgress = 0;
                maximumProgress = 0;

                mSaveState = new LoLSaveState() { encodedData = "" };
            }

            base.Load();
        }
    }
}