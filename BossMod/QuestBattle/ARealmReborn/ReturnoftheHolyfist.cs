namespace BossMod.QuestBattle.ARealmReborn;

[Quest(BossModuleInfo.Maturity.WIP, 323)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeAll();
}
