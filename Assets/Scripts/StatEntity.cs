using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Common stats for entities (cells, pathogens, etc.)
/// </summary>
[CreateAssetMenu(fileName = "statEntity", menuName = "Stats/Entity")]
public class StatEntity : ScriptableObject {
    public float HP;
    public float stamina;

    public float attack;
    public float attackPerSecond;

    public float attackStamina;
    public float attackStaminaPerSecond;

    public float seekDelay; //for seek state
    public string seekTag;

    public float lifeSpan; //how long before this thing just dies

    public int score; //score upon death
}
