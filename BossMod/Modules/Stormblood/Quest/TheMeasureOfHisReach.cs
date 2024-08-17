namespace BossMod.Modules.Stormblood.Quest.TheMeasureOfHisReach;

public enum OID : uint
{
    Boss = 0x1C48,
    Helper = 0x233C,
    Whitefang = 0x1C5A
}

public enum AID : uint
{
    HowlingIcewind = 8397, // 1C4F->self, 2.5s cast, range 44+R width 4 rect
    Dragonspirit = 8450, // 1C5A/1C5B->self, 3.0s cast, range 6+R circle
    HowlingMoonlight = 8398, // 1C59->self, 7.0s cast, range 22+R circle
    HowlingBloomshower = 8399, // 1C4F->self, 2.5s cast, range 8+R ?-degree cone
}

class Moonlight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HowlingMoonlight), new AOEShapeCircle(10));
class Icewind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HowlingIcewind), new AOEShapeRect(44, 2));
class Dragonspirit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Dragonspirit), new AOEShapeCircle(7.5f));
class Bloomshower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HowlingBloomshower), new AOEShapeDonutSector(4, 8, 45.Degrees()));
class Adds(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.Whitefang).Where(e => !e.IsDead), ArenaColor.Object, true);
    }
}

class HakuroWhitefangStates : StateMachineBuilder
{
    public HakuroWhitefangStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Icewind>()
            .ActivateOnEnter<Moonlight>()
            .ActivateOnEnter<Dragonspirit>()
            .ActivateOnEnter<Bloomshower>()
            .ActivateOnEnter<Adds>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 468, NameID = 5975)]
public class HakuroWhitefang(WorldState ws, Actor primary) : BossModule(ws, primary, new(504, -133), new ArenaBoundsCircle(20));

