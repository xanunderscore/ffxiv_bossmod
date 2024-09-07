using Dalamud.Plugin.Ipc;
using System.Threading.Tasks;

namespace BossMod;

public record struct DutyObjective(Vector3 Destination, bool PauseForCombat = true)
{
    public override readonly string ToString() => $"{Utils.Vec3String(Destination)}";
}

public sealed class DutyDirector : IDisposable
{
    private WorldState _ws;
    private BossModuleManager _mgr;
    private EventSubscriptions _subscriptions;
    public Event<DutyObjective> ObjectiveChanged = new();
    public Event ObjectiveCleared = new();
    private List<Vector3> Waypoints = [];

    public const float Tolerance = 0.25f;

    private BossModule? _module;
    private DutyObjective? _objective;

    private ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?> _pathfind;
    private ICallGateSubscriber<bool> _isMeshReady;

    public DutyDirector(WorldState ws, BossModuleManager mgr)
    {
        _ws = ws;
        _mgr = mgr;
        _subscriptions = new(
            _mgr.ModuleLoaded.Subscribe(OnModuleLoaded),
            _mgr.ModuleUnloaded.Subscribe(OnModuleUnloaded),
            ObjectiveChanged.Subscribe(OnObjectiveChanged),
            ObjectiveCleared.Subscribe(OnObjectiveCleared)
        );

        _pathfind = Service.PluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>("vnavmesh.Nav.Pathfind");
        _isMeshReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
    }

    private Task<List<Vector3>>? Pathfind(Vector3 source, Vector3 target) => _pathfind.InvokeFunc(source, target, false);
    private bool IsMeshReady() => _isMeshReady.InvokeFunc();

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update(AIHints hints)
    {
        var next = _module?.GetNextObjective();
        if (next != _objective)
        {
            Service.Log($"old objective: {_objective}, new objective: {next}");
            _objective = next;
            if (next == null)
                ObjectiveCleared.Fire();
            else
                ObjectiveChanged.Fire(next.Value);
        }

        if (_ws.Party.Player() is Actor p && _objective != null)
            MoveNext(p, _objective.Value, hints);
    }

    private void MoveNext(Actor player, DutyObjective objective, AIHints hints)
    {
        if (Waypoints.Count == 0)
            return;

        var nextwp = Waypoints[0];
        var playerPos = player.PosRot.XYZ();
        var direction = nextwp - playerPos;
        if (direction.XZ().Length() < Tolerance)
        {
            Waypoints.RemoveAt(0);
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

    private void OnObjectiveChanged(DutyObjective obj)
    {
        Service.Log($"[DD] New objective: {obj}");
        if (_ws.Party.Player() is Actor p)
            TryPathfind(p.PosRot.XYZ(), obj.Destination);
    }

    private async void TryPathfind(Vector3 start, Vector3 end, int maxRetries = 5)
    {
        if (IsMeshReady())
        {
            var task = Pathfind(start, end);
            if (task == null)
                Service.Log($"[DD] Pathfind failure!");
            else
            {
                Waypoints = await task;
                Service.Log($"[DD] waypoints: {Waypoints}");
            }
        }
        else
        {
            await Task.Delay(500);
            TryPathfind(start, end, maxRetries);
        }
    }

    private void OnObjectiveCleared()
    {
        Service.Log($"[DD] Current objective cleared");
        _objective = null;
        Waypoints.Clear();
    }

    private void OnModuleLoaded(BossModule module)
    {
        Service.Log($"[DD] Module loaded: {module}");
        _module?.Dispose();
        _module = module;
    }

    private void OnModuleUnloaded(BossModule module)
    {
        Service.Log($"[DD] Module unloaded: {module}");
        _module = null;
    }
}
