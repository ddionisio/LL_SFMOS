using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MissionSelectItem : MonoBehaviour {
    public int missionIndex;

    public GameObject selectGO;
    public GameObject gradeAnchor;
    
    private static MissionSelectItem mCurSelect;
    
    public void Execute() {
        //if(mCurSelect == this)
            Play();
        //else
            //Select();
    }

    public void PointerEnter(PointerEventData eventData) {
        //Debug.LogWarning("blarg enter");
        Select();
    }

    public void PointerExit(PointerEventData eventData) {
        //Debug.LogWarning("blarg exit");
        Unselect();
    }

    void Select() {
        if(mCurSelect != null)
            mCurSelect.Unselect();

        mCurSelect = this;

        if(selectGO)
            selectGO.SetActive(true);
    }

    void Unselect() {
        if(selectGO)
            selectGO.SetActive(false);
    }

    void Play() {
        MissionManager.instance.Begin(missionIndex);
    }

    void OnDestroy() {
        if(mCurSelect == this)
            mCurSelect = null;
    }

    void Awake() {
        //check if it's already completed
        //apply grade
    }
}
