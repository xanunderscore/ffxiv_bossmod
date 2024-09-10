namespace BossMod.QuestBattle.Stormblood.TheResonant;

[Quest(BossModuleInfo.Maturity.WIP, 269)]
public class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(676.12f, 70.00f, 512.76f))
            .WithConnection(new Vector3(623.58f, 70.00f, 529.54f))
    ];
}
