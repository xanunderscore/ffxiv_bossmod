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
        Service.Log($"calculating auto hints, current center is {hints.Center}");

        var emmanellain = World.Actors.FirstOrDefault(i => i.OID == 0x1003);
        if (emmanellain != null)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(15, 100), emmanellain.Position);
            foreach (var h in hints.PotentialTargets)
                if (h.Actor.TargetID == emmanellain.InstanceID)
                    h.Priority = 0;
        }
    }
}

[Quest(BossModuleInfo.Maturity.WIP, 395)]
public sealed class Emmanellain(WorldState ws) : QuestBattle(ws, [new Free(ws), new Escort(ws)]);

/*
[Quest(BossModuleInfo.Maturity.WIP, 395)]
internal class ASeriesOfUnfortunateEvents(WorldState ws) : SimpleQuestBattle(ws, Navigation)
{
    public static readonly List<QuestObjective> Navigation = [
        new("Free Emmanellain", new Waypoint(657.58f, -65.54f, -123.75f), false)
    ];

    public override void CalculateAIHints(Actor player, AIHints hints)
    {
        switch (CurrentStep)
        {
            case 0:
                if (!player.InCombat)
                    hints.InteractWithTarget = World.Actors.FirstOrDefault(x => x.OID == 0x1E9ACE && x.IsTargetable);
                break;
            case 1:
                var idiot = World.Actors.FirstOrDefault(i => i.OID == 0x1003);
                if (idiot != null)
                {
                    hints.AddForbiddenZone(new AOEShapeDonut(15, 100), idiot.Position, activation: World.FutureTime(10));
                    foreach (var h in hints.PotentialTargets)
                        if (h.Actor.TargetID == idiot.InstanceID)
                            h.Priority = 1;
                }
                break;
        }
    }

    public override void OnActorModelStateChanged(Actor actor)
    {
        if (actor.OID == 0x1003 && actor.ModelState.ModelState == 0)
            Advance(0);
    }
}
*/
