namespace BossMod.QuestBattle.ARealmReborn.OhCaptainMyCaptain;

[Quest(BossModuleInfo.Maturity.WIP, 337)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                hints.PrioritizeTargetsByOID(0x3BC, 2);
            })
    ];
}
