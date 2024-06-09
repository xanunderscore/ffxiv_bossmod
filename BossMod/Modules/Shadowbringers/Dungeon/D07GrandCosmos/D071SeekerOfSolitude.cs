
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

class DeepClean(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IEnumerable<Actor> Brooms = module.Enemies(OID.Broom);
    private readonly List<(WPos position, DateTime time)> broomDanger = [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Tribulation)
        {
            foreach ((var b, var zcoord) in Brooms.OrderBy(x => x.Position.Z).Zip([175, 181, 187, 193, 199]))
            {
                var time = WorldState.CurrentTime.AddSeconds(7f);
                var pos = new WPos(b.Position.X, zcoord);
                var direction = b.Position.X > 0 ? new WDir(-1, 0) : new WDir(1, 0);
                for (int i = 0; i < 20; i++)
                {
                    broomDanger.Add((pos, time));
                    time = time.AddSeconds(1);
                    pos += direction * 3.6f;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in Brooms)
            Arena.Actor(b.Position, b.Rotation, ArenaColor.Danger);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in broomDanger)
        {
            if (aoe.time < WorldState.CurrentTime || aoe.time > WorldState.CurrentTime.AddSeconds(3))
                continue;
            yield return new(new AOEShapeDonut(4, 4.1f), aoe.position, default, aoe.time);
        }
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
    BossModuleInfo.Maturity.WIP,
    Contributors = "xan",
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 692,
    NameID = 9041
)]
public class D071SeekerOfSolitude(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(0, 187), new ArenaBoundsRect(20.5f, 15));
