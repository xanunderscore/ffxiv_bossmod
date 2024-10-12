namespace BossMod.Endwalker.Dungeon.D08Troia.D083Scarmiglione;

public enum OID : uint
{
    Boss = 0x39C5,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 30258, // Boss->player, no cast, single-target
    _Spell_CursedEcho = 30257, // Boss->self, 4.0s cast, range 40 circle
    _Ability_ = 30237, // Boss->location, no cast, single-target
    _Weaponskill_RottenRampage = 30231, // Boss->self, 8.0+2.0s cast, single-target
    _Weaponskill_RottenRampage1 = 30232, // Helper->location, 10.0s cast, range 6 circle
    _Weaponskill_RottenRampage2 = 30233, // Helper->player, 10.0s cast, range 6 circle
    _Weaponskill_ = 30234, // Boss->self, no cast, single-target
    _Weaponskill_BlightedBedevilment = 30235, // Boss->self, 4.9s cast, range 9 circle
    _Weaponskill_VacuumWave = 30236, // Helper->self, 5.4s cast, range 40 circle
    _Weaponskill_BlightedBladework = 30259, // Boss->location, 10.0+1.0s cast, single-target
    _Weaponskill_BlightedBladework1 = 30260, // Helper->self, 11.0s cast, range 25 circle
    _Weaponskill_BlightedSweep = 30261, // Boss->self, 7.0s cast, range 40 180-degree cone
}

class BlightedBladework(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BlightedBladework1), new AOEShapeCircle(25));
class BlightedSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BlightedSweep), new AOEShapeCone(40, 90.Degrees()));
class BlightedBedevilment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BlightedBedevilment), new AOEShapeCircle(9));
class CursedEcho(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_CursedEcho));
class RottenRampage(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RottenRampage1), 6);
class RottenRampagePlayer(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_RottenRampage2), 6);
class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_VacuumWave), 30)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, WallDist(actor.Position) > 0 ? 19 - (actor.Position - Module.Arena.Center).Length() : Distance, Module.CastFinishAt(c.CastInfo));
    }

    private float WallDist(WPos pos)
    {
        var zones = Module.Enemies(0x39C8).Select(w =>
        {
            var off = w.Position - Arena.Center;
            return ShapeDistance.InvertedCone(Arena.Center, off.Length(), Angle.FromDirection(off), 6.5f.Degrees());
        }).ToList();
        return zones.Max(e => e(pos));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(WallDist, Module.CastFinishAt(c.CastInfo));
    }
}

class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampagePlayer>()
            .ActivateOnEnter<BlightedBedevilment>()
            .ActivateOnEnter<CursedEcho>()
            .ActivateOnEnter<BlightedBladework>()
            .ActivateOnEnter<BlightedSweep>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11372)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -298), new ArenaBoundsCircle(25))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var w in Enemies(0x39C8))
        {
            // Arena.Actor(w, ArenaColor.Object, true);
            // Arena.AddRect(w.Position, w.Rotation.ToDirection(), 1, 0, 2, ArenaColor.Object);
            var off = w.Position - Arena.Center;
            Arena.PathArcTo(Arena.Center, off.Length() - 1.5f, Angle.FromDirection(off).Rad - 6.5f.Degrees().Rad, Angle.FromDirection(off).Rad + 6.5f.Degrees().Rad);
            Arena.PathStroke(false, ArenaColor.Border);
        }
    }

    protected override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneDonut(Arena.Center, 21, 25, ArenaColor.AOE);
    }
}

