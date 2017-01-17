using UnityEngine;
using System.Collections;

public class FlockFilter : MonoBehaviour {
    public int id = 1;
    public int avoidTypeFilter; //which types to avoid, like the plague

    private FlockUnit mFlockUnit;
    private Rigidbody2D mBody;

    public Rigidbody2D body { get { return mBody; } }

    public bool isLegit { get { return mFlockUnit != null && mFlockUnit.groupMoveEnabled; } }

    public void Awake() {
        mFlockUnit = GetComponent<FlockUnit>();
        mBody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Check given 'other' to see if we should avoid it.
    /// </summary>
    public bool CheckAvoid(int otherId) {
        return (avoidTypeFilter & (1 << otherId)) != 0;
    }
}
