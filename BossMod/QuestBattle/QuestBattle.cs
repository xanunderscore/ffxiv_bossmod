using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle;

public record struct Waypoint(Vector3 Position, bool Pathfind = true);

public enum NavigationStrategy
{
    Continue,
    PauseOnCombat,
    CancelOnCombat
}

public class QuestObjective
{
    public readonly WorldState World;
    public string Name { get; private set; } = "";
    public readonly List<Waypoint> Connections = [];
    public NavigationStrategy NavigationStrategy = NavigationStrategy.PauseOnCombat;

    public bool Completed;

    public Action<Actor, AIHints> AddAIHints = (_, _) => { };
    public Action<Actor> OnModelStateChanged = (_) => { };
    public Action<Actor, ActorStatus> OnStatusGain = (_, _) => { };
    public Action<Actor, ActorStatus> OnStatusLose = (_, _) => { };
    public Action<Actor> OnActorEventStateChanged = (_) => { };
    public Action<Actor> OnActorCreated = (_) => { };
    public Action<Actor> OnActorDestroyed = (_) => { };
    public Action<Actor> OnActorCombatChanged = (_) => { };
    public Action<Actor> OnActorKilled = (_) => { };
    public Action<WorldState.OpDirectorUpdate> OnDirectorUpdate = (_) => { };
    public Action<ConditionFlag, bool> OnConditionChange = (_, _) => { };
    public Action OnNavigationComplete = () => { };

    public QuestObjective(WorldState ws)
    {
        World = ws;
        OnActorCombatChanged += (act) =>
        {
            if (act.OID == 0 && act.InCombat && NavigationStrategy == NavigationStrategy.CancelOnCombat)
            {
                Service.Log($"Entered combat -> canceling navigation");
                ShouldCancelNavigation = true;
            }
        };
    }

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

    public QuestObjective NavStrategy(NavigationStrategy strat)
    {
        NavigationStrategy = strat;
        return this;
    }

    public QuestObjective CancelNavigationOnCombat()
    {
        NavigationStrategy = NavigationStrategy.CancelOnCombat;
        return this;
    }

    public QuestObjective Hints(Action<Actor, AIHints> addHints)
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

    public QuestObjective ModelState(Action<Actor> fun)
    {
        OnModelStateChanged += fun;
        return this;
    }

    public QuestObjective StatusGain(Action<Actor, ActorStatus> fun)
    {
        OnStatusGain += fun;
        return this;
    }

    public QuestObjective StatusLose(Action<Actor, ActorStatus> fun)
    {
        OnStatusLose += fun;
        return this;
    }

    public QuestObjective CompleteOnActorAdded(uint oid)
    {
        OnActorCreated += (act) => CompleteIf(act.OID == oid);
        return this;
    }

    public QuestObjective CompleteOnKilled(uint oid)
    {
        OnActorKilled += (act) => CompleteIf(act.OID == oid);
        return this;
    }

    public QuestObjective CompleteOnDestroyed(uint oid)
    {
        OnActorDestroyed += (act) => CompleteIf(act.OID == oid);
        return this;
    }

    public override string ToString() => $"{Name}{(Connections.Count == 0 ? "" : Utils.Vec3String(Connections.Last().Position))}";

    public bool ShouldCancelNavigation;

    public virtual void Update() { }

    public void CompleteIf(bool c) { Completed = c; }
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

    protected static Vector3 V3(float x, float y, float z) => new(x, y, z);

    protected static QuestObjective Combat(WorldState ws, Vector3 destination)
    {
        var obj = new QuestObjective(ws).WithConnection(destination).CancelNavigationOnCombat();
        obj.AddAIHints += (player, hints) =>
            obj.CompleteIf(obj.ShouldCancelNavigation && !player.InCombat && !hints.PriorityTargets.Any(x => hints.Bounds.Contains(x.Actor.Position - player.Position)));
        return obj;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        if (Service.Condition != null)
            Service.Condition.ConditionChange -= OnConditionChange;
    }

    protected QuestBattle(WorldState ws)
    {
        World = ws;
        Objectives = DefineObjectives(ws);

        _subscriptions = new(
            ws.Actors.EventStateChanged.Subscribe(act => CurrentObjective?.OnActorEventStateChanged(act)),
            ws.Actors.StatusLose.Subscribe((act, ix) => CurrentObjective?.OnStatusLose(act, act.Statuses[ix])),
            ws.Actors.StatusGain.Subscribe((act, ix) => CurrentObjective?.OnStatusGain(act, act.Statuses[ix])),
            ws.Actors.ModelStateChanged.Subscribe(act => CurrentObjective?.OnModelStateChanged(act)),
            ws.Actors.Added.Subscribe(act => CurrentObjective?.OnActorCreated(act)),
            ws.Actors.Removed.Subscribe(act => CurrentObjective?.OnActorDestroyed(act)),
            ws.Actors.InCombatChanged.Subscribe(act => CurrentObjective?.OnActorCombatChanged(act)),
            ws.Actors.IsDeadChanged.Subscribe(act =>
            {
                if (act.IsDead)
                    CurrentObjective?.OnActorKilled(act);
            }),
            ws.DirectorUpdate.Subscribe(op => CurrentObjective?.OnDirectorUpdate(op))
        );
        if (Service.Condition == null)
            Service.Log($"[QuestBattle] UIDev detected, not registering hook");
        else
            Service.Condition.ConditionChange += OnConditionChange;
    }

    public abstract List<QuestObjective> DefineObjectives(WorldState ws);

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
