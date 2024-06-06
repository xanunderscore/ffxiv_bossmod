namespace BossMod.Shadowbringers.Dungeon.D07GrandCosmos.D071SeekerOfSolitude;

public enum OID : uint
{
    Boss = 0x2C1A, // R2.000, x?
    BossHelper = 0x233C,
    Broom = 0x2C1B,
    Voidzone = 0x1EAEAE
}

public enum AID : uint
{
    Shadowbolt = 18281, // Boss->player, 4.0s cast, single-target
    ImmortalAnathema = 18851, // Boss->self, 4.0s cast, range 60 circle
    Tribulation = 18283, // Boss->self, 3.0s cast, single-target
    TribulationHelper = 18852, // BossHelper->location, 3.0s cast, range 3 circle
    DarkPulse = 18282, // Boss->none, 5.0s cast, range 6 circle
    DarkShock = 18286, // Boss->self, 3.0s cast, single-target
    DarkShockHelper = 18287, // BossHelper->location, 3.0s cast, range 6 circle
    DarkWell = 18284, // Boss->self, no cast, single-target
    DarkWellHelper = 18285, // BossHelper->player, 5.0s cast, range 5 circle
}

class Shadowbolt(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Shadowbolt));

class ImmortalAnathema(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ImmortalAnathema));

class DarkPulse(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DarkPulse), 5);

class DarkShock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkShockHelper), 6);

class DarkWell(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkWellHelper), 5);

class Tribulation(BossModule module)
    : Components.PersistentVoidzoneAtCastTarget(
        module,
        3,
        ActionID.MakeSpell(AID.TribulationHelper),
        m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7),
        0.8f
    );

class DeepClean(BossModule module) : BossComponent(module)
{
    private readonly IEnumerable<Actor> Brooms = module.Enemies((uint)OID.Broom);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Brooms, ArenaColor.Danger, true);
        foreach (var b in Brooms)
            if (module.InBounds(b.Position))
                // approximation of how close you need to be to the broom to get hit by Sweep
                Arena.AddCircle(b.Position, 4.25f, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in Brooms)
            if (module.InBounds(b.Position))
                hints.AddForbiddenZone(new AOEShapeCircle(6), b.Position);
    }
}

class D071SeekerOfSolitudeStates : StateMachineBuilder
{
    public D071SeekerOfSolitudeStates(BossModule module)
        : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shadowbolt>()
            .ActivateOnEnter<ImmortalAnathema>()
            .ActivateOnEnter<DeepClean>()
            .ActivateOnEnter<Tribulation>()
            .ActivateOnEnter<DarkPulse>()
            .ActivateOnEnter<DarkShock>()
            .ActivateOnEnter<DarkWell>();
    }
}

[ModuleInfo(
    BossModuleInfo.Maturity.Contributed,
    Contributors = "xan",
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 692,
    NameID = 9041
)]
public class D071SeekerOfSolitude(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(0, 187), new ArenaBoundsRect(20.5f, 15));
