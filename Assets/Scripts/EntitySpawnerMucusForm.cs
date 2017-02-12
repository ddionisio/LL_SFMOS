using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnerMucusForm : MonoBehaviour {
    public StatEntityMucusForm stats;

    public EntitySpawner spawner;

    public int growth;
    public float growthScaleAdd = 0.15f;
    public float growthScaleDelay = 0.3f;

    public bool isGrowthFull {
        get {
            return growth >= stats.growthMaxCount;
        }
    }
    
    public void SetGrowth(int toGrowth) {
        growth = Mathf.Clamp(toGrowth, 0, stats.growthMaxCount);

        if(spawner.spawnedEntities != null) {
            //clear out current spawn to allow new growths
            var spawns = spawner.spawnedEntities;
            for(int i = 0; i < spawns.Count; i++)
                spawns[i].state = (int)EntityState.Dead;
        }
    }
    
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
