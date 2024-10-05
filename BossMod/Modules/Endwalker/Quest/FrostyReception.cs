namespace BossMod.Endwalker.Quest.AFrostyReception;

public enum OID : uint
{
    Boss = 0x3646,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Weaponskill_AirborneExplosion = 28364, // 233C->363F, 5.0s cast, range 6 circle
    _Weaponskill_GigaTempest = 27440, // 3646->self, 5.0s cast, range 20 circle
    _Weaponskill_SpiralScourge = 27441, // 3646->364F, 4.0s cast, single-target
    _Weaponskill_Ruination = 27443, // 3646->self, 5.0s cast, range 40 width 8 cross
    _Weaponskill_Ruination1 = 27444, // 233C->self, 5.0s cast, range 30 width 8 rect

    _Weaponskill_Gunberd = 27447, // VergiliaVanCorculum->self, 5.0s cast, single-target
    _Weaponskill_Gunberd1 = 27448, // VergiliaVanCorculum->3641, no cast, range 40 width 6 rect
    _Weaponskill_ResinBomb = 27449, // 233C->363F/364F/363D/3641/3640/363E, 5.0s cast, range 5 circle
    _Ability_BombsAway = 27460, // VergiliaVanCorculum->self, 3.0s cast, single-target
    _Weaponskill_LockOn = 27461, // 3648->self, 1.0s cast, range 6 circle
    _Ability_Reinforcements = 27456, // VergiliaVanCorculum->self, 3.0s cast, single-target
    _Weaponskill_Bombardment = 27458, // 363A->self, no cast, single-target
    _Weaponskill_MagitekCannon = 27457, // 363B->player/363E/364F/3640/363D/363F/3641, 5.0s cast, range 6 circle
    _Weaponskill_Bombardment1 = 27459, // 233C->location, 4.0s cast, range 6 circle
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_GigaTempest));
class Ruination(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination), new AOEShapeCross(40, 4));
class Ruination2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination1), new AOEShapeRect(30, 4));
class LockOn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LockOn), new AOEShapeCircle(6));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_ResinBomb), 5);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_MagitekCannon), 6);
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bombardment1), 6);

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
public class VergiliaVanCorculum(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -78), new ArenaBoundsCircle(19.5f));

