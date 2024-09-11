namespace BossMod.Stormblood.Quest.ARequiemForHeroes;

public enum OID : uint
{
    Boss = 0x268A,
    BossP2 = 0x268C,
    Helper = 0x233C,
    AmeNoHabakiri = 0x2692, // R3.000, x0 (spawn during fight)
    TheStorm = 0x2760, // R3.000, x0 (spawn during fight)
    TheSwell = 0x275F, // R3.000, x0 (spawn during fight)
    DarkAether = 0x2694, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    FloodOfDarkness = 14808, // Helper->self, 3.5s cast, range 6 circle
    VeinSplitter = 14839, // Boss->self, 4.0s cast, range 10 circle
    LightlessSpark = 14838, // Boss->self, 4.0s cast, range 40+R 90-degree cone
    LightlessSparkAdds = 14824, // 268D->self, 4.0s cast, range 40+R 90-degree cone
    ArtOfTheSwell = 14812, // Boss->self, 4.0s cast, range 33 circle
    TheSwellUnbound = 14813, // Helper->self, 8.0s cast, range 8-20 donut
    ArtOfTheSword1 = 14819, // Helper->self, 4.0s cast, range 40+R width 6 rect
    ArtOfTheSword2 = 14818, // Helper->self, 6.0s cast, range 40+R width 6 rect
    ArtOfTheSword3 = 14820, // Helper->self, 2.0s cast, range 40+R width 6 rect
    ArtOfTheStorm = 14814, // Boss->self, 4.0s cast, range 8 circle
    TheStormUnboundCast = 14815, // Helper->self, 3.0s cast, range 5 circle
    TheStormUnboundRepeat = 14816, // Helper->self, no cast, range 5 circle
    EntropicFlame = 14833, // Helper->self, 4.0s cast, range 50+R width 8 rect
}

class HienAI(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        Hints.RecommendedRangeToTarget = 3;

        var ajisai = StatusDetails(primaryTarget, Roleplay.SID.Ajisai, Player.InstanceID);

        switch (ComboAction)
        {
            case Roleplay.AID.Gofu:
                UseAction(Roleplay.AID.Yagetsu, primaryTarget);
                break;

            case Roleplay.AID.Kyokufu:
                UseAction(Roleplay.AID.Gofu, primaryTarget);
                break;

            default:
                if (ajisai.Left < 5)
                    UseAction(Roleplay.AID.Ajisai, primaryTarget);
                UseAction(Roleplay.AID.Kyokufu, primaryTarget);
                break;
        }

        if (PredictedHP(Player) < 5000)
            UseAction(Roleplay.AID.SecondWind, Player, -10);

        UseAction(Roleplay.AID.HissatsuGyoten, primaryTarget, -10);
    }
}

class StormUnbound(BossModule module) : Components.Exaflare(module, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheStormUnboundCast)
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
        if ((AID)spell.Action.ID is AID.TheStormUnboundCast or AID.TheStormUnboundRepeat)
        {
            foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
                AdvanceLine(l, caster.Position);
            ++NumCasts;
        }
    }
}

class LightlessSpark2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightlessSparkAdds), new AOEShapeCone(40, 45.Degrees()));

class ArtOfTheStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheStorm), new AOEShapeCircle(8));
class EntropicFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(50, 4));

class FloodOfDarkness(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FloodOfDarkness), new AOEShapeCircle(6), maxCasts: 6);
class VeinSplitter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VeinSplitter), new AOEShapeCircle(10));
class LightlessSpark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightlessSpark), new AOEShapeCone(40, 45.Degrees()));
class SwellUnbound(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheSwellUnbound), new AOEShapeDonut(8, 20));
class Swell(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ArtOfTheSwell), 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(8, 50), Arena.Center);
    }
}
class ArtOfTheSword1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheSword1), new AOEShapeRect(40, 3));
class ArtOfTheSword2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheSword2), new AOEShapeRect(40, 3));
class ArtOfTheSword3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheSword3), new AOEShapeRect(40, 3));

class DarkAether(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.DarkAether).Select(e => new AOEInstance(new AOEShapeCircle(1.5f), e.Position, e.Rotation));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(new AOEShapeRect(3, 1.5f, 1.5f), c.Origin, c.Rotation, c.Activation);
    }
}

public class ZenosYaeGalvusStates : StateMachineBuilder
{
    public ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<HienAI>();
        TrivialPhase(1)
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
            .ActivateOnEnter<StormUnbound>()
            .Raw.Update = () => module.Enemies(OID.BossP2).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68721, NameID = 6039)]
public class ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
