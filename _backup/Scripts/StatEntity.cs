using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Common stats for entities (cells, pathogens, etc.)
/// </summary>
[CreateAssetMenu(fileName = "statEntity", menuName = "Stats/Entity")]
public class StatEntity : ScriptableObject {
    public enum Type {
        Biological,
        Waste
    }

    public Type type;

    [Header("Stats")]
    public float HP;
    public float stamina;

    public float damage;
    public float attackSpeed; //how long it takes to dish out damage

    public float damageStamina;
    public float attackStaminaSpeed; //how long it takes to dish out damage

    public float lifeSpan; //how long before this thing just dies, 0 = infinite

    public int score; //score upon death

    public bool registerAsEnemy; //register as enemy for mission controller
    public bool isEdible; //can be processed by macrophage?

    [Header("Seek")]
    public float seekDelay; //for seek state
    public string seekTag;
    public float seekCloseInSpeed;
    public float seekFlockMoveToScale = 1.0f;

    [Header("Split")]
    public string splitSpawnPoolGroup;
    public string[] splitSpawnPoolEntityTypes; //variations
    public float splitSpawnRadius; //used to determine position around the surface
    public int splitAngleVariance; //number of angles to split off around the surface
    public float splitImpulse;

    [Header("Roam")]
    public float roamForceMin;
    public float roamForceMax;
    public float roamChangeDelayMin;
    public float roamChangeDelayMax;

    [Header("Hurt")]
    public float hurtDuration = 0.3f;

    [Header("Death")]
    public bool releaseOnDeath = true;

    [Header("Takes")]
    public string takeSpawn; //animation to play on spawn start, usu. same as takeNormal
    public string takeNormal;
    public string takeSeek;
    public string takeDeath;
    public string takeDeathInstant; //insta-death, should release no matter what
    public string takeAttack;
    public string takeHurt;

    public bool canSplit {
        get { return !string.IsNullOrEmpty(splitSpawnPoolGroup) && splitSpawnPoolEntityTypes.Length > 0; }
    }

    public string splitEntityType {
        get { return splitSpawnPoolEntityTypes[Random.Range(0, splitSpawnPoolEntityTypes.Length)]; }
    }

    public Vector2 splitDir {
        get {
            var dir = Vector2.up;

            float angle = 360.0f * ((float)Random.Range(1, splitAngleVariance)/splitAngleVariance);

            dir = M8.MathUtil.Rotate(dir, angle);

            return dir;
        }
    }

    public bool IsSeekValid(string tag) {
        return seekTag == tag;
    }
}
