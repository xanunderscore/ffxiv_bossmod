namespace BossMod.QuestBattle.Stormblood.InCrimsonItBegan;

[Quest(BossModuleInfo.Maturity.WIP, 464)]
public sealed class InCrimsonItBegan(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        return [
            Combat(ws, new(-33.61f, 13.63f, 112.63f)),
            Combat(ws, new(11.37f, -1.31f, 60.38f)),
            Combat(ws, new(65.57f, 0.00f, -3.53f)),
            Combat(ws, new(76.48f, 0.31f, -73.51f))
        ];
    }

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
