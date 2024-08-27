namespace BossMod.Stormblood.Quest.RhalgrsBeacon;

public enum OID : uint
{
    Boss = 0x1A88,
    Helper = 0x233C,
    TerminusEst = 0x1BCA
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 1A8B/Boss->player/1F75/1F77/1F76, no cast, single-target
    _AutoAttack_Attack1 = 871, // 1A8C->1F74/1F78/1F79, no cast, single-target
    _Weaponskill_Innocence = 8343, // Boss->self, no cast, range 6+R ?-degree cone
    _Weaponskill_FastBlade = 717, // 1A8B->1F77/1F76/1F75, no cast, single-target
    _Weaponskill_TrueThrust = 722, // 1A8C->1F78/1F79/1F74, no cast, single-target
    _Ability_Quickstep = 8696, // Boss->location, no cast, ???
    _Weaponskill_TerminusEst = 8371, // Boss->self, no cast, single-target
    _Weaponskill_TheOrder = 8370, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TerminusEst1 = 8337, // 1BCA->self, no cast, range 40+R width 4 rect
    _Weaponskill_Gunblade = 8310, // Boss->player, 5.0s cast, single-target, 10y knockback
    _Weaponskill_PlanB = 8344, // Boss->self, 3.0s cast, single-target
    _Weaponskill_DiffractiveLaser = 8340, // 1BC7->self, 2.5s cast, range 18+R 60-degree cone
    _Weaponskill_ChoppingBlock = 8345, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ChoppingBlock1 = 8346, // 1A57->location, 3.0s cast, range 5 circle
}

class Gunblade(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID._Weaponskill_Gunblade), stopAtWall: true)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, 10, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

class FordolaRemLupisStates : StateMachineBuilder
{
    public FordolaRemLupisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gunblade>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 466, NameID = 5953)]
public class FordolaRemLupis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-195.33f, 150.68f), new ArenaBoundsCircle(20));

