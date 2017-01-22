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

    public float spawnStartDelay;
    public float spawnDelay;
    public float spawnFullDelay;
        
    public bool spawnIgnoreRotation;
}
