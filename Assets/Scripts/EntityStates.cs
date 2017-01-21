
public enum EntityState {
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
    Launch,

    //cell
    Inflamed,
    Infected
}
