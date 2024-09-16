namespace BossMod.QuestBattle.Dawntrail.TheProtectorAndTheDestroyer;

[Quest(BossModuleInfo.Maturity.WIP, 998)]
public class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Waypoint(new Vector3(-0.57f, -6.05f, 209.93f), false))
            .WithConnection(new Waypoint(new Vector3(0.01f, 0.00f, 114.44f), false))
            .WithConnection(new Vector3(7.51f, 7.89f, 21.07f))
    ];
}
