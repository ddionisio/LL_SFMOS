using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonCard : MonoBehaviour {
    public enum Type {
        Invalid = -1,

        //Systems
        CirculatorySystem,
        DigestiveSystem,
        ExcretorySystem,
        MuscularSystem,
        NervousSystem,
        RespiratorySystem,
        ReproductionSystem,

        //Sub systems
        Heart,
        Intestines,
        Kidney,
        Muscles,
        Brain,
        Lungs,
        Ovaries
    }

    [SerializeField]
    public Type type;
    [SerializeField]
    Bounds _bounds = new Bounds(); //local

    public SpriteRenderer spriteRender;

    public float moveSpeed = 8f;
    public float dragMoveDelay = 0.2f;

    public int orderOnDrag = 10;

    [Header("Animations")]
    public M8.Animator.AnimatorData animator;
    public string takeEnter;
    public string takeMove;
    public string takeDock;
    public string takePointerDown;
    public string takePointerDrag;
    public string takeIncorrect;
    
    private Coroutine mRout;
    private bool mIsDocked;

    private Vector2 mCurDragPos;
    private Vector2 mPlacedPos;

    private int mOrderDefault;
    
    public Bounds worldBounds {
        get {
            return new Bounds(transform.position + _bounds.center, _bounds.size);
        }
    }

    public bool isMoving {
        get {
            return mRout != null;
        }
    }

    public bool isDocked {
        get {
            return mIsDocked;
        }
    }

    public void Place(Vector2 pos) {
        mPlacedPos = pos;
        transform.position = pos;

        animator.Play(takeEnter);
    }
    
    public void PointerDown(Vector2 pos) {
        animator.Play(takePointerDown);
    }

    public void PointerDragUpdate(Vector2 pos) {
        mCurDragPos = pos;

        if(mRout == null) {
            animator.Play(takePointerDrag);

            mRout = StartCoroutine(DoDrag());
        }
    }

    public void Return() {
        mIsDocked = false;

        if(mRout != null)
            StopCoroutine(mRout);

        animator.Play(takeMove);

        mRout = StartCoroutine(DoMove(mPlacedPos));
    }

    public void ReturnIncorrect() {
        mIsDocked = false;

        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoIncorrect());
    }

    public void Dock(Vector2 pos) {
        mIsDocked = true;

        if(mRout != null)
            StopCoroutine(mRout);

        animator.Play(takeMove);

        mRout = StartCoroutine(DoMove(pos));
    }
    
    public void Reset() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(animator) {
            animator.Stop();

            int lastTakeInd = animator.lastPlayingTakeIndex;
            if(lastTakeInd != -1)
                animator.ResetTake(lastTakeInd);
        }

        mIsDocked = false;

        spriteRender.sortingOrder = mOrderDefault;
    }

    void OnDisable() {
        Reset();
    }

    void Awake() {
        mOrderDefault = spriteRender.sortingOrder;
    }

    IEnumerator DoDrag() {
        spriteRender.sortingOrder = orderOnDrag;

        Vector2 vel = Vector2.zero;

        while(true) {
            transform.position = Vector2.SmoothDamp(transform.position, mCurDragPos, ref vel, dragMoveDelay, float.MaxValue, Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator DoMove(Vector2 dest) {
        float delay = 0f;

        Vector2 start = transform.position;
        Vector2 dPos = dest - start;
        float sqrDist = dPos.sqrMagnitude;
        if(sqrDist > 0f)
            delay = Mathf.Sqrt(sqrDist)/moveSpeed;

        var eval = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);

        float curTime = 0f;

        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;
            if(curTime > delay) curTime = delay;

            float t = eval(curTime, delay, 0f, 0f);

            transform.position = Vector2.Lerp(start, dest, t);
        }

        mRout = null;

        //dock animation
        if(mIsDocked)
            animator.Play(takeDock);

        spriteRender.sortingOrder = mOrderDefault;
    }

    IEnumerator DoIncorrect() {
        animator.Play(takeIncorrect);
        while(animator.isPlaying)
            yield return null;

        mRout = null;

        Return();
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        var b = worldBounds;
        Gizmos.DrawWireCube(b.center, b.size);

    }
}
