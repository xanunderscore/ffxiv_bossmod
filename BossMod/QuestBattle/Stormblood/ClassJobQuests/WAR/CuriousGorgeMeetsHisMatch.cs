namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.WAR;

[Quest(BossModuleInfo.Maturity.Contributed, 260)]
internal class CuriousGorgeMeetsHisMatch(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).Hints((player, hints) => {
            hints.Center = new(145, -15);
            hints.Bounds = new ArenaBoundsSquare(15.5f);
        })
    ];
}

