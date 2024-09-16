namespace BossMod.QuestBattle.ARealmReborn.UnderneathTheSultantree;

[Quest(BossModuleInfo.Maturity.WIP, 335)]
public class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(179.20f, 9.55f, 544.76f))
            .Hints((player, hints) => {
                hints.PrioritizeTargetsByOID(0x3A5, 1);
                hints.PrioritizeTargetsByOID(0x375, 2);
            })
    ];
}
