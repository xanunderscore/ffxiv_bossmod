namespace BossMod.QuestBattle.ARealmReborn.TheSpiritIsWilling;

[Quest(BossModuleInfo.Maturity.WIP, 319)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                hints.Center = new(-240, -6);
                hints.Bounds = new ArenaBoundsSquare(15);

                hints.AddForbiddenZone(new AOEShapeRect(4, 3, 4), new(-229, -2));

                foreach(var h in hints.PotentialTargets)
                    h.Priority = 0;
            })
        ];
}
