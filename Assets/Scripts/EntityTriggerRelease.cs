using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTriggerRelease : MonoBehaviour {
    public string[] tagFilter;

    void OnTriggerEnter2D(Collider2D coll) {
        if(tagFilter.Length > 0) {
            bool isValid = false;
            for(int i = 0; i < tagFilter.Length; i++) {
                if(coll.CompareTag(tagFilter[i])) {
                    isValid = true;
                    break;
                }
            }

            if(!isValid)
                return;
        }

        var ent = coll.GetComponent<M8.EntityBase>();
        if(ent)
            ent.Release();
    }

    void OnDrawGizmos() {

        var bound = new Bounds();

        BoxCollider2D bc2D = GetComponent<BoxCollider2D>();
        if(bc2D != null) {
            bound.center = bc2D.offset;
            bound.extents = new Vector3(bc2D.size.x*transform.localScale.x, bc2D.size.y*transform.localScale.y, 0f) * 0.5f;
        }

        if(bound.size.x + bound.size.y + bound.size.z > 0) {
            Gizmos.color = Color.red;
            Gizmos.color *= 0.5f;

            Gizmos.DrawWireCube(transform.position + bound.center, bound.size);
        }
    }
}
