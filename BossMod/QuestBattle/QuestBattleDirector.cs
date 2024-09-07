using Dalamud.Plugin.Ipc;
using System.Threading.Tasks;

namespace BossMod.QuestBattle;
public sealed class QuestBattleDirector : IDisposable
{
    public readonly WorldState World;
    private readonly BossModuleManager Bossmods;
    private readonly EventSubscriptions _subscriptions;
    private List<Vector3> Waypoints = [];

    public QuestBattle? CurrentModule { get; private set; }
    public QuestNavigation? CurrentNavigation { get; private set; }

    public Event<QuestBattle> QuestActivated = new();
    public Event<QuestNavigation> NavigationChanged = new();
    public Event<QuestNavigation> NavigationCleared = new();

    public const float Tolerance = 0.25f;

    private readonly ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?> _pathfind;
    private readonly ICallGateSubscriber<bool> _isMeshReady;

    public QuestBattleDirector(WorldState ws, BossModuleManager bmm)
    {
        World = ws;
        Bossmods = bmm;

        _subscriptions = new(
            ws.CurrentZoneChanged.Subscribe(OnZoneChange),
            NavigationChanged.Subscribe(OnNavigationChange),
            NavigationCleared.Subscribe(OnNavigationClear)
        );

        _pathfind = Service.PluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>("vnavmesh.Nav.Pathfind");
        _isMeshReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
    }

    private Task<List<Vector3>>? Pathfind(Vector3 source, Vector3 target) => _pathfind.InvokeFunc(source, target, false);
    private bool IsMeshReady() => _isMeshReady.InvokeFunc();

    public void Update(AIHints hints)
    {
        var player = World.Party.Player();
        if (player == null)
            return;

        if (CurrentModule != null)
        {
            CurrentModule.Update();
            var obj = CurrentModule.GetNextDestination();
            if (obj == null)
            {
                if (CurrentNavigation != null)
                {
                    NavigationCleared.Fire(CurrentNavigation.Value);
                    CurrentNavigation = null;
                }
            }
            else if (obj != CurrentNavigation)
            {
                NavigationChanged.Fire(obj.Value);
                CurrentNavigation = obj;
            }
        }

        if (CurrentNavigation != null)
            MoveNext(player, CurrentNavigation.Value, hints);
    }

    private void MoveNext(Actor player, QuestNavigation objective, AIHints hints)
    {
        if (Waypoints.Count == 0)
            return;

        var nextwp = Waypoints[0];
        var playerPos = player.PosRot.XYZ();
        var direction = nextwp - playerPos;
        if (direction.XZ().Length() < Tolerance)
        {
            Waypoints.RemoveAt(0);
            if (Waypoints.Count == 0)
                CurrentModule?.OnNavigationComplete(objective);
            MoveNext(player, objective, hints);
            return;
        }
        else
        {
            var paused = player.InCombat && objective.PauseForCombat;
            Camera.Instance?.DrawWorldLine(playerPos, nextwp, paused ? 0x80ffffff : ArenaColor.Safe);
            if (!paused)
                hints.ForcedMovement = direction;
        }
    }

    private async void TryPathfind(Vector3 start, List<Vector3> connections, int maxRetries = 5)
    {
        Waypoints = await TryPathfind(Enumerable.Repeat(start, 1).Concat(connections), maxRetries);
    }

    private async Task<List<Vector3>> TryPathfind(IEnumerable<Vector3> connectionPoints, int maxRetries = 5)
    {
        if (!IsMeshReady())
        {
            await Task.Delay(500);
            return await TryPathfind(connectionPoints, maxRetries - 1);
        }
        var points = connectionPoints.Take(3).ToList();
        if (points.Count < 2)
        {
            Service.Log($"[QuestBattle] pathfind called with too few points (need 2, got {points.Count})");
            return [];
        }
        var start = points[0];
        var end = points[1];
        var task = Pathfind(start, end);
        if (task == null)
        {
            Service.Log($"[QuestBattle] Pathfind failure");
            return [];
        }

        var thesePoints = await task;
        if (points.Count > 2)
            thesePoints.AddRange(await TryPathfind(connectionPoints.Skip(1)));
        return thesePoints;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void OnNavigationChange(QuestNavigation obj)
    {
        Service.Log($"[QuestBattle] next objective: {obj}");
        if (World.Party.Player() is Actor player)
            TryPathfind(player.PosRot.XYZ(), obj.Connections);
    }

    private void OnNavigationClear(QuestNavigation obj)
    {
        Service.Log($"[QuestBattle] cleared objective: {obj}");
        Waypoints.Clear();
    }

    private void OnZoneChange(WorldState.OpZoneChange change)
    {
        var newHandler = QuestBattleRegistry.GetHandler(World, change.CFCID);
        CurrentNavigation = null;
        CurrentModule?.Dispose();
        CurrentModule = newHandler;
        if (newHandler != null)
            QuestActivated.Fire(newHandler);
    }
}
