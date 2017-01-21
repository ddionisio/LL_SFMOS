
public enum StatEntitySignals {
    Bind, //sender is binding receiver
    Unbind, //sender is releasing bind to receiver

    Hit, //sender is hitting receiver, provides StatEntitySignalHit
}
