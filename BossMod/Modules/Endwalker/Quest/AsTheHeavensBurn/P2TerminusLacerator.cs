﻿using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P2TerminusLacerator;

public enum OID : uint
{
    Boss = 0x35EC,
    Helper = 0x233C,
    Meteorite = 0x35ED
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // 3759/375B/375C/375A/Boss->player/35F2/3604/3605/34A4/3226/361A, no cast, single-target
    _Weaponskill_BlackStar = 27011, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BlackStar1 = 27012, // Helper->location, 6.0s cast, range 40 circle
    _Weaponskill_TheBlackDeath = 27010, // Boss->self, no cast, range 25 ?-degree cone
    _Weaponskill_DeadlyImpact = 27013, // Boss->self, 4.0s cast, single-target
    _Weaponskill_DeadlyImpact1 = 27014, // Helper->location, 7.0s cast, range 10 circle
    _Weaponskill_DeadlyImpact2 = 27020, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Burst = 27021, // Helper->location, 7.5s cast, range 5 circle
    _Weaponskill_BigBurst = 27022, // Helper->location, no cast, range 40 circle
    DeadlyImpactMeteorite = 27025, // 35ED->self, 5.0s cast, range 20 circle
    _Weaponskill_DeadlyImpact4 = 27023, // Boss->self, 6.0s cast, single-target
    DeadlyImpactHelper = 27024, // Helper->location, 6.0s cast, range 20 circle
    _Weaponskill_CosmicKiss = 27027, // Helper->location, no cast, range 40 circle
    _Weaponskill_Explosion = 27026, // 35ED->self, 3.0s cast, range 6 circle
}

class Burst(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Weaponskill_Burst), 5);
class DeadlyImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact1), 10, maxCasts: 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_BlackStar1));
class DeadlyImpactProximity(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeadlyImpactMeteorite), new AOEShapeCircle(8));
class DeadlyImpactProximity2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact4), new AOEShapeCircle(10));
class MeteorExplosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Explosion), new AOEShapeCircle(6));
class Meteor(BossModule module) : Components.GenericLineOfSightAOE(module, default, 100, false)
{
    public record MeteorObj(Actor Actor, DateTime Explosion);

    private readonly List<MeteorObj> Meteors = [];

    private void Refresh()
    {
        var meteor = Meteors.FirstOrDefault();
        Modify(meteor?.Actor.Position, Module.Enemies(0x35ED).Where(m => !m.IsDead && m.ModelState.AnimState1 != 1).Select(m => (m.Position, m.HitboxRadius)), meteor?.Explosion ?? default);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EB291)
        {
            Meteors.Add(new(actor, WorldState.FutureTime(11.9f)));
            Refresh();
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (Meteors.RemoveAll(x => x.Actor == actor) > 0)
            Refresh();
    }
}

class AutoAlisaie(BossModule module) : Components.RotationModule<AlisaieAI>(module);

class TerminusLaceratorStates : StateMachineBuilder
{
    public TerminusLaceratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>()
            .ActivateOnEnter<DeadlyImpactProximity>()
            .ActivateOnEnter<DeadlyImpactProximity2>()
            .ActivateOnEnter<MeteorExplosion>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<AutoAlisaie>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10933)]
public class TerminusLacerator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-260.28f, 80.75f), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}

