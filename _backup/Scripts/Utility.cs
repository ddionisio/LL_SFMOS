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

    public static string[] GrabLocalizeGroup(string s) {
        //remove numbers at the end
        int numberEndInd = -1;
        for(int i = s.Length - 1; i >= 0; i--) {
            if(s[i] >= '0' && s[i] <= '9')
                numberEndInd = i;
            else
                break;
        }

        string[] ret;

        if(numberEndInd > 0) {
            //split
            string baseS = s.Substring(0, numberEndInd);
            string numS = s.Substring(numberEndInd, s.Length - numberEndInd);

            int ind;
            int.TryParse(numS, out ind);

            List<string> refs = new List<string>();

            while(true) {
                string subS = baseS + ind.ToString();
                if(M8.Localize.instance.Exists(subS))
                    refs.Add(subS);
                else
                    break;

                ind++;
            }

            ret = refs.ToArray();
        }
        else
            ret = new string[] { s };

        return ret;
    }
}
