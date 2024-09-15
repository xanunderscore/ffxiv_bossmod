namespace BossMod.Shadowbringers.Dungeon.D08HeroesGauntlet;

public enum OID : uint
{
    Boss = 0x2DEC,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_SpectralDream = 20427, // Boss->players, 4.0s cast, single-target
    _Ability_Dash = 20435, // Boss->self, 3.0s cast, single-target
    _Ability_ = 20437, // Boss->location, no cast, single-target
    _Weaponskill_ = 20429, // Boss->self, no cast, single-target
    _Weaponskill_VacuumBlade = 20577, // Helper->self, no cast, range 15 circle
    _Spell_SpectralWhirlwind = 20428, // Boss->self, 4.0s cast, range 60 circle
    _Spell_SpectralGust = 21454, // Boss->self, no cast, single-target
    _Spell_SpectralGust1 = 21455, // Helper->player, 6.0s cast, range 5 circle
    _Ability_ChickenKnife = 20438, // Boss->self, 2.0s cast, single-target
    _Ability_Shadowdash = 20436, // Boss->self, 3.0s cast, single-target
    _Weaponskill_CowardsCunning = 20439, // 2E71->self, 3.0s cast, range 60 width 2 rect
    _Ability_1 = 20501, // 2DED->location, no cast, single-target
    _Weaponskill_1 = 20431, // Boss->self, no cast, single-target
    _Weaponskill_2 = 20432, // 2DED->self, no cast, single-target
    _Weaponskill_Papercutter = 20434, // Helper->self, no cast, range 80 width 14 rect
    _Weaponskill_Papercutter1 = 20433, // Helper->self, no cast, range 80 width 14 rect
    _Weaponskill_3 = 20430, // 2DED->self, no cast, single-target
    _Weaponskill_VacuumBlade1 = 20578, // Helper->self, no cast, range 15 circle
}

class CowardsCunning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CowardsCunning), new AOEShapeRect(60, 1));
class SpectralGust(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_SpectralGust1), 5);
class SpectralWhirlwind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_SpectralWhirlwind));
class SpectralDream(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_SpectralDream));
class ChickenKnife(BossModule module) : Components.GenericAOEs(module)
{
    enum Shape : ushort
    {
        None,
        Circle = 0xB0,
        RectVertical = 0xB1,
        RectHorizontal = 0xB2,
        Circle2 = 0xB3,
        RectVertical2 = 0xB4,
        RectHorizontal2 = 0xB5,
    }

    private Shape NextShape;

    private readonly List<(Actor, DateTime)> CastCenters = [];

    private AOEShape NextAOE => NextShape switch
    {
        Shape.Circle or Shape.Circle2 => new AOEShapeCircle(15),
        Shape.RectVertical or Shape.RectVertical2 => new AOEShapeRect(40, 7, 40),
        Shape.RectHorizontal or Shape.RectHorizontal2 => new AOEShapeRect(40, 7, 40, 90.Degrees()),
        _ => new AOEShapeDonut(1, 2)
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => CastCenters.Select(act => new AOEInstance(NextAOE, act.Item1.Position, act.Item1.Rotation, act.Item2));

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == 2193)
            NextShape = (Shape)status.Extra;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EAED9 && state == 0x10002)
            CastCenters.Add((actor, WorldState.FutureTime(8.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_VacuumBlade or AID._Weaponskill_Papercutter1)
            CastCenters.Clear();
    }
}

class SpectralThiefStates : StateMachineBuilder
{
    public SpectralThiefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CowardsCunning>()
            .ActivateOnEnter<ChickenKnife>()
            .ActivateOnEnter<SpectralGust>()
            .ActivateOnEnter<SpectralWhirlwind>()
            .ActivateOnEnter<SpectralDream>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9505)]
public class SpectralThief(WorldState ws, Actor primary) : BossModule(ws, primary, new(-680, 450), new ArenaBoundsSquare(20));
