using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTypewriter : MonoBehaviour {
    public Text text;
    public float delay;

    public bool useRealTime;

    public event System.Action proceedCallback;
    public event System.Action doneCallback;

    public bool isTyping { get { return mRout != null; } }

    private string mString;
    private System.Text.StringBuilder mStringBuff;
    private Coroutine mRout;

    public void SetText(string s) {
        if(mRout != null)
            StopCoroutine(mRout);

        mString = s;

        mRout = StartCoroutine(DoType());
    }

    public void Skip() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;

            text.text = mString;

            if(doneCallback != null)
                doneCallback();
        }
    }
    
    void OnDisable() {
        text.text = "";

        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
    
    void Awake() {
        text.text = "";
    }

    IEnumerator DoType() {
        var wait = useRealTime ? null : new WaitForSeconds(delay);

        text.text = "";
        
        if(mStringBuff == null || mStringBuff.Capacity < mString.Length)
            mStringBuff = new System.Text.StringBuilder(mString.Length);
        else
            mStringBuff.Remove(0, mStringBuff.Length);

        int count = mString.Length;
        for(int i = 0; i < count; i++) {
            if(useRealTime) {
                var lastTime = Time.realtimeSinceStartup;
                while(Time.realtimeSinceStartup - lastTime < delay)
                    yield return null;
            }
            else
                yield return wait;

            mStringBuff.Append(mString[i]);

            text.text = mStringBuff.ToString();

            if(proceedCallback != null)
                proceedCallback();
        }

        mRout = null;

        if(doneCallback != null)
            doneCallback();
    }
}
