namespace BossMod.Autorotation;
public abstract class RoleHealerUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Surecast, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self)
            .AddAssociatedActions(ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "", 20, ClassShared.AID.Surecast, 6);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
            Hints.ActionsToExecute.Push(lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.BreathOfTheEarth : ClassShared.AID.HealingWind), Player, ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn);
    }
}
