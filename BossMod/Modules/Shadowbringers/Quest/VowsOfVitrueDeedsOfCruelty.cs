namespace BossMod.Shadowbringers.Quest.VowsOfVitrueDeedsOfCruelty;

public enum OID : uint
{
    Boss = 0x2C85, // R6.000, x1
    TerminusEstVisual = 0x2C98, // R1.000, x3
    BossHelper = 0x233C, // R0.500, x15, 523 type
    SigniferPraetorianus = 0x2C9A, // R0.500, x0 (spawn during fight), the adds on the catwalk that just rain down Fire II
    LembusPraetorianus = 0x2C99, // R2.400, x0 (spawn during fight), two large magitek ships
    MagitekBit = 0x2C9C, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Aero = 969, // 2C8F->player, 1.0s cast, single-target
    _AutoAttack_ = 18767, // 2C93->2C89, no cast, single-target
    _Weaponskill_GrandStrike = 9614, // 2C94->self, 2.5s cast, range 45+R width 4 rect
    _Ability_ = 18853, // 2C96->location, no cast, single-target
    _Ability_1 = 18854, // 2C97->location, no cast, single-target
    _Weaponskill_Crossbones = 18797, // 2C96->self, no cast, single-target
    _Weaponskill_TheOrder = 18798, // TerminusEstVisual->self, 3.0s cast, range 40+R width 4 rect
    _Weaponskill_AngrySalamander = 18799, // 2C97->self, 3.0s cast, range 45+R width 6 rect
    _Weaponskill_GrandSword = 9426, // 2C94->self, 3.0s cast, range 15+R 120-degree cone
    _Weaponskill_MagitekRay = 9422, // 2C91->self, 3.0s cast, range 40+R width 6 rect

    LoadData = 18786, // Boss->self, 3.0s cast, single-target
    AutoAttack = 870, // Boss/LembusPraetorianus->player, no cast, single-target
    MagitekRayRightArm = 18783, // Boss->self, 3.2s cast, range 45+R width 8 rect
    MagitekRayLeftArm = 18784, // Boss->self, 3.2s cast, range 45+R width 8 rect
    SystemError = 18785, // Boss->self, 1.0s cast, single-target
    AngrySalamander = 18787, // Boss->self, 3.0s cast, range 40+R width 6 rect
    FireII = 18959, // SigniferPraetorianus->location, 3.0s cast, range 5 circle
    TerminusEstBossCast = 18788, // Boss->self, 3.0s cast, single-target
    TerminusEstLocationHelper = 18889, // BossHelper->self, 4.0s cast, range 3 circle
    TerminusEstVisual = 18789, // TerminusEstVisual->self, 1.0s cast, range 40+R width 4 rect
    HorridRoar = 18779, // 2CC5->location, 2.0s cast, range 6 circle, this is your own attack. It spawns an aoe at the location of any enemy it initally hits
    GarleanFire = 4007, // LembusPraetorianus->location, 3.0s cast, range 5 circle
    MagitekBit = 18790, // Boss->self, no cast, single-target
    MetalCutterCast = 18793, // Boss->self, 6.0s cast, single-target
    MetalCutter = 18794, // BossHelper->self, 6.0s cast, range 30+R 20-degree cone
    AtomicRayCast = 18795, // Boss->self, 6.0s cast, single-target
    AtomicRay = 18796, // BossHelper->location, 6.0s cast, range 10 circle
    MagitekRayBit = 18791, // MagitekBit->self, 6.0s cast, range 50+R width 2 rect
    SelfDetonate = 18792, // MagitekBit->self, 7.0s cast, range 40+R circle, enrage if bits are not killed before cast
}

class MagitekRayTrash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekRay), new AOEShapeRect(42.1f, 3));
class GrandStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GrandStrike), new AOEShapeRect(48.2f, 2));
class TheOrder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TheOrder), new AOEShapeRect(41, 2));
class AngrySalamanderTrash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AngrySalamander), new AOEShapeRect(45.6f, 3));
class GrandSword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GrandSword), new AOEShapeCone(18.2f, 60.Degrees()));

class MagitekRayRightArm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayRightArm), new AOEShapeRect(45, 4));
class MagitekRayLeftArm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayLeftArm), new AOEShapeRect(45, 4));
class AngrySalamander(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AngrySalamander), new AOEShapeRect(40, 3));
class TerminusEstRects(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(40, 2);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEstLocationHelper)
        {
            _aoes.AddRange(
            [
                new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)),
                new(_shape, caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)),
                new(_shape, caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell))
            ]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEstVisual)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
class TerminusEstCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TerminusEstLocationHelper), new AOEShapeCircle(3));
class FireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireII), 5);
class GarleanFire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GarleanFire), 5);
class MetalCutter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MetalCutter), new AOEShapeCone(30, 10.Degrees()));
class MagitekRayBits(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayBit), new AOEShapeRect(50, 1));
class AtomicRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AtomicRay), new AOEShapeCircle(10));
class SelfDetonate(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SelfDetonate), "Enrage if bits are not killed before cast");

class EstinienP1(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        Hints.RecommendedRangeToTarget = 10;

        if (primaryTarget == null)
            return;

        if (ComboAction == Roleplay.AID.SonicThrust)
            UseAction(Roleplay.AID.CoerthanTorment, primaryTarget);
        if (ComboAction == Roleplay.AID.DoomSpike)
            UseAction(Roleplay.AID.SonicThrust, primaryTarget);
        UseAction(Roleplay.AID.DoomSpike, primaryTarget);

        if (Player.HPMP.CurHP * 2 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.AquaVitae, Player, -100);

        UseAction(Roleplay.AID.SkydragonDive, primaryTarget, -100);
    }
}

class P1Bounds(BossModule module) : BossComponent(module)
{
    private bool Hallway = true;

    public override void Update()
    {
        if (Hallway && Raid.Player()?.PosRot.Y < -5)
            Transition();

        if (Hallway)
            Arena.Center = new(Raid.Player()?.Position.X ?? 0, 400);
        else
            Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }

    private void Transition()
    {
        Hallway = false;
        Arena.Bounds = new ArenaBoundsCircle(20);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Hallway && hints.PotentialTargets.Count == 0)
            hints.ForcedMovement = new(1, 0, 0);
    }
}

class EstinienP2(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        Hints.RecommendedRangeToTarget = 3;

        if (primaryTarget == null)
            return;

        if (Module.Enemies(OID.SigniferPraetorianus).Any(x => x.IsTargetable && !x.IsDead))
            UseAction(Roleplay.AID.HorridRoar, Player);

        if (WorldState.Party.LimitBreakCur == 10000)
            UseAction(Roleplay.AID.DragonshadowDive, primaryTarget, 100);

        var dotRemaining = StatusDetails(primaryTarget, Roleplay.SID.StabWound, Player.InstanceID).Left;
        if (dotRemaining < 2.3f)
            UseAction(Roleplay.AID.Drachenlance, primaryTarget);

        UseAction(Roleplay.AID.AlaMorn, primaryTarget);
        UseAction(Roleplay.AID.Stardiver, primaryTarget, -100);
    }
}

class VowsOfVirtueDeedsOfCrueltyStates : StateMachineBuilder
{
    public VowsOfVirtueDeedsOfCrueltyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EstinienP1>()
            .ActivateOnEnter<P1Bounds>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<TheOrder>()
            .ActivateOnEnter<AngrySalamanderTrash>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<MagitekRayTrash>()
            .Raw.Update = () => Module.Enemies(OID.Boss).Any(x => x.IsTargetable);
        TrivialPhase(1)
            .ActivateOnEnter<EstinienP2>()
            .ActivateOnEnter<MagitekRayRightArm>()
            .ActivateOnEnter<MagitekRayLeftArm>()
            .ActivateOnEnter<AngrySalamander>()
            .ActivateOnEnter<TerminusEstCircle>()
            .ActivateOnEnter<TerminusEstRects>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<GarleanFire>()
            .ActivateOnEnter<MetalCutter>()
            .ActivateOnEnter<MagitekRayBits>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<SelfDetonate>()
            .OnEnter(() =>
            {
                Module.Arena.Bounds = new ArenaBoundsSquare(20);
                Module.Arena.Center = new(240, 230);
            })
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 702, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class VowsOfVirtueDeedsOfCruelty(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsRect(20, 14))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
