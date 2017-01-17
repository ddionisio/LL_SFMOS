using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockTest : MonoBehaviour {
    public FlockUnit unit;

    public Transform target;

    public bool wanderEnable;
    
    void Awake() {
        unit.wanderEnabled = wanderEnable;

        unit.moveTarget = target;
    }
}
