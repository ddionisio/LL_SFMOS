using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLSpeakTextListProxy : MonoBehaviour {
        [M8.Localize]
        public string[] keys;
        public string playGroup;
        public int playStartIndex;
        public bool clearQueue = true;

        public void Play() {
            if(string.IsNullOrEmpty(playGroup)) //requires a group
                return;

            if(clearQueue)
                LoLManager.instance.StopSpeakQueue();
            
            for(int i = 0; i < keys.Length; i++)
                LoLManager.instance.SpeakTextQueue(keys[i], playGroup, playStartIndex + i);
        }
    }
}