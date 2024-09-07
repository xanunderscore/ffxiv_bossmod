namespace BossMod.QuestBattle;

public record struct QuestNavigation
{
    public string Name;
    public List<Vector3> Connections;
    public bool PauseForCombat = true;

    public QuestNavigation(string name, Vector3 dest, params Vector3[] rest) : this(name, true, dest, rest) { }

    public QuestNavigation(string name, bool pauseForCombat, Vector3 dest, params Vector3[] rest)
    {
        var items = rest.ToList();
        items.Insert(0, dest);

        Name = name;
        Connections = items;
        PauseForCombat = pauseForCombat;
    }

    public override readonly string ToString() => $"{Name} {Utils.Vec3String(Connections.Last()!)}";
}

public abstract class QuestBattle : IDisposable
{
    public readonly WorldState World;
    private readonly EventSubscriptions _subscriptions;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
    }

    public QuestBattle(WorldState ws)
    {
        World = ws;

        _subscriptions = new(
            ws.Actors.EventStateChanged.Subscribe(OnActorEventStateChanged),
            ws.Actors.StatusLose.Subscribe((act, ix) => OnStatusLose(act, act.Statuses[ix])),
            ws.Actors.StatusGain.Subscribe((act, ix) => OnStatusGain(act, act.Statuses[ix])),
            ws.Actors.ModelStateChanged.Subscribe(OnActorModelStateChanged)
        );
    }

    public virtual QuestNavigation? GetNextDestination() => null;
    public virtual void Update() { }
    public virtual void OnNavigationComplete(QuestNavigation obj) { }
    public virtual void CalculateAIHints(Actor player, AIHints hints) { }
    public virtual void OnActorEventStateChanged(Actor actor) { }
    public virtual void OnActorModelStateChanged(Actor actor) { }
    public virtual void OnStatusLose(Actor actor, ActorStatus status) { }
    public virtual void OnStatusGain(Actor actor, ActorStatus status) { }
}

public abstract class SimpleQuestBattle(WorldState ws, List<QuestNavigation> objectives) : QuestBattle(ws)
{
    public int CurrentStep { get; private set; } = 0;

    public void Advance(int currentStep)
    {
        if (CurrentStep == currentStep)
            CurrentStep++;
        else
            Service.Log($"called Advance({currentStep}), but current step number is {CurrentStep}");
    }

    public void Advance() => Advance(CurrentStep);

    public override sealed QuestNavigation? GetNextDestination() => CurrentStep >= 0 && CurrentStep < objectives.Count ? objectives[CurrentStep] : null;
}
