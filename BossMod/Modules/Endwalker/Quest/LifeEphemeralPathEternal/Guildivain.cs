namespace BossMod.Endwalker.Quest.LifeEphemeralPathEternal;

class AetherstreamTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 2), (uint)TetherID.Noulith)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_AetherstreamPlayer or AID._Weaponskill_AetherstreamTank)
            CurrentBaits.RemoveAll(x => x.Target.InstanceID == spell.MainTargetID);
    }
}

class Tracheostomy : Components.SelfTargetedAOEs
{
    public Tracheostomy(BossModule module) : base(module, ActionID.MakeSpell(AID._Weaponskill_Tracheostomy), new AOEShapeDonut(10, 20))
    {
        WorldState.Actors.EventStateChanged.Subscribe((act) =>
        {
            if (act.OID == 0x1EA1A1 && act.EventState == 7)
                Arena.Bounds = new ArenaBoundsCircle(20);
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Arena.Bounds = new ArenaBoundsCircle(10);
    }
}

class RightScalpel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RightScalpel), new AOEShapeCone(15, 105.Degrees()));
class LeftScalpel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LeftScalpel), new AOEShapeCone(15, 105.Degrees()));
class Laparotomy(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Laparotomy), new AOEShapeCone(15, 60.Degrees()));
class Amputation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Amputation), new AOEShapeCone(20, 60.Degrees()));

class Hypothermia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Hypothermia));
class Cryonics(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Cryonics), 6);
class Craniotomy(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Craniotomy));
class RightLeftScalpel1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RightLeftScalpel), new AOEShapeCone(15, 105.Degrees()));
class RightLeftScalpel2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RightLeftScalpel1), new AOEShapeCone(15, 105.Degrees()));
class LeftRightScalpel1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LeftRightScalpel), new AOEShapeCone(15, 105.Degrees()));
class LeftRightScalpel2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LeftRightScalpel1), new AOEShapeCone(15, 105.Degrees()));

class EnhancedNoulith(BossModule module) : Components.Adds(module, (uint)OID._Gen_EnhancedNoulith)
{
    private readonly List<(Actor, Actor)> Tethers = [];
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Craniotomy && WorldState.Actors.Find(tether.Target) is Actor target)
            Tethers.Add((source, target));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Craniotomy)
            Tethers.RemoveAll(t => t.Item2 == actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var t in Tethers)
            Arena.AddLine(t.Item1.Position, t.Item2.Position, ArenaColor.Danger);
    }
}
class Frigotherapy(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Frigotherapy1), 5);

class GuildivainOfTheTaintedEdgeStates : StateMachineBuilder
{
    public GuildivainOfTheTaintedEdgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherstreamTether>()
            .ActivateOnEnter<Tracheostomy>()
            .ActivateOnEnter<RightScalpel>()
            .ActivateOnEnter<LeftScalpel>()
            .ActivateOnEnter<Laparotomy>()
            .ActivateOnEnter<Amputation>()
            .ActivateOnEnter<Hypothermia>()
            .ActivateOnEnter<Cryonics>()
            .ActivateOnEnter<EnhancedNoulith>()
            .ActivateOnEnter<Craniotomy>()
            .ActivateOnEnter<RightLeftScalpel1>()
            .ActivateOnEnter<RightLeftScalpel2>()
            .ActivateOnEnter<LeftRightScalpel1>()
            .ActivateOnEnter<LeftRightScalpel2>()
            .ActivateOnEnter<Frigotherapy>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69608, NameID = 10733, PrimaryActorOID = (uint)OID.BossP2)]
public class GuildivainOfTheTaintedEdge(WorldState ws, Actor primary) : BossModule(ws, primary, new(224.8f, -855.8f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => hints.PrioritizeAll();
}

