namespace BossMod.QuestBattle.Heavensward;

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
