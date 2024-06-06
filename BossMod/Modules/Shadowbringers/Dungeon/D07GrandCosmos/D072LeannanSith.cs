namespace BossMod.Shadowbringers.Dungeon.D07GrandCosmos.D072LeannanSith;

public enum OID : uint
{
    Boss = 0x2C04,
    BossHelper = 0x233C,
    LoversRing = 0x2C05,
    EnslavedLove = 0x2C06,
}

public enum AID : uint
{
    StormOfColor = 18203, // Boss->player, 4.0s cast, single-target
    OdeToLostLove = 18204, // Boss->self, 4.0s cast, range 60 circle
    DirectSeeding = 18205, // Boss->self, 4.0s cast, single-target
    GardenersHymn = 18206, // Boss->self, 14.0s cast, single-target
    FarWindAOE = 18211, // 233C->location, 5.0s cast, range 8 circle
    FarWindSpread = 18212, // 233C->player, 5.0s cast, range 5 circle
    OdeToFallenPetals = 18768, // Boss->self, 4.0s cast, range ?-60 donut
    IrefulWind = 18209, // 2C06->self, 13.0s cast, range 40+R width 40 rect
}

class StormOfColor(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.StormOfColor));

class OdeToLostLove(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OdeToLostLove));

class FarWindAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FarWindAOE), 8);
class FarWindSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FarWindSpread), 5);
class OdeToFallenPetals(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OdeToFallenPetals), new AOEShapeDonut(4, 60));

class IrefulWind(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.IrefulWind), 10, false, int.MaxValue, null, Kind.DirForward);

class DirectSeeding(BossModule module) : BossComponent(module)
{
    // only two tile arrangements for Direct Seeding - they seem to always be cast in the same order
    private readonly List<WPos> badSquares1 =
    [
        new(-5, -75),
        new(5, -75),
        new(-15, -65),
        new(15, -65),
        new(-15, -55),
        new(5, -55),
        new(-5, -45),
        new(15, -45)
    ];
    private readonly List<WPos> badSquares2 =
    [
        new(-5, -75),
        new(5, -75),
        new(-15, -65),
        new(15, -65),
        new(-15, -45),
        new(5, -45),
        new(-5, -55),
        new(15, -55)
    ];
    private bool isGrowthActive = false;
    private uint castCount = 0;
    private bool isKnockback = false;
    private Angle knockbackDirection = new();

    private List<WPos> BadSquares => castCount > 1 ? badSquares2 : badSquares1;

    private IEnumerable<(Actor actor, WPos expectedPos)> SeedObjs()
        => module.Enemies((uint)OID.LoversRing).Where(x => x.EventState != 7 && !x.IsDeadOrDestroyed).Select(act =>
        {
            if (!isKnockback)
                return (act, act.Position);
            var dir = knockbackDirection.ToDirection();
            var distance = Math.Min(10, Module.Arena.IntersectRayBounds(act.Position, dir));
            return (act, act.Position + dir * distance);
        });

    private IEnumerable<Components.GenericAOEs.AOEInstance> GrowthSquares()
    {
        if (isGrowthActive)
            foreach (var bs in BadSquares)
                yield return new(new AOEShapeRect(5, 5, 5), bs);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IrefulWind)
        {
            isKnockback = true;
            knockbackDirection = caster.Rotation;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DirectSeeding)
        {
            castCount++;
            isGrowthActive = true;
        }
        if (spell.Action.ID == (uint)AID.GardenersHymn)
            isGrowthActive = false;
        if (spell.Action.ID == (uint)AID.IrefulWind)
        {
            isGrowthActive = false;
            isKnockback = false;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in GrowthSquares())
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach ((var seed, var expectedPos) in SeedObjs())
        {
            Arena.Actor(seed.Position, seed.Rotation, ArenaColor.Danger);
            if (isKnockback)
            {
                Arena.ActorOutsideBounds(expectedPos, seed.Rotation, ArenaColor.Danger);
                Arena.AddLine(seed.Position, expectedPos, ArenaColor.Danger);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (GrowthSquares().Any(c => SeedObjs().Any(x => c.Check(x.expectedPos))))
            hints.Add("Move seeds to dry ground!");
    }
}

class D072LeannanSithStates : StateMachineBuilder
{
    public D072LeannanSithStates(BossModule module)
        : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormOfColor>()
            .ActivateOnEnter<OdeToLostLove>()
            .ActivateOnEnter<DirectSeeding>()
            .ActivateOnEnter<FarWindAOE>()
            .ActivateOnEnter<FarWindSpread>()
            .ActivateOnEnter<OdeToFallenPetals>()
            .ActivateOnEnter<IrefulWind>();
    }
}

[ModuleInfo(
    BossModuleInfo.Maturity.Contributed,
    Contributors = "xan",
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 692,
    NameID = 9044
)]
public class D072LeannanSith(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(0, -60), new ArenaBoundsSquare(20));
