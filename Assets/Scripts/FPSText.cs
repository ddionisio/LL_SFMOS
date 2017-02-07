using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FPSText : MonoBehaviour {
    public Text label;

    private float mLastDeltaTime;

	void Update () {
        mLastDeltaTime += (Time.deltaTime - mLastDeltaTime) * 0.1f;
        
        float fps = 1.0f / mLastDeltaTime;
        label.text = fps.ToString();
    }
}
