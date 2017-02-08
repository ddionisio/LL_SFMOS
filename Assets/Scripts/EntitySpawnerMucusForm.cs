using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnerMucusForm : MonoBehaviour {
    public EntitySpawner spawner;

    public int growth;
    public bool isMaxGrowth;
    
    void OnDestroy() {
        if(spawner)
            spawner.spawnCallback -= OnEntitySpawn;
    }

    void Awake() {
        spawner.spawnCallback += OnEntitySpawn;
    }

    void OnEntitySpawn(EntitySpawner aSpawner, M8.EntityBase ent) {
        if(isMaxGrowth || growth > 0) {
            var mucusForm = (EntityMucusForm)ent;

            var growVal = isMaxGrowth ? mucusForm.stats.growthMaxCount : growth;

            mucusForm.SetGrow(growVal);
        }
    }
}
