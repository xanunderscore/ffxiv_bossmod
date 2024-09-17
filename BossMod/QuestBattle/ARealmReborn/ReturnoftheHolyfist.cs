namespace BossMod.QuestBattle.ARealmReborn.ReturnOfTheHolyfist;

[Quest(BossModuleInfo.Maturity.WIP, 323)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeAll();
}
