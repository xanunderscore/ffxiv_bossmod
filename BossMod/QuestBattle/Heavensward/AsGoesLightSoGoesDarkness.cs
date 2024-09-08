namespace BossMod.QuestBattle.Heavensward.AsGoesLightSoGoesDarkness;

enum OID : uint
{
    Boss = 0x148A,
    Helper = 0x233C,
    VaultDoor1 = 0x1E9ED7,
    VaultDoor2 = 0x1E9ED8,
    ArenaDoor = 0x1E9ED9,
    VaultDoor3 = 0x1E9EDA,
    ArenaWall = 0x1E9EDD,

    Refugee1 = 0x14BE,
    Refugee2 = 0x14BF,
    Refugee3 = 0x14C0,
    Refugee4 = 0x14C1,
    Refugee5 = 0x14C2,
    Bonds = 0x1E9EE0,
}

abstract class FreeRefugee(WorldState ws, string name, Waypoint wp, uint oid, bool combatPausesNavigation = true, bool cancelNavOnCombat = false) : QuestObjective(ws, name, [wp], combatPausesNavigation, cancelNavOnCombat)
{
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        Completed |= actor.OID == oid && status.ID == 990;
    }
}

abstract class TrackEventState(WorldState ws, string name, Waypoint wp, uint oid, bool combatPausesNavigation = true, bool combatCancelsNavigation = false) : QuestObjective(ws, name, [wp], combatPausesNavigation, combatCancelsNavigation)
{
    public override void OnActorEventStateChanged(Actor actor)
    {
        Completed |= actor.OID == oid && actor.EventState == 7;
    }
}

class Refugee1(WorldState ws) : FreeRefugee(ws, "Refugee 1", new(0, -300, 75), (uint)OID.Refugee1, cancelNavOnCombat: true);
class Door1(WorldState ws) : TrackEventState(ws, "Pack 1", new(16, -300, 30), (uint)OID.VaultDoor1);
class Refugee2(WorldState ws) : TrackEventState(ws, "Refugee 2", new(52, -300, -30), (uint)OID.VaultDoor2);
class ArenaDoor(WorldState ws) : TrackEventState(ws, "Pack 2", new(-30, -300, -75), (uint)OID.ArenaDoor);
class Door3(WorldState ws) : TrackEventState(ws, "Cutscene", new(-17.5f, -292, -100), (uint)OID.VaultDoor3);
class Refugee34(WorldState ws) : FreeRefugee(ws, "Refugee 3+4", new(-52, -300, -30), (uint)OID.Refugee3, cancelNavOnCombat: true)
{
    public override void AddAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
            hints.InteractWithOID(World, OID.Bonds);
    }
}
class Refugee5(WorldState ws) : FreeRefugee(ws, "Refugee 5", new(55, -300, -68), (uint)OID.Refugee5, cancelNavOnCombat: true)
{
    public override void AddAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
            hints.InteractWithOID(World, OID.Bonds);
    }
}
class HelpAymeric(WorldState ws) : TrackEventState(ws, "Help Aymeric", new(0, -292, -100), (uint)OID.ArenaWall, combatCancelsNavigation: true);
class Refugee6(WorldState ws) : QuestObjective(ws, "Refugee 6", [new(2, -282.35f, -151)]);

[Quest(BossModuleInfo.Maturity.WIP, 441)]
public sealed class AsGoesLightSoGoesDarkness(WorldState ws) : QuestBattle(ws, [
    new Refugee1(ws),
    new Door1(ws),
    new Refugee2(ws),
    new ArenaDoor(ws),
    new Door3(ws),
    new Refugee34(ws),
    new Refugee5(ws),
    new HelpAymeric(ws),
    new Refugee6(ws),
]);
