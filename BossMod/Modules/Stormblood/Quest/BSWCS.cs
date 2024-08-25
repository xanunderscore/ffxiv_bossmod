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
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(-473, -214), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = BuildCustomBounds();

    private static ArenaBoundsCustom BuildCustomBounds()
    {
        WDir center = new(-473, -214);
        List<WDir> complexArenaBounds = [
  new WDir(-485.98f, -232.35f),
  new WDir(-486.42f, -232.26f),
  new WDir(-486.47f, -231.53f),
  new WDir(-486.56f, -230.01f),
  new WDir(-486.65f, -228.52f),
  new WDir(-486.74f, -227.02f),
  new WDir(-486.83f, -225.52f),
  new WDir(-486.92f, -224.01f),
  new WDir(-487.01f, -222.50f),
  new WDir(-487.11f, -220.97f),
  new WDir(-487.20f, -219.46f),
  new WDir(-487.30f, -217.85f),
  new WDir(-487.62f, -216.55f),
  new WDir(-487.30f, -215.35f),
  new WDir(-486.87f, -213.75f),
  new WDir(-486.90f, -211.97f),
  new WDir(-487.35f, -210.38f),
  new WDir(-487.35f, -210.38f),
  new WDir(-487.78f, -208.89f),
  new WDir(-488.19f, -207.44f),
  new WDir(-488.60f, -205.98f),
  new WDir(-488.60f, -205.98f),
  new WDir(-489.03f, -204.48f),
  new WDir(-489.03f, -204.48f),
  new WDir(-489.45f, -202.99f),
  new WDir(-489.86f, -201.55f),
  new WDir(-490.28f, -200.07f),
  new WDir(-490.61f, -198.91f),
  new WDir(-490.23f, -197.75f),
  new WDir(-489.71f, -196.36f),
  new WDir(-489.15f, -194.93f),
  new WDir(-488.60f, -193.51f),
  new WDir(-488.04f, -192.07f),
  new WDir(-487.50f, -190.66f),
  new WDir(-486.94f, -189.23f),
  new WDir(-486.39f, -187.82f),
  new WDir(-486.39f, -187.82f),
  new WDir(-485.85f, -186.40f),
  new WDir(-485.85f, -186.40f),
  new WDir(-485.30f, -185.00f),
  new WDir(-484.76f, -183.60f),
  new WDir(-484.21f, -182.19f),
  new WDir(-483.66f, -180.76f),
  new WDir(-483.25f, -179.71f),
  new WDir(-482.47f, -179.34f),
  new WDir(-480.97f, -179.44f),
  new WDir(-479.46f, -178.72f),
  new WDir(-478.22f, -178.43f),
  new WDir(-476.82f, -178.44f),
  new WDir(-475.28f, -178.46f),
  new WDir(-473.77f, -178.47f),
  new WDir(-472.20f, -178.48f),
  new WDir(-470.66f, -178.50f),
  new WDir(-470.66f, -178.50f),
  new WDir(-469.10f, -178.51f),
  new WDir(-467.57f, -178.52f),
  new WDir(-466.05f, -178.53f),
  new WDir(-466.05f, -178.53f),
  new WDir(-464.50f, -178.55f),
  new WDir(-464.50f, -178.55f),
  new WDir(-462.95f, -178.56f),
  new WDir(-461.42f, -178.57f),
  new WDir(-459.87f, -178.59f),
  new WDir(-458.32f, -178.60f),
  new WDir(-458.32f, -178.60f),
  new WDir(-456.78f, -178.61f),
  new WDir(-455.23f, -178.63f),
  new WDir(-453.72f, -178.64f),
  new WDir(-453.72f, -178.64f),
  new WDir(-452.21f, -178.65f),
  new WDir(-450.69f, -178.66f),
  new WDir(-450.69f, -178.66f),
  new WDir(-449.13f, -178.68f),
  new WDir(-449.13f, -178.68f),
  new WDir(-447.59f, -178.69f),
  new WDir(-447.59f, -178.69f),
  new WDir(-446.04f, -178.70f),
  new WDir(-444.54f, -178.72f),
  new WDir(-442.98f, -178.73f),
  new WDir(-442.98f, -178.73f),
  new WDir(-441.45f, -178.74f),
  new WDir(-439.89f, -178.76f),
  new WDir(-438.36f, -178.77f),
  new WDir(-436.84f, -178.78f),
  new WDir(-435.33f, -178.79f),
  new WDir(-435.33f, -178.79f),
  new WDir(-433.78f, -178.81f),
  new WDir(-433.26f, -178.81f),
  new WDir(-433.28f, -179.05f),
  new WDir(-433.93f, -180.14f),
  new WDir(-433.93f, -180.14f),
  new WDir(-434.71f, -181.47f),
  new WDir(-435.49f, -182.76f),
  new WDir(-435.49f, -182.76f),
  new WDir(-436.28f, -184.09f),
  new WDir(-437.05f, -185.38f),
  new WDir(-437.05f, -185.38f),
  new WDir(-437.82f, -186.68f),
  new WDir(-438.59f, -187.97f),
  new WDir(-439.36f, -189.26f),
  new WDir(-440.15f, -190.59f),
  new WDir(-440.92f, -191.88f),
  new WDir(-441.70f, -193.20f),
  new WDir(-442.49f, -194.52f),
  new WDir(-443.26f, -195.81f),
  new WDir(-443.89f, -196.87f),
  new WDir(-445.11f, -197.47f),
  new WDir(-446.73f, -198.71f),
  new WDir(-447.03f, -200.70f),
  new WDir(-446.96f, -202.03f),
  new WDir(-447.59f, -203.09f),
  new WDir(-448.36f, -204.38f),
  new WDir(-449.13f, -205.67f),
  new WDir(-449.85f, -206.88f),
  new WDir(-450.59f, -207.68f),
  new WDir(-451.76f, -207.95f),
  new WDir(-453.27f, -208.26f),
  new WDir(-455.23f, -208.67f),
  new WDir(-456.24f, -210.53f),
  new WDir(-456.68f, -212.11f),
  new WDir(-457.14f, -213.78f),
  new WDir(-456.96f, -215.53f),
  new WDir(-456.75f, -216.91f),
  new WDir(-456.90f, -218.19f),
  new WDir(-457.24f, -219.27f),
  new WDir(-458.18f, -220.04f),
  new WDir(-459.76f, -221.16f),
  new WDir(-459.95f, -223.15f),
  new WDir(-460.04f, -224.69f),
  new WDir(-460.15f, -226.37f),
  new WDir(-459.73f, -228.09f),
  new WDir(-459.09f, -229.58f),
  new WDir(-458.80f, -230.24f),
  new WDir(-459.14f, -230.49f),
  new WDir(-460.35f, -230.57f),
  new WDir(-461.90f, -230.68f),
  new WDir(-461.90f, -230.68f),
  new WDir(-462.95f, -230.76f),
  new WDir(-464.13f, -229.49f),
  new WDir(-466.24f, -230.36f),
  new WDir(-467.44f, -231.07f),
  new WDir(-468.74f, -231.16f),
  new WDir(-468.74f, -231.16f),
  new WDir(-470.31f, -231.27f),
  new WDir(-470.31f, -231.27f),
  new WDir(-471.85f, -231.38f),
  new WDir(-473.38f, -231.49f),
  new WDir(-474.89f, -231.60f),
  new WDir(-474.89f, -231.60f),
  new WDir(-476.39f, -231.70f),
  new WDir(-477.92f, -231.81f),
  new WDir(-479.42f, -231.92f),
  new WDir(-479.42f, -231.92f),
  new WDir(-480.97f, -232.03f),
  new WDir(-481.74f, -232.08f),
  new WDir(-482.67f, -229.58f),
  new WDir(-483.14f, -229.96f),
  new WDir(-484.10f, -231.16f),
  new WDir(-485.01f, -232.00f),
];
        return new ArenaBoundsCustom(30, new(complexArenaBounds.Select(p => new WDir(center.X - p.X, p.Z - center.Z))));

    }
}
