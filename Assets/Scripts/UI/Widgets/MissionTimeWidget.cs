using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionTimeWidget : MonoBehaviour {
    public Text text;
    
    void OnDisable() {
        if(HUD.instance)
            HUD.instance.timeUpdateCallback -= OnTimeUpdate;
    }

    void OnEnable() {
        HUD.instance.timeUpdateCallback += OnTimeUpdate;
    }

    void OnTimeUpdate(float time) {
        //do fancy stuff
        text.text = time.ToString("sss\\.ff");
    }
}
