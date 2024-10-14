namespace BossMod.Endwalker.Quest.TheGameIsAfoot;

public enum OID : uint
{
    Boss = 0x4037,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->402D, no cast, single-target
    _Weaponskill_WindUnbound = 34883, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_SnatchMorsel = 34884, // Boss->402D, 5.0s cast, single-target
    _Ability_ = 34885, // Boss->location, no cast, single-target
    _Weaponskill_PeckingFlurry = 34886, // Boss->self, 4.0s cast, range 40 circle
    _Weaponskill_FallingRock = 34888, // Helper->location, 5.0s cast, range 6 circle
    _Weaponskill_StickySpit = 34889, // Boss->self, 4.0s cast, single-target
    _Weaponskill_StickySpit1 = 34890, // Helper->player, 5.0s cast, range 6 circle
    _Weaponskill_GlidingSwoop = 34891, // Boss->location, 10.0s cast, width 16 rect charge - scripted, not dodgeable
    _Weaponskill_Swoop = 35717, // Boss->location, 5.0s cast, width 16 rect charge
    _Weaponskill_FurlingFlapping = 34892, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FurlingFlapping1 = 34893, // Helper->players/402D/402E, 5.0s cast, range 8 circle
    _Weaponskill_DeadlySwoop = 35888, // Boss->location, 30.0s cast, width 16 rect charge
}

class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_PeckingFlurry));
class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_WindUnbound));
class SnatchMorsel(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_SnatchMorsel), "Wukbuster");
class FallingRock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FallingRock), 6, maxCasts: 8);
class StickySpit(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_StickySpit1), 6, minStackSize: 1);
class Swoop(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Swoop), 8);
class FurlingFlapping(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_FurlingFlapping1), 8);
class DeadlySwoop(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlySwoop), 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Raid.WithoutSlot().Count() == 3)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GiantColibriStates : StateMachineBuilder
{
    public GiantColibriStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PeckingFlurry>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<SnatchMorsel>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<StickySpit>()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<DeadlySwoop>()
            .ActivateOnEnter<FurlingFlapping>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70288, NameID = 12499)]
public class GiantColibri(WorldState ws, Actor primary) : BossModule(ws, primary, new(425, -440), new ArenaBoundsCircle(15))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }
}

