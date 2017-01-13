using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MucusGather : MonoBehaviour {
    public Transform mucusFormSpawnAt;

    public M8.Animator.AnimatorData animator;

    [Header("Mucus Form Info")]
    public string mucusFormPoolGroup;
    public string mucusFormSpawnRef;

    [Header("Takes")]
    public string takeActivate;
    public string takeDeactivate;

    private bool mIsActive;

    private int mTakeActivateInd;
    private int mTakeDeactivateInd;

    private EntityMucusForm mSpawnedMucusForm;

    private List<EntityMucus> mMucusGathering;

    public bool isActive { get { return mIsActive; } }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;
    }

    void Awake() {
        if(animator) {
            animator.takeCompleteCallback += OnAnimatorComplete;

            mTakeActivateInd = animator.GetTakeIndex(takeActivate);
            mTakeDeactivateInd = animator.GetTakeIndex(takeDeactivate);
        }

        mMucusGathering = new List<EntityMucus>();

        //must be activated manually
        mIsActive = false;
        gameObject.SetActive(false);
    }

    public void Activate() {
        if(mIsActive)
            return;

        mIsActive = true;

        gameObject.SetActive(true);

        //play active
        if(animator && mTakeActivateInd != -1)
            animator.Play(mTakeActivateInd);
    }

    public void Release(Vector2 dir, float dist) {
        if(!mIsActive)
            return;

        mIsActive = false;

        //release from gathering mucuses
        for(int i = 0; i < mMucusGathering.Count; i++) {
            if(mMucusGathering[i])
                mMucusGathering[i].SetGather(null);
        }

        mMucusGathering.Clear();

        //launch mucus form if available
        if(mSpawnedMucusForm) {
            mSpawnedMucusForm = null;
        }

        //play inactive
        if(animator && mTakeDeactivateInd != -1)
            animator.Play(mTakeDeactivateInd);
        else
            gameObject.SetActive(false);
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takeDeactivate)
            gameObject.SetActive(false);
    }
}
