namespace BossMod.QuestBattle.Stormblood.ItsProbablyATrap;

enum SID : uint
{
    Bind = 280,
}

[Quest(BossModuleInfo.Maturity.WIP, 237)]
public class Quest(WorldState ws) : QuestBattle(ws)
{
    private bool SmokeBomb = false;

    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        // have to walk up the stairs to trigger dialogue
        var combat1 = new QuestObjective(ws).WithConnection(new Vector3(-80.82f, -3.00f, 46.31f));

        combat1.OnDirectorUpdate = (op) =>
        {
            // this op enables the Smoke Bomb duty action
            if (op.Param1 == 14801)
            {
                combat1.Completed = true;
                SmokeBomb = true;
            }
        };

        var stealth = new QuestObjective(ws)
            .WithConnection(new Vector3(-65.45f, -3.00f, 26.26f))
            .WithConnection(new Vector3(119.86f, 12.00f, 61.87f))
            .WithConnection(new Vector3(117.25f, 12.00f, 17.32f))
            .WithConnection(new Vector3(70, -5, 17))
            .NavStrategy(NavigationStrategy.Continue);

        return [combat1, stealth];
    }

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            // attacking sekiseigumi fails the mission
            if (h.Actor.OID is 0x1A6B or 0x1A66)
                h.Priority = AIHints.Enemy.PriorityForbidFully;

        if (SmokeBomb && player.FindStatus(SID.Bind) != null)
            hints.ActionsToExecute.Push(ActionDefinitions.IDGeneralDuty1, player, ActionQueue.Priority.Medium);
    }
}
