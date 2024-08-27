namespace BossMod.Stormblood.Quest.HopeOnTheWaves;

public enum OID : uint
{
    Boss = 0x2114,
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

class EncounterStates : StateMachineBuilder
{
    public EncounterStates(BossModule module) : base(module)
    {
        TrivialPhase(0, 1800)
            .ActivateOnEnter<BoundsP1>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<P1Barrier>()
            .ActivateOnEnter<CermetPile>()
            .Raw.Update = () => Module.Enemies(0x21CC).Any();
        TrivialPhase(1, 1800)
            .ActivateOnEnter<CircleOfDeath>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<MagitekMissileProximity>()
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MineSelfDetonate>()
            .ActivateOnEnter<AssaultCannon>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(472.40f, 751.06f);
                Module.Arena.Bounds = Encounter.BoundsP2;
            })
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

class P1Barrier(BossModule module) : BossComponent(module)
{
    public bool Transition = false;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x0008)
            Transition = true;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Transition)
            hints.Add("Go upstairs!", false);
    }
}

class Adds : Components.AddsMulti
{
    private Actor? Alphinaud => WorldState.Actors.FirstOrDefault(a => a.OID == 0x21AC);

    public Adds(BossModule module) : base(module, [0x2114, 0x21B3, 0x21B4, 0x21B2, 0x21B7, 0x21B1, 0x2115])
    {
        KeepOnPhaseChange = true;
    }

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

class BoundsP1(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()!.Position;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 472, NameID = 6200)]
public class Encounter(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    private static readonly List<WDir> vertices = [
  new WDir(14.43f, -26.75f),
  new WDir(16.73f, -26.35f),
  new WDir(18.88f, -25.99f),
  new WDir(21.02f, -25.62f),
  new WDir(21.21f, -23.55f),
  new WDir(22.63f, -21.88f),
  new WDir(24.19f, -20.36f),
  new WDir(25.66f, -21.84f),
  new WDir(27.55f, -20.45f),
  new WDir(28.51f, -18.49f),
  new WDir(29.44f, -16.47f),
  new WDir(29.91f, -14.34f),
  new WDir(29.94f, -12.19f),
  new WDir(29.97f, -10.09f),
  new WDir(30.01f, -8.00f),
  new WDir(29.36f, -5.91f),
  new WDir(28.72f, -3.93f),
  new WDir(28.05f, -1.85f),
  new WDir(27.40f, 0.17f),
  new WDir(26.38f, 2.02f),
  new WDir(25.24f, 3.84f),
  new WDir(24.12f, 5.63f),
  new WDir(23.00f, 7.42f),
  new WDir(21.88f, 9.16f),
  new WDir(20.38f, 10.72f),
  new WDir(18.88f, 12.28f),
  new WDir(17.41f, 13.78f),
  new WDir(15.98f, 15.27f),
  new WDir(14.49f, 16.82f),
  new WDir(12.75f, 18.04f),
  new WDir(10.96f, 19.30f),
  new WDir(9.18f, 20.55f),
  new WDir(7.48f, 21.74f),
  new WDir(5.66f, 22.98f),
  new WDir(3.74f, 23.81f),
  new WDir(1.80f, 24.64f),
  new WDir(-0.18f, 25.49f),
  new WDir(-2.13f, 26.33f),
  new WDir(-4.15f, 27.03f),
  new WDir(-6.30f, 27.37f),
  new WDir(-8.39f, 27.68f),
  new WDir(-10.48f, 28.01f),
  new WDir(-12.55f, 28.28f),
  new WDir(-14.72f, 27.91f),
  new WDir(-16.84f, 27.52f),
  new WDir(-18.94f, 27.16f),
  new WDir(-20.86f, 26.31f),
  new WDir(-22.53f, 25.14f),
  new WDir(-24.21f, 23.91f),
  new WDir(-25.09f, 21.59f),
  new WDir(-26.44f, 20.05f),
  new WDir(-27.86f, 18.08f),
  new WDir(-28.36f, 16.06f),
  new WDir(-28.39f, 13.90f),
  new WDir(-28.41f, 11.73f),
  new WDir(-28.44f, 9.62f),
  new WDir(-27.85f, 7.52f),
  new WDir(-27.20f, 5.53f),
  new WDir(-26.53f, 3.44f),
  new WDir(-25.89f, 1.46f),
  new WDir(-24.93f, -0.33f),
  new WDir(-22.66f, -3.94f),
  new WDir(-21.53f, -5.74f),
  new WDir(-20.40f, -7.55f),
  new WDir(-18.38f, -6.30f),
  new WDir(-16.41f, -6.77f),
  new WDir(-14.87f, -8.06f),
  new WDir(-15.53f, -10.37f),
  new WDir(-15.59f, -12.60f),
  new WDir(-14.12f, -14.12f),
  new WDir(-12.61f, -15.58f),
  new WDir(-10.86f, -16.81f),
  new WDir(-9.07f, -18.06f),
  new WDir(-7.33f, -19.28f),
  new WDir(-5.56f, -20.52f),
  new WDir(-3.79f, -21.65f),
  new WDir(-1.77f, -22.52f),
  new WDir(0.31f, -22.92f),
  new WDir(2.47f, -22.80f),
  new WDir(4.32f, -21.81f),
  new WDir(6.80f, -21.99f),
  new WDir(7.54f, -24.27f),
  new WDir(8.90f, -26.06f),
  new WDir(11.15f, -26.42f),
  new WDir(13.30f, -26.74f),
];

    public static readonly ArenaBoundsCustom BoundsP2 = new(30, new(vertices));
}
