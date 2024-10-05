namespace BossMod.Endwalker.Quest.AFrostyReception;

public enum OID : uint
{
    Boss = 0x3646,
    Helper = 0x233C,
    _Gen_ = 0x345C, // R2.100, x1
    _Gen_1 = 0x3415, // R0.500-2.000, x2
    _Gen_2 = 0x3414, // R0.500, x3
    _Gen_3 = 0x3461, // R1.600, x1
    _Gen_4 = 0x364B, // R2.800, x2
    _Gen_5 = 0x3458, // R2.000, x1
    _Gen_TelotekReaper = 0x34F9, // R0.500-2.000, x3
    LockOn = 0x3648, // R1.200, x0 (spawn during fight)
    _Gen_TelotekSkyArmor = 0x363A, // R2.000, x0 (spawn during fight)
    _Gen_TelotekReaper1 = 0x363B, // R0.500-2.000, x0 (spawn during fight)
    _Gen_TelotekPredator = 0x363C, // R2.100, x0 (spawn during fight)
    _Gen_TemperedImperial = 0x3637, // R0.500, x0 (spawn during fight)
    _Gen_TemperedImperial1 = 0x3638, // R0.500, x0 (spawn during fight)
    _Gen_TemperedImperial2 = 0x3636, // R0.500, x0 (spawn during fight)
    _Gen_TelotekArmoredWeapon = 0x3865, // R3.960, x0 (spawn during fight)
    _Gen_TelotekHexadrone = 0x3866, // R3.445, x0 (spawn during fight)

    Lyse = 0x363D,
    Pipin = 0x363E,
    Magnai = 0x363F,
    Sadu = 0x3640,
    Cirina = 0x3641,
    Lucia = 0x364F,
}

public enum AID : uint
{
    _Weaponskill_PhotonStream = 28296, // _Gen_TelotekReaper/_Gen_1/_Gen_TelotekReaper1->player/Lucia/3643/365D/Lyse/Sadu/Pipin/Magnai/Cirina/364A, no cast, single-target
    _AutoAttack_Attack = 870, // _Gen_2/Boss/_Gen_/_Gen_5/_Gen_TelotekSkyArmor/_Gen_TelotekPredator/_Gen_TemperedImperial2->player/3413/337A/Lucia/365C/Cirina/Pipin/Lyse/364A, no cast, single-target
    _AutoAttack_Attack1 = 872, // _Gen_3/_Gen_TemperedImperial1->3642/Pipin, no cast, single-target
    _Weaponskill_FastBlade = 28299, // _Gen_2/_Gen_TemperedImperial2->337A/3413/Pipin/Lyse/364A, no cast, single-target
    _Ability_ = 27442, // Boss->location, no cast, single-target
    _Weaponskill_GigaTempest = 27440, // Boss->self, 5.0s cast, range 20 circle
    _Weaponskill_MagitekMissiles = 28297, // _Gen_/_Gen_TelotekPredator->365C/Cirina/364A, no cast, single-target
    _Weaponskill_SpiralScourge = 27441, // Boss->Lucia, 4.0s cast, single-target
    _Weaponskill_Ruination = 27443, // Boss->self, 5.0s cast, range 40 width 8 cross
    _Weaponskill_Ruination1 = 27444, // Helper->self, 5.0s cast, range 30 width 8 rect
    _Weaponskill_ResinAmmunition = 27445, // Boss->self, 3.0s cast, single-target
    _Ability_1 = 14588, // Helper->Cirina, no cast, single-target
    _Weaponskill_Gunberd = 27447, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Gunberd1 = 27448, // Boss->Cirina, no cast, range 40 width 6 rect
    _Weaponskill_ResinBomb = 27449, // Helper->Lyse/Magnai/Sadu/Pipin/Lucia/Cirina, 5.0s cast, range 5 circle
    _Ability_BombsAway = 27460, // Boss->self, 3.0s cast, single-target
    _Weaponskill_LockOn = 27461, // _Gen_6->self, 1.0s cast, range 6 circle
    _Ability_Reinforcements = 27456, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Bombardment = 27458, // _Gen_TelotekSkyArmor->self, no cast, single-target
    _Weaponskill_MagitekCannon = 27457, // _Gen_TelotekReaper1->player/Lyse/Sadu/Magnai/Lucia/Cirina/Pipin, 5.0s cast, range 6 circle
    _Weaponskill_Bombardment1 = 27459, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_AetherochemicalGrenado = 27501, // Boss->player, 5.0s cast, single-target
    _Weaponskill_AetherochemicalGrenado1 = 27510, // Helper->players, no cast, range 6 circle
    _Ability_BombsAway1 = 27462, // Boss->self, 3.0s cast, single-target
    _Weaponskill_GaseousBomb = 27464, // Helper->location, 10.0s cast, range 20 circle
    _Weaponskill_LockOn1 = 27463, // _Gen_6->self, 1.0s cast, range 6 circle
    _Ability_RequestAssistance = 27466, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TrueThrust = 28300, // _Gen_TemperedImperial->Lyse/Pipin/364A, no cast, single-target
    _Spell_Fire = 28301, // _Gen_TemperedImperial1->Pipin/Lyse/364A, 1.5s cast, single-target
    _AutoAttack_Attack2 = 871, // _Gen_TemperedImperial->Pipin/Lyse/364A, no cast, single-target
    _AutoAttack_ = 28425, // _Gen_TelotekHexadrone->player, no cast, single-target
    _AutoAttack_1 = 28424, // _Gen_TelotekArmoredWeapon->364A, no cast, single-target
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_GigaTempest));
class Ruination(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination), new AOEShapeCross(40, 4));
class Ruination2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination1), new AOEShapeRect(30, 4));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_ResinBomb), 5);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_MagitekCannon), 6);
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bombardment1), 6);

class LockOn(BossModule module) : Components.GenericAOEs(module)
{
    private class Caster
    {
        public required Actor Actor;
        public required DateTime FinishAt;
    }

    private readonly List<Caster> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCircle(6), c.Actor.Position, default, c.FinishAt));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LockOn)
            Casters.Add(new() { Actor = actor, FinishAt = WorldState.FutureTime(6.6f) });
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_LockOn or AID._Weaponskill_LockOn1)
        {
            var c = Casters.FindIndex(p => p.Actor == caster);
            if (c >= 0)
                Casters[c].FinishAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_LockOn or AID._Weaponskill_LockOn1)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}

class VergiliaVanCorculumStates : StateMachineBuilder
{
    public VergiliaVanCorculumStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GigaTempest>()
            .ActivateOnEnter<Ruination>()
            .ActivateOnEnter<Ruination2>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<ResinBomb>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<Bombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69919, NameID = 10572)]
public class VergiliaVanCorculum(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -78), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

