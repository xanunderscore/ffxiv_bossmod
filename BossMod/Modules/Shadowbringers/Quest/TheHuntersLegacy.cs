using BossMod.Autorotation;

namespace BossMod.Shadowbringers.Quest.TheHuntersLegacy;

public enum OID : uint
{
    Boss = 0x29EE,
    Helper = 0x233C
}

public enum AID : uint
{
    _AutoAttack_Attack = 871, // Boss->player, no cast, single-target
    _Weaponskill_BalamBlaster = 17137, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    _Weaponskill_PreternaturalRoar = 17136, // Boss->self, 8.0s cast, range 40 circle
    _Weaponskill_MortalBlast = 17143, // 233C->self, 5.0s cast, range 40 circle
    _Weaponskill_MortalBlast1 = 17142, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ElectricWhisker = 17126, // Boss->self, 3.5s cast, range 8+R 90-degree cone
    _Weaponskill_RoaringThunder = 17135, // Boss->self, 4.0s cast, range 8-30 donut
    _Weaponskill_StreakLightning = 17148, // 233C->location, 2.5s cast, range 3 circle
    _Weaponskill_AlternatingCurrent = 17149, // Boss->self, 4.0s cast, single-target
    _Weaponskill_AlternatingCurrent1 = 17150, // Helper->self, 4.0s cast, range 60 width 5 rect
    _Weaponskill_RumblingThunder = 17133, // Boss->self, 6.0s cast, single-target
    _Weaponskill_RumblingThunderStun = 17473, // Helper->player, 6.0s cast, range 5 circle
    _Weaponskill_HighCaterwaul = 17128, // Boss->location, 17.0s cast, range 4 circle
    _Weaponskill_RumblingThunderStack = 17134, // Helper->player, 6.0s cast, range 5 circle
    _Weaponskill_FrenziedRage = 17146, // Boss->self, 2.0s cast, single-target
    _Weaponskill_Thunderbolt = 17139, // Boss->self, 6.0s cast, single-target
    _Weaponskill_Thunderbolt1 = 17140, // Helper->players/29EC, 6.0s cast, range 5 circle
    _Weaponskill_BalamBlaster1 = 17138, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    _Weaponskill_StreakLightning1 = 17147, // Helper->location, 2.5s cast, range 3 circle
    _Weaponskill_MortalBlast2 = 17144, // Boss->self, 45.0s cast, single-target
    _Weaponskill_MortalBlast3 = 17145, // Helper->self, 45.0s cast, range 40 circle
}

class Thunderbolt(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Thunderbolt1), 5);
class BalamBlaster(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BalamBlaster), new AOEShapeCone(38.05f, 135.Degrees()));
class BalamBlasterRear(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BalamBlaster1), new AOEShapeCone(38.05f, 135.Degrees()));
class ElectricWhisker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ElectricWhisker), new AOEShapeCone(16.05f, 45.Degrees()));
class RoaringThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RoaringThunder), new AOEShapeDonut(8, 30));
class StreakLightning(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StreakLightning), 3);
class StreakLightning1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StreakLightning1), 3);
class AlternatingCurrent(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AlternatingCurrent1), new AOEShapeRect(60, 2.5f));
class RumblingThunder(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_RumblingThunderStack), 5, 1);

class RendaRae(WorldState ws) : UnmanagedRotation(ws, 20)
{
    protected override void Exec(Actor? primaryTarget)
    {
        var dot = StatusDetails(primaryTarget, Roleplay.SID.AcidicBite, Player.InstanceID);
        if (dot.Left < 2.5f)
            UseAction(Roleplay.AID.AcidicBite, primaryTarget, 10);

        UseAction(Roleplay.AID.RadiantArrow, primaryTarget, -5);
        UseAction(Roleplay.AID.HeavyShot, primaryTarget);

        if (primaryTarget?.CastInfo?.Interruptible ?? false)
            UseAction(Roleplay.AID.DullingArrow, primaryTarget, 5);

        if (Player.HPMP.MaxHP * 0.8f > Player.HPMP.CurHP)
            UseAction(Roleplay.AID.HuntersPrudence, Player, -15);
    }
}

class RendaRaeAI(BossModule module) : Components.RotationModule<RendaRae>(module);

class RonkanAura(BossModule module) : BossComponent(module)
{
    private Actor? AuraCenter => Module.Enemies(0x1EADA5).FirstOrDefault();

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (AuraCenter is Actor a)
            Arena.ZoneCircle(a.Position, 10, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AuraCenter is Actor a)
            hints.AddForbiddenZone(new AOEShapeDonut(10, 100), a.Position, activation: WorldState.FutureTime(5));
    }
}

class BalamQuitzStates : StateMachineBuilder
{
    public BalamQuitzStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RendaRaeAI>()
            .ActivateOnEnter<BalamBlaster>()
            .ActivateOnEnter<BalamBlasterRear>()
            .ActivateOnEnter<ElectricWhisker>()
            .ActivateOnEnter<RoaringThunder>()
            .ActivateOnEnter<StreakLightning>()
            .ActivateOnEnter<StreakLightning1>()
            .ActivateOnEnter<AlternatingCurrent>()
            .ActivateOnEnter<RumblingThunder>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<RonkanAura>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68812, NameID = 8397)]
public class BalamQuitz(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247.11f, 688.33f), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(a => a.IsAlly), ArenaColor.PlayerGeneric);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(PrimaryActor.OID);
    }
}
