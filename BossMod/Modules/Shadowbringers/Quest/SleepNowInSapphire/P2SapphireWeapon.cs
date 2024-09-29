using BossMod.Shadowbringers.Quest.SleepNowInSapphire.P1GuidanceSystem;

namespace BossMod.Shadowbringers.Quest.SleepNowInSapphire.P2SapphireWeapon;

public enum OID : uint
{
    Boss = 0x2DFA,
    Helper = 0x233C,
    _Gen_RegulasImage = 0x2DFB, // R0.750, x0 (spawn during fight)
    _Gen_MagitekTurret = 0x2DFC, // R1.650, x0 (spawn during fight)
    _Gen_CeruleumServant = 0x2DFD, // R6.250, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 6499, // Boss->player, no cast, single-target
    _Ability_TailSwing = 20326, // Boss->self, 4.0s cast, range 46 circle
    _Spell_OptimizedJudgment = 20325, // Boss->self, 4.0s cast, range -60 donut
    _Ability_Activate = 20335, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MagitekSpread = 20336, // _Gen_RegulasImage->self, 5.0s cast, range 43 ?-degree cone
    _Spell_ = 20337, // _Gen_RegulasImage->self, no cast, single-target
    _Spell_OptimizedUltima = 20342, // Boss->self, 6.0s cast, range 100 circle
    _Ability_Swiftbreach = 20330, // Boss->self, 4.5s cast, single-target
    _Ability_Swiftbreach1 = 21418, // Helper->self, 4.5s cast, range 120 width 120 rect
    _Spell_Siderays = 20328, // Boss->self, 8.0s cast, single-target
    SideraysRight = 20329, // Helper->self, 8.0s cast, range 128 ?-degree cone
    SideraysLeft = 21021, // Helper->self, 8.0s cast, range 128 ?-degree cone
    _Spell_SapphireRay = 20327, // Boss->self, 8.0s cast, range 120 width 40 rect
    _Ability_Turret = 20331, // Boss->self, 3.0s cast, single-target
    _Ability_ = 21465, // 2DFC->self, no cast, single-target
    _Spell_MagitekRay = 20332, // 2DFC->self, 3.0s cast, range 100 width 6 rect
    _Ability_SimultaneousActivation = 21395, // Boss->self, 3.0s cast, single-target
    _Spell_ServantRoar = 20339, // 2DFD->self, 2.5s cast, range 100 width 8 rect
    _Spell_FloodRay = 20338, // Boss->self, 115.0s cast, range 120 width 120 rect
}

public enum SID : uint
{
    _Gen_ = 2056, // none->_Gen_RegulasImage, extra=0x45
    _Gen_Invincibility = 775, // none->Boss, extra=0x0
}

class MagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_MagitekRay), new AOEShapeRect(100, 3));
class ServantRoar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ServantRoar), new AOEShapeRect(100, 4));
class TailSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_TailSwing), new AOEShapeCircle(46));
class OptimizedJudgment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_OptimizedJudgment), new AOEShapeDonut(21, 60));
class MagitekSpread(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekSpread), new AOEShapeCone(43, 120.Degrees()));
class SapphireRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SapphireRay), new AOEShapeRect(120, 20));
class Siderays(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor, WPos)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCone(128, 45.Degrees()), c.Item2, c.Item1.CastInfo!.Rotation, Module.CastFinishAt(c.Item1.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SideraysLeft:
                Casters.Add((caster, caster.Position + caster.Rotation.ToDirection().OrthoL() * 15));
                break;
            case AID.SideraysRight:
                Casters.Add((caster, caster.Position + caster.Rotation.ToDirection().OrthoR() * 15));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Casters.RemoveAll(c => c.Item1 == caster);
    }
}

class TheSapphireWeaponStates : StateMachineBuilder
{
    public TheSapphireWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<OptimizedJudgment>()
            .ActivateOnEnter<MagitekSpread>()
            .ActivateOnEnter<Siderays>()
            .ActivateOnEnter<SapphireRay>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ServantRoar>()
            .ActivateOnEnter<GWarrior>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69431, NameID = 9458)]
public class TheSapphireWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 610), new ArenaBoundsSquare(60, 1))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID._Gen_Invincibility) == null ? 1 : 0;
    }
}

