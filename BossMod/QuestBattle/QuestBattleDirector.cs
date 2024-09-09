using Dalamud.Plugin.Ipc;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BossMod.QuestBattle;
public sealed class QuestBattleDirector : IDisposable
{
    public readonly WorldState World;
    private readonly EventSubscriptions _subscriptions;
    private readonly QuestBattleConfig _config;

    public List<Vector3> CurrentWaypoints { get; private set; } = [];
    public QuestBattle? CurrentModule { get; private set; }
    public QuestObjective? CurrentObjective { get; private set; }

    public bool Paused = false;
    public bool WaitCommence = false;

    public Event<QuestBattle> QuestActivated = new();
    public Event<QuestObjective> ObjectiveChanged = new();
    public Event<QuestObjective> ObjectiveCleared = new();

    public const float Tolerance = 0.25f;

    private readonly ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?> _pathfind;
    private readonly ICallGateSubscriber<bool> _isMeshReady;

    public QuestBattleDirector(WorldState ws, BossModuleManager bmm)
    {
        World = ws;
        _config = Service.Config.Get<QuestBattleConfig>();

        _subscriptions = new(
            ws.CurrentZoneChanged.Subscribe(OnZoneChange),
            ws.Actors.Added.Subscribe(OnActorAdded),
            ws.Actors.EventStateChanged.Subscribe(OnActorEventState),
            ObjectiveChanged.Subscribe(OnNavigationChange),
            ObjectiveCleared.Subscribe(OnNavigationClear),
            _config.Modified.Subscribe(OnConfigChange),

            ws.Actors.StatusGain.Subscribe((a, i) =>
            {
                if (a.OID == 0)
                    return;
                Service.Log($"[QBD] {a} gain {a.Statuses[i]}");
            }),
            ws.Actors.StatusGain.Subscribe((a, i) =>
            {
                if (a.OID == 0)
                    return;
                Service.Log($"[QBD] {a} lose {a.Statuses[i]}");
            }),
            ws.Actors.ModelStateChanged.Subscribe(a =>
            {
                if (a.OID == 0)
                    return;
                Service.Log($"[QBD] {a} model state: {a.ModelState}");
            }),
            ws.Actors.EventStateChanged.Subscribe(a =>
            {
                if (a.OID == 0)
                    return;
                Service.Log($"[QBD] {a} event state: {a.EventState}");
            }),
            ws.DirectorUpdate.Subscribe(diru =>
            {
                Service.Log($"[QBD] Director update: {diru}");
            }),
            ws.Actors.EventObjectAnimation.Subscribe((act, p1, p2) =>
            {
                Service.Log($"[QBD] EObjAnim: {act}, {p1}, {p2}");
            })
        );

        _pathfind = Service.PluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>("vnavmesh.Nav.Pathfind");
        _isMeshReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
    }

    private Task<List<Vector3>>? Pathfind(Vector3 source, Vector3 target) => _pathfind.InvokeFunc(source, target, false);
    private bool IsMeshReady() => _isMeshReady.InvokeFunc();

    public void Update(AIHints hints)
    {
        if (Paused || WaitCommence)
            return;

        var player = World.Party.Player();
        if (player == null)
            return;

        if (CurrentModule != null)
        {
            CurrentModule.Update();
            var obj = CurrentModule.CurrentObjective;
            if (obj == null)
            {
                if (CurrentObjective != null)
                {
                    ObjectiveCleared.Fire(CurrentObjective);
                    CurrentObjective = null;
                }
            }
            else if (obj != CurrentObjective)
            {
                ObjectiveChanged.Fire(obj);
                CurrentObjective = obj;
            }
        }

        if (CurrentObjective != null)
            MoveNext(player, CurrentObjective, hints);
    }

    private void MoveNext(Actor player, QuestObjective objective, AIHints hints)
    {
        if (CurrentWaypoints.Count == 0)
            return;

        if (objective.ShouldCancelNavigation)
        {
            CurrentWaypoints.Clear();
            return;
        }

        var nextwp = CurrentWaypoints[0];
        var playerPos = player.PosRot.XYZ();
        var direction = nextwp - playerPos;
        if (direction.XZ().Length() < Tolerance)
        {
            CurrentWaypoints.RemoveAt(0);
            if (CurrentWaypoints.Count == 0)
                CurrentModule?.OnNavigationComplete();
            MoveNext(player, objective, hints);
            return;
        }
        else
        {
            var paused = hints.PriorityTargets.Any(x => hints.Bounds.Contains(x.Actor.Position - player.Position)) && objective.NavigationStrategy == NavigationStrategy.PauseOnCombat;
            Camera.Instance?.DrawWorldLine(playerPos, nextwp, paused ? 0x80ffffff : ArenaColor.Safe);
            if (!paused)
            {
                Dash(player, direction, hints);
                hints.ForcedMovement = direction;
            }
        }
    }

    private void Dash(Actor player, Vector3 destination, AIHints hints)
    {
        if (!_config.UseDash)
            return;

        var moveDist = destination.Length();
        var moveAngle = Angle.FromDirection(new WDir(destination.XZ()));

        switch (player.Class)
        {
            case Class.PCT:
                if (moveDist >= 15)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.Smudge), null, ActionQueue.Priority.Low, facingAngle: moveAngle);
                break;
            case Class.DRG:
                if (moveDist >= 15)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.ElusiveJump), null, ActionQueue.Priority.Low, facingAngle: Angle.FromDirection(-moveAngle.ToDirection()));
                break;
        }
    }

    private async void TryPathfind(Vector3 start, List<Waypoint> connections, int maxRetries = 5)
    {
        CurrentWaypoints = await TryPathfind(Enumerable.Repeat(new Waypoint(start, false), 1).Concat(connections), maxRetries).ConfigureAwait(false);
    }

    private async Task<List<Vector3>> TryPathfind(IEnumerable<Waypoint> connectionPoints, int maxRetries = 5)
    {
        if (!IsMeshReady())
        {
            await Task.Delay(500).ConfigureAwait(false);
            return await TryPathfind(connectionPoints, maxRetries - 1).ConfigureAwait(false);
        }
        var points = connectionPoints.Take(3).ToList();
        if (points.Count < 2)
        {
            Service.Log($"[QuestBattle] pathfind called with too few points (need 2, got {points.Count})");
            return [];
        }
        var start = points[0];
        var end = points[1];

        List<Vector3> thesePoints;

        if (end.Pathfind)
        {

            var task = Pathfind(start.Position, end.Position);
            if (task == null)
            {
                Service.Log($"[QuestBattle] Pathfind failure");
                return [];
            }

            thesePoints = await task.ConfigureAwait(false);
        }
        else
        {
            thesePoints = [start.Position, end.Position];
        }

        if (points.Count > 2)
            thesePoints.AddRange(await TryPathfind(connectionPoints.Skip(1)).ConfigureAwait(false));
        return thesePoints;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void OnNavigationChange(QuestObjective obj)
    {
        Service.Log($"[QuestBattle] next objective: {obj}");
        if (World.Party.Player() is Actor player)
            TryPathfind(player.PosRot.XYZ(), obj.Connections);
    }

    private void OnNavigationClear(QuestObjective obj)
    {
        Service.Log($"[QuestBattle] cleared objective: {obj}");
        CurrentWaypoints.Clear();
    }

    private void OnZoneChange(WorldState.OpZoneChange change)
    {
        SetMoveSpeedFactor(1);
        var newHandler = QuestBattleRegistry.GetHandler(World, change.CFCID);
        CurrentObjective = null;
        CurrentModule?.Dispose();
        CurrentModule = newHandler;
        if (newHandler != null)
        {
            if (_config.Speedhack)
                SetMoveSpeedFactor(5);
            QuestActivated.Fire(newHandler);
        }
    }

    private void OnActorAdded(Actor actor)
    {
        if (actor.OID == 0x1E8536)
            WaitCommence = true;
    }

    private void OnActorEventState(Actor actor)
    {
        if (actor.OID == 0x1E8536 && actor.EventState == 7)
            WaitCommence = false;
    }

    private void OnConfigChange()
    {
        if (_config.Speedhack && CurrentModule != null)
            SetMoveSpeedFactor(5);
        else if (!_config.Speedhack)
            SetMoveSpeedFactor(1);
    }

    private void SetMoveSpeedFactor(float f)
    {
        var speedBase = f * 6;
        Service.SigScanner.TryScanText("F3 0F 59 05 ?? ?? ?? ?? F3 0F 59 05 ?? ?? ?? ?? F3 0F 58 05 ?? ?? ?? ?? 44 0F 28 C8", out var address);
        address = address + 4 + Marshal.ReadInt32(address + 4) + 4;
        Dalamud.SafeMemory.Write(address + 20, speedBase);
        SetMoveControlData(speedBase);
    }

    private unsafe void SetMoveControlData(float speed)
    {
        Dalamud.SafeMemory.Write(((delegate* unmanaged[Stdcall]<byte, nint>)Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 AE 83 FD 05"))(1) + 8, speed);
    }
}
