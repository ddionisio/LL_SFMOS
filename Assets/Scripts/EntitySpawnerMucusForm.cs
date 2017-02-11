using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnerMucusForm : MonoBehaviour {
    public EntitySpawner spawner;

    public int growth;
    
    void OnDestroy() {
        if(spawner)
            spawner.spawnCallback -= OnEntitySpawn;
    }

    void Awake() {
        spawner.spawnCallback += OnEntitySpawn;
    }

    void OnEntitySpawn(EntitySpawner aSpawner, M8.EntityBase ent) {
        if(growth > 0) {
            var mucusForm = (EntityMucusForm)ent;
            
            mucusForm.SetGrow(growth);
        }
    }
}
