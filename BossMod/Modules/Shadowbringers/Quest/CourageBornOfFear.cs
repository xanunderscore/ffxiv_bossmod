namespace BossMod.Shadowbringers.Quest.CourageBornOfFear;
public enum OID : uint
{
    Boss = 0x29E1,
    Helper = 0x233C,
}

class ImmaculateWarriorStates : StateMachineBuilder
{
    public ImmaculateWarriorStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68814, NameID = 8782)]
public class ImmaculateWarrior(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 688.25f), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
