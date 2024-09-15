namespace BossMod.Dawntrail.Quest.TheFeatOfTheBrotherhood;

public enum OID : uint
{
    Boss = 0x4206,
    Helper = 0x233C,
    OathOfFire = 0x4229,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->420A, no cast, single-target
    _Ability_ = 37199, // Boss->location, no cast, single-target
    _Spell_RoaringStar = 39305, // Helper->self, 8.0s cast, range 40 circle
    _Spell_RoaringStar1 = 39304, // Boss->self, 8.0s cast, single-target
    _Ability_RoaringStar = 39302, // 44B6->self, 6.0s cast, range 50 width 10 rect
    _Weaponskill_CoiledStrike = 37187, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_CoiledStrike1 = 37188, // Helper->self, 6.0s cast, range 30 150-degree cone
    _Spell_CelestialFlame = 37175, // Boss->self, 4.0s cast, single-target
    _Ability_CelestialFlame = 37206, // 4228->self, no cast, range 40 circle
    _Ability_SublimeHeat = 37176, // 4228->self, 5.0s cast, range 10 circle
    _Weaponskill_DualPyres = 37181, // Boss->self, 7.0s cast, single-target
    _Weaponskill_DualPyres1 = 37310, // Helper->self, 8.0s cast, range 30 180-degree cone
    _Weaponskill_DualPyres2 = 37309, // Boss->self, no cast, single-target
    _Weaponskill_DualPyres3 = 37311, // Helper->self, 10.5s cast, range 30 180-degree cone
    _Ability_Burn = 39299, // Helper->self, 5.0s cast, range 46 width 5 rect
    _Weaponskill_SteelfoldStrike = 37182, // Boss->location, 5.0+1.0s cast, single-target
    _Spell_FirstLight = 39298, // Helper->location, 6.0s cast, range 6 circle
    _Weaponskill_SteelfoldStrike1 = 37183, // Helper->self, 6.0s cast, range 30 width 8 cross
    _Weaponskill_ = 39532, // Helper->self, 6.0s cast, range 30 width 8 cross
    _Weaponskill_SteelfoldStrike2 = 39300, // Boss->location, no cast, single-target
    _Ability_OuterWake = 37202, // Boss->self, 2.5+2.5s cast, single-target
    _Ability_OuterWake1 = 37203, // Helper->self, 5.0s cast, range 6-40 donut
    _Spell_FallenStar = 37205, // Helper->player/4210/420C/420D/420B/420E/420F/420A, 5.0s cast, range 6 circle
    _Weaponskill_LayOfTheSun = 37207, // Boss->420A, 4.9+3.1s cast, range 6 circle
    _Weaponskill_LayOfTheSun1 = 37208, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun2 = 37209, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun3 = 37210, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun4 = 37211, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun5 = 37212, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun6 = 37213, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun7 = 37214, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun8 = 37215, // Helper->420A, no cast, range 6 circle
    _Spell_Oathbind = 37216, // Boss->self, 11.0s cast, single-target
    _Spell_ = 40064, // Helper->420E/420D/420C/420A, no cast, single-target
    _Ability_1 = 37217, // Helper->420E/420D/420C/420A, no cast, single-target
    _Ability_2 = 39810, // Helper->self, no cast, single-target
    _Weaponskill_NobleTrail = 37218, // Boss->location, 50.0s cast, width 20 rect charge
    _Ability_InnerWake = 37200, // Boss->self, 2.5+2.5s cast, single-target
    _Ability_InnerWake1 = 37201, // Helper->self, 5.0s cast, range 10 circle
    _Weaponskill_DualPyres4 = 37179, // Helper->self, 8.0s cast, range 30 180-degree cone
    _Weaponskill_DualPyres5 = 37177, // Boss->self, 7.0s cast, single-target
    _Weaponskill_DualPyres6 = 37178, // Boss->self, no cast, single-target
    _Weaponskill_DualPyres7 = 37180, // Helper->self, 10.5s cast, range 30 180-degree cone
    _Weaponskill_CoiledStrike2 = 37186, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_BattleBreaker = 37192, // Boss->self, 5.0s cast, range 40 width 40 rect
    _Ability_SagaOfDawnAndDuty = 37193, // Boss->location, no cast, ???
    _Ability_SagaOfDawnAndDuty1 = 37196, // Helper->self, no cast, range 60 circle
    _Ability_Shockwave = 39711, // Helper->self, no cast, range 60 circle
    _Ability_HeavyImpact = 37197, // Helper->self, no cast, range 60 circle
    _Weaponskill_LayOfTheSun9 = 40065, // Boss->420A, 4.9+3.1s cast, range 6 circle
    _Weaponskill_LayOfTheSun10 = 40066, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun11 = 40067, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun12 = 40068, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun13 = 40069, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun14 = 40070, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun15 = 40071, // Helper->420A, no cast, range 6 circle
    _Weaponskill_LayOfTheSun16 = 40072, // Helper->420A, no cast, range 6 circle
}

class LayOfTheSun(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_LayOfTheSun or AID._Weaponskill_LayOfTheSun9 && WorldState.Actors.Find(spell.TargetID) is Actor target)
            Stacks.Add(new(target, 6, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_LayOfTheSun8 or AID._Weaponskill_LayOfTheSun16)
            Stacks.Clear();
    }
}
class NobleTrail(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NobleTrail), 10)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in base.ActiveAOEs(slot, actor))
            if ((e.Activation - WorldState.CurrentTime).TotalSeconds < 5)
                yield return e;
    }
}
class OathOfFire(BossModule module) : Components.Adds(module, (uint)OID.OathOfFire);
class InnerWake(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_InnerWake1), new AOEShapeCircle(10));
class DualPyre3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DualPyres4), new AOEShapeCone(30, 90.Degrees()));
class DualPyre4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DualPyres7), new AOEShapeCone(30, 90.Degrees()))
{
    private DualPyre3? DP3;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DP3 ??= Module.FindComponent<DualPyre3>();
        if (DP3?.Casters.Count > 0)
            yield break;

        foreach (var e in base.ActiveAOEs(slot, actor))
            yield return e;
    }
}
class RoaringStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_RoaringStar));
class RoaringStar1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_RoaringStar), new AOEShapeRect(50, 5));
class CoiledStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CoiledStrike1), new AOEShapeCone(30, 75.Degrees()));
class SublimeHeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_SublimeHeat), new AOEShapeCircle(10));
class DualPyres1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DualPyres1), new AOEShapeCone(30, 90.Degrees()));
class DualPyres2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DualPyres3), new AOEShapeCone(30, 90.Degrees()))
{
    private DualPyres1? DP1;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DP1 ??= Module.FindComponent<DualPyres1>();
        if (DP1?.ActiveCasters.Any() ?? false)
            yield break;

        foreach (var e in base.ActiveAOEs(slot, actor))
            yield return e;
    }
}
class Burn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Burn), new AOEShapeRect(46, 2.5f), maxCasts: 8);
class FirstLight(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_FirstLight), 6);
class SteelfoldStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SteelfoldStrike1), new AOEShapeCross(30, 4));
// class SteelfoldStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SteelfoldStrike1), new AOEShapeCross(30, 4));
class OuterWake(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_OuterWake1), new AOEShapeDonut(6, 40));
class FallenStar(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_FallenStar), 6);
class DawnAndDuty(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Wuks = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == 37195)
            Wuks.Add(caster);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var w in Wuks)
            yield return new AOEInstance(new AOEShapeRect(20, 20), w.Position, w.Rotation, Module.CastFinishAt(w.CastInfo));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_HeavyImpact)
            Wuks.Clear();
    }
}

class GuloolJaJasGloryStates : StateMachineBuilder
{
    public GuloolJaJasGloryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RoaringStar>()
            .ActivateOnEnter<RoaringStar1>()
            .ActivateOnEnter<CoiledStrike>()
            .ActivateOnEnter<SublimeHeat>()
            .ActivateOnEnter<DualPyres1>()
            .ActivateOnEnter<DualPyres2>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<FirstLight>()
            .ActivateOnEnter<SteelfoldStrike>()
            .ActivateOnEnter<OuterWake>()
            .ActivateOnEnter<FallenStar>()
            .ActivateOnEnter<LayOfTheSun>()
            .ActivateOnEnter<NobleTrail>()
            .ActivateOnEnter<InnerWake>()
            .ActivateOnEnter<DualPyre3>()
            .ActivateOnEnter<DualPyre4>()
            .ActivateOnEnter<OathOfFire>()
            .ActivateOnEnter<DawnAndDuty>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70444, NameID = 12734)]
public class GuloolJaJasGlory(WorldState ws, Actor primary) : BossModule(ws, primary, new(353.47f, 596.4f), new ArenaBoundsRect(20, 20, 12.5f.Degrees()))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.Type == ActorType.Enemy && x.IsAlly), ArenaColor.PlayerGeneric);
}
