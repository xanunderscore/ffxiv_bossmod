namespace BossMod.Heavensward.Dungeon.D03TheAery.D031Gyascutus;

public enum OID : uint
{
    Boss = 0x3970,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_ProximityPyre = 30191, // Boss->self, 4.0s cast, range 12 circle
    _Ability_InflammableFumes = 30181, // Boss->self, 4.0s cast, single-target
    _Ability_ = 31232, // 3972->location, no cast, single-target
    _Weaponskill_Burst = 30184, // Helper->self, 10.0s cast, range 10 circle
    _Ability_Burst = 30183, // 3972->self, no cast, single-target
    _Weaponskill_DeafeningBellow = 31233, // Boss->self, 4.0s cast, range 55 circle
    _Weaponskill_AshenOuroboros = 30190, // Boss->self, 8.0s cast, range 11-20 donut
    _Weaponskill_BodySlam = 31234, // Boss->self, 4.0s cast, range 30 circle
    _Weaponskill_CripplingBlow = 30193, // Boss->player, 5.0s cast, single-target
}

class AshenOuroboros(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AshenOuroboros), new AOEShapeDonut(11, 20));
class Burst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Burst), new AOEShapeCircle(10));
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DeafeningBellow));
class ProximityPyre(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ProximityPyre), new AOEShapeCircle(12));
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_CripplingBlow));

class GyascutusStates : StateMachineBuilder
{
    public GyascutusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AshenOuroboros>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<ProximityPyre>()
            .ActivateOnEnter<CripplingBlow>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3455)]
public class Gyascutus(WorldState ws, Actor primary) : BossModule(ws, primary, new(12, 68), new ArenaBoundsCircle(20));

