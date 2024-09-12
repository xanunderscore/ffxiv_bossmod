namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D061Regula;

public enum OID : uint
{
    Boss = 0xE97, // R1.65
    ClockworkHunter = 0xF5C, // R1.25
    MagitekTurretI = 0xE98, // R0.6
    MagitekTurretII = 0xE99, // R0.6
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherochemicalGrenado = 4322, // MagitekTurretII->location, 3.0s cast, range 8 circle
    AetherochemicalLaser = 4321, // MagitekTurretI->player, 3.0s cast, range 50 width 4 rect
    Bastardbluss = 4314, // Boss->player, no cast, single-target, tankbuster + stun

    Judgment = 4317, // Boss->player, no cast, single-target
    JudgmentAOE = 4318, // Helper->self, 3.0s cast, range 8 circle

    SelfDetonate = 4323, // MagitekTurretI/MagitekTurretII->self, 5.0s cast, range 40+R circle

    MagitekSlug = 4315, // Boss->self, 2.5s cast, range 60+R width 4 rect
    MagitekSpread = 4316, // Boss->self, 4.5s cast, range 30+R 240-degree cone, knockback 20, away from source
    MagitekTurret = 4320, // Boss->self, no cast, single-target
    Quickstep = 4319 // Boss->location, no cast, single-target
}

public enum TetherID : uint
{
    BaitAway = 17 // MagitekTurretI->player
}

class SelfDetonate(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SelfDetonate));
class AetherochemicalGrenado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalGrenado), 8);
class AetherochemicalLaser(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 2), (uint)TetherID.BaitAway, ActionID.MakeSpell(AID.AetherochemicalLaser));
class AetherochemicalLaserAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaser), new AOEShapeRect(50, 2));

class JudgmentAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JudgmentAOE), new AOEShapeCircle(8));
class MagitekSlug(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekSlug), new AOEShapeRect(61.65f, 2));
class MagitekSpread(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekSpread), new AOEShapeCone(31.65f, 120.Degrees()));

class D061RegulaStates : StateMachineBuilder
{
    public D061RegulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<AetherochemicalLaser>()
            .ActivateOnEnter<AetherochemicalLaserAOE>()
            .ActivateOnEnter<JudgmentAOE>()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team (Malediktus, LTS), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3818)]
public class D061Regula(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110.79f, -295.64f), new ArenaBoundsCircle(22.25f))
{
    public static readonly ArenaBoundsCustom HexBounds = new(30, new RelSimplifiedComplexPolygon([new WDir(-89.11f, -301.95f), new(-93.32f, -310.37f), new(-106.99f, -318.19f), new(-115.01f, -318.19f),
    new(-128.64f, -310.32f), new(-132.28f, -303.07f), new(-132.29f, -287.96f), new(-128.64f, -280.69f), new(-115.04f, -272.8f), new(-107.04f, -272.8f),
    new(-93.35f, -280.7f), new(-89.11f, -289.1f)]));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.MagitekTurretI), ArenaColor.Enemy);
    }
}
