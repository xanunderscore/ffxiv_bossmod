namespace BossMod.Heavensward.Quest.AsGoesLightSoGoesDarkness;
public enum OID : uint
{
    Boss = 0x148A,
    Helper = 0x233C,
    VaultDoor1 = 0x1E9ED7,
    VaultDoor2 = 0x1E9ED8,
    ArenaDoor = 0x1E9ED9,
    VaultDoor3 = 0x1E9EDA,

    Refugee1 = 0x14BE,
    Refugee2 = 0x14BF,
    Refugee3 = 0x14C0,
    Refugee4 = 0x14C1,
    Refugee5 = 0x14C2,
    Bonds = 0x1E9EE0
}

abstract class ProgressComponent(BossModule module) : BossComponent(module)
{
    public bool Complete { get; protected set; }
}

abstract class Refugee(BossModule module, uint OID) : ProgressComponent(module)
{
    private readonly uint ActorID = OID;

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor.OID == ActorID && status.ID == 990)
            Complete = true;
    }
}

abstract class EventStateComponent : ProgressComponent
{
    private readonly uint OID;

    public EventStateComponent(BossModule module, uint OID) : base(module)
    {
        this.OID = OID;
        module.WorldState.Actors.EventStateChanged.Subscribe(OnActorEventStateChanged);
    }

    private void OnActorEventStateChanged(Actor act)
    {
        if (act.OID == OID && act.EventState == 7)
            Complete = true;
    }
}

class VaultDoor1(BossModule module) : EventStateComponent(module, (uint)OID.VaultDoor1);
class VaultDoor2(BossModule module) : EventStateComponent(module, (uint)OID.VaultDoor2);
class ArenaDoor(BossModule module) : EventStateComponent(module, (uint)OID.ArenaDoor);
class VaultDoor3(BossModule module) : EventStateComponent(module, (uint)OID.VaultDoor3);
class Refugee1(BossModule module) : Refugee(module, (uint)OID.Refugee1);
class Refugee2(BossModule module) : Refugee(module, (uint)OID.Refugee2);
class Refugee3(BossModule module) : Refugee(module, (uint)OID.Refugee3);
class Refugee4(BossModule module) : Refugee(module, (uint)OID.Refugee4);
class Refugee5(BossModule module) : Refugee(module, (uint)OID.Refugee5);
class ArenaWall(BossModule module) : EventStateComponent(module, 0x1E9EDD);

class Bonds(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!actor.InCombat)
            hints.InteractWithTarget = WorldState.Actors.FirstOrDefault(x => x.OID == (uint)OID.Bonds && x.IsTargetable);
    }
}

class StateDebug : BossComponent
{
    public StateDebug(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
        module.WorldState.Actors.EventStateChanged.Subscribe(OnActorEventStateChanged);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        Service.Log($"[Vault] Model state: {actor} {modelState:X2} {animState1:X2} {animState2:X2}");
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        Service.Log($"[Vault] Director update: {updateID:X8} {param1:X2} {param2:X2} {param3:X2} {param4:X2}");
    }

    private void OnActorEventStateChanged(Actor act)
    {
        Service.Log($"[Vault] Event state: {act} changed to {act.EventState}");
    }
}

class QuestStates : StateMachineBuilder
{
    private readonly Quest _module;
    public QuestStates(Quest module) : base(module)
    {
        _module = module;

        QuestPhase<Refugee1>(0, "Refugee 1").ActivateOnEnter<StateDebug>();
        QuestPhase<VaultDoor1>(1, "Pack 1", () => !(module.Raid.Player()?.InCombat ?? true));
        QuestPhase<Refugee2, VaultDoor2>(2, "Refugee 2");
        QuestPhase<ArenaDoor>(3, "Pack 2");
        QuestPhase<VaultDoor3>(4, "Cutscene");
        QuestPhase<Refugee3, Refugee4>(5, "Refugee 3+4").ActivateOnEnter<Bonds>();
        QuestPhase<Refugee5>(6, "Refugee 5").ActivateOnEnter<Bonds>();
        QuestPhase<ArenaWall>(7, "Help Aymeric");
        QuestPhase(8, "Complete", () => false);
    }

    private Phase QuestPhase(uint id, string name, Func<bool>? updateFunc = null)
    {
        return Make(id, name, updateFunc).Item1;
    }

    private Phase QuestPhase<C>(uint id, string name, Func<bool>? updateFunc = null) where C : ProgressComponent
    {
        var (phase, updates) = Make(id, name, updateFunc);

        AddObjective<C>(phase, updates);

        return phase;
    }
    private Phase QuestPhase<C, C2>(uint id, string name, Func<bool>? updateFunc = null) where C : ProgressComponent where C2 : ProgressComponent
    {
        var (phase, updates) = Make(id, name, updateFunc);

        AddObjective<C>(phase, updates);
        AddObjective<C2>(phase, updates);

        return phase;
    }

    private (Phase, List<Func<bool>>) Make(uint id, string name, Func<bool>? updateFunc = null)
    {
        List<Func<bool>> updaters = [];
        if (updateFunc != null)
            updaters.Add(updateFunc);

        var p = SimplePhase(id, i => SimpleState(i, 10000, name), $"Step {id + 1}").OnExit(() => _module.Step++);
        p.Raw.Update = () => _module.WorldState.CurrentCFCID != 441 || updaters.All(u => u.Invoke());

        return (p, updaters);
    }

    private void AddObjective<C>(Phase p, List<Func<bool>> updaters) where C : ProgressComponent
    {
        p.ActivateOnEnter<C>();
        updaters.Add(() => _module.FindComponent<C>()?.Complete ?? false);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 441, PrimaryActorOID = (uint)OID.VaultDoor1)]
public class Quest(WorldState ws, Actor primary) : DutyModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    public int Step = 0;

    private static readonly List<DutyObjective> Objectives = [
        new(new(0, -300, 75)),
        new(new(16, -300, 30)),
        new(new(52, -300, -30)),
        new(new(-30, -300, -75), false),
        new(new(-17.5f, -292, -100)),
        new(new(-52, -300, -30)),
        new(new(55, -300, -68)),
        new(new(0, -292, -100)),
        new(new(2, -282.35f, -151))
    ];

    public override DutyObjective? GetNextObjective()
    {
        if (Step < 0)
            return null;

        if (Step >= Objectives.Count)
            return null;

        return Objectives[Step];
    }

    protected override void UpdateModule()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }
}

