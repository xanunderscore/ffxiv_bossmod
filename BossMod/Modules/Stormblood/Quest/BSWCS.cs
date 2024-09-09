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
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, CustomBounds)
{
    private static readonly List<WDir> vertices = [
        new(-487.40f, -230.79f), new(-487.56f, -188.08f), new(-478.75f, -181.25f), new(-439.37f, -183.46f), new(-457.85f, -211.90f), new(-461.13f, -228.75f)
    ];

    public static readonly WPos ArenaCenter = new(-465.40f, -202.09f);
    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices.Select(v => v - ArenaCenter.ToWDir())));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
