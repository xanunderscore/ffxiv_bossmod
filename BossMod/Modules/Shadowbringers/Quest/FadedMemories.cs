/*
namespace BossMod.Shadowbringers.Quest.FadedMemories;

public enum OID : uint
{
    Ardbert = 0x2F2E
}

public enum AID : uint
{
    // trash
    GateOfTartarus = 21079, // 2F13->self, 3.0s cast, range 6 120-degree cone
    FlamingTizona = 21094, // player->location, 4.0s cast, range 6 circle
    HighJump = 21299, // player->self, 4.0s cast, range 8 circle
    Geirskogul = 21098, // 2F22/2F21->self, 4.0s cast, range 62 width 8 rect
    AugmentedSuffering = 21080, // 2F14->self, 4.0s cast, range 8 circle
    TheDragonsGaze = 21090, // 2F1D->self, 4.0s cast, range 80 circle
    HyperdimensionalSlash = 21086, // 2F1A->self, 3.5s cast, range 45 width 8 rect

    // zenos
    EntropicFlame = 21117, // 233C->self, 5.0s cast, range 50 width 8 rect
    VeinSplitter = 21118, // 2F29->self, 5.0s cast, range 10 circle

    // ardbert
    Overcome = 21126, // Ardbert->self, 2.5s cast, range 8 120-degree cone
    Skydrive = 21127, // Ardbert->self, 2.5s cast, range 5 circle
    SkyHighDriveCCW = 21138, // Ardbert->self, 4.5s cast, single-target
    SkyHighDriveCW = 21139, // Ardbert->self, 4.5s cast, single-target
    _Weaponskill_SkyHighDrive1 = 21140, // 233C->self, 5.0s cast, range 40 width 8 rect
    _Weaponskill_SkyHighDrive2 = 21141, // 233C->self, no cast, range 40 width 8 rect
    _Weaponskill_AvalancheAxe1 = 21145, // 233C->self, 4.0s cast, range 10 circle
    _Weaponskill_AvalancheAxe2 = 21144, // 233C->self, 7.0s cast, range 10 circle
    _Weaponskill_AvalancheAxe3 = 21143, // 233C->self, 10.0s cast, range 10 circle
    _Weaponskill_OvercomeAllOdds = 21130, // 233C->self, 2.5s cast, range 60 30-degree cone
    _Weaponskill_Soulflash1 = 21136, // 233C->self, 4.0s cast, range 4 circle
    _Weaponskill_EtesianAxe1 = 21147, // 233C->self, 6.5s cast, range 80 circle
    _Weaponskill_Soulflash2 = 21137, // 233C->self, 4.0s cast, range 8 circle
    _Weaponskill_Groundbreaker1 = 21563, // 233C->self, 5.0s cast, range 6 circle
    _Weaponskill_Groundbreaker2 = 21151, // 233C->self, no cast, range 6 circle
    _Weaponskill_Groundbreaker4 = 21153, // 233C->self, 6.0s cast, range 40 90-degree cone
    _Weaponskill_Groundbreaker6 = 21157, // 233C->self, 6.0s cast, range 5-20 donut
    _Weaponskill_Groundbreaker8 = 21155, // 233C->self, 6.0s cast, range 15 circle
}

public enum SID : uint
{
    Invincibility = 671
}

class GateOfTartarus(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GateOfTartarus), new AOEShapeCone(6, 60.Degrees()));
class FlamingTizona(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FlamingTizona), 6);
class HighJump(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HighJump), new AOEShapeCircle(8));
class Geirskogul(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Geirskogul), new AOEShapeRect(62, 4));
class AugmentedSuffering(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AugmentedSuffering), new AOEShapeCircle(8));
class HyperdimensionalSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdimensionalSlash), new AOEShapeRect(45, 4));
class DragonsGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TheDragonsGaze));
class InteractHelper(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.InteractWithTarget = Module.Enemies(0x2F0D).FirstOrDefault(x => x.IsTargetable);
    }
}

class EntropicFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(50, 4));
class VeinSplitter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VeinSplitter), new AOEShapeCircle(10));

class Overcome(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Overcome), new AOEShapeCone(8, 60.Degrees()), 2);
class Skydrive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Skydrive), new AOEShapeCircle(5));

class SkyHighDrive(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle angle;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SkyHighDriveCCW:
                angle = -20.Degrees();
                return;
            case AID.SkyHighDriveCW:
                angle = 20.Degrees();
                return;
            case AID._Weaponskill_SkyHighDrive1:
                if (angle != default)
                {
                    Sequences.Add(new(new AOEShapeRect(40, 4), caster.Position, spell.Rotation, angle, Module.CastFinishAt(spell, 0.5f), 0.6f, 10, 4));
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_SkyHighDrive1 or AID._Weaponskill_SkyHighDrive2)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
            if (Sequences.Count == 0)
                angle = default;
        }
    }
}

class AvalancheAxe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe1), new AOEShapeCircle(10));
class AvalancheAxe2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe2), new AOEShapeCircle(10));
class AvalancheAxe3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe3), new AOEShapeCircle(10));
class OvercomeAllOdds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OvercomeAllOdds), new AOEShapeCone(60, 15.Degrees()), 1)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts > 0)
            MaxCasts = 2;
    }
}
class Soulflash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Soulflash1), new AOEShapeCircle(4));
class EtesianAxe(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_EtesianAxe1), 15, kind: Kind.DirForward);
class Soulflash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Soulflash2), new AOEShapeCircle(8));

class GroundbreakerExaflares(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Groundbreaker1)
        {
            Lines.Add(new Line
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 6,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 8,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID._Weaponskill_Groundbreaker1 or (uint)AID._Weaponskill_Groundbreaker2)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class GroundbreakerCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker4), new AOEShapeCone(40, 45.Degrees()));
class GroundbreakerDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker6), new AOEShapeDonut(5, 20));
class GroundbreakerCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker8), new AOEShapeCircle(15));

class P1Bounds(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }
}

public class ElidibusStates : StateMachineBuilder
{
    public ElidibusStates(BossModule module) : base(module)
    {
        bool checkDutyEnded() => Module.WorldState.CurrentCFCID != 743;

        TrivialPhase()
            .ActivateOnEnter<GateOfTartarus>()
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<HighJump>()
            .ActivateOnEnter<Geirskogul>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<HyperdimensionalSlash>()
            .ActivateOnEnter<DragonsGaze>()
            .ActivateOnEnter<InteractHelper>()
            .ActivateOnEnter<P1Bounds>()
            .Raw.Update = () => checkDutyEnded() || Module.Enemies(OID.Ardbert).Any(x => x.IsTargetable);
        TrivialPhase(1)
            .ActivateOnEnter<SkyHighDrive>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<AvalancheAxe>()
            .ActivateOnEnter<AvalancheAxe2>()
            .ActivateOnEnter<AvalancheAxe3>()
            .ActivateOnEnter<OvercomeAllOdds>()
            .ActivateOnEnter<Soulflash>()
            .ActivateOnEnter<EtesianAxe>()
            .ActivateOnEnter<Soulflash2>()
            .ActivateOnEnter<GroundbreakerExaflares>()
            .ActivateOnEnter<GroundbreakerCone>()
            .ActivateOnEnter<GroundbreakerDonut>()
            .ActivateOnEnter<GroundbreakerCircle>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(-392, 780);
            })
            .Raw.Update = checkDutyEnded;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 743, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class Elidibus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
*/
