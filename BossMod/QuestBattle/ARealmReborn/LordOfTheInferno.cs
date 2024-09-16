namespace BossMod.QuestBattle.ARealmReborn.LordOfTheInferno;

[Quest(BossModuleInfo.Maturity.WIP, 339)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                foreach(var h in hints.PotentialTargets)
                    h.Priority = h.Actor.OID == 0x3C7 ? 0 : 1;
            })
        ];
}
