namespace BossMod.QuestBattle.Heavensward.KeepingTheFlameAlive;

enum OID : uint
{
    HummingAtomizer = 0xF88,
    IronCell = 0xF89,
    IdentificationKey = 0x1E9A2A
}

class TriggerCutscene(WorldState ws) : QuestObjective(ws, "Trigger cutscene", [
    new Waypoint(-30.25f, 0.14f, -132.16f),
    // drop off bridge
    new Waypoint(new(-42.44f, -10.85f, -122.70f), false),
    new Waypoint(-78.03f, -10.18f, -98.29f)
])
{
    public override void OnActorCreated(Actor actor)
    {
        Completed |= actor.OID == (uint)OID.IronCell;
    }
}

// this step requires a separate waypoint because the cell's actual position vs its translated position are off by over 2y, making it impossible to attack for melee AI
class OpenCell(WorldState ws) : QuestObjective(ws, "Open cell", [new(-44.18f, -10.72f, -120.78f)], combatPausesNavigation: false)
{
    public override void OnActorKilled(Actor actor)
    {
        Completed |= actor.OID == (uint)OID.IronCell;
    }

    public override void AddAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.IronCell);
    }
}

class DestroyGenerator(WorldState ws) : QuestObjective(ws, "Destroy generator", new Waypoint(163.35f, 6.26f, -65.16f))
{
    public override void OnActorKilled(Actor actor)
    {
        Completed |= actor.OID == (uint)OID.HummingAtomizer;
    }

    public override void AddAIHints(Actor player, AIHints hints)
    {
        // budget LOS check
        if (player.PosRot.Y >= 6)
            hints.PrioritizeTargetsByOID(OID.HummingAtomizer);
    }
}
class FindKey(WorldState ws) : QuestObjective(ws, "Find key", new Waypoint(117.31f, -3.71f, 36.29f))
{
    private static readonly uint[] CrystalBraves = [0xF70, 0xF71, 0xF72];

    public override void OnActorDestroyed(Actor actor)
    {
        Completed |= actor.OID == (uint)OID.IdentificationKey;
    }

    public override void OnActorCombatChanged(Actor actor)
    {
        ShouldCancelNavigation |= CrystalBraves.Contains(actor.OID);
    }

    public override void AddAIHints(Actor player, AIHints hints)
    {
        var inCombat = false;
        foreach (var h in hints.PotentialTargets)
        {
            if (CrystalBraves.Contains(h.Actor.OID))
            {
                h.Priority = 0;
                inCombat = true;
            }
        }

        if (!inCombat)
            hints.InteractWithOID(World, OID.IdentificationKey);
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
    new OpenCell(ws),
    new DestroyGenerator(ws),
    new FindKey(ws),
    new FreeRaubahn(ws)
]);
