namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.BLM;

[Quest(BossModuleInfo.Maturity.Contributed, 366)]
internal class TheVoidgateBreathesGloomy(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeTargetsByOID(0x6B1);
}

