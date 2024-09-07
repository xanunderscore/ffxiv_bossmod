namespace BossMod;

public abstract class DutyModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds) : BossModule(ws, primary, center, bounds)
{
    protected override bool CheckPull() => true;
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.InCombat), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly && x.IsTargetable && x.Type != ActorType.EventObj), ArenaColor.PlayerGeneric);
    }
}
