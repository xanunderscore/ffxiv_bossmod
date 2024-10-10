namespace BossMod.Pathfinding;

public sealed class PathManager
{
    public readonly WorldState World;
    public readonly PathDatabase Database = new();
    private readonly QuestBattleConfig _config = Service.Config.Get<QuestBattleConfig>();

    public PathManager(WorldState ws)
    {
        World = ws;
        ReloadDatabase();
    }

    public uint ZoneKey(ushort zoneId, ushort cfcId) => ((uint)zoneId << 16) | cfcId;
    public uint CurrentKey() => ZoneKey(World.CurrentZone, World.CurrentCFCID);

    public void ReloadDatabase()
    {
        var dbPath = _config.LoadFromSource ? _config.SourcePath : "";
        Database.Load(dbPath);
    }

    public void SaveDatabase()
    {
        if (!_config.LoadFromSource)
            return;
        Database.Save(_config.SourcePath);
    }

    public bool TryGetPath(Vector3 destination, out List<Vector3> waypoints)
    {
        waypoints = [];

        if (!Database.Entries.TryGetValue(CurrentKey(), out var entries))
            return false;

        if (entries.Find(e => e.Destination == destination) is not PathDatabase.Entry pe)
            return false;

        waypoints = pe.Waypoints;
        return true;
    }

    public void SavePath(Vector3 destination, List<Vector3> waypoints)
    {
        var k = CurrentKey();
        Database.Entries.TryAdd(k, []);
        Database.Entries[k].Add(new PathDatabase.Entry(destination, waypoints));
        SaveDatabase();
    }
}
