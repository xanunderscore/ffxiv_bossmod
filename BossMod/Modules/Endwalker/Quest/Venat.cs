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
    _Weaponskill_Parhelion1 = 25609, // Boss->self, 5.0s cast, single-target
}

class Kleos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_Kleos), "Raidwide + death wall spawn");

class Parhelion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Parhelion1), new AOEShapeCone(20, 22.5f.Degrees()));
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
class CircumzenithalArc2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CircumzenithalArc2), new AOEShapeCone(40, 90.Degrees()));
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
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10586)]
public class Venat(WorldState ws, Actor primary) : BossModule(ws, primary, new(-630, 72), new ArenaBoundsCircle(24.5f))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PotentialTargets[0].Priority = 0;
    }
}
