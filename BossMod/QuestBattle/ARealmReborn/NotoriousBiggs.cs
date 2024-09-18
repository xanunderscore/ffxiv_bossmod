namespace BossMod.QuestBattle.ARealmReborn;

[Quest(BossModuleInfo.Maturity.Contributed, 351)]
internal class NotoriousBiggs(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        hints.PrioritizeAll();
    }
}

