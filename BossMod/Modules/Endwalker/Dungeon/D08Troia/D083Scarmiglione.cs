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
    private readonly List<(ulong ID, Angle Angle)> Walls = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var (_, w) in Walls)
        {
            Arena.PathArcTo(Arena.Center, 20, (w - 6.5f.Degrees()).Rad, (w + 6.5f.Degrees()).Rad);
            Arena.PathStroke(false, ArenaColor.Border);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x39C8)
            Walls.Add((actor.InstanceID, Angle.FromDirection(actor.Position - Arena.Center)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == 0x39C8)
            Walls.RemoveAll(w => w.ID == actor.InstanceID);
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, WallCheck(actor.Position) > 0 ? 19 - (actor.Position - Module.Arena.Center).Length() : Distance, Module.CastFinishAt(c.CastInfo));
    }

    private float WallCheck(WPos pos) => Walls.Max(w => pos.InCone(Arena.Center, w.Angle, 6.5f.Degrees()) ? 1 : 0);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(p => WallCheck(p) - 1, Module.CastFinishAt(c.CastInfo));
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
    protected override void DrawArenaBackground(int pcSlot, Actor pc) => Arena.ZoneDonut(Arena.Center, 21, 25, ArenaColor.AOE);
}

