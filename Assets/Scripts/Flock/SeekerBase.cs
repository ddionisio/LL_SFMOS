using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekerBase : MonoBehaviour {
    public delegate void PathCallback(SeekerBase seeker, Vector2[] path);

    public event PathCallback pathCallback;

    public bool StartPath(Vector2 start, Vector2 dest) {
        return false;
    }
}
