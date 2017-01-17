using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : M8.Sensor2D<FlockFilter> {
	protected override bool UnitVerify(FlockFilter unit) {
		return unit.transform.parent != transform.parent;
	}
	
	protected override void UnitAdded(FlockFilter unit) {
	}
	
	protected override void UnitRemoved(FlockFilter unit) {
	}
}
