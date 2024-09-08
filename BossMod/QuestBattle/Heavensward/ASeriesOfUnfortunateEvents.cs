namespace BossMod.QuestBattle.Heavensward;

class Free(WorldState ws) : QuestObjective(ws, "Free Emmanellain", [
    new Waypoint(657.58f, -65.54f, -123.75f)
])
{
    public override void AddAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
            hints.InteractWithOID(World, 0x1E9ACE);
    }

    public override void OnActorModelStateChanged(Actor actor)
    {
        Completed |= actor.OID == 0x1003 && actor.ModelState.ModelState == 0;
    }
}

class Escort(WorldState ws) : QuestObjective(ws, "Escort Emmanellain to safety", [])
{
    public override void AddAIHints(Actor player, AIHints hints)
    {
        hints.Bounds = QuestBattle.OverworldBounds;

        var emmanellain = World.Actors.FirstOrDefault(i => i.OID == 0x1003);
        if (emmanellain != null)
        {
            foreach (var h in hints.PotentialTargets)
                if (h.Actor.TargetID == emmanellain.InstanceID)
                    h.Priority = 0;

            if (!player.InCombat && player.DistanceToHitbox(emmanellain) > 50)
                hints.ForcedMovement = player.DirectionTo(emmanellain).ToVec3();
        }
    }
}

[Quest(BossModuleInfo.Maturity.WIP, 395)]
public sealed class Emmanellain(WorldState ws) : QuestBattle(ws, [new Free(ws), new Escort(ws)]);
