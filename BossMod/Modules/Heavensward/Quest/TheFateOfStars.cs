namespace BossMod.Heavensward.Quest.TheFateOfStars;

public enum OID : uint
{
    Boss = 0x161E,
    Helper = 0x233C,
    _Gen_MagitekTurretI = 0x161F, // R0.600, x0 (spawn during fight)
    _Gen_MagitekTurretII = 0x1620, // R0.600, x0 (spawn during fight)
    _Gen_TerminusEst = 0x1621, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_MagitekTurret = 6029, // Boss->self, no cast, single-target
    _Weaponskill_MagitekSlug = 6026, // Boss->self, 2.5s cast, range 60+R width 4 rect
    _Weaponskill_AetherochemicalLaser = 6030, // 161F->player, no cast, single-target
    _Weaponskill_Quickstep = 6028, // Boss->location, no cast, single-target
    _Weaponskill_AetherochemicalGrenado = 6031, // 1620->location, 3.0s cast, range 8 circle
    _Weaponskill_TerminusEst = 6022, // Boss->self, no cast, single-target
    _Weaponskill_ = 6024, // Boss->self, no cast, single-target
    _Weaponskill_SelfDetonate = 6032, // 161F/1620->self, 5.0s cast, range 40+R circle
    _Weaponskill_MagitekSpread = 6027, // Boss->self, 3.0s cast, range 20+R 240-degree cone
}

class MagitekSlug(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekSlug), new AOEShapeRect(60, 2));
class AetherochemicalGrenado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AetherochemicalGrenado), 8);
class SelfDetonate(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_SelfDetonate), "Kill turret before detonation!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}
class MagitekSpread(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekSpread), new AOEShapeCone(20.55f, 120.Degrees()));

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67824, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(230, 79), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

