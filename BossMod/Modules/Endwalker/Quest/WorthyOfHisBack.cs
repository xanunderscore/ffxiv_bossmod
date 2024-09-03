namespace BossMod.Endwalker.Quest.WorthyOfHisBack;

public enum OID : uint
{
    Boss = 0x342C,
    DeathWall = 0x1EB27A,
}

public enum AID : uint
{
    _AutoAttack_ = 28383, // Boss->player, no cast, single-target
    _Spell_Kleos = 25597, // Boss->self, 3.0s cast, range 40 circle
    _Ability_ = 25599, // Boss->location, no cast, single-target
    _Weaponskill_CircumzenithalArc = 25598, // Boss->self, 5.0+2.2s cast, single-target
    _Weaponskill_CircumzenithalArc1 = 28466, // 233C->self, 7.3s cast, range 40 180-degree cone
    _Spell_TrueBlink = 25600, // Boss->self, 3.0s cast, single-target
    _Ability_1 = 25601, // 342D->location, no cast, single-target
    _Weaponskill_CrepuscularRay = 25595, // 342D->location, 5.0s cast, width 8 rect charge
    _Weaponskill_CircumzenithalArc2 = 28376, // 233C->self, 7.3s cast, range 40 180-degree cone
    _Spell_Enomotos = 25603, // 233C->self, 5.0s cast, range 6 circle
    _Weaponskill_CircleOfBrilliance = 25602, // Boss->self, 5.0s cast, range 5 circle
    _Spell_Enomotos1 = 25604, // 233C->self, no cast, range 6 circle
    _Ability_MousasMantle = 25592, // Boss->self, 6.5s cast, single-target
    _Weaponskill_EpeaPteroenta = 25605, // Boss->self, 7.0s cast, range 20 120-degree cone
    _Weaponskill_EpeaPteroenta1 = 25606, // Boss->self, no cast, range 20 ?-degree cone
    _Weaponskill_EpeaPteroenta2 = 25607, // 233C->self, 8.0s cast, range 20 120-degree cone
    _Weaponskill_Parhelion = 25610, // 233C->self, 5.0s cast, range 20 45-degree cone
    _Weaponskill_Parhelion2 = 25612, // 233C->self, 4.0s cast, range 10 circle
    _Weaponskill_Parhelion3 = 25611, // 233C->self, 1.5s cast, range 20 45-degree cone
    _Weaponskill_Parhelion4 = 25613, // 233C->self, 4.0s cast, range 10-15 donut
    _Weaponskill_Parhelion5 = 25614, // 233C->self, 4.0s cast, range 15-20 donut
    _AutoAttack_1 = 28384, // Boss->player, no cast, single-target
    _Weaponskill_Parhelion1 = 25608, // Boss->self, 5.0s cast, single-target
    _Ability_MagossMantle = 25593, // Boss->self, 6.5s cast, single-target
    _Spell_TrueStone = 28385, // Boss->player, no cast, single-target
    _Spell_TrueAeroIV = 25615, // Boss->self, 3.0s cast, range 40 circle
    _Spell_Windage = 25616, // 342E->self, 2.5s cast, range 5 circle
    _Spell_AfflatusAzem = 25617, // Boss->location, 4.0s cast, range 5 circle
    _Weaponskill_Parhelion6 = 25609, // Boss->self, 5.0s cast, single-target
    _Spell_AfflatusAzem1 = 25618, // Boss->location, 1.0s cast, range 5 circle
    _Spell_TrueHoly = 25619, // Boss->self, 5.0s cast, range 40 circle
    _Spell_Windage1 = 28116, // 342E->self, 9.0s cast, range 5 circle
    _Spell_TrueStoneIV = 25620, // Boss->self, 3.0s cast, single-target
    _Spell_TrueStoneIV1 = 25621, // 233C->location, 6.0s cast, range 10 circle
    _Weaponskill_ = 25622, // Boss->self, no cast, single-target
    _Spell_Enomotos2 = 28392, // 233C->location, 3.0s cast, range 4 circle
}

class Kleos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_Kleos), "Raidwide + death wall spawn");

class ParhelionCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    enum Direction
    {
        Unknown,
        CW,
        CCW
    }

    private Direction NextDirection;

    private Angle GetAngle(Direction d) => d switch
    {
        Direction.CCW => 45.Degrees(),
        Direction.CW => -45.Degrees(),
        _ => default
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Parhelion)
            Sequences.Add(new(new AOEShapeCone(20, 22.5f.Degrees()), caster.Position, caster.Rotation, GetAngle(NextDirection), Module.CastFinishAt(spell), 2.6f, 9));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Parhelion or AID._Weaponskill_Parhelion3)
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == 168)
            NextDirection = Direction.CCW;
        if (iconID == 167)
            NextDirection = Direction.CW;

        for (var i = 0; i < Sequences.Count; i++)
            Sequences[i] = Sequences[i] with { Increment = GetAngle(NextDirection) };
    }
}
class Parhelion(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 15), new AOEShapeDonut(15, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Parhelion2)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_Parhelion2 => 0,
            AID._Weaponskill_Parhelion4 => 1,
            AID._Weaponskill_Parhelion5 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}

class EpeaPteroenta(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminent = true;
        foreach (var c in Casters.Take(2))
        {
            yield return new AOEInstance(new AOEShapeCone(20, 60.Degrees()), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo), imminent ? ArenaColor.Danger : ArenaColor.AOE);
            imminent = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_EpeaPteroenta or AID._Weaponskill_EpeaPteroenta2)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_EpeaPteroenta or AID._Weaponskill_EpeaPteroenta2)
            Casters.Remove(caster);
    }
}

class CrepuscularRay(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CrepuscularRay), 4);
class CircumzenithalArc(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CircumzenithalArc1), new AOEShapeCone(40, 90.Degrees()));
class CircumzenithalArc2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CircumzenithalArc2), new AOEShapeCone(40, 90.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // skip forbidden zone creation if charges are still active, AI doesn't handle it well
        if (Module.FindComponent<CrepuscularRay>()?.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class CircleOfBrilliance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CircleOfBrilliance), new AOEShapeCircle(5));
class Enomotos(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), ActionID.MakeSpell(AID._Spell_Enomotos))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            Lines.Add(new Line()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 5,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 9,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_Enomotos or AID._Spell_Enomotos1)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class DeathWall(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Spell_Kleos))
{
    private DateTime? ExpectedActivation;
    private readonly AOEShape Voidzone = new AOEShapeDonut(19.5f, 100);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (ExpectedActivation != null)
            yield return new AOEInstance(Voidzone, Arena.Center, Activation: ExpectedActivation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            ExpectedActivation = WorldState.FutureTime(4.1f);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DeathWall)
        {
            ExpectedActivation = null;
            Arena.Bounds = new ArenaBoundsCircle(19.5f);
        }
    }
}

class Windage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Windage), new AOEShapeCircle(5));
class AfflatusAzem(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(5), ActionID.MakeSpell(AID._Spell_AfflatusAzem), ActionID.MakeSpell(AID._Spell_AfflatusAzem1), 5, 2.1f, 5);
class WindageSlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Windage1), new AOEShapeCircle(5));
class TrueHoly(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Spell_TrueHoly), 20)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(ClassShared.AID.Surecast) : ActionID.MakeSpell(ClassShared.AID.ArmsLength);
        if (Casters.FirstOrDefault()?.CastInfo?.NPCRemainingTime is var t && t < 5)
            hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.Medium);
    }
}
class TrueStoneIV(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_TrueStoneIV1), 10, maxCasts: 7);
class Enomotos3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Enomotos2), 4);

public class VenatStates : StateMachineBuilder
{
    public VenatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathWall>()
            .ActivateOnEnter<Kleos>()
            .ActivateOnEnter<CircumzenithalArc>()
            .ActivateOnEnter<CircumzenithalArc2>()
            .ActivateOnEnter<CrepuscularRay>()
            .ActivateOnEnter<CircleOfBrilliance>()
            .ActivateOnEnter<Enomotos>()
            .ActivateOnEnter<EpeaPteroenta>()
            .ActivateOnEnter<Parhelion>()
            .ActivateOnEnter<ParhelionCone>()
            .ActivateOnEnter<Windage>()
            .ActivateOnEnter<AfflatusAzem>()
            .ActivateOnEnter<WindageSlow>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<TrueStoneIV>()
            .ActivateOnEnter<Enomotos3>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10586)]
public class Venat(WorldState ws, Actor primary) : BossModule(ws, primary, new(-630, 72), new ArenaBoundsCircle(24.5f))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.OID == 0x3864 ? 1 : 0;
    }
}
