
public enum EntityState {
    //general    
    Normal,
    Dead,

    Control, //manual control of entity
    Bind,

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
