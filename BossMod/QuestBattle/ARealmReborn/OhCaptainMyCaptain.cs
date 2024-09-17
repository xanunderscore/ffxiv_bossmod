namespace BossMod.QuestBattle.ARealmReborn.OhCaptainMyCaptain;

[Quest(BossModuleInfo.Maturity.WIP, 337)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeTargetsByOID(0x3BC, 2);
}
