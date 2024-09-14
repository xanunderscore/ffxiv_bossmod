namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Sophrosyne;

public enum OID : uint
{
    Boss = 0x29AA,
    Helper = 0x233C,
}

class SaveTheDipshit(BossModule module) : BossComponent(module)
{
    private Actor? Dwarf;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Dwarf ??= WorldState.Actors.FirstOrDefault(x => x.OID == 0x29A9);
        if (Dwarf?.IsTargetable ?? false)
            hints.Allies.Add(Dwarf);
    }
}

class SophrosyneStates : StateMachineBuilder
{
    public SophrosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SaveTheDipshit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8395)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(632, 64.15f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
