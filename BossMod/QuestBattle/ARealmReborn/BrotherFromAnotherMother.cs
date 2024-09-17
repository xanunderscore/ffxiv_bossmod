namespace BossMod.QuestBattle.ARealmReborn.BrotherFromAnotherMother;

[Quest(BossModuleInfo.Maturity.WIP, 363)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeTargetsByOID(0x815, 0);
}

