namespace BossMod.Endwalker.Quest.WhereEverythingBegins.P1;
public enum OID : uint
{
    Boss = 0x39B3,
    Helper = 0x233C,
    _Gen_PlunderedButler = 0x39B5, // R1.300, x2 (spawn during fight)
    _Gen_PlunderedSteward = 0x39B6, // R1.950, x2 (spawn during fight)
    _Gen_PlunderedPawn = 0x39B4, // R1.800, x2 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_ = 31241, // _Gen_PlunderedPawn->player/39BE, no cast, single-target
    _AutoAttack_2 = 31240, // _Gen_PlunderedSteward->player/39BF/39BC/39BE, no cast, single-target
    _AutoAttack_1 = 31261, // _Gen_PlunderedButler->player/39BE/39BC, no cast, single-target
    _AutoAttack_ = 31239, // _Gen_PlunderedPawn->39BE, no cast, single-target
    _Weaponskill_Recomposition = 30019, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Nox = 30020, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Nox1 = 30021, // Helper->self, 8.0s cast, range 10 circle
    _Weaponskill_VoidVortex = 30025, // Helper->39BE, 5.0s cast, range 6 circle
    _Weaponskill_VoidVortex1 = 30024, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_VoidGravity = 30022, // Boss->self, 5.0s cast, single-target
    _Weaponskill_VoidGravity1 = 30023, // Helper->player/39BC/39BF/39BE, 5.0s cast, range 6 circle
}

class Nox(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Nox1), new AOEShapeCircle(10), maxCasts: 5);
class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidGravity1), 6);
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidVortex), 6);

class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<VoidVortex>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70130, NameID = 11407)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

