using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCell : M8.EntityBase {
    public enum Team {
        None,

        Human,
        Antigen,
    }

    public Team team;
}
