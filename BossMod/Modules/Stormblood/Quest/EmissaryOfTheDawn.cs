namespace BossMod.Stormblood.Quest.EmissaryOfTheDawn;

// TODO use limit break
// TODO the RP component targets the event object with the Boss OID, even though it's not a valid target

public enum OID : uint
{
    Boss = 0x1EA9D9,
    Helper = 0x233C,
}

class Step1(BossModule module) : BossComponent(module)
{
    public bool Complete;
    public Actor? Wreckage => Module.Enemies(0x234C).FirstOrDefault();
    public WPos WreckagePosition = new(-6.3f, 5.75f);
    public Actor? Popularis => Module.Enemies(0x2344).FirstOrDefault();
    public Actor? PopularisEvent => WorldState.Actors.FirstOrDefault(a => a.OID == 0x1EA9D9);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.DistanceToHitbox(WreckagePosition, 0) > 5)
            hints.ForcedMovement = actor.DirectionTo(WreckagePosition).ToVec3();

        if (Wreckage != null && Wreckage.IsTargetable && !Wreckage.IsDeadOrDestroyed)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.RuinIII), Wreckage, ActionQueue.Priority.High);

        if (Popularis?.IsTargetable ?? false)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.Physick), Popularis, ActionQueue.Priority.High);

        if (PopularisEvent?.IsTargetable ?? false)
            hints.InteractWithTarget = PopularisEvent;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000001 && param1 == 0x00000027)
            Complete = true;
    }
}

class AlphiAI(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget != null)
            UseAction(Roleplay.AID.RuinIII, primaryTarget);
    }
}

class AirshipWreckageStates : StateMachineBuilder
{
    public AirshipWreckageStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Step1>()
            .Raw.Update = () => (Module.FindComponent<Step1>()?.Complete ?? true) || (Module.Raid.Player()?.IsDeadOrDestroyed ?? true);
        TrivialPhase(1)
            .ActivateOnEnter<AlphiAI>()
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 582, NameID = 2009561)]
public class AirshipWreckage(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void UpdateModule()
    {
        Arena.Center = Raid.Player()!.Position;
    }

    protected override bool CheckPull() => true;
}

