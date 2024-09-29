namespace BossMod.QuestBattle.Stormblood.SideQuests;

[Quest(BossModuleInfo.Maturity.Contributed, 275)]
internal class InterdimensionalRift(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(0.11f, -272.00f, 432.78f))
    ];
}
