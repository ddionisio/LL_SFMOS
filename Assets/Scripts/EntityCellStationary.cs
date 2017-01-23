using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCellStationary : EntityCommon {

    protected override void StateChanged() {
        switch((EntityState)state) {
            case EntityState.Dead:
                Debug.Log("dead: "+name);

                //animate

                if(body)
                    body.simulated = false;

                if(coll)
                    coll.enabled = false;
                break;
        }
    }
}
