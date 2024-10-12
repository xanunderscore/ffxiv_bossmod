﻿namespace BossMod.Endwalker.Dungeon.D08Troia.D081EvilDreamer;

public enum OID : uint
{
    Boss = 0x3966,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 30246, // Boss/3967/3968->player, no cast, single-target
    _Weaponskill_DarkVision = 29624, // Boss->self, 8.0s cast, range 40 width 5 rect
    _Weaponskill_VoidGravity = 29626, // Boss->player, 6.0s cast, range 6 circle
    _Weaponskill_DarkVision1 = 29627, // Boss->self, 15.0s cast, range 41 width 5 rect
    _Weaponskill_UniteMare = 29628, // 3968->self, 10.0s cast, range 12 circle
    _Ability_ = 29629, // Boss->location, no cast, single-target
    _Ability_1 = 29623, // Boss->location, no cast, single-target
    UniteMareNormal = 29621, // Boss->self, 11.0s cast, range 6 circle
    UniteMareBig = 29622, // Boss->self, 11.0s cast, range 6 circle
    _Weaponskill_EndlessNightmare = 29630, // 3988->self, 60.0s cast, range 20 circle
}

public enum TetherID : uint
{
    _Gen_Tether_14 = 14, // Boss->Boss
}

class DarkVision(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DarkVision), new AOEShapeRect(40, 2.5f));
class DarkVision1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DarkVision1), new AOEShapeRect(41, 2.5f));
class UniteMare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UniteMare), new AOEShapeCircle(12))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count < 7)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class UniteMare1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UniteMareNormal), new AOEShapeCircle(6));
class UniteMare2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UniteMareBig), new AOEShapeCircle(18));
class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidGravity), 6);
class EndlessNightmare(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_EndlessNightmare), "Kill before enrage!", showCastTimeLeft: true);

class EvilDreamerStates : StateMachineBuilder
{
    private readonly EvilDreamer _module;

    public EvilDreamerStates(EvilDreamer module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<DarkVision>()
            .ActivateOnEnter<DarkVision1>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<UniteMare>()
            .ActivateOnEnter<UniteMare1>()
            .ActivateOnEnter<UniteMare2>()
            .ActivateOnEnter<EndlessNightmare>()
            .Raw.Update = () => _module.ReallyBigEvilDreamer?.IsDeadOrDestroyed ?? false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11382)]
public class EvilDreamer(WorldState ws, Actor primary) : BossModule(ws, primary, new(168, 90), new ArenaBoundsCircle(20))
{
    public Actor? ReallyBigEvilDreamer => Enemies(0x3988).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
