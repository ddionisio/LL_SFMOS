using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MucusGather : MonoBehaviour {
    public CircleCollider2D sphereCollider;

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

    private float mRadius;
        
    public bool isActive { get { return mIsActive; } }

    public float radius {
        get {
            return mRadius;
        }
    }

    public bool Contains(Vector2 pos) {
        float r = radius;
        Vector2 _pos = mucusFormSpawnAt.position;

        return (_pos - pos).sqrMagnitude <= r*r;
    }
        
    public void Activate() {
        if(mIsActive)
            return;

        mIsActive = true;

        gameObject.SetActive(true);

        sphereCollider.enabled = true;

        //play active
        if(animator && mTakeActivateInd != -1)
            animator.Play(mTakeActivateInd);
    }
    
    void Inactive() {
        sphereCollider.enabled = false;

        //play inactive
        if(animator && mTakeDeactivateInd != -1)
            animator.Play(mTakeDeactivateInd);
        else
            gameObject.SetActive(false);
    }

    public void Release(Vector2 dir, float dist) {
        if(!mIsActive)
            return;

        mIsActive = false;

        //revert currently gathering mucuses, release those that are already gathered
        for(int i = 0; i < mMucusGathering.Count; i++) {
            if(mMucusGathering[i]) {
                if(mMucusGathering[i].state == (int)EntityState.Gather)
                    mMucusGathering[i].SetGather(null);
                else if(mMucusGathering[i].state == (int)EntityState.Gathered)
                    mMucusGathering[i].Release();

                mMucusGathering[i].setStateCallback -= OnMucusStateChanged;
            }
        }

        mMucusGathering.Clear();

        //launch mucus form if available
        if(mSpawnedMucusForm) {
            mSpawnedMucusForm = null;
        }

        Inactive();
    }

    public void Cancel() {
        if(!mIsActive)
            return;

        mIsActive = false;

        //revert all mucus
        for(int i = 0; i < mMucusGathering.Count; i++) {
            if(mMucusGathering[i]) {
                if(mMucusGathering[i].state == (int)EntityState.Gather || mMucusGathering[i].state == (int)EntityState.Gathered)
                    mMucusGathering[i].SetGather(null);

                mMucusGathering[i].setStateCallback -= OnMucusStateChanged;
            }
        }

        mMucusGathering.Clear();

        if(mSpawnedMucusForm) {
            mSpawnedMucusForm.Cancel();
            mSpawnedMucusForm = null;
        }

        Inactive();
    }

    void OnTriggerEnter2D(Collider2D other) {        
        //check if we are full
        if(mSpawnedMucusForm && mSpawnedMucusForm.currentGrowthCount >= mSpawnedMucusForm.growthMaxCount)
            return;

        var mucus = other.GetComponent<EntityMucus>();

        if(mucus && !mMucusGathering.Contains(mucus)) {
            mucus.SetGather(mucusFormSpawnAt);

            mucus.setStateCallback += OnMucusStateChanged;

            mMucusGathering.Add(mucus);
        }
    }
    
    //void OnTriggerExit2D(Collider2D other) {
    //}

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorComplete;
    }

    void Awake() {
        if(!mucusFormSpawnAt)
            mucusFormSpawnAt = transform;

        mRadius = sphereCollider ? sphereCollider.radius : 1.0f;

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

    void Grow() {
        if(mSpawnedMucusForm)
            mSpawnedMucusForm.Grow();
        else {
            //generate a new form
            Vector2 spawnPos = mucusFormSpawnAt.position;
            var spawned = M8.PoolController.SpawnFromGroup(mucusFormPoolGroup, mucusFormSpawnRef, mucusFormSpawnRef, null, spawnPos, null);
            mSpawnedMucusForm = spawned.GetComponent<EntityMucusForm>();
        }

        //check if form becomes bigger than us
        //make our area bigger
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takeDeactivate)
            gameObject.SetActive(false);
    }

    void OnMucusStateChanged(M8.EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Gathered:
                Grow();
                break;
        }
    }
}
