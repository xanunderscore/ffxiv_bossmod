namespace BossMod.QuestBattle.Heavensward;

class Mills(WorldState ws) : QuestObjective(ws, "Gorgagne Mills", [
    new Waypoint(455.42f, 164.31f, -542.78f),
    // basement
    new Waypoint(456.10f, 157.41f, -554.90f)
])
{
    public override bool PauseNavigationDuringCombat() => false;

    public override void AddAIHints(Actor player, AIHints hints)
    {
        hints.InteractWithOID(World, 0x1E9B5A);
    }
}

[Quest(BossModuleInfo.Maturity.WIP, 416)]
public sealed class AtTheEndOfOurHope(WorldState ws) : QuestBattle(ws, [new Mills(ws)]);
