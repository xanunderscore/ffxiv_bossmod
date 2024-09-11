/*
using RID = BossMod.Roleplay.AID;

namespace BossMod.Endwalker.Quest.HeavensBurn;

public enum OID : uint
{
    Boss = 0x35E9
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // 375C/3759/375A/375B/35E0/35E1->35F2, no cast, single-target
    _AutoAttack_ = 26994, // Boss->35F5, no cast, single-target
    _Weaponskill_GriefOfParting = 26996, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_DeadlyTentacles = 26997, // Boss->35F5, 5.0s cast, single-target
    _Weaponskill_DeadlyTentacles1 = 26998, // Boss->35F5, no cast, single-target
    _Weaponskill_TentacleWhipRFirst = 27001, // Boss->self, 5.0s cast, range 60 180-degree cone
    _Weaponskill_TentacleWhipLSecond = 27003, // 233C->self, 7.0s cast, range 60 180-degree cone
    _Weaponskill_TentacleWhip2 = 27002, // Boss->self, no cast, single-target
    _Ability_Shout = 27000, // Boss->self, no cast, single-target
    _Weaponskill_SelfDestruct = 26991, // 35E5->self, no cast, range 6 circle
    _Weaponskill_SelfDestruct1 = 26992, // 35EB->self, no cast, range 6 circle
    _Weaponskill_DeadlyCharge = 26995, // Boss->location, 5.0s cast, width 10 rect charge
    _Weaponskill_TentacleWhipLFirst = 27004, // Boss->self, 5.0s cast, range 60 180-degree cone
    _Weaponskill_TentacleWhipRSecond = 27006, // 233C->self, 7.0s cast, range 60 180-degree cone
    _Weaponskill_TentacleWhip5 = 27005, // Boss->self, no cast, single-target
    _Weaponskill_Petrifaction = 26999, // Boss->self, 4.0s cast, range 60 circle
    _Weaponskill_DeadlyImpact = 27014, // 233C->location, 7.0s cast, range 10 circle
    _Weaponskill_Burst = 27021, // 233C->location, 7.5s cast, range 5 circle
    _Weaponskill_TheBlackDeath = 27010, // 35EC->self, no cast, range 25 ?-degree cone
    _Weaponskill_DeadlyImpact1 = 27025, // 35ED->self, 5.0s cast, range 20 circle
    _Weaponskill_DeadlyImpact2 = 27024, // 233C->location, 6.0s cast, range 20 circle
    _Weaponskill_CosmicKiss = 27027, // 233C->location, no cast, range 40 circle
    _Weaponskill_Explosion = 27026, // 35ED->self, 3.0s cast, range 6 circle
    _Weaponskill_ForcefulImpact = 26239, // 35EF->location, 5.0s cast, range 7 circle
    _Weaponskill_ForcefulImpactKB = 27030, // 233C->self, 5.6s cast, range 20 circle
    _Weaponskill_WaveOfLoathing = 27032, // 35EF->self, 5.0s cast, range 40 circle
    _Weaponskill_ForceOfLoathing = 27031, // 35EF->self, no cast, range 10 ?-degree cone
    _Weaponskill_ = 27033, // 35EF->location, no cast, single-target
    _Weaponskill_MutableLaws = 27039, // 35EF->self, 4.0s cast, single-target
    _Weaponskill_MutableLaws1 = 27041, // 233C->location, 10.0s cast, range 6 circle
    _Weaponskill_MutableLaws2 = 27040, // 233C->location, 10.0s cast, range 6 circle
    _Weaponskill_AccursedTongue = 27037, // 35EF->self, 4.0s cast, single-target
    _Weaponskill_AccursedTongue1 = 27038, // 233C->players/35F5/35FA/35F9/35F7, 5.0s cast, range 6 circle
    _Weaponskill_Thundercall = 27034, // 35EF->self, 4.0s cast, single-target
    _Weaponskill_Shock = 27035, // 35F0->self, 5.0s cast, range 10 circle
    _Weaponskill_Depress = 27036, // 35EF->35FA, 5.0s cast, range 7 circle
    _Weaponskill_ForcefulImpact2 = 27029, // 35EF->location, 5.0s cast, range 7 circle
}

class DeadlyImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact), 10, maxCasts: 6);
class Petrifaction(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_Petrifaction));
class GriefOfParting(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_GriefOfParting));
class DeadlyTentacles(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyTentacles));
class TentacleWhipR1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TentacleWhipRFirst), new AOEShapeCone(60, 90.Degrees()));
class TentacleWhipR2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TentacleWhipLSecond), new AOEShapeCone(60, 90.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<TentacleWhipR1>()?.Casters.Count > 0)
            yield break;
        else
            foreach (var h in base.ActiveAOEs(slot, actor))
                yield return h;
    }
}
class TentacleWhipL1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TentacleWhipLFirst), new AOEShapeCone(60, 90.Degrees()));
class TentacleWhipL2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TentacleWhipRSecond), new AOEShapeCone(60, 90.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<TentacleWhipL1>()?.Casters.Count > 0)
            yield break;
        else
            foreach (var h in base.ActiveAOEs(slot, actor))
                yield return h;
    }
}

public class AlphinaudAI(BossModule module) : Components.RoleplayModule(module)
{
    private Actor? Estinien => Module.Enemies(0x35F5).FirstOrDefault();
    private readonly List<Actor> Grenades = [];

    public override void Execute(Actor? primaryTarget)
    {
        Hints.InteractWithTarget = WorldState.Actors.FirstOrDefault(x => x.OID is 0x1EB44F or 0x1EB2FB && x.IsTargetable);

        foreach (var e in Module.Enemies(0x35F1))
            if (e.HPMP.CurHP < e.HPMP.MaxHP && e.IsTargetable)
                UseAction(RID.Diagnosis, e);

        if (Estinien?.IsTargetable ?? false)
        {
            UseAction(RID.LeveilleurDruochole, Estinien, -100);

            var minHP = Grenades.Count > 0 ? Estinien.HPMP.MaxHP * 0.8f : Estinien.HPMP.MaxHP * 0.5f;

            if (Estinien.HPMP.CurHP < minHP)
                UseAction(RID.Diagnosis, Estinien);
        }

        if (!(primaryTarget?.IsAlly ?? true))
        {
            if (primaryTarget?.OID == (uint)OID.Boss && StatusDetails(primaryTarget, Roleplay.SID.LeveilleurDosisIII, Player.InstanceID).Left < 3)
                UseAction(RID.LeveilleurDosisIII, primaryTarget);

            UseAction(RID.DosisIII, primaryTarget);
            UseAction(RID.LeveilleurToxikon, primaryTarget, -100);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 17)
            Grenades.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 17)
            Grenades.Remove(source);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Grenades.Any(x => !x.IsDead) && Estinien is Actor e)
            hints.AddForbiddenZone(new AOEShapeCircle(10), e.Position);
        foreach (var f in hints.PotentialTargets)
            f.Priority = 0;
    }
}

public class AlisaieAI(BossModule module) : Components.RoleplayModule(module)
{
    public const ushort StatusParam = 192;

    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (WorldState.Party.LimitBreakCur == 10000)
            UseAction(RID.VermilionPledge, primaryTarget, 100);

        switch (ComboAction)
        {
            case RID.EWVerfire:
                UseAction(RID.EWVeraero, primaryTarget);
                break;
            case RID.EWVeraero:
                UseAction(RID.EWVerstone, primaryTarget);
                break;
            case RID.EWVerstone:
                UseAction(RID.EWVerthunder, primaryTarget);
                break;
            case RID.EWVerthunder:
                UseAction(RID.EWVerflare, primaryTarget);
                break;
            case RID.EWCorpsACorps:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EWEnchantedRiposte, primaryTarget);
                break;
            case RID.EWEnchantedRiposte:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EWEnchantedZwerchhau, primaryTarget);
                break;
            case RID.EWEnchantedZwerchhau:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EWEnchantedRedoublement, primaryTarget);
                break;
            case RID.EWEnchantedRedoublement:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EWEngagement, primaryTarget);
                break;
            case RID.EWEngagement:
                UseAction(RID.EWVerholy, primaryTarget);
                break;
            case RID.EWVerholy:
                UseAction(RID.EWScorch, primaryTarget);
                break;
            default:
                UseAction(RID.EWCorpsACorps, primaryTarget);
                UseAction(RID.EWVerfire, primaryTarget);
                break;
        }

        UseAction(RID.EWContreSixte, primaryTarget);
        UseAction(RID.EWEmbolden, Player, -100);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var f in hints.PotentialTargets)
            f.Priority = 0;
    }
}

class Burst(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Weaponskill_Burst), 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var graha = Module.Enemies(0x35F6).FirstOrDefault();
        if (graha == null)
            return;

        var nextTower = Towers.FirstOrDefault(t => !t.IsInside(graha), new(default, 0));
        if (nextTower.Radius != 0)
            hints.AddForbiddenZone(new AOEShapeDonut(5, 100), nextTower.Position);
    }
}

class DeadlyImpactProximity(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact1), new AOEShapeCircle(8));
class DeadlyImpactProximity2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyImpact2), new AOEShapeCircle(10));
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

class ForcefulImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpact), 7);
class ForcefulImpactKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpactKB), 10, stopAtWall: true);
class MutableLaws1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MutableLaws1), 15);
class MutableLaws2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MutableLaws2), 6);
class AccursedTongue(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_AccursedTongue1), 6);
class ForcefulImpact2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ForcefulImpact2), 7);
class Shock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Shock), new AOEShapeCircle(10), maxCasts: 6);
class Depress(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Depress), 7);

public class HBStates : StateMachineBuilder
{
    public HBStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 804;
        ushort GetRPParam() => (ushort)((Module.Raid.Player()?.FindStatus(Roleplay.SID.RolePlaying)?.Extra ?? 0) & 0xFF);

        bool P1End() => GetRPParam() == AlisaieAI.StatusParam || P2End();
        bool P2End() => GetRPParam() == 0 || DutyEnd();

        TrivialPhase()
            .ActivateOnEnter<AlphinaudAI>()
            .ActivateOnEnter<GriefOfParting>()
            .ActivateOnEnter<DeadlyTentacles>()
            .ActivateOnEnter<TentacleWhipL1>()
            .ActivateOnEnter<TentacleWhipL2>()
            .ActivateOnEnter<TentacleWhipR1>()
            .ActivateOnEnter<TentacleWhipR2>()
            .ActivateOnEnter<Petrifaction>()
            .Raw.Update = P1End;
        TrivialPhase(1)
            .ActivateOnEnter<AlisaieAI>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeadlyImpactProximity>()
            .ActivateOnEnter<DeadlyImpactProximity2>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MeteorExplosion>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(-260.28f, 80.75f);
            })
            .Raw.Update = P2End;
        TrivialPhase(2)
            .ActivateOnEnter<Enemies>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ForcefulImpactKB>()
            .ActivateOnEnter<MutableLaws1>()
            .ActivateOnEnter<MutableLaws2>()
            .ActivateOnEnter<AccursedTongue>()
            .ActivateOnEnter<ForcefulImpact2>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Depress>()
            .Raw.Update = DutyEnd;
    }
}

class Enemies(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class HB(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300.75f, 151.5f), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => true;
}
*/
