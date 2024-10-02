﻿namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SCH;

[Quest(BossModuleInfo.Maturity.Contributed, 404)]
internal class Quarantine(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => hints.PrioritizeTargetsByOID(0x1231, 5);
}

