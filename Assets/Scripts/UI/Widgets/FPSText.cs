using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FPSText : MonoBehaviour {
    public Text label;

    private float mLastRT;
    private float mLastDeltaTime;

    void Start() {
        mLastRT = Time.realtimeSinceStartup;
    }

	void Update () {
        float rt = Time.realtimeSinceStartup;
        float deltaRT = rt - mLastRT;
        mLastRT = rt;

        mLastDeltaTime += (deltaRT - mLastDeltaTime) * 0.1f;
        
        float fps = 1.0f / mLastDeltaTime;
        label.text = fps.ToString("###.##");
    }
}
