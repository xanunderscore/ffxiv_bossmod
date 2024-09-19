namespace BossMod.Endwalker.Quest.SagesFocus;

public enum OID : uint
{
    Boss = 0x3587,
    Helper = 0x233C,
    _Gen_ChiBomb = 0x358D, // R1.000, x0 (spawn during fight)
    Mahaud = 0x3586,
    Loifa = 0x3588,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->3589, no cast, single-target
    _Weaponskill_TripleThreat = 26535, // Boss->3589, 8.0s cast, single-target
    _Weaponskill_ChiBomb = 26536, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ = 26544, // Boss->location, no cast, single-target
    _Weaponskill_Explosion = 26537, // 358D->self, 5.0s cast, range 6 circle
    _Weaponskill_ArmOfTheScholar = 26543, // Boss->self, 5.0s cast, range 5 circle
    _Weaponskill_Nouliths = 26538, // 3588->self, 5.0s cast, single-target
    _Weaponskill_Noubelea = 26541, // 3588->self, 5.0s cast, single-target
    _Weaponskill_Noubelea1 = 26542, // 358E->self, 5.0s cast, range 50 width 4 rect
    _Weaponskill_1 = 26540, // 358E->location, no cast, single-target
    _Weaponskill_DemiblizzardIII = 26545, // 3586->self, 5.0s cast, single-target
    _Weaponskill_DemiblizzardIII1 = 26546, // Helper->self, 5.0s cast, range -40 donut
    _Weaponskill_Demigravity = 26539, // 3586->location, 5.0s cast, range 6 circle
    _Weaponskill_Demigravity1 = 26550, // Helper->location, 5.0s cast, range 6 circle
    _AutoAttack_Demifire = 26558, // 3586->3589, no cast, single-target
    _Weaponskill_DemifireIII = 26547, // 3586->self, 5.0s cast, single-target
    _Weaponskill_DemifireIII1 = 26548, // Helper->self, 5.6s cast, range 40 circle
    _Spell_Diagnosis = 26555, // 3588->3586, 3.0s cast, single-target
    _Weaponskill_DemifireII = 26552, // Mahaud->self, 7.0s cast, single-target
    _Weaponskill_DemifireII1 = 26553, // Helper->player/3589, 5.0s cast, range 5 circle
    _Weaponskill_DemifireII2 = 26554, // Helper->location, 5.0s cast, range 14 circle
}

class DemifireSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_DemifireII1), 5);
class DemifireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DemifireII2), 14);
class DemifireIII(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DemifireIII1));
class Noubelea(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Noubelea1), new AOEShapeRect(50, 2));
class Demigravity(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Demigravity), 6);
class Demigravity1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Demigravity1), 6);
class Demiblizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DemiblizzardIII1), new AOEShapeDonut(10, 40));
class TripleThreat(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_TripleThreat));
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Explosion), new AOEShapeCircle(6));
class ArmOfTheScholar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArmOfTheScholar), new AOEShapeCircle(5));

class AncelRockfistStates : StateMachineBuilder
{
    public AncelRockfistStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleThreat>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ArmOfTheScholar>()
            .ActivateOnEnter<Noubelea>()
            .ActivateOnEnter<Demiblizzard>()
            .ActivateOnEnter<Demigravity>()
            .ActivateOnEnter<Demigravity1>()
            .ActivateOnEnter<DemifireIII>()
            .ActivateOnEnter<DemifireII>()
            .ActivateOnEnter<DemifireSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69604, NameID = 10732)]
public class AncelRockfist(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -82.17f), new ArenaBoundsCircle(18.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.Loifa, 1);
    }
}

