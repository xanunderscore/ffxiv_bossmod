namespace BossMod.QuestBattle.Stormblood.RhalgrsBeacon;

[Quest(BossModuleInfo.Maturity.WIP, 466)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        Combat(ws, new Vector3(-184.57f, 40.62f, -10.64f)),
        Combat(ws, new Vector3(-166.68f, 39.10f, 163.64f))
    ];
}
