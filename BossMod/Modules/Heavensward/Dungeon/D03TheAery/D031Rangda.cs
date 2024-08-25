namespace BossMod.Heavensward.Dungeon.D03TheAery.D031Rangda;

public enum OID : uint
{
    Boss = 0xEA6,
    Helper = 0x233C,
    //_Gen_DarkCloud = 0x1143, // R0.500, x1
    //_Gen_BlackenedStatue = 0xEA8, // R1.400, x7
    //_Gen_Rangda = 0x1144, // R0.500, x7, mixed types
    //_Gen_Leyak = 0xEA7, // R3.600, x0 (spawn during fight)
}

public enum SID : uint
{
    _Gen_LightningRod = 2574, // none->player/EA8, extra=0x0
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_ElectricPredation = 3887, // Boss->self, no cast, range 8+R ?-degree cone
    _Weaponskill_ElectricCachexia = 3889, // Boss->self, 7.0s cast, range 60+R circle
    _Weaponskill_Electrocution = 3890, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Electrocution1 = 3891, // 1144->self, no cast, range 60+R width 5 rect
    _Ability_IonosphericCharge = 3888, // Boss->self, 3.0s cast, single-target
    _Weaponskill_LightningBolt = 3893, // 1144->location, 3.0s cast, range 3 circle
    _Weaponskill_Reflux = 3894, // EA7->player, no cast, single-target
    _AutoAttack_Attack1 = 870, // EA7->player, no cast, single-target
    _Weaponskill_Ground = 3892, // 1144->player/EA8, no cast, single-target
}

class ElectroBait(BossModule module) : Components.GenericBaitAway(module)
{
    private bool Active = false;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Active)
            CurrentBaits.AddRange(Raid.WithoutSlot().OrderByDescending(p => p.DistanceToHitbox(Module.PrimaryActor)).Take(3).Select(p => new Bait(Module.PrimaryActor, p, new AOEShapeRect(50, 2.5f))));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Electrocution)
            Active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Electrocution1)
            Active = false;
    }
}

class ElectricCachexia(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ElectricCachexia), new AOEShapeDonut(6, 60));
class ElectroKB(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID._Weaponskill_Electrocution), stopAtWall: true)
{
    private bool Active = false;
    private HashSet<int> ClosestPlayers = [];

    public override void Update()
    {
        ClosestPlayers.Clear();
        if (Active)
            ClosestPlayers = Raid.WithSlot().OrderByDescending(p => p.Item2.DistanceToHitbox(Module.PrimaryActor)).Take(3).Select(x => x.Item1).ToHashSet();
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Active && ClosestPlayers.Contains(slot))
            yield return new Source(Module.PrimaryActor.Position, 15, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Electrocution1)
            Active = false;
    }
}

class LightningRod(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Statues => Module.Enemies(0xEA8).Where(e => !UsedStatues.Contains(e.InstanceID));

    private readonly HashSet<ulong> UsedStatues = [];

    private int LightningRodTarget => WorldState.Party.WithSlot().Where(x => x.Item2.FindStatus(SID._Gen_LightningRod) != null).Select(x => x.Item1).FirstOrDefault(-1);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot == LightningRodTarget)
            hints.Add("Go to a tower!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor.OID == 0xEA8 && (SID)status.ID == SID._Gen_LightningRod)
            UsedStatues.Add(actor.InstanceID);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot == LightningRodTarget)
        {
            var closestTower = Statues.MinBy(actor.DistanceToHitbox)!;
            hints.AddForbiddenZone(new AOEShapeDonut(3, 60), closestTower.Position);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Statues, ArenaColor.Object, true);

        if (pcSlot == LightningRodTarget)
            foreach (var enemy in Statues)
                Arena.AddCircle(enemy.Position, 3, ArenaColor.Safe);
    }
}

class RangdaStates : StateMachineBuilder
{
    public RangdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricCachexia>()
            .ActivateOnEnter<ElectroKB>()
            .ActivateOnEnter<LightningRod>()
            .ActivateOnEnter<ElectroBait>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3452)]
public class Rangda(WorldState ws, Actor primary) : BossModule(ws, primary, new(335, -203), new ArenaBoundsCircle(27));

