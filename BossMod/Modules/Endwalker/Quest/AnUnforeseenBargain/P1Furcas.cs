using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AnUnforeseenBargain.P1Furcas;

public enum OID : uint
{
    Boss = 0x3D71,
    Helper = 0x233C,
    _Gen_VisitantBlackguard = 0x3EA2, // R1.700, x3
    _Gen_VisitantPersona = 0x3D74, // R1.600, x1
    _Gen_VisitantArchDemon = 0x3EA3, // R1.000, x1
    _Gen_VisitantDahak = 0x3D75, // R2.750, x2
    _Gen_VisitantTaurus = 0x3EA7, // R1.680, x2
    _Gen_VisitantVoidskipper = 0x3D72, // R1.080, x2 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // _Gen_VisitantVoidskipper->player, no cast, single-target
    _AutoAttack_Attack1 = 6497, // _Gen_VisitantPersona->player, no cast, single-target
    _AutoAttack_Attack2 = 870, // _Gen_VisitantBlackguard->player, no cast, single-target
    _AutoAttack_Attack3 = 6499, // _Gen_VisitantDahak->player, no cast, single-target
    _AutoAttack_ = 33023, // Boss->player, no cast, single-target
    _Weaponskill_VoidSlash = 33027, // _Gen_VisitantBlackguard->self, 4.0s cast, range 8+R 90-degree cone
    _Ability_SinisterSphere = 33003, // Boss->self, 4.0s cast, single-target
    _Ability_Explosion = 33004, // Helper->self, 10.0s cast, range 5 circle
    _Ability_UnmitigatedExplosion = 33005, // Helper->self, no cast, range 60 circle
    _Weaponskill_JongleursX = 31802, // Boss->player, 4.0s cast, single-target
    _Weaponskill_StraightSpindle = 31796, // _Gen_VisitantVoidskipper->self, 4.0s cast, range 50+R width 5 rect
    _Ability_VoidTorch = 33006, // Boss->self, 3.0s cast, single-target
    _Ability_VoidTorch1 = 33007, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_HellishScythe = 31800, // Boss->self, 5.0s cast, range 10 circle
    _Ability_FlameBlast = 33008, // 3ED4->self, 4.0s cast, range 80+R width 4 rect
    _Weaponskill_Blackout = 31798, // _Gen_VisitantVoidskipper->self, 13.0s cast, range 60 circle
    _Weaponskill_JestersReward = 33031, // Boss->self, 6.0s cast, range 28 180-degree cone
}

class AutoZero(BossModule module) : Components.RotationModule<ZeroAI>(module);
class Explosion(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Ability_Explosion), 5);
class VoidSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VoidSlash), new AOEShapeCone(9.7f, 45.Degrees()));
class JongleursX(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_JongleursX));
class StraightSpindle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StraightSpindle), new AOEShapeRect(50, 2.5f));
class VoidTorch(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_VoidTorch1), 6);
class HellishScythe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HellishScythe), new AOEShapeCircle(10));
class FlameBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_FlameBlast), new AOEShapeRect(80, 2));
class JestersReward(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_JestersReward), new AOEShapeCone(28, 90.Degrees()));
class Blackout(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_Blackout), "Kill wasp before enrage!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}

class FurcasStates : StateMachineBuilder
{
    public FurcasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoZero>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<JongleursX>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<VoidTorch>()
            .ActivateOnEnter<HellishScythe>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<Blackout>()
            .ActivateOnEnter<JestersReward>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209, NameID = 12066)]
public class Furcas(WorldState ws, Actor primary) : BossModule(ws, primary, new(97.85f, 286), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
