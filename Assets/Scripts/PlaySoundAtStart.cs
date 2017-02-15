using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAtStart : MonoBehaviour {
    public string path;
    public bool background;
    public bool loop;
    
	void Start () {
        LoLManager.instance.PlaySound(path, background, loop);
	}
}
