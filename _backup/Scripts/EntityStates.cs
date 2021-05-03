
public enum EntityState {
    Invalid = -1,

    //general    
    Normal,
    Dead,

    Control, //manual control of entity
    Bind,

    Leave, //for certain entities that need to leave when a new stage happens

    //pathogen
    Wander, //simply just wander
    Seek,


    //mucus
    Gather,
    Gathered,

    //mucus form
    Select,
    Launch,

    //cell
    Inflamed,
    Infected,

    //mast cell
    Alert,
    Active,
    Sleep,

    //misc.
    DeadInstant, //special death to pathogens targetted by neutrophil
}
