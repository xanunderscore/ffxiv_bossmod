/*
namespace BossMod.Endwalker.Quest.WhereEverythingBegins;

public enum OID : uint
{
    Boss = 0x39B7,
    FilthyShackle = 0x39BB,
    VarshahnShield = 0x1EB762,
}

public enum AID : uint
{
    _AutoAttack_ = 31261, // 39B5->player/39BE, no cast, single-target
    _Weaponskill_ = 31241, // 39B4->player/39BE, no cast, single-target
    _Weaponskill_Recomposition = 30019, // 39B3->self, 8.0s cast, single-target
    _Weaponskill_Nox = 30020, // 39B3->self, 5.0s cast, single-target
    _Weaponskill_Nox1 = 30021, // 233C->self, 8.0s cast, range 10 circle
    _Weaponskill_VoidGravity = 30022, // 39B3->self, 5.0s cast, single-target
    _Weaponskill_VoidGravity1 = 30023, // 233C->player/39BC/39BF/39BE, 5.0s cast, range 6 circle
    BlightedSweep = 30052,
    CursedNoise = 30026,
    BlightedBuffet = 30032,
    VacuumWave = 30033,
    BlightedSwathe = 30044,
    VoidQuakeIII = 30046,
    _Weaponskill_CursedNoise = 30027, // 233C->self, no cast, range 60 circle
    _Ability_ = 30335, // 39B7->location, no cast, single-target
    _Weaponskill_DarkMist = 30034, // 39B8->self, 17.0s cast, range 8 circle
    _Spell_VoidThunderIII = 30054, // 39B8->39C1, no cast, single-target
    _Weaponskill_VoidSlash = 30048, // 39BA->self, 14.7s cast, single-target
    _Spell_VoidQuakeIII = 30045, // 39B7->self, 20.0s cast, single-target
    _Weaponskill_VoidSlash1 = 30049, // 233C->self, 15.0s cast, range 30 90-degree cone
    _Weaponskill_VoidVortex = 30024, // 39B7->self, 4.0+1.0s cast, single-target
    // stack on varshahn
    _Weaponskill_VoidVortex1 = 30025, // 233C->players, 5.0s cast, range 6 circle
    _Weaponskill_RottenRampage = 30028, // 39B7->self, 8.0+2.0s cast, single-target
    _Weaponskill_RottenRampage1 = 30031, // 233C->location, 10.0s cast, range 6 circle
    _Weaponskill_RottenRampage2 = 30056, // 233C->player/39C1/39BC/39BF/39BE, 10.0s cast, range 6 circle
    _Weaponskill_DeathStreak = 31242, // 39BA->self, 20.0s cast, range 60 circle
}

class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidVortex1), 6, minStackSize: 1);
class RottenRampageSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_RottenRampage2), 6);
class RottenRampage(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RottenRampage1), 6);
class BlightedSwathe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedSwathe), new AOEShapeCone(40, 90.Degrees()));
class Nox(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Nox1), new AOEShapeCircle(10), maxCasts: 5);
class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidGravity1), 6);
class BlightedSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedSweep), new AOEShapeCone(40, 90.Degrees()));
class BlightedBuffet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedBuffet), new AOEShapeCircle(9));
class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VacuumWave), 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(new AOEShapeDonut(13, 60), c.Position, activation: Module.CastFinishAt(c.CastInfo));
    }
}
class VoidQuakeIII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VoidQuakeIII), new AOEShapeCross(40, 5));
class DeathWall(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.CursedNoise))
{
    private DateTime? WallActivate;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (WallActivate is DateTime dt)
            yield return new AOEInstance(new AOEShapeDonut(18, 100), Arena.Center, Activation: dt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        // 5.7 seconds

        if (spell.Action == WatchedAction && NumCasts == 0)
            WallActivate = WorldState.FutureTime(5.7f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (WallActivate != null)
            hints.Add("Raidwide + poison wall spawn", false);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
        {
            WallActivate = null;
            Arena.Bounds = new ArenaBoundsCircle(18);
        }
    }
}

class Shield(BossModule module) : BossComponent(module)
{
    private const float ShieldRadius = 5;
    private Actor? ShieldObj => Module.Enemies(OID.VarshahnShield).FirstOrDefault();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ShieldObj != null)
            hints.AddForbiddenZone(new AOEShapeDonut(ShieldRadius, 30), ShieldObj.Position);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ShieldObj is Actor obj && (actor.Position - obj.Position).Length() > ShieldRadius)
            hints.Add("Go to safe zone!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (ShieldObj is Actor obj)
            Arena.AddCircleFilled(obj.Position, ShieldRadius, ArenaColor.SafeFromAOE);
    }
}

public class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 874;
        TrivialPhase()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<BlightedBuffet>()
            .ActivateOnEnter<BlightedSwathe>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<VoidQuakeIII>()
            .ActivateOnEnter<DeathWall>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampageSpread>()
            .ActivateOnEnter<Shield>()
            .ActivateOnEnter<VoidVortex>()
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 874, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.OID == (uint)OID.FilthyShackle ? 1 : 0;
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }
}
*/
