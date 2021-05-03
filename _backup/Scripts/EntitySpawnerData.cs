using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "entitySpawnerData", menuName = "Data/Entity Spawner")]
public class EntitySpawnerData : ScriptableObject {
    [Header("Spawn Info")]
    public string poolGroup;
    public string entityRef;

    [Header("Info")]
    public int maxSpawn;

    public float spawnStartDelayMin;
    public float spawnStartDelayMax;

    public float spawnDelayMin;
    public float spawnDelayMax;

    public float spawnFullDelay;
        
    public bool spawnIgnoreRotation;

    public EntityState spawnRemoveFromCheckState = EntityState.Invalid; //remove from tracking list if entity enters this state
}
