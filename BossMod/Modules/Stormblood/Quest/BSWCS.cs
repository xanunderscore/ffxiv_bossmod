namespace BossMod.Stormblood.Quest;

public enum OID : uint
{
    Boss = 0x1A53,
    Helper = 0x233C,
    _Gen_12ThLegionSlasher = 0x1D2F, // R1.050f, x2 (spawn during fight)
    _Gen_12ThLegionHastatus = 0x1A54, // R0.500f, x1
    _Gen_12ThLegionPrinceps = 0x1A55, // R0.500f, x1
    _Gen_MagitekVanguardIPrototype = 0x1A52, // R2.100f, x1
}

public enum AID : uint
{
    _Spell_Fire = 966, // 1A55->1A5C, 1.0fs cast, single-target
    _Weaponskill_CermetPile = 8117, // 1A52->self, 3.0fs cast, range 4$1fR width 6 rect
    _Weaponskill_Firebomb = 8495, // Boss->location, 3.0fs cast, range 4 circle

    _Weaponskill_FastBlade = 717, // _Gen_12ThLegionHastatus->1A5B, no cast, single-target
    _AutoAttack_Attack = 870, // _Gen_MagitekVanguardIPrototype/_Gen_12ThLegionHastatus/_Gen_12ThLegionSlasher->player/1A5A/1A5B, no cast, single-target
    _Weaponskill_AugmentedShatter = 8494, // Boss->1A59, no cast, single-target
    _Weaponskill_OpenFire = 8120, // _Gen_MagitekVanguardIPrototype->self, 3.0fs cast, single-target
    _Weaponskill_OpenFire1 = 8121, // 19D9->location, 3.0fs cast, range 6 circle
    _Weaponskill_AugmentedSuffering = 8492, // Boss->self, 3.5fs cast, range $1fR circle
    _AutoAttack_Attack1 = 872, // Boss->1A59, no cast, single-target
    _Weaponskill_AugmentedUprising = 8493, // Boss->self, 3.0fs cast, range $1fR 120-degree cone
}

class AugmentedUprising(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedUprising), new AOEShapeCone(8.5f, 60.Degrees()));
class AugmentedSuffering(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedSuffering), new AOEShapeCircle(6.5f));
class OpenFire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OpenFire1), 6);

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID._Gen_12ThLegionPrinceps, (uint)OID._Gen_12ThLegionSlasher, (uint)OID._Gen_12ThLegionHastatus, (uint)OID._Gen_MagitekVanguardIPrototype])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.TargetID == actor.InstanceID ? 2
                : (OID)e.Actor.OID == OID._Gen_MagitekVanguardIPrototype ? 1
                : 0;
    }
}

class CermetPile(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CermetPile), new AOEShapeRect(42.1f, 3f));
class Firebomb(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Firebomb), 4);

class GrynewahtStates : StateMachineBuilder
{
    public GrynewahtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<OpenFire>()
            .ActivateOnEnter<AugmentedUprising>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 463, NameID = 5576)]
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(-465.40f, -202.09f), CustomBounds)
{
    private static readonly List<WDir> vertices = [
  new WDir(-21.00f, -31.31f),
  new WDir(-19.06f, -30.72f),
  new WDir(-16.89f, -31.01f),
  new WDir(-14.78f, -30.86f),
  new WDir(-12.65f, -30.71f),
  new WDir(-10.52f, -30.56f),
  new WDir(-8.42f, -30.41f),
  new WDir(-6.28f, -30.26f),
  new WDir(-4.17f, -30.11f),
  new WDir(-2.02f, -29.95f),
  new WDir(-0.02f, -28.84f),
  new WDir(1.85f, -29.69f),
  new WDir(4.04f, -29.53f),
  new WDir(6.08f, -29.38f),
  new WDir(8.16f, -29.24f),
  new WDir(7.30f, -27.27f),
  new WDir(6.45f, -25.31f),
  new WDir(6.30f, -23.24f),
  new WDir(6.43f, -21.11f),
  new WDir(7.45f, -18.97f),
  new WDir(9.04f, -17.62f),
  new WDir(9.67f, -15.66f),
  new WDir(9.45f, -13.55f),
  new WDir(9.33f, -11.47f),
  new WDir(9.89f, -9.44f),
  new WDir(10.66f, -7.47f),
  new WDir(12.70f, -7.05f),
  new WDir(14.80f, -6.61f),
  new WDir(16.59f, -4.94f),
  new WDir(17.67f, -3.12f),
  new WDir(18.74f, -1.32f),
  new WDir(19.77f, 0.48f),
  new WDir(19.62f, 2.89f),
  new WDir(21.84f, 3.98f),
  new WDir(22.98f, 5.79f),
  new WDir(24.06f, 7.61f),
  new WDir(25.13f, 9.40f),
  new WDir(26.19f, 11.19f),
  new WDir(27.27f, 13.00f),
  new WDir(28.32f, 14.76f),
  new WDir(29.39f, 16.55f),
  new WDir(30.46f, 18.36f),
  new WDir(31.52f, 20.14f),
  new WDir(32.61f, 21.96f),
  new WDir(33.69f, 23.78f),
  new WDir(31.55f, 24.31f),
  new WDir(29.38f, 24.33f),
  new WDir(27.24f, 24.34f),
  new WDir(25.15f, 24.36f),
  new WDir(23.08f, 24.38f),
  new WDir(20.97f, 24.40f),
  new WDir(18.82f, 24.42f),
  new WDir(16.70f, 24.43f),
  new WDir(14.53f, 24.45f),
  new WDir(12.38f, 24.47f),
  new WDir(10.23f, 24.49f),
  new WDir(8.09f, 24.51f),
  new WDir(5.96f, 24.52f),
  new WDir(3.86f, 24.54f),
  new WDir(1.66f, 24.56f),
  new WDir(-0.48f, 24.58f),
  new WDir(-2.59f, 24.60f),
  new WDir(-4.68f, 24.61f),
  new WDir(-6.82f, 24.63f),
  new WDir(-8.95f, 24.65f),
  new WDir(-11.09f, 24.67f),
  new WDir(-13.19f, 24.69f),
  new WDir(-15.05f, 23.86f),
  new WDir(-17.16f, 23.76f),
  new WDir(-18.86f, 22.63f),
  new WDir(-19.62f, 20.67f),
  new WDir(-20.36f, 18.75f),
  new WDir(-21.12f, 16.80f),
  new WDir(-21.87f, 14.86f),
  new WDir(-22.62f, 12.93f),
  new WDir(-23.37f, 10.99f),
  new WDir(-24.13f, 9.03f),
  new WDir(-24.92f, 7.00f),
  new WDir(-25.68f, 4.99f),
  new WDir(-26.21f, 2.99f),
  new WDir(-25.64f, 0.99f),
  new WDir(-25.07f, -1.02f),
  new WDir(-24.49f, -3.06f),
  new WDir(-23.93f, -5.03f),
  new WDir(-23.34f, -7.10f),
  new WDir(-22.77f, -9.14f),
  new WDir(-22.39f, -11.16f),
  new WDir(-22.92f, -13.11f),
  new WDir(-22.93f, -15.54f),
  new WDir(-22.80f, -17.62f),
  new WDir(-22.68f, -19.71f),
  new WDir(-22.55f, -21.81f),
  new WDir(-22.42f, -23.89f),
  new WDir(-22.30f, -25.98f),
  new WDir(-22.17f, -28.08f),
  new WDir(-22.04f, -30.14f),
];

    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices));
}
