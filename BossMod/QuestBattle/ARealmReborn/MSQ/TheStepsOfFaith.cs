namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[Quest(BossModuleInfo.Maturity.Contributed, 885)]
internal class TheStepsOfFaith(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(0, 0, 260))
    ];
}
