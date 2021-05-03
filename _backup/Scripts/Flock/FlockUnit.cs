using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockUnit : MonoBehaviour {
    public enum State {
        Idle, //no cohesion, move, alignment
        Move, //move toward moveTarget, will use seek if blocked
        Waypoint, //go through generated waypoint
        Wander, //cohesion and alignment
        //Disperse //anti-cohesion and no alignment
    }

    public FlockUnitData data;

    public M8.SensorCollider2D sensor;
    public SeekerBase seeker; //use for when our target is blocked
        
    [System.NonSerialized]
    public bool groupMoveEnabled = true; //false = no cohesion and alignment
    [System.NonSerialized]
    public bool catchUpEnabled = true; //false = don't use catch up factor

    //Set unit to wander if there's no move target 
    public bool wanderEnabled {
        get { return mWanderEnabled; }
        set {
            mWanderEnabled = value;
            if(mWanderEnabled && mState == State.Idle) {
                mState = State.Wander;
            }
        }
    }

    [System.NonSerialized]
    public float minMoveTargetDistance = 0.0f; //minimum distance to maintain from move target
    
    private Transform mMoveTarget = null;
    private float mMoveTargetDist = 0;
    private Vector2 mMoveTargetDir = Vector2.right;

    private float mCurUpdateDelay = 0;
    private float mCurSeekDelay = 0;
    private float mWanderStartTime = 0;

    private Rigidbody2D mBody;
    private Collider2D mColl;
    private Transform mTrans;
    
    private Vector2[] mSeekPath = null;
    private int mSeekCurPath = -1;
    private bool mSeekStarted = false;

    private bool mWallCheck = false;
    private RaycastHit2D mWallHit = new RaycastHit2D();

    private float mRadius = 0f;

    private State mState = State.Move;

    private bool mWanderEnabled = false;
    private Vector2 mWanderOrigin;

    private float mMaxSpeedScale = 1.0f;
    private float mMoveScale = 1.0f;

    public float maxSpeedScale { get { return mMaxSpeedScale; } set { mMaxSpeedScale = value; } }

    public Rigidbody2D body {
        get { return mBody; }
    }

    public Collider2D coll {
        get { return mColl; }
    }

    public Transform moveTarget {
        get { return mMoveTarget; }

        set {
            if(mMoveTarget != value) {
                mMoveTarget = value;

                SeekPathStop();
            }
        }
    }

    public float moveTargetDistance {
        get { return mMoveTargetDist; }
    }

    /// <summary>
    /// Direction from unit towards move target. (Note: this is updated based on seek delay or update delay)
    /// </summary>
    public Vector2 moveTargetDir {
        get { return mMoveTargetDir; }
    }
        
    public float moveScale {
        get {
            return mMoveScale;
        }

        set {
            mMoveScale = value;
        }
    }

    public virtual void Stop() {
        moveTarget = null;
        minMoveTargetDistance = 0.0f;
    }

    public void ResetData() {
        wanderEnabled = false;
        groupMoveEnabled = true;
        catchUpEnabled = true;
        minMoveTargetDistance = 0.0f;
        moveTarget = null;

        mMaxSpeedScale = 1.0f;

        mMoveScale = 1.0f;

        sensor.items.Clear();
    }

    protected virtual void OnDestroy() {
        if(seeker)
            seeker.pathCallback -= OnSeekPathComplete;
    }

    protected void Awake() {

        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<Collider2D>();
        mTrans = transform;

        if(seeker)
            seeker.pathCallback += OnSeekPathComplete;

        mRadius = ComputeRadius(mColl);
    }

    float ComputeRadius(Collider2D coll) {
        if(coll is CircleCollider2D)
            return ((CircleCollider2D)coll).radius;
        else {
            var ext = coll.bounds.extents;
            return Mathf.Max(ext.x, ext.y);
        }
    }

    protected virtual void Update() {
        Vector2 pos = transform.position;

        //check current pathing
        if(moveTarget != null) {
            if(mSeekStarted) {
                if(mSeekPath != null) {
                    mCurSeekDelay += Time.deltaTime;
                    if(mCurSeekDelay >= data.seekDelay) {
                        //check if target is blocked, also update move dir
                        Vector2 dest = moveTarget.position;
                        mMoveTargetDir = (dest - pos);
                        mMoveTargetDist = mMoveTargetDir.magnitude;
                        mMoveTargetDir /= mMoveTargetDist;

                        //check to see if destination has changed or no longer blocked
                        if(dest != mSeekPath[mSeekPath.Length - 1]
                            || !CheckTargetBlock(pos, mMoveTargetDir, mMoveTargetDist - minMoveTargetDistance, mRadius)) {
                            SeekPathStop();
                        }
                        else {
                            mCurSeekDelay = 0.0f;
                        }
                    }
                }
            }
            else {
                mCurSeekDelay += Time.deltaTime;
                if(mCurSeekDelay >= data.seekDelay) {
                    //check if target is blocked, also update move dir
                    Vector2 dest = moveTarget.position;
                    mMoveTargetDir = (dest - pos);
                    mMoveTargetDist = mMoveTargetDir.magnitude;
                    mMoveTargetDir /= mMoveTargetDist;

                    mCurSeekDelay = 0.0f;

                    if(seeker != null && CheckTargetBlock(pos, mMoveTargetDir, mMoveTargetDist - minMoveTargetDistance, mRadius))
                        SeekPathStart(pos, dest);
                }
            }
        }

        //check wall        
        //mWallHit = Physics2D.CircleCast(pos, data.wallRadius, dir, 0.1f, data.wallMask.value);
        //mWallCheck = mWallHit.collider != null;
    }

    void ApplyForce(Vector2 force) {
        float curSpdSqr = body.velocity.sqrMagnitude;
        float maxSpdSqr = data.maxSpeed*mMaxSpeedScale; maxSpdSqr *= maxSpdSqr;

        if(curSpdSqr < maxSpdSqr)
            body.AddForce(force);
    }

    // Update is called once per frame
    protected void FixedUpdate() {
        if(mState == State.Waypoint) {
            //need to keep checking per frame if we have reached a waypoint
            Vector2 desired = mSeekPath[mSeekCurPath] - (Vector2)transform.position;

            //check if we need to move to next waypoint
            if(desired.sqrMagnitude < data.pathRadius * data.pathRadius) {
                //path complete?
                int nextPath = mSeekCurPath + 1;
                if(nextPath == mSeekPath.Length) {
                    SeekPathStop();
                }
                else {
                    mSeekCurPath = nextPath;
                }
            }
            else {
                mCurUpdateDelay += Time.fixedDeltaTime;
                if(mCurUpdateDelay >= data.updateDelay) {
                    mCurUpdateDelay = 0;

                    //continue moving to wp
                    desired.Normalize();
                    Vector2 sumForce = ComputeSeparate() + desired * (data.maxForce * data.pathFactor);
                    ApplyForce(sumForce);
                }
            }
        }
        else {
            mCurUpdateDelay += Time.fixedDeltaTime;
            if(mCurUpdateDelay >= data.updateDelay) {
                mCurUpdateDelay = 0;

                int numFollow = 0;

                Vector2 sumForce = Vector2.zero;

                switch(mState) {
                    case State.Move:
                        if(moveTarget == null) {
                            ApplyState(wanderEnabled ? State.Wander : State.Idle);
                        }
                        else {
                            //move to destination
                            Vector2 pos = mTrans.position;
                            Vector2 dest = moveTarget.position;
                            Vector2 _dir = dest - pos;
                            mMoveTargetDist = _dir.magnitude;

                            sumForce = groupMoveEnabled ? ComputeMovement(out numFollow) : ComputeSeparate();

                            if(mMoveTargetDist > 0) {
                                //catch up?
                                float factor = catchUpEnabled
                                    && (sensor == null || numFollow == 0)
                                    && mMoveTargetDist > data.catchUpMinDistance ?
                                        data.catchUpFactor : data.moveToFactor;

                                factor *= mMoveScale;

                                //determine direction if distance is too close
                                _dir /= mMoveTargetDist < minMoveTargetDistance ? -mMoveTargetDist : mMoveTargetDist;

                                mMoveTargetDir = _dir;

                                sumForce += M8.MathUtil.Steer(body.velocity, _dir * data.maxSpeed, data.maxForce, factor);
                            }
                        }
                        break;

                    case State.Wander:
                        if(Time.fixedTime - mWanderStartTime >= data.wanderDelay) {
                            WanderRefresh();
                        }

                        sumForce = ComputeSeparate() + M8.MathUtil.Steer(body.velocity, mMoveTargetDir * data.maxSpeed, data.maxForce, data.moveToFactor);
                        break;

                    default:
                        sumForce = ComputeMovement(out numFollow); //ComputeSeparate();
                        break;
                }

                ApplyForce(sumForce);
            }
        }

        if(mWallCheck) {
            Vector2 wall = Wall();
            ApplyForce(wall);
        }
    }

    void OnDrawGizmosSelected() {
        if(data == null)
            return;

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, data.pathRadius);

        Gizmos.color *= 1.25f;
        Gizmos.DrawWireSphere(transform.position, data.wallRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.separateDistance);

        Gizmos.color *= 0.75f;
        Gizmos.DrawWireSphere(transform.position, data.avoidDistance);

        if(data.wanderRestrict) {
            Gizmos.color = Color.cyan;
            Gizmos.color *= 0.75f;
            Gizmos.DrawWireSphere(transform.position, data.wanderRestrictRadius);
        }
    }

    void OnSeekPathComplete(SeekerBase aSeeker, Vector2[] p) {
        if(p == null || p.Length == 0) {
            SeekPathStop();
        }
        else {
            mSeekPath = p;
            mSeekCurPath = 0;

            ApplyState(State.Waypoint);
        }
    }

    private bool CheckTargetBlock(Vector2 pos, Vector2 dir, float dist, float radius) {
        if(dist > 0.0f) {
            Ray ray = new Ray(pos, dir / dist);
            return Physics.SphereCast(ray, data.pathRadius, dist, data.wallMask.value);
        }
        else {
            return false;
        }
    }

    private void SeekPathStart(Vector2 start, Vector2 dest) {
        if(seeker.StartPath(start, dest)) {
            mSeekPath = null;
            mCurSeekDelay = 0.0f;
            mSeekStarted = true;

            ApplyState(State.Idle);
        }
    }

    private void SeekPathStop() {
        mSeekPath = null;
        mCurSeekDelay = 0.0f;
        mSeekStarted = false;
        mSeekCurPath = -1;

        ApplyState(mMoveTarget == null ? wanderEnabled ? State.Wander : State.Idle : State.Move);
    }

    private void ApplyState(State toState) {

        mState = toState;

        switch(mState) {
            case State.Wander:
                mWanderOrigin = transform.position;

                WanderRefresh();
                break;
        }
    }

    private void WanderRefresh() {
        mWanderStartTime = Time.fixedTime;

        if(data.wanderRestrict) {
            Vector2 curPos = transform.position;
            Vector2 dpos = mWanderOrigin - curPos;
            float sqrDist = dpos.sqrMagnitude;
            if(sqrDist > data.wanderRestrictRadius*data.wanderRestrictRadius) {
                mMoveTargetDir = dpos/Mathf.Sqrt(sqrDist);
            }
            else
                mMoveTargetDir = Random.onUnitSphere;
        }
        else
            mMoveTargetDir = Random.onUnitSphere;
    }

    //use if mWallCheck is true
    private Vector2 Wall() {
        return M8.MathUtil.Steer(body.velocity, mWallHit.normal * data.maxSpeed, data.maxForce, data.wallFactor);
    }

    private Vector2 Seek(Vector2 target, float factor) {
        Vector2 pos = mTrans.position;

        Vector2 desired = target - pos;
        desired.Normalize();

        return M8.MathUtil.Steer(body.velocity, desired * data.maxSpeed, data.maxForce, factor);
    }

    //use for idle, waypoint, etc.
    private Vector2 ComputeSeparate() {
        Vector2 forceRet = Vector2.zero;

        if(sensor != null && sensor.items.Count > 0) {
            Vector2 separate = Vector2.zero;
            Vector2 avoid = Vector2.zero;

            Vector2 pos = mTrans.position;

            Vector2 dPos;
            float dist;

            int numSeparate = 0;
            int numAvoid = 0;

            foreach(var unit in sensor.items) {
                if(unit != null) {
                    Vector2 otherPos = unit.transform.position;
                    float otherRadius = ComputeRadius(unit);

                    dPos = pos - otherPos;
                    dist = dPos.magnitude - otherRadius;

                    if(data.CheckAvoid(unit.tag)) {
                        //avoid
                        if(dist < mRadius + data.avoidDistance) {
                            dPos /= dist;
                            avoid += dPos;
                            numAvoid++;
                        }
                    }
                    else {
                        //separate	
                        if(dist < mRadius + data.separateDistance) {
                            dPos /= dist;
                            separate += dPos;
                            numSeparate++;
                        }
                    }
                }
            }

            //calculate avoid
            if(numAvoid > 0) {
                avoid /= (float)numAvoid;

                dist = avoid.magnitude;
                if(dist > 0) {
                    avoid /= dist;
                    forceRet += M8.MathUtil.Steer((Vector2)body.velocity, avoid * data.maxSpeed, data.maxForce, data.avoidFactor);
                }
            }

            //calculate separate
            if(numSeparate > 0) {
                separate /= (float)numSeparate;

                dist = separate.magnitude;
                if(dist > 0) {
                    separate /= dist;
                    forceRet += M8.MathUtil.Steer((Vector2)body.velocity, separate * data.maxSpeed, data.maxForce, data.separateFactor);
                }
            }


        }

        return forceRet;
    }

    private Vector2 ComputeMovement(out int numFollow) {
        Vector2 forceRet = Vector2.zero;

        numFollow = 0;

        if(sensor != null && sensor.items.Count > 0) {
            Vector2 separate = Vector2.zero;
            Vector2 align = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 avoid = Vector2.zero;

            Vector2 pos = mTrans.position;

            Vector2 dPos;
            float dist, distOfs;

            int numSeparate = 0;
            int numAvoid = 0;

            foreach(var unit in sensor.items) {
                if(unit != null) {
                    Vector2 otherPos = unit.transform.position;
                    float otherRadius = ComputeRadius(unit);

                    dPos = pos - otherPos;
                    dist = dPos.magnitude;
                    distOfs = dist - otherRadius;

                    if(data.CheckAvoid(unit.tag)) {
                        //avoid
                        if(distOfs < mRadius + data.avoidDistance) {
                            dPos /= dist;
                            avoid += dPos;
                            numAvoid++;
                        }
                    }
                    else { 
                        //separate	
                        if(distOfs < mRadius + data.separateDistance) {
                            dPos /= dist;
                            separate += dPos;
                            numSeparate++;
                        }

                        //only follow if the same group
                        var otherFlockUnit = unit.GetComponent<FlockUnit>();
                        if(otherFlockUnit && otherFlockUnit.groupMoveEnabled && data.group == otherFlockUnit.data.group) {
                            Rigidbody2D otherBody = otherFlockUnit.body;
                            if(otherBody != null && !otherBody.isKinematic) {
                                //align speed
                                Vector2 vel = otherBody.velocity;
                                align += vel;

                                //cohesion
                                cohesion += otherPos;

                                numFollow++;
                            }
                        }
                    }
                }
            }

            //calculate avoid
            if(numAvoid > 0) {
                avoid /= (float)numAvoid;

                dist = avoid.magnitude;
                if(dist > 0) {
                    avoid /= dist;
                    forceRet += M8.MathUtil.Steer(body.velocity, avoid * data.maxSpeed, data.maxForce, data.avoidFactor);
                }
            }

            //calculate separate
            if(numSeparate > 0) {
                separate /= (float)numSeparate;

                dist = separate.magnitude;
                if(dist > 0) {
                    separate /= dist;
                    forceRet += M8.MathUtil.Steer(body.velocity, separate * data.maxSpeed, data.maxForce, data.separateFactor);

                }
            }

            if(numFollow > 0) {
                float fCount = (float)numFollow;

                //calculate align
                align /= fCount;
                align.Normalize();
                align = M8.MathUtil.Steer(body.velocity, align * data.maxSpeed, data.maxForce, data.alignFactor);

                //calculate cohesion
                cohesion /= fCount;
                cohesion = Seek(cohesion, data.cohesionFactor);

                forceRet += align + cohesion;
            }
        }

        return forceRet;
    }
}
