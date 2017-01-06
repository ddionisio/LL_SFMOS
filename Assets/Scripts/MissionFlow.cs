using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionFlow : MonoBehaviour {
    public void Play() {
        MissionManager.instance.Play();
    }
    
    public void Quiz() {
        MissionManager.instance.Quiz();
    }
}
