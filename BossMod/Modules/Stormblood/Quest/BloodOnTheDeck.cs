namespace BossMod.Stormblood.Quest;
public enum OID : uint
{
    Boss = 0x1BED,
    Helper = 0x233C,
    ShamShinobi = 0x1BE8, // R0.500, x4 (spawn during fight)
    AdjunctOstyrgreinHelper = 0x1BEB, // R0.500, x0 (spawn during fight), Helper type
    AdjunctOstyrgrein = 0x1BEA, // R0.500, x0 (spawn during fight)
    Vanara = 0x1BE9, // R3.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // ShamShinobi/Vanara/AdjunctOstyrgrein->player, no cast, single-target
    _Weaponskill_SpinningEdge = 2240, // ShamShinobi->player, no cast, single-target
    _Ability_KissOfTheWasp = 2243, // ShamShinobi->self, no cast, single-target
    _Weaponskill_GustSlash = 2242, // ShamShinobi->player, no cast, single-target
    _Weaponskill_ThrowingDagger = 2247, // ShamShinobi->player, no cast, single-target
    _Weaponskill_ScytheTail = 8407, // Vanara->self, 5.0s cast, range 4+R circle
    _Weaponskill_Butcher = 8405, // Vanara->self, 5.0s cast, range 6+R ?-degree cone
    _Weaponskill_Rip = 8406, // Vanara->self, no cast, range 6+R ?-degree cone
    _Weaponskill_FastBlade = 8718, // AdjunctOstyrgrein->player, no cast, single-target
    _Weaponskill_SavageBlade = 8719, // AdjunctOstyrgrein->player, no cast, single-target
    _Weaponskill_RageOfHalone = 8720, // AdjunctOstyrgrein->player, no cast, single-target
    _Weaponskill_TenkaGoken = 8408, // AdjunctOstyrgrein->self, 5.0s cast, range 8+R 120-degree cone
    _Weaponskill_Mudslinger = 8409, // AdjunctOstyrgrein->player, no cast, single-target
    _Weaponskill_Bombslinger = 8410, // AdjunctOstyrgrein->self, 2.3s cast, single-target
    _Weaponskill_Bombslinger1 = 8411, // AdjunctOstyrgreinHelper->location, 3.0s cast, range 6 circle
}

class ScytheTail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ScytheTail), new AOEShapeCircle(7));
class Butcher(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Butcher), new AOEShapeCone(9, 45.Degrees()));
class TenkaGoken(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TenkaGoken), new AOEShapeCone(8.5f, 60.Degrees()));
class Bombslinger(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bombslinger1), 6);

class GurumiBorlumiStates : StateMachineBuilder
{
    public GurumiBorlumiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScytheTail>()
            .ActivateOnEnter<Butcher>()
            .ActivateOnEnter<TenkaGoken>()
            .ActivateOnEnter<Bombslinger>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68098, NameID = 6289)]
public class GurumiBorlumi(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 15.8f), new ArenaBoundsRect(8, 7.5f))
{
    protected override bool CheckPull() => true;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

