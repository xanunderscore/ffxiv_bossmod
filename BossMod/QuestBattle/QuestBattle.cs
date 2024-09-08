using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle;

public record struct Waypoint(Vector3 Position, bool Pathfind = true);

public class QuestObjective(WorldState ws)
{
    public readonly WorldState World = ws;
    public string Name { get; protected set; } = "";
    public readonly List<Waypoint> Connections = [];
    public bool Completed;

    public QuestObjective Named(string name)
    {
        Name = name;
        return this;
    }

    public QuestObjective WithConnection(Vector3 conn) => WithConnection(new Waypoint(conn));
    public QuestObjective WithConnection(Waypoint conn)
    {
        Connections.Add(conn);
        return this;
    }

    public QuestObjective WithConnections(params Vector3[] connections)
    {
        Connections.AddRange(connections.Select(c => new Waypoint(c)));
        return this;
    }
    public QuestObjective WithConnections(params Waypoint[] connections)
    {
        Connections.AddRange(connections);
        return this;
    }

    public QuestObjective WithPauseOnCombat(bool pause = true)
    {
        ShouldPauseNavigationInCombat = pause;
        return this;
    }

    public QuestObjective WithStopOnCombat(bool stop = false)
    {
        ShouldCancelNavigation = stop;
        return this;
    }

    public QuestObjective WithHints(AddAIHintsDelegate addHints)
    {
        AddAIHints += addHints;
        return this;
    }

    public QuestObjective WithInteract(uint targetOid, bool allowInCombat = false)
    {
        AddAIHints += (player, hints) =>
        {
            if (!player.InCombat || allowInCombat)
                hints.InteractWithOID(World, targetOid);
        };
        return this;
    }

    public QuestObjective OnModelState(ActorModelStateChangedDelegate fun)
    {
        OnActorModelStateChanged += fun;
        return this;
    }

    public delegate void AddAIHintsDelegate(Actor actor, AIHints hints);
    public AddAIHintsDelegate AddAIHints = delegate { };
    public delegate void ActorModelStateChangedDelegate(Actor actor);
    public ActorModelStateChangedDelegate OnActorModelStateChanged = delegate { };

    public override string ToString() => $"{Name}{(Connections.Count == 0 ? "" : Utils.Vec3String(Connections.Last().Position))}";

    public bool ShouldPauseNavigationInCombat { get; protected set; } = true;
    public bool ShouldStopNavigationInCombat { get; protected set; } = false;
    public bool ShouldCancelNavigation { get; protected set; }

    public virtual void Update() { }
    public virtual void OnNavigationComplete() { }
    public virtual void OnActorCombatChanged(Actor actor) { }
    public virtual void OnActorEventStateChanged(Actor actor) { }
    public virtual void OnStatusLose(Actor actor, ActorStatus status) { }
    public virtual void OnStatusGain(Actor actor, ActorStatus status) { }
    public virtual void OnActorCreated(Actor actor) { }
    public virtual void OnActorDestroyed(Actor actor) { }
    public virtual void OnActorKilled(Actor actor) { }
    public virtual void OnConditionChange(ConditionFlag flag, bool value) { }
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
        Service.Condition.ConditionChange -= OnConditionChange;
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
        Service.Condition.ConditionChange += OnConditionChange;
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
    public void OnConditionChange(ConditionFlag flag, bool value) => CurrentObjective?.OnConditionChange(flag, value);
}
