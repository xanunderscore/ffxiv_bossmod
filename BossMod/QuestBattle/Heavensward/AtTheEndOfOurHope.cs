namespace BossMod.QuestBattle.Heavensward;

[Quest(BossModuleInfo.Maturity.WIP, 416)]
public sealed class AtTheEndOfOurHope(WorldState ws) : SimpleQuestBattle(ws, Steps)
{
    public static readonly List<QuestNavigation> Steps = [
        new("Gorgagne Mills", false,
            // inside doorway
            new Vector3(455.42f, 164.31f, -542.78f),
            // basement
            new Vector3(456.10f, 157.41f, -554.90f)
        )
    ];

    public override void CalculateAIHints(Actor player, AIHints hints)
    {
        hints.InteractWithTarget = World.Actors.FirstOrDefault(x => x.OID == 0x1E9B5A && x.IsTargetable);
    }
}
