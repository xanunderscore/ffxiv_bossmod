namespace BossMod.Heavensward.Quest.DivineIntervention;

public enum OID : uint
{
    Boss = 0x1010,
    Helper = 0x233C,
    _Gen_IshgardianSteelChain = 0x102C, // R1.000, x1
    _Gen_SerPaulecrainColdfire = 0x1011, // R0.500, x1
    _Gen_ThunderPicket = 0xEC4, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->100F, no cast, single-target
    _AutoAttack_Attack1 = 871, // 1011->player, no cast, single-target
    _Ability_FirstLesson = 3991, // Boss->100F, no cast, single-target
    _Ability_LifeSurge = 83, // 1011->self, no cast, single-target
    _Ability_LightningBolt = 3993, // EC4->E0F, 2.0s cast, width 4 rect charge
    _Weaponskill_IronTempest = 1003, // Boss->self, 3.5s cast, range 5+R circle
    _Weaponskill_ThunderThrust = 3992, // 1011->self, 4.0s cast, range 40 circle
    _Ability_Bloodbath = 34, // Boss->self, no cast, single-target
    _Weaponskill_Overpower = 720, // Boss->self, 2.5s cast, range 6+R 90-degree cone
    _Weaponskill_RingOfFrost = 1316, // 1011->self, 3.0s cast, range 6+R circle
    _Weaponskill_Rive = 1135, // Boss->self, 2.5s cast, range 30+R width 2 rect
    _Weaponskill_Heartstopper = 866, // 1011->self, 2.5s cast, range 3+R width 3 rect
}

class LightningBolt(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Ability_LightningBolt), 2);
class IronTempest(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_IronTempest), new AOEShapeCircle(5.5f));
class Overpower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Overpower), new AOEShapeCone(6.5f, 45.Degrees()));
class RingOfFrost(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RingOfFrost), new AOEShapeCircle(6.5f));
class Rive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Rive), new AOEShapeRect(30.5f, 1));
class Heartstopper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Heartstopper), new AOEShapeRect(3.5f, 1.5f));
class Chain(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.OID == (uint)OID._Gen_IshgardianSteelChain ? 1 : 0;
    }
}

class SerGrinnauxTheBullStates : StateMachineBuilder
{
    public SerGrinnauxTheBullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<RingOfFrost>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<Chain>()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 396;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 396, NameID = 3850)]
public class SerGrinnauxTheBull(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 2), B)
{
    public static ArenaBoundsCustom NewBounds()
    {
        var arc = CurveApprox.CircleArc(new(3.6f, 0), 11.5f, 0.Degrees(), 180.Degrees(), 0.01f);
        var arc2 = CurveApprox.CircleArc(new(-3.6f, 0), 11.5f, 180.Degrees(), 360.Degrees(), 0.01f);

        return new(16, new(arc.Concat(arc2).Select(a => a.ToWDir())));
    }

    public static readonly ArenaBoundsCustom B = NewBounds();
}
