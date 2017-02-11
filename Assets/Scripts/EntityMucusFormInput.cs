using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EntityMucusFormInput : MonoBehaviour {
    
    public M8.EntityBase entity;

    private EntityMucusForm mEntityMucusForm;

    [SerializeField]
    public float _radius;

    public float radius {
        get {
            if(mEntityMucusForm)
                return mEntityMucusForm.stats.GetScale(mEntityMucusForm.currentGrowthCount)*_radius;

            return _radius;
        }
    }
    
    public bool isLocked {
        get {
            return entity.isReleased || !(entity.state == (int)EntityState.Normal || entity.state == (int)EntityState.Select);
        }
    }
    
    public void Cancel() {
        entity.state = (int)EntityState.Normal;
    }
    
    public void Select() {
        entity.state = (int)EntityState.Select;
    }

    void OnDestroy() {
        if(entity) {
            entity.spawnCallback -= OnEntitySpawn;
            entity.releaseCallback -= OnEntityRelease;
        }
    }

    void Awake() {
        entity.spawnCallback += OnEntitySpawn;
        entity.releaseCallback += OnEntityRelease;

        mEntityMucusForm = entity as EntityMucusForm;
    }
    
    void OnEntitySpawn(M8.EntityBase ent) {
        MucusGatherInputField.instance.SetRegisterInput(this, true);
    }

    void OnEntityRelease(M8.EntityBase ent) {
        MucusGatherInputField.instance.SetRegisterInput(this, false);
    }

    void OnDrawGizmos() {
        if(radius > 0f) {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
