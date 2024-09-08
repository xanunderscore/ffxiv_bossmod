namespace BossMod.QuestBattle;

public record struct Waypoint(Vector3 Position, bool Pathfind = true)
{
    public Waypoint(float X, float Y, float Z) : this(new(X, Y, Z), true) { }
}

public abstract class QuestObjective(WorldState ws, string name, List<Waypoint> connections)
{
    public readonly WorldState World = ws;
    public string Name = name;
    public List<Waypoint> Connections = connections;
    public bool Completed;

    public QuestObjective(WorldState ws, string name, Waypoint conn) : this(ws, name, [conn]) { }

    public override string ToString() => $"{Name}{(Connections.Count == 0 ? "" : Utils.Vec3String(Connections.Last().Position))}";

    public virtual bool ShouldPauseNavigationInCombat() => true;
    public virtual bool ShouldCancelNavigation() => false;

    public virtual void Update() { }
    public virtual void OnNavigationComplete() { }
    public virtual void AddAIHints(Actor player, AIHints hints) { }
    public virtual void OnActorCombatChanged(Actor actor) { }
    public virtual void OnActorEventStateChanged(Actor actor) { }
    public virtual void OnActorModelStateChanged(Actor actor) { }
    public virtual void OnStatusLose(Actor actor, ActorStatus status) { }
    public virtual void OnStatusGain(Actor actor, ActorStatus status) { }
    public virtual void OnActorCreated(Actor actor) { }
    public virtual void OnActorDestroyed(Actor actor) { }
    public virtual void OnActorKilled(Actor actor) { }
}

public abstract class QuestBattle : IDisposable
{
    public readonly WorldState World;
    private readonly EventSubscriptions _subscriptions;

    public readonly List<QuestObjective> Objectives = [];
    public int CurrentObjectiveIndex { get; private set; } = 0;
    public QuestObjective? CurrentObjective => CurrentObjectiveIndex >= 0 && CurrentObjectiveIndex < Objectives.Count ? Objectives[CurrentObjectiveIndex] : null;

    // low-resolution bounds centered on player character, with radius roughly equal to object load range
    // this allows AI to pathfind to any priority target regardless of distance, as long as it's loaded - this makes it easier to complete quest objectives which require combat
    // note that precision for aoe avoidance will obviously suffer
    public static readonly ArenaBoundsSquare OverworldBounds = new(100, 2.5f);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
    }

    protected QuestBattle(WorldState ws, List<QuestObjective> objectives)
    {
        World = ws;
        Objectives = objectives;

        _subscriptions = new(
            ws.Actors.EventStateChanged.Subscribe(act => CurrentObjective?.OnActorEventStateChanged(act)),
            ws.Actors.StatusLose.Subscribe((act, ix) => CurrentObjective?.OnStatusLose(act, act.Statuses[ix])),
            ws.Actors.StatusGain.Subscribe((act, ix) => CurrentObjective?.OnStatusGain(act, act.Statuses[ix])),
            ws.Actors.ModelStateChanged.Subscribe(act => CurrentObjective?.OnActorModelStateChanged(act)),
            ws.Actors.Added.Subscribe(act => CurrentObjective?.OnActorCreated(act)),
            ws.Actors.Removed.Subscribe(act => CurrentObjective?.OnActorDestroyed(act)),
            ws.Actors.InCombatChanged.Subscribe(act => CurrentObjective?.OnActorCombatChanged(act)),
            ws.Actors.IsDeadChanged.Subscribe(act =>
            {
                if (act.IsDead)
                    CurrentObjective?.OnActorKilled(act);
            })
        );
    }

    public void Update()
    {
        CurrentObjective?.Update();
        if (CurrentObjective?.Completed ?? false)
            CurrentObjectiveIndex++;
    }
    public void OnNavigationComplete()
    {
        CurrentObjective?.OnNavigationComplete();
    }
    public virtual void AddQuestAIHints(Actor player, AIHints hints) { }
    public void AddAIHints(Actor player, AIHints hints)
    {
        AddQuestAIHints(player, hints);
        CurrentObjective?.AddAIHints(player, hints);
    }
    public void Advance() => CurrentObjectiveIndex++;
}
