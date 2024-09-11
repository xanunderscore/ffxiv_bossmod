/*
namespace BossMod.Endwalker.Quest.FrostyReception;

public class CarriageBounds(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Module.Raid.Player()?.Position ?? Arena.Center;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // add some massive forbidden zones to represent arena walls. this one is WAY too big to display all at once
        hints.AddForbiddenZone(new AOEShapeRect(500, 500), new(-34, 0), 270.Degrees());
        hints.AddForbiddenZone(new AOEShapeRect(500, 500), new(34, 0), 90.Degrees());

        var haveTarget = false;

        foreach (var e in hints.PotentialTargets)
        {
            haveTarget = true;
            e.Priority = 0;
        }

        if (!haveTarget)
            hints.ForcedMovement = new(0, 0, -1);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
*/
