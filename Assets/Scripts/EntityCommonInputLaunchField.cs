using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EntityCommonInputLaunchField : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler {

    public EntityCommonSpawnLaunch[] spawnLaunches;
        
    public Transform pointer;
    public GameObject pointerGO;

    [Header("Launch Info")]
    public Transform launchPoint;
    public float launchRadiusMin;
    public float launchAngleLimit = 60f;
    public float launchForceMin;
    public float launchForceMax;
    public float launchForceMaxDistance = 4.0f;
    public AnimationCurve launchForceCurve;

    private PointerEventData mLastPointerEventEnter;
    private EntityCommonSpawnLaunch mCurSpawnLauncherReady;

    private Coroutine mRout;

    private bool mIsPointerActive;
    private Vector2 mLastDir;
    private float mLastDist;

    void OnDestroy() {
        for(int i = 0; i < spawnLaunches.Length; i++) {
            if(spawnLaunches[i])
                spawnLaunches[i].launchReadyCallback -= OnSpawnLauncherReady;
        }
    }

    void Awake() {
        for(int i = 0; i < spawnLaunches.Length; i++) {
            if(spawnLaunches[i])
                spawnLaunches[i].launchReadyCallback += OnSpawnLauncherReady;
        }

        if(pointerGO)
            pointerGO.SetActive(false);

        if(pointer)
            pointer.gameObject.SetActive(false);
    }

    void SetPointerActive(bool active) {
        if(mIsPointerActive != active) {
            mIsPointerActive = active;

            if(pointerGO)
                pointerGO.SetActive(mIsPointerActive);

            if(pointer)
                pointer.gameObject.SetActive(mIsPointerActive);
        }
    }

    public float GetLaunchForce() {
        float t = Mathf.Clamp01(launchForceCurve.Evaluate(Mathf.Clamp01((mLastDist - launchRadiusMin)/launchForceMaxDistance)));
        return Mathf.Lerp(launchForceMin, launchForceMax, t);
    }

    void OnSpawnLauncherReady(EntityCommonSpawnLaunch launcher, bool ready) {
        if(ready) {
            mCurSpawnLauncherReady = launcher;

            SetPointerActive(true);

            if(mRout == null) {
                mRout = StartCoroutine(DoLaunchMouseOver());
            }
        }
        else {
            if(mCurSpawnLauncherReady == launcher) {
                mCurSpawnLauncherReady = null;

                SetPointerActive(false);

                if(mRout != null) {
                    StopCoroutine(mRout);
                    mRout = null;
                }
            }
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(mCurSpawnLauncherReady && mIsPointerActive) {
            mCurSpawnLauncherReady.Launch(mLastDir, GetLaunchForce());
        }
    }
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        mLastPointerEventEnter = eventData;
    }

    IEnumerator DoLaunchMouseOver() {
        while(mCurSpawnLauncherReady) {
            //launchAngleLimit
            Vector2 pos = mLastPointerEventEnter.pointerPressRaycast.worldPosition;
            Vector2 pointerPos = pointer.position;

            mLastDir = pos - pointerPos;
            mLastDist = mLastDir.magnitude;
            if(mLastDist > 0f)
                mLastDir /= mLastDist;

            bool valid = mLastPointerEventEnter != null && Vector2.Angle(Vector2.up, mLastDir) <= launchAngleLimit && mLastDist >= launchRadiusMin;

            SetPointerActive(valid);

            if(valid) {
                pointer.position = mLastPointerEventEnter.pointerPressRaycast.worldPosition;
            }

            yield return null;
        }

        mRout = null;
    }

    void OnDrawGizmos() {
        var bound = new Bounds();
        BoxCollider2D bc2D = GetComponent<BoxCollider2D>();
        if(bc2D != null) {
            bound.center = bc2D.offset;
            bound.extents = new Vector3(bc2D.size.x*transform.localScale.x, bc2D.size.y*transform.localScale.y, 0f) * 0.5f;
        }

        if(bound.size.x + bound.size.y + bound.size.z > 0) {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(transform.position + bound.center, bound.size);
        }

        if(launchPoint && launchRadiusMin > 0f) {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(launchPoint.position, launchRadiusMin);
        }
    }
}
