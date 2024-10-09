using BossMod.QuestBattle;

namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P3TerminusLacerator;

public enum OID : uint
{
    Boss = 0x35EE,
    Helper = 0x233C,
    Vanquisher = 0x35EF,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // 375A/3759/375B/375C/Boss->35F2/361A/3226/3604/3605/34A4/35F7, no cast, single-target
    _Weaponskill_BlackStar = 27011, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BlackStar1 = 27012, // Helper->location, 6.0s cast, range 40 circle
    _Weaponskill_DeadlyImpact = 27013, // Boss->self, 4.0s cast, single-target
    _Weaponskill_DeadlyImpact1 = 27014, // Helper->location, 7.0s cast, range 10 circle
    _Weaponskill_TheBlackDeath = 27010, // Boss->self, no cast, range 25 ?-degree cone
    _Weaponskill_ForcefulImpact = 26239, // Vanquisher->location, 5.0s cast, range 7 circle
    _Weaponskill_ForcefulImpactKB = 27030, // Helper->self, 5.6s cast, range 20 circle
    _AutoAttack_ = 27028, // Vanquisher->35F7, no cast, single-target
    _Weaponskill_WaveOfLoathing = 27032, // Vanquisher->self, 5.0s cast, range 40 circle
    _Weaponskill_ForceOfLoathing = 27031, // Vanquisher->self, no cast, range 10 ?-degree cone
    _Weaponskill_ = 27033, // Vanquisher->location, no cast, single-target
    _Weaponskill_MutableLaws = 27039, // Vanquisher->self, 4.0s cast, single-target
    _Weaponskill_MutableLaws1 = 27041, // Helper->location, 10.0s cast, range 6 circle
    _Weaponskill_MutableLaws2 = 27040, // Helper->location, 10.0s cast, range 6 circle
    _Weaponskill_AccursedTongue = 27037, // Vanquisher->self, 4.0s cast, single-target
    _Weaponskill_AccursedTongue1 = 27038, // Helper->35F5/35FA/35F9/35F7, 5.0s cast, range 6 circle
    _Weaponskill_Shock = 27035, // 35F0->self, 5.0s cast, range 10 circle
    _Weaponskill_Depress = 27036, // 35EF->35FA, 5.0s cast, range 7 circle
    _Weaponskill_ForcefulImpact2 = 27029, // 35EF->location, 5.0s cast, range 7 circle
}

class DeadlyImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact1), 10, maxCasts: 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_BlackStar1));

class ForcefulImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpact), 7);
class ForcefulImpactKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpactKB), 10, stopAtWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is Actor c)
            hints.PredictedDamage.Add((WorldState.Party.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo)));
    }
}
class MutableLaws1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MutableLaws1), 15);
class MutableLaws2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MutableLaws2), 6);
class AccursedTongue(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_AccursedTongue1), 6);
class ForcefulImpact2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpact2), 7);
class Shock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Shock), new AOEShapeCircle(10), maxCasts: 6);
class Depress(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Depress), 7);

class TerminusLaceratorStates : StateMachineBuilder
{
    private readonly TerminusLacerator _module;

    public TerminusLaceratorStates(TerminusLacerator module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>();
        TrivialPhase(1)
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ForcefulImpactKB>()
            .ActivateOnEnter<MutableLaws1>()
            .ActivateOnEnter<MutableLaws2>()
            .ActivateOnEnter<AccursedTongue>()
            .ActivateOnEnter<ForcefulImpact2>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Depress>()
            .Raw.Update = () => _module.BossP2?.IsDeadOrDestroyed ?? false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10933)]
public class TerminusLacerator(WorldState ws, Actor primary) : InstapullModule(ws, primary, new(-260.28f, 80.75f), new ArenaBoundsCircle(19.5f))
{
    public Actor? BossP2 => Enemies(OID.Vanquisher).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2, ArenaColor.Enemy);
    }
}

