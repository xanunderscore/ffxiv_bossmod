namespace BossMod.QuestBattle.ARealmReborn;

[Quest(BossModuleInfo.Maturity.Contributed, 358)]
internal class EveryLittleThingSheDoesIsMagitek(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeAll();
}

