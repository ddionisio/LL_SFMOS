using UnityEngine;
using System.Collections;

[AddComponentMenu("M8/2D/MotionBase")]
public class MotionBase : MonoBehaviour {
	public virtual float maxSpeed { get { return 20.0f; } } //meters/sec
	
	protected Vector2 mDir = Vector2.right;
	protected float mCurSpeed;
	
	private float mMaxSpeed;
	private float mMaxSpeedScale = 1.0f;
	
	private Rigidbody2D mBody;
		
	public Vector2 dir {
		get { return mDir; }
	}
	
	public Rigidbody2D body {
		get { return mBody; }
	}
	
	public float curSpeed {
		get { return mCurSpeed; }
	}
	
	public float maxSpeedScale {
		get { return mMaxSpeedScale; }
		set {
			if(mMaxSpeedScale != value) {
				mMaxSpeedScale = value;
				mMaxSpeed = maxSpeed*mMaxSpeedScale;
				UpdateVelocity();
			}
		}
	}
	
	public virtual void ResetData() {
		if(mBody != null) {
			mBody.isKinematic = false;
			mBody.velocity = Vector3.zero;
		}
		
		mMaxSpeed = maxSpeed;
		mMaxSpeedScale = 1.0f;
	}
	
	protected virtual void Awake() {
        mBody = GetComponent<Rigidbody2D>();
        
        mMaxSpeed = maxSpeed;
	}
	
	protected virtual void FixedUpdate() {
		//get direction and limit speed
		UpdateVelocity();
	}
	
	void UpdateVelocity() {
		Vector2 vel = mBody.velocity;
		mCurSpeed = vel.magnitude;
		
		if(mCurSpeed > 0) {
			mDir = vel/mCurSpeed;
			
			if(mCurSpeed > mMaxSpeed) {
				mBody.velocity = mDir*mMaxSpeed;
				mCurSpeed = mMaxSpeed;
			}
		}
	}
}
