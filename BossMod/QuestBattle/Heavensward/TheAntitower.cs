using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle.Heavensward.TheAntitower;

public class AreaTransition(WorldState ws, string name, List<Waypoint> waypoints) : QuestObjective(ws, name, waypoints)
{
    public override void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.Jumping61)
        {
            if (value)
                ShouldCancelNavigation = true;
            else
                Completed = true;
        }
    }
}

class Pad1(WorldState ws) : AreaTransition(ws, "Launchpad 1", [new Waypoint(-315.14f, 220.00f, 136.34f)]);
class Pad2(WorldState ws) : AreaTransition(ws, "Launchpad 2", [new Waypoint(-352.59f, 256.62f, -1.29f)]);
class Pad3(WorldState ws) : AreaTransition(ws, "Launchpad 3", [new Waypoint(-364.91f, 290.00f, -130.12f)]);
class Boss1(WorldState ws) : QuestObjective(ws, "Boss 1", new Waypoint(-365.02f, 325.00f, -250.57f))
{
    public override void OnActorKilled(Actor actor)
    {
        Completed |= actor.OID == 0x14FC;
    }
}
class Pad4(WorldState ws) : AreaTransition(ws, "Launchpad 4", [new Waypoint(-365, 325, -279)]);
class Pack1(WorldState ws) : QuestObjective(ws, "Pack 1", [new Waypoint(192.38f, 2.00f, 190.60f)]);

// [Quest(BossModuleInfo.Maturity.WIP, 141)]
public class TheAntitower(WorldState ws) : QuestBattle(ws, [new Pad1(ws), new Pad2(ws), new Pad3(ws), new Boss1(ws), new Pad4(ws), new Pack1(ws)]);
