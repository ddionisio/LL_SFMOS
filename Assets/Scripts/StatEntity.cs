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

    public float HP;
    public float stamina;

    public float damage;
    public float attackSpeed; //how long it takes to dish out damage

    public float damageStamina;
    public float attackStaminaSpeed; //how long it takes to dish out damage

    public float seekDelay; //for seek state
    public string seekTag;
    public float seekCloseInSpeed;

    

    public float lifeSpan; //how long before this thing just dies, 0 = infinite
        
    public int score; //score upon death

    public bool IsSeekValid(string tag) {
        return seekTag == tag;
    }
}
