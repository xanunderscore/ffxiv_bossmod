namespace BossMod.QuestBattle.ARealmReborn.TheLominsanWay;

[Quest(BossModuleInfo.Maturity.WIP, 354)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        hints.PrioritizeTargetsByOID(0x6EA, 1);
        hints.PrioritizeTargetsByOID(0x6E9, -1);
    }
}
