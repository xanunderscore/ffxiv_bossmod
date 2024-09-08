namespace BossMod.QuestBattle.Heavensward;

class TriggerCutscene(WorldState ws) : QuestObjective(ws, "Trigger cutscene", [
    new Waypoint(-30.25f, 0.14f, -132.16f),
    // drop off bridge
    new Waypoint(new(-42.44f, -10.85f, -122.70f), false),
    new Waypoint(-78.03f, -10.18f, -98.29f)
])
{
    public static uint IronCell = 0xF89;

    public override void OnActorCreated(Actor actor)
    {
        Completed |= actor.OID == IronCell;
    }
}
class DestroyGenerator(WorldState ws) : QuestObjective(ws, "Destroy generator", new Waypoint(163.35f, 6.26f, -65.16f))
{
    public static uint HummingAtomizer = 0xF88;

    public override void OnActorKilled(Actor actor)
    {
        Completed |= actor.OID == HummingAtomizer;
    }

    public override void AddAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.OID == TriggerCutscene.IronCell)
            {
                h.Priority = 0;
                hints.RecommendedRangeToTarget = 2.5f;
            }

            if (h.Actor.OID == HummingAtomizer && player.PosRot.Y >= 6)
                h.Priority = 0;
        }
    }
}
class FindKey(WorldState ws) : QuestObjective(ws, "Find key", new Waypoint(105.59f, -3.35f, 57.61f))
{
    public static uint IdentificationKey = 0x1E9A2A;

    public override void OnActorDestroyed(Actor actor)
    {
        Completed |= actor.OID == IdentificationKey;
    }

    public override void AddAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID([0xF70, 0xF71, 0xF72]);

        if (!player.InCombat)
            hints.InteractWithOID(World, IdentificationKey);
    }
}
class FreeRaubahn(WorldState ws) : QuestObjective(ws, "Free Raubahn", [
    new Waypoint(-30.25f, 0.14f, -132.16f),
    new Waypoint(new(-42.44f, -10.85f, -122.70f), false),
    new Waypoint(-86.78f, -10.18f, -96.53f)
])
{
    public override void AddAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
            hints.InteractWithOID(World, 0x1E9D3C);
    }
}

[Quest(BossModuleInfo.Maturity.WIP, 400)]
public class KeepingTheFlameAlive(WorldState ws) : QuestBattle(ws, [
    new TriggerCutscene(ws),
    // new OpenCell(ws),
    new DestroyGenerator(ws),
    new FindKey(ws),
    new FreeRaubahn(ws)
]);
