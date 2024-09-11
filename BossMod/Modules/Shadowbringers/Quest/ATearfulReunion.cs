namespace BossMod.Shadowbringers.Quest.ATearfulReunion;

public enum OID : uint
{
    Boss = 0x29C5,
    _Gen_Phronesis = 0x29E7, // R0.500, x3
    _Gen_ = 0x2A1A, // R0.500, x0 (spawn during fight)
    _Gen_1 = 0x2A1B, // R0.500, x0 (spawn during fight)
    _Gen_2 = 0x2A1C, // R0.500, x0 (spawn during fight)
    _Gen_3 = 0x2A19, // R0.500, x0 (spawn during fight)
    _Gen_Hollow = 0x29C6, // R0.750-2.250, x0 (spawn during fight)
    _Gen_4 = 0x2AC5, // R0.500, x0 (spawn during fight)
    _Gen_5 = 0x2A1D, // R0.500, x0 (spawn during fight)
    _Gen_LightningGlobe = 0x29C8, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_SanctifiedFire = 17032, // Boss->player/29C3, no cast, single-target
    _Spell_Hollow = 17033, // Boss->location, 3.0s cast, single-target
    _Spell_Hollow1 = 17034, // Boss->location, no cast, single-target
    _Spell_Hollow2 = 17218, // Boss->location, no cast, single-target
    _Spell_Hollow3 = 17219, // Boss->location, no cast, single-target
    _Ability_ = 17455, // 29C6->player, no cast, single-target
    _Weaponskill_HollowGravity = 17048, // 29C6->player, no cast, single-target
    _Spell_SanctifiedFireIII = 17035, // Boss->self, 4.2s cast, single-target
    _Spell_SanctifiedFireIII1 = 17036, // 29E7->location, 4.0s cast, range 6 circle
    _Ability_1 = 17456, // 29C6->player/29C3, no cast, single-target
    _Spell_SanctifiedBlizzardIII = 17045, // Boss->self, 4.0s cast, range 40+R 45-degree cone
    _Ability_AetherialManipulation = 17041, // Boss->location, no cast, single-target

    // stack with npc
    _Spell_SanctifiedFlare = 17039, // Boss->players, 5.0s cast, range 6 circle

    // spread from npc
    _Spell_SanctifiedFireIV = 17037, // Boss->self, 4.2s cast, single-target
    _Spell_SanctifiedFireIV1 = 17038, // _Gen_Phronesis->players/29C3, 4.0s cast, range 10 circle

    _Spell_Hollow4 = 17220, // Boss->location, no cast, single-target
    _Spell_SanctifiedThunderIII = 17040, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 17050, // _Gen_LightningGlobe/_Gen_Hollow->self, no cast, single-target
    _Spell_SanctifiedBlizzardIV = 17046, // Boss->self, 5.2s cast, single-target
    __SanctifiedBlizzardIV = 17047, // _Gen_Phronesis->self, 5.0s cast, range 5-20 donut
    _Spell_SanctifiedBlizzardII = 17044, // Boss->self, 3.0s cast, range 5 circle
}

class SanctifiedBlizzardIV(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.__SanctifiedBlizzardIV), new AOEShapeDonut(5, 20));
class SanctifiedBlizzardII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SanctifiedBlizzardII), new AOEShapeCircle(5));
class SanctifiedFireIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SanctifiedFireIII1), 6);
class SanctifiedBlizzardIII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SanctifiedBlizzardIII), new AOEShapeCone(40.5f, 22.5f.Degrees()));
class Hollow(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID._Gen_Hollow));
class HollowTether(BossModule module) : Components.Chains(module, 1, chainLength: 5);
class SanctifiedFireIV(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_SanctifiedFireIV1), 10);
class SanctifiedFlare(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Spell_SanctifiedFlare), 6, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveStacks.Any() && WorldState.Actors.First(x => x.OID == 0x29C3) is Actor cerigg)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(6, 100), cerigg.Position, default, ActiveStacks.First().Activation);
        }
    }
}

class LightningGlobe(BossModule module) : Components.GenericLineOfSightAOE(module, default, 100, false)
{
    private readonly List<Actor> Balls = [];
    private IEnumerable<(WPos Center, float Radius)> Hollows => Module.Enemies(OID._Gen_Hollow).Select(h => (h.Position, h.HitboxRadius));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 6)
            Balls.Add(source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in Balls)
            Arena.AddLine(pc.Position, b.Position, ArenaColor.Danger);
    }

    public override void Update()
    {
        var player = Raid.Player();
        if (player == null)
            return;

        Balls.RemoveAll(b => b.IsDead);

        var closestBall = Balls.OrderBy(player.DistanceToHitbox).FirstOrDefault();
        Modify(closestBall?.Position, Hollows);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Origin != null
            && actor.Position.InCircle(Origin.Value, MaxRange)
            && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
        {
            hints.Add("Pull lightning orb into black hole!");
        }
    }
}

class PhronesisStates : StateMachineBuilder
{
    public PhronesisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SanctifiedFireIII>()
            .ActivateOnEnter<SanctifiedBlizzardIII>()
            .ActivateOnEnter<Hollow>()
            .ActivateOnEnter<HollowTether>()
            .ActivateOnEnter<SanctifiedFireIV>()
            .ActivateOnEnter<SanctifiedFlare>()
            .ActivateOnEnter<LightningGlobe>()
            .ActivateOnEnter<SanctifiedBlizzardII>()
            .ActivateOnEnter<SanctifiedBlizzardIV>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69164, NameID = 8931)]
public class Phronesis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-256, -284), new ArenaBoundsCircle(20))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in WorldState.Actors.Where(a => a.IsAlly && a.Type == ActorType.Enemy))
            Arena.Actor(a, ArenaColor.PlayerGeneric);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }

    protected override bool CheckPull() => true;
}
