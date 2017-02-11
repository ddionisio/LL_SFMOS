using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utility {
    private static System.Text.StringBuilder mStringBuff;

    private static System.Text.StringBuilder InitStringBuffer(int capacity) {
        if(mStringBuff == null)
            mStringBuff = new System.Text.StringBuilder(capacity);
        else {
            if(mStringBuff.Length > 0)
                mStringBuff.Remove(0, mStringBuff.Length);

            if(mStringBuff.Capacity < capacity)
                mStringBuff.EnsureCapacity(capacity);
        }

        return mStringBuff;
    }

    public static string TimerFormat(float time) {
        var sb = InitStringBuffer(6);

        float seconds = Mathf.Floor(time);
        float centiseconds = Mathf.Floor((time - seconds)*100f);

        sb.Append((int)seconds);
        sb.Append('.');
        sb.Append(((int)centiseconds).ToString("D2"));

        return sb.ToString();
    }
}
