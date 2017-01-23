
public struct Params {
    public const string impulse = "impulse";
    public const string dir = "dir";
    public const string state = "state"; //starting state
    public const string anchor = "anchor"; //Transform to attach entity, mostly used when "state"=Control during spawn (for rigidbody to move to)
    public const string spawnedFrom = "spawnedFrom"; //EntityCommon of who spawned us

    //Score Dealer
    public const string scoreMultiplier = "scoreX";
}
