namespace BossMod.QuestBattle.Heavensward.TheAntitower;

// [Quest(BossModuleInfo.Maturity.WIP, 141)]
// public class TheAntitower(WorldState ws) : QuestBattle(ws, [new Pad1(ws), new Pad2(ws), new Pad3(ws), new Boss1(ws), new Pad4(ws), new Pack1(ws)]);

[Quest(BossModuleInfo.Maturity.WIP, 182)]
public class Xelphatol(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        var p1 = new Vector3(-120.55f, -9.75f, 160.65f);
        return [new QuestObjective(ws).WithConnection(p1).NavStrategy(NavigationStrategy.Continue)];
    }
}
