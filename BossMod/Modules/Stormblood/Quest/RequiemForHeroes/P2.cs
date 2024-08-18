
namespace BossMod.Modules.Stormblood.Quest.RequiemForHeroesP2;

public enum OID : uint
{
    Boss = 0x268C,
    Helper = 0x233C,

    _Gen_SpecterOfZenos = 0x268D, // R0.920, x3
    _Gen_AmeNoHabakiri = 0x2692, // R3.000, x0 (spawn during fight)
    _Gen_TheStorm = 0x2760, // R3.000, x0 (spawn during fight)
    _Gen_TheSwell = 0x275F, // R3.000, x0 (spawn during fight)
    _Gen_AetherialTear = 0x2693, // R1.095-1.500, x0 (spawn during fight)
    _Gen_DarkAether = 0x2694, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_ = 14806, // Boss->self, no cast, single-target
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_Triple = 14835, // Boss->self, 3.0s cast, single-target
    _Spell_Spark = 14836, // Boss->player, 4.0s cast, single-target
    _Spell_Spark1 = 14837, // Boss->player, no cast, single-target
    _Weaponskill_UnmovingTroika = 14829, // Boss->self, no cast, range 9+R ?-degree cone
    _Weaponskill_UnmovingTroika1 = 14830, // Helper->self, 1.7s cast, range 9+R ?-degree cone
    _Weaponskill_UnmovingTroika2 = 14831, // Helper->self, 2.1s cast, range 9+R ?-degree cone
    _Weaponskill_FloodOfDarkness = 14807, // Boss->self, 3.0s cast, single-target
    _Ability_FloodOfDarkness = 14808, // Helper->self, 3.5s cast, range 6 circle
    _Ability_ = 14794, // Boss->location, no cast, ???
    _Weaponskill_VeinSplitter = 14839, // Boss->self, 4.0s cast, range 10 circle
    _Weaponskill_LightlessSpark = 14838, // Boss->self, 4.0s cast, range 40+R 90-degree cone
    _Weaponskill_Concentrativity = 14834, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_ArtOfTheSwell = 14812, // Boss->self, 4.0s cast, range 33 circle
    _Weaponskill_TheSwellUnbound = 14813, // Helper->self, 8.0s cast, range 8-20 donut
    _Weaponskill_ArtOfTheSword = 14817, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ArtOfTheSword1 = 14819, // Helper->self, 4.0s cast, range 40+R width 6 rect
    _Weaponskill_ArtOfTheSword2 = 14818, // Helper->self, 6.0s cast, range 40+R width 6 rect
    _Spell_TheSwordUnbound = 14821, // Helper->self, 6.0s cast, range 20+R circle
    _Weaponskill_ArtOfTheSword3 = 14820, // Helper->self, 2.0s cast, range 40+R width 6 rect
    _Weaponskill_ArtOfTheStorm = 14814, // Boss->self, 4.0s cast, range 8 circle
    _Spell_TheStormUnbound = 14815, // Helper->self, 3.0s cast, range 5 circle
    _Spell_EntropicFlame = 14833, // Helper->self, 4.0s cast, range 50+R width 8 rect
    _Weaponskill_EntropicFlame = 14832, // Boss->self, 4.0s cast, single-target

    _Spell_TheStormUnbound1 = 14816, // Helper->self, no cast, range 5 circle
    _Weaponskill_TheFinalArt = 14827, // Boss->self, no cast, single-target
    _Spell_Darkblight = 14826, // 2693->self, no cast, range 55 circle
    _Weaponskill_TheFinalArt1 = 14828, // Helper->self, 7.0s cast, range 40 circle
    _Weaponskill_UmbralRays = 14810, // Boss->self, 3.0s cast, single-target
    __Burst = 14811, // 2694->self, no cast, range 6 circle
    _Weaponskill_LightlessSpark1 = 14824, // 268D->self, 4.0s cast, range 40+R 90-degree cone
}

class StormUnbound(BossModule module) : Components.Exaflare(module, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_TheStormUnbound)
        {
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 5,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_TheStormUnbound or AID._Spell_TheStormUnbound1)
        {
            foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
                AdvanceLine(l, caster.Position);
            ++NumCasts;
        }
    }
}

class LightlessSpark2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LightlessSpark1), new AOEShapeCone(40, 45.Degrees()));

class ArtOfTheStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheStorm), new AOEShapeCircle(8));
class EntropicFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_EntropicFlame), new AOEShapeRect(50, 4));

class FloodOfDarkness(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_FloodOfDarkness), new AOEShapeCircle(6), maxCasts: 6);
class VeinSplitter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VeinSplitter), new AOEShapeCircle(10));
class LightlessSpark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LightlessSpark), new AOEShapeCone(40, 45.Degrees()));
class SwellUnbound(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TheSwellUnbound), new AOEShapeDonut(8, 20));
class Swell(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheSwell), 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(8, 50), Arena.Center);
    }
}
class ArtOfTheSword1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheSword1), new AOEShapeRect(40, 3));
class ArtOfTheSword2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheSword2), new AOEShapeRect(40, 3));
class ArtOfTheSword3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheSword3), new AOEShapeRect(40, 3));
class Adds(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => (OID)x.OID is OID._Gen_AmeNoHabakiri or OID._Gen_TheSwell or OID._Gen_TheStorm), ArenaColor.Enemy);
}

class DarkAether(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID._Gen_DarkAether).Select(e => new AOEInstance(new AOEShapeCircle(1.5f), e.Position, e.Rotation));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(new AOEShapeRect(3, 1.5f, 1.5f), c.Origin, c.Rotation, c.Activation);
    }
}

class ZenosYaeGalvusP2States : StateMachineBuilder
{
    public ZenosYaeGalvusP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FloodOfDarkness>()
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<LightlessSpark>()
            .ActivateOnEnter<LightlessSpark2>()
            .ActivateOnEnter<SwellUnbound>()
            .ActivateOnEnter<Swell>()
            .ActivateOnEnter<ArtOfTheSword1>()
            .ActivateOnEnter<ArtOfTheSword2>()
            .ActivateOnEnter<ArtOfTheSword3>()
            .ActivateOnEnter<ArtOfTheStorm>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<DarkAether>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<StormUnbound>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 648, NameID = 6039)]
public class ZenosYaeGalvusP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(16));

