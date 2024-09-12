namespace BossMod.Stormblood.Quest.EmissaryOfTheDawn;

// TODO use limit break

public enum OID : uint
{
    Boss = 0x234B,
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

class AlphiAI(BossModule module) : Components.DeprecatedRoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget != null)
            UseAction(Roleplay.AID.RuinIII, primaryTarget);
    }
}

class HostileSkyArmorStates : StateMachineBuilder
{
    public HostileSkyArmorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlphiAI>()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 582;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68612, NameID = 2009561)]
public class HostileSkyArmor(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override void UpdateModule()
    {
        Arena.Center = Raid.Player()!.Position;
    }

    protected override bool CheckPull() => true;
}

