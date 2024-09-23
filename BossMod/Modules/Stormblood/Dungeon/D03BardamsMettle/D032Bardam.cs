namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D032Bardam;

public enum OID : uint
{
    Boss = 0x1AA3,
    Helper = 0x233C,
    _Gen_Bardam = 0x18D6, // R0.500, x34
    _Gen_HunterOfBardam = 0x1AA5, // R2.200, x0 (spawn during fight)
    _Gen_ThrowingSpear = 0x1F49, // R1.250, x0 (spawn during fight)
    _Gen_StarShard = 0x1F4A, // R2.400, x0 (spawn during fight)
    _Gen_LoomingShadow = 0x1F4D, // R1.000, x0 (spawn during fight)
    _Gen_WarriorOfBardam = 0x1AA4, // R1.100, x0 (spawn during fight)
}

public enum AID : uint
{
    _Ability_Travail = 7935, // Boss->self, no cast, single-target
    _Ability_Magnetism = 7944, // _Gen_HunterOfBardam->self, no cast, range 40+R circle
    __Tremblor = 9596, // _Gen_Bardam->self, 4.0s cast, range 10 circle
    __Tremblor1 = 9595, // _Gen_Bardam->self, 4.0s cast, range 10-20 donut
    _Weaponskill_EmptyGaze = 7940, // _Gen_HunterOfBardam->self, 6.5s cast, range 40+R circle
    _Weaponskill_Charge = 9599, // _Gen_ThrowingSpear->self, 2.5s cast, range 45+R width 5 rect
    _Ability_GreenCheckmark = 9450, // _Gen_Bardam->player, no cast, single-target
    _Weaponskill_Sacrifice = 7937, // _Gen_Bardam->location, 7.0s cast, range 3 circle
    _Spell_BardamsRing = 9601, // _Gen_Bardam->self, no cast, range -20 donut
    _Weaponskill_Comet = 9597, // _Gen_Bardam->location, 4.0s cast, range 4 circle
    _Weaponskill_Comet1 = 9598, // _Gen_Bardam->location, 1.5s cast, range 4 circle
    __HeavyStrike = 9591, // _Gen_HunterOfBardam/_Gen_WarriorOfBardam->self, 4.0s cast, single-target
    __HeavyStrike1 = 9592, // _Gen_Bardam->self, 4.0s cast, range 6+R ?-degree cone
    __HeavyStrike2 = 9593, // _Gen_Bardam->self, 4.0s cast, range 12+R ?-degree cone
    __HeavyStrike3 = 9594, // _Gen_Bardam->self, 4.0s cast, range 18+R ?-degree cone
    __CometImpact = 9600, // _Gen_StarShard->self, 4.0s cast, range 9 circle
    _Weaponskill_Reconstruct = 7933, // Boss->self, no cast, single-target
    _Weaponskill_Reconstruct1 = 7934, // _Gen_Bardam->location, 4.0s cast, range 5 circle
    __ = 9611, // _Gen_StarShard->self, no cast, single-target
    __Tremblor2 = 9605, // _Gen_HunterOfBardam->self, 3.5s cast, single-target
    _Ability_MeteorImpact = 9602, // _Gen_LoomingShadow->self, 30.0s cast, ???
}

class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Comet), 4);
class Comet1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Comet1), 4);

class HeavyStrike(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees())])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.__HeavyStrike1)
            AddSequence(caster.Position, Module.CastFinishAt(spell, 1), caster.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var idx = (AID)spell.Action.ID switch
        {
            AID.__HeavyStrike1 => 0,
            AID.__HeavyStrike2 => 1,
            AID.__HeavyStrike3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(idx, caster.Position, WorldState.FutureTime(1.35f), caster.Rotation))
            Module.ReportError(this, $"invalid sequence {idx}");
    }
}

class Tremblor(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.__Tremblor)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.__Tremblor => 0,
            AID.__Tremblor1 => 1,
            _ => -1
        };
        if (!AdvanceSequence(order, Module.Center, WorldState.FutureTime(1.6f)))
            ReportError($"unexpected order {order}");
    }
}

class EmptyGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_EmptyGaze));

class Charge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Charge), new AOEShapeRect(50, 2.5f, 50));
class Sacrifice(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Weaponskill_Sacrifice), 3, maxSoakers: 2);
class CometImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.__CometImpact), new AOEShapeCircle(9));
class Reconstruct(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Reconstruct1), 5);

class MeteorImpact : Components.GenericLineOfSightAOE
{
    private Actor? Caster;

    public MeteorImpact(BossModule module) : base(module, ActionID.MakeSpell(AID._Ability_MeteorImpact), 100, false)
    {
        WorldState.Actors.IsDeadChanged.Subscribe(OnActorDeath);
    }

    private void Refresh()
    {
        var blockers = Module.Enemies(OID._Gen_StarShard).Where(s => !s.IsDead).Select(s => (s.Position, s.HitboxRadius)).ToList();

        var pos = blockers.Count == 1 ? Caster?.Position : null;

        Modify(pos, pos == null ? [] : blockers.Select(s => (s.Position, s.HitboxRadius)), Module.CastFinishAt(Caster?.CastInfo));
    }

    private void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID._Gen_StarShard)
            Refresh();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_MeteorImpact)
        {
            Caster = caster;
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_MeteorImpact)
        {
            Caster = null;
            Refresh();
        }
    }
}

class BardamStates : StateMachineBuilder
{
    public BardamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<EmptyGaze>()
            .ActivateOnEnter<Charge>()
            .ActivateOnEnter<Sacrifice>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<Comet1>()
            .ActivateOnEnter<HeavyStrike>()
            .ActivateOnEnter<CometImpact>()
            .ActivateOnEnter<Reconstruct>()
            .ActivateOnEnter<MeteorImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6177)]
public class Bardam(WorldState ws, Actor primary) : BossModule(ws, primary, new(-28.5f, -14), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = AIHints.Enemy.PriorityForbidFully;
    }
}

