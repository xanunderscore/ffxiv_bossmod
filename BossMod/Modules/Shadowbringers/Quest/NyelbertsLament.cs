namespace BossMod.Shadowbringers.Quest.NyelbertsLament;

// TODO: add AI hint for the "enrage" + paladin safe zone

public enum OID : uint
{
    Boss = 0x2971,
    Helper = 0x233C,

    Troodon = 0x2975,
    Bovian = 0x2977,
    BovianBull = 0x2976,

    _Gen_LooseBoulder = 0x2978, // R2.400, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Troodon/Bovian/BovianBull->296F/296E, no cast, single-target
    _Weaponskill_DeadlyHold = 16588, // Troodon->296F, no cast, single-target
    _Weaponskill_WildCharge = 16589, // Troodon->player, no cast, single-target
    _Weaponskill_RipperClaw = 16590, // Troodon->self, 3.7s cast, range 9 90-degree cone
    _Weaponskill_2000MinaSwipe = 16591, // Bovian->self, 3.0s cast, range 10 ?-degree cone
    _Weaponskill_DisorientingGroan = 16594, // Bovian->self, 3.0s cast, range 40 circle
    _Ability_ = 16593, // Bovian->self, no cast, single-target
    _Weaponskill_DisorientingGroan1 = 16606, // BovianBull->self, 5.0s cast, range 40 circle
    _Weaponskill_2000MinaSwipe1 = 16605, // BovianBull->self, 3.0s cast, range 9 ?-degree cone
    _Weaponskill_FallingRock = 16595, // Helper->location, 3.0s cast, range 4 circle
    _Ability_1 = 16600, // Bovian->location, no cast, single-target
    _Ability_2 = 16599, // Helper->player, no cast, single-target
    _Weaponskill_ZoomIn = 16596, // Bovian->self, 5.0s cast, single-target
    _Weaponskill_ZoomIn1 = 16597, // Bovian->location, no cast, single-target
    _Weaponskill_ZoomIn2 = 16598, // Helper->self, no cast, range 42 width 8 rect
    _Weaponskill_FallingBoulder = 16607, // _Gen_LooseBoulder->self, no cast, range 4 circle
    _Weaponskill_2000MinaSlashOnPlayer = 16601, // Bovian->self/player, 5.0s cast, range 40 ?-degree cone
    _Weaponskill_2000MinaSlashOnNPC = 16602, // Bovian->self, no cast, range 40 ?-degree cone
    _Weaponskill_Shatter = 16608, // _Gen_LooseBoulder->self, no cast, range 8 circle
}

class TwoThousandMinaSlash : Components.GenericLineOfSightAOE
{
    private readonly List<Actor> _casters = [];

    public TwoThousandMinaSlash(BossModule module) : base(module, ActionID.MakeSpell(AID._Weaponskill_2000MinaSlashOnPlayer), 40, false)
    {
        Refresh();
    }

    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.RemainingTime);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Remove(caster);
            Refresh();
        }
    }

    private void Refresh()
    {
        var blockers = Module.Enemies(OID._Gen_LooseBoulder);

        Modify(ActiveCaster?.CastInfo?.LocXZ, blockers.Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(ActiveCaster?.CastInfo));
    }
}

class FallingRock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FallingRock), 4);
class ZoomIn(BossModule module) : Components.SimpleLineStack(module, 4, 42, ActionID.MakeSpell(AID._Ability_2), ActionID.MakeSpell(AID._Weaponskill_ZoomIn2), 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null)
            hints.AddForbiddenZone(new AOEShapeDonut(3, 100), Arena.Center, default, Activation);
    }
}

class NyelbertAI : Components.RoleplayModule
{
    public NyelbertAI(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget == null || Player.DistanceToHitbox(primaryTarget) > 25)
            return;

        if (WorldState.Party.LimitBreakCur == 10000)
        {
            Hints.RecommendedRangeToTarget = 20;
            if ((primaryTarget.Position - Player.Position).Length() < 25)
                UseAction(Roleplay.AID.FallingStar, null, 10, primaryTarget.PosRot.XYZ());
        }

        var numAOETargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);

        if (MP < 800)
            UseAction(Roleplay.AID.RonkanBlizzard3, primaryTarget);
        else if (MP < 1800)
            UseAction(Roleplay.AID.RonkanFlare, primaryTarget);
        else if (numAOETargets > 1)
            UseAction(Roleplay.AID.RonkanFlare, primaryTarget);
        else
        {
            if (primaryTarget.OID is 0x2975 or 0x2977)
            {
                var dotRemaining = StatusDetails(primaryTarget, Roleplay.SID.Electrocution, Player.InstanceID).Left;
                if (dotRemaining < 5)
                    UseAction(Roleplay.AID.RonkanThunder3, primaryTarget);
            }

            UseAction(Roleplay.AID.RonkanFire3, primaryTarget);
        }
    }
}

class Hints : BossComponent
{
    public Hints(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in WorldState.Actors)
        {
            if (e.IsAlly)
                Arena.Actor(e, ArenaColor.PlayerGeneric);
            else if (!e.IsDead && e.IsTargetable)
                Arena.Actor(e, ArenaColor.Enemy);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BovianBull => 1,
                _ => 0
            };
    }
}

class BoundsP1(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }
}

class BovianStates : StateMachineBuilder
{
    public BovianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<NyelbertAI>()
            .ActivateOnEnter<BoundsP1>()
            .Raw.Update = () => Module.Enemies(OID.Bovian).Any(x => x.InCombat);
        TrivialPhase(1)
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<ZoomIn>()
            .ActivateOnEnter<TwoThousandMinaSlash>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(-440, -691);
            })
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 686, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class Bovian(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;
}

