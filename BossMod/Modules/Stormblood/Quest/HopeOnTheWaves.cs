namespace BossMod.Stormblood.Quest.HopeOnTheWaves;

public enum OID : uint
{
    Boss = 0x21B1,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 871, // 21B2/Boss->player/21B0/21AE/21AC, no cast, single-target
    _Weaponskill_FastBlade = 717, // 21B3/21B1->player/21B0/21AC/21AE, no cast, single-target
    _AutoAttack_Attack1 = 870, // 21B3/2112/21B1/2115->player/21AC/21B0/2113/21AE, no cast, single-target
    _Spell_Thunder = 968, // 21B4->player/21B0/21AC/21AE, 1.0s cast, single-target
    _Weaponskill_Heartstopper = 866, // 21B2->self, 2.5s cast, range 3+R width 3 rect
    _Weaponskill_ShieldBash = 718, // 21B3/21B1->21B0/21AC/21AE, no cast, single-target
    _Weaponskill_CermetPile = 9425, // Boss->self, 2.5s cast, range 40+R width 6 rect
    _Ability_Fortis = 403, // 21B4/21B2->self, 2.5s cast, single-target
    _Spell_Paralyze = 308, // 21B4->21B0/21AC, 4.0s cast, single-target
    _Ability_Sentinel = 17, // 21B1->self, no cast, single-target
    _Ability_Celeris = 404, // 21B3->self, 2.5s cast, single-target
    _Ability_Rampart = 10, // 21B1/21B3->self, no cast, single-target
    _Weaponskill_TrueThrust = 722, // 21B2->player, no cast, single-target
    _Ability_FightOrFlight = 20, // 21B3->self, no cast, single-target
    _Weaponskill_SelfDetonate = 10928, // Boss->self, 30.0s cast, range 100 circle
    _Weaponskill_CircleOfDeath = 9428, // 2115->self, 3.0s cast, range 6+R circle
    _Weaponskill_2TonzeMagitekMissile = 10929, // 2115->location, 3.0s cast, range 6 circle
    _Weaponskill_MagitekMissile = 10816, // 2115->self, 3.0s cast, single-target
    _Weaponskill_SelfDetonate1 = 10930, // 21B6->self, 5.0s cast, range 6 circle
    _Weaponskill_MagitekMissile1 = 10893, // 21B7->location, 10.0s cast, range 60 circle
    _Weaponskill_Feint = 76, // 21B2->player/21B9, no cast, single-target
    _Ability_AssaultCannon = 10823, // 21B5->self, 2.5s cast, range 75+R width 2 rect
}

class AssaultCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_AssaultCannon), new AOEShapeRect(75, 1));
class CircleOfDeath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CircleOfDeath), new AOEShapeCircle(10.24f));
class TwoTonzeMagitekMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_2TonzeMagitekMissile), 6);
class MagitekMissileProximity(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekMissile1), 11.75f);
class CermetPile(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CermetPile), new AOEShapeRect(42, 3));
class SelfDetonate(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_SelfDetonate), "Kill before detonation!", true);
class MineSelfDetonate(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SelfDetonate1), new AOEShapeCircle(6));

class Adds(BossModule module) : BossComponent(module)
{
    private Actor? Alphinaud => WorldState.Actors.FirstOrDefault(a => a.OID == 0x21AC);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        WPos? lbCenter = Alphinaud?.CastInfo is { Action.ID: 10894 } castInfo
            ? castInfo.LocXZ
            : null;

        foreach (var e in hints.PotentialTargets)
        {
            if (lbCenter != null && e.Actor.OID == 0x2114)
            {
                e.ShouldBeTanked = true;
                e.DesiredPosition = lbCenter.Value;
                e.Priority = 5;
            }
            else if (e.Actor.CastInfo?.Action.ID == (uint)AID._Weaponskill_SelfDetonate)
                e.Priority = 5;
            else
                e.Priority = 0;
        }
    }
}

class ImperialCenturionStates : StateMachineBuilder
{
    public ImperialCenturionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<CircleOfDeath>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<MagitekMissileProximity>()
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MineSelfDetonate>()
            .ActivateOnEnter<AssaultCannon>()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 472;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68560, NameID = 4148)]
public class ImperialCenturion(WorldState ws, Actor primary) : BossModule(ws, primary, new(473.25f, 751.75f), BoundsP2)
{
    public static readonly ArenaBoundsCustom BoundsP2 = new(30, new(CurveApprox.Ellipse(34, 21, 0.25f).Select(p => p.Rotate(140.Degrees()))));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
