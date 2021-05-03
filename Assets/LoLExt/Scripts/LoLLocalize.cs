using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace LoLExt {
    [CreateAssetMenu(fileName = "localize", menuName = "LoL/Localize")]
    public class LoLLocalize : M8.Localize, ISerializationCallbackReceiver {
        [System.Serializable]
        public class LanguageExtraInfo {
            public string key;
            public float voiceDuration;
        }

#if UNITY_EDITOR
        [Header("Editor")]
        [M8.FileSystemPath("Open Excel File", "Excel files;*.xls;*.xlsx")]
        public string editorExcelPath;

        public string editorLanguageCode = "en";
        public string editorLanguageRef = "language.json";

        /// <summary>
        /// used for mockup.
        /// </summary>
        [M8.FileSystemPath("Open JSON File", "json")]
        public string editorLanguagePathExtra;

        public string editorLanguagePath {
            get {
                return System.IO.Path.Combine(Application.streamingAssetsPath, editorLanguageRef);
            }
        }
#endif

        [Header("Extra Info")]
        public LanguageExtraInfo[] languageExtraInfos;

        private Dictionary<string, M8.LocalizeData> mEntries;
        private Dictionary<string, LanguageExtraInfo> mEntryExtras;

        private string mCurLang;

        public bool isLoaded {
            get {
                return mCurLang != null;
            }
        }

        public override string[] languages {
            get {
                return new string[] { mCurLang };
            }
        }

        public override int languageCount {
            get {
                return 1;
            }
        }

        /// <summary>
        /// Called via LoLManager when language data is received
        /// </summary>
        public void Load(string langCode, string json) {
            mCurLang = langCode;
            if(mCurLang == null) //langCode shouldn't be null
                mCurLang = "";

            //load up the language
            Dictionary<string, object> defs;
            if(!string.IsNullOrEmpty(json)) {
                defs = Json.Deserialize(json) as Dictionary<string, object>;
            }
            else
                defs = new Dictionary<string, object>();

            mEntries = new Dictionary<string, M8.LocalizeData>(defs.Count);

            foreach(var item in defs) {
                string key = item.Key;
                string val = item.Value.ToString();

                M8.LocalizeData dat = new M8.LocalizeData(val, new string[0]);

                mEntries.Add(key, dat);
            }

            Refresh();
        }

        /// <summary>
        /// Called in editor when localization has been updated
        /// </summary>
        public void ClearEntries() {
            mEntries = null;
        }

        public LanguageExtraInfo GetExtraInfo(string key) {
            LanguageExtraInfo ret;
            if(!mEntryExtras.TryGetValue(key, out ret)) {
                Debug.LogWarning("No extra info for: " + key);
            }

            return ret;
        }

        public override bool Exists(string key) {
#if UNITY_EDITOR
            if(mEntries == null)
                LoadFromReference();
#endif

            if(mEntries == null)
                return false;

            return mEntries.ContainsKey(key);
        }

        public override bool IsLanguageFile(string filepath) {
#if UNITY_EDITOR
            return filepath.Contains(editorLanguageRef);
#else
        return false;
#endif
        }

        public override int GetLanguageIndex(string lang) {
            if(lang == mCurLang)
                return 0;

            return -1;
        }

        public override string GetLanguageName(int langInd) {
            if(langInd == 0)
                return mCurLang;

            return "";
        }

        protected override void HandleLoad() {

        }

        protected override void HandleLanguageChanged() {
            //Language is not changed via language for LoL, use Load
        }

        protected override string[] HandleGetKeys() {
#if UNITY_EDITOR
            if(mEntries == null)
                LoadFromReference();
#endif

            if(mEntries == null)
                return new string[0];

            var keyColl = mEntries.Keys;
            var keys = new string[keyColl.Count];
            keyColl.CopyTo(keys, 0);

            return keys;
        }

        protected override bool TryGetData(string key, out M8.LocalizeData data) {
#if UNITY_EDITOR
            if(mEntries == null)
                LoadFromReference();
#endif

            if(mEntries == null) {
                data = new M8.LocalizeData();
                return false;
            }

            return mEntries.TryGetValue(key, out data);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            mEntryExtras = new Dictionary<string, LanguageExtraInfo>();

            if(languageExtraInfos != null) {
                for(int i = 0; i < languageExtraInfos.Length; i++)
                    mEntryExtras.Add(languageExtraInfos[i].key, languageExtraInfos[i]);
            }
        }

#if UNITY_EDITOR
        private void LoadFromReference() {
            if(string.IsNullOrEmpty(editorLanguageRef))
                return;

            string filepath = editorLanguagePath;

            string json = System.IO.File.ReadAllText(filepath);

            var defs = Json.Deserialize(json) as Dictionary<string, object>;

            Load(editorLanguageCode, Json.Serialize(defs[editorLanguageCode]));
        }
#endif
    }
}