namespace BossMod.Modules.Dawntrail.Savage.M3SBruteBomber;

public enum OID : uint
{
    Boss = 0x42C6, // R5.016, x?
    _Gen_BruteBomber = 0x233C, // R0.500, x?, 523 type
    _Gen_ = 0x42C9, // R1.000, x?
    _Gen_LitFuse = 0x42C7, // R1.200, x?
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    _Gen_Refbot = 0x42C8, // R3.360, x?
    _Gen_1 = 0x43F5, // R0.100, x?
    _Gen_BruteDistortion = 0x42CB, // R5.016, x?
}

public enum SID : uint
{
    ShortFuse = 4024,
    LongFuse = 4025,
    ShortFuseExplode = 4026,
    LongFuseExplode = 4027
}

public enum AID : uint
{
    //_AutoAttack_ = 39554, // Boss->player, no cast, single-target
    //_Weaponskill_BrutalImpact = 37925, // Boss->self, 5.0s cast, range 60 circle
    //_Weaponskill_BrutalImpact = 37926, // Boss->self, no cast, range 60 circle
    //_Weaponskill_KnuckleSandwich = 37923, // Boss->players, 5.0s cast, range 6 circle
    //_Weaponskill_KnuckleSandwich = 37924, // Boss->players, no cast, range 6 circle
    //_Ability_ = 37876, // Boss->location, no cast, single-target
    //_Weaponskill_OctupleLariat = 37848, // Boss->self, 6.2+0.8s cast, single-target
    //_Weaponskill_BlazingLariat = 37852, // _Gen_BruteBomber->self, no cast, range 40 ?-degree cone
    //_Weaponskill_OctupleLariat = 37864, // _Gen_BruteBomber->self, 7.0s cast, range 10 circle
    //_Weaponskill_OctoboomDive = 37854, // Boss->location, 7.2+0.8s cast, single-target
    //_Ability_OctoboomDive = 37868, // _Gen_BruteBomber->self, 8.0s cast, range 60 circle
    //_Weaponskill_Diveboom = 37858, // _Gen_BruteBomber->players, no cast, range 5 circle
    //_Ability_BarbarousBarrage = 37883, // Boss->self, 4.0s cast, single-target
    //_Spell_Explosion = 38542, // _Gen_BruteBomber->self, no cast, range 4 circle
    //_Spell_UnmitigatedExplosion = 37885, // _Gen_BruteBomber->self, no cast, range 60 circle
    //_Spell_Explosion = 37884, // _Gen_BruteBomber->self, no cast, range 4 circle
    //_Spell_Explosion = 38543, // _Gen_BruteBomber->self, no cast, range 4 circle
    //_Ability_MurderousMist = 37886, // Boss->self, 6.0s cast, range 40 ?-degree cone
    //_Weaponskill_QuadrupleLariat = 37850, // Boss->self, 6.2+0.8s cast, single-target
    //_Weaponskill_QuadrupleLariat = 37866, // _Gen_BruteBomber->self, 7.0s cast, range 10 circle
    //_Weaponskill_BlazingLariat = 37853, // _Gen_BruteBomber->self, no cast, range 40 ?-degree cone
    //_Weaponskill_QuadroboomDive = 37856, // Boss->location, 7.2+0.8s cast, single-target
    //_Ability_QuadroboomDive = 37877, // _Gen_BruteBomber->self, 8.0s cast, range 60 circle
    //_Weaponskill_Diveboom = 37859, // _Gen_BruteBomber->players, no cast, range 5 circle
    //_Ability_DopingDraught = 37895, // Boss->self, 4.0s cast, single-target
    //_Weaponskill_OctupleLariat = 37849, // Boss->self, 6.2+0.8s cast, single-target
    //_Weaponskill_OctupleLariat = 37865, // _Gen_BruteBomber->self, 7.0s cast, range ?-60 donut
    //_Weaponskill_OctoboomDive = 37855, // Boss->location, 7.2+0.8s cast, single-target
    //_Ability_OctoboomDive = 37869, // _Gen_BruteBomber->self, 8.0s cast, range 60 circle
    //_Ability_TagTeam = 37863, // Boss->self, 4.0s cast, single-target
    //_Ability_ChainDeathmatch = 37861, // Boss->self, 7.0s cast, single-target
    //_Ability_ChainDeathmatch = 37862, // _Gen_BruteDistortion->self, 7.0s cast, single-target
    //_Ability_ = 39809, // _Gen_->self, 4.9s cast, single-target
    //_Ability_ = 39741, // _Gen_->self, 4.9s cast, single-target
    //_Weaponskill_LariatCombo = 39724, // _Gen_BruteDistortion->location, 4.9+1.2s cast, single-target
    //_Weaponskill_LariatCombo = 39726, // _Gen_BruteDistortion->location, 4.9+1.2s cast, single-target
    //_Weaponskill_LariatCombo = 39732, // _Gen_BruteBomber->self, 6.1s cast, range 70 width 34 rect
    //_Weaponskill_LariatCombo = 39733, // _Gen_BruteBomber->self, 6.1s cast, range 70 width 34 rect
    //_Ability_PunishingChain = 39886, // _Gen_BruteBomber->player, no cast, single-target
    //_Weaponskill_LariatCombo = 39728, // _Gen_BruteDistortion->location, 3.0s cast, single-target
    //_Weaponskill_LariatCombo = 39730, // _Gen_BruteDistortion->location, 3.0s cast, single-target
    //_Weaponskill_LariatCombo = 39734, // _Gen_BruteBomber->self, 3.1s cast, range 50 width 34 rect
    //_Weaponskill_LariatCombo = 39735, // _Gen_BruteBomber->self, 3.1s cast, range 50 width 34 rect
    //_Ability_ = 39742, // _Gen_->self, no cast, single-target
    //_Ability_FinalFusedown = 37894, // Boss->self, 4.0s cast, single-target
    _Spell_SelfDestruct1 = 37889, // _Gen_LitFuse->self, 5.0s cast, range 8 circle
    _Spell_Explosion1 = 37892, // _Gen_BruteBomber->location, no cast, range 6 circle
    _Spell_SelfDestruct2 = 37890, // _Gen_LitFuse->self, 10.0s cast, range 8 circle
    _Spell_Explosion2 = 37893, // _Gen_BruteBomber->location, no cast, range 6 circle
}

class BruteBomberStates : StateMachineBuilder
{
    public BruteBomberStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FinalFusedown>()
            .ActivateOnEnter<Bombs>();
    }
}

class FinalFusedown(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> ShortFuses => WorldState.Party.WithoutSlot().Where(IsShortFuse);
    private IEnumerable<Actor> LongFuses => WorldState.Party.WithoutSlot().Where(IsLongFuse);

    private bool IsShortFuse(Actor a) => a.Statuses.Any(s => (SID)s.ID is SID.ShortFuse or SID.ShortFuseExplode);
    private bool IsLongFuse(Actor a) => a.Statuses.Any(s => (SID)s.ID is SID.LongFuse or SID.LongFuseExplode);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var shortAny = false;
        foreach (var s in ShortFuses)
        {
            shortAny = true;
            Arena.AddCircle(s.Position, 6, ArenaColor.Danger);
        }

        if (!shortAny)
            foreach (var s in LongFuses)
                Arena.AddCircle(s.Position, 6, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsShortFuse(actor))
            hints.Add("Short fuse", false);
        if (IsLongFuse(actor))
            hints.Add("Long fuse", false);
    }
}

class Bombs(BossModule module) : Components.GenericAOEs(module)
{
    private IEnumerable<AOEInstance> GetActiveAOEs(int slot, Actor actor)
    {
        foreach (var enemy in Module.Enemies(OID._Gen_LitFuse))
        {
            if (enemy.CastInfo?.Action.ID is (uint)AID._Spell_SelfDestruct1 or (uint)AID._Spell_SelfDestruct2)
                yield return new AOEInstance(new AOEShapeCircle(8), enemy.Position, default, Module.CastFinishAt(enemy.CastInfo));
            else if (enemy.FindStatus(4015) != null)
                yield return new AOEInstance(new AOEShapeCircle(8), enemy.Position, default, WorldState.CurrentTime.AddSeconds(10));
            else if (enemy.FindStatus(4016) != null)
                yield return new AOEInstance(new AOEShapeCircle(8), enemy.Position, default, WorldState.CurrentTime.AddSeconds(20));
        }
    }
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => GetActiveAOEs(slot, actor).OrderBy(i => i.Activation).Take(4);
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 990, NameID = 13356)]
public class BruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20)) { }

