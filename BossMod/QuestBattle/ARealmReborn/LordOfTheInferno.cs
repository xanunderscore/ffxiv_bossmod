namespace BossMod.QuestBattle.ARealmReborn.LordOfTheInferno;

[Quest(BossModuleInfo.Maturity.WIP, 339)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x3C7 ? 0 : 1;
    }
}
