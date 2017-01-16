
public enum EntityState {
    //general
    Normal,
    Dead,

    //antigen
    Bind, //bound by Y things, or mucus

    //mucus
    Gather,
    Gathered,

    //mucus form
    Launch,

    //cell
    Inflamed,
    Infected
}
