using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : M8.Sensor2D<FlockUnit> {
    public string[] tagFilter;
    public bool useTagFilter;

    protected override bool ColliderVerify(Collider2D other) {
        if(useTagFilter) {
            for(int i = 0; i < tagFilter.Length; i++) {
                if(other.CompareTag(tagFilter[i]))
                    return true;
            }

            return false;
        }

        return true;
    }

    protected override bool UnitVerify(FlockUnit unit) {
		return unit.transform.parent != transform.parent;
	}
	
	protected override void UnitAdded(FlockUnit unit) {
	}
	
	protected override void UnitRemoved(FlockUnit unit) {
	}
}
