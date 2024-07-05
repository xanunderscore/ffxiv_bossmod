using BossMod.Autorotation.Legacy;
using static BossMod.Autorotation.StrategyValues;

namespace BossMod.Autorotation.xan;

// frick you i'll name my class whatever i want
#pragma warning disable CS8981
#pragma warning disable IDE1006

public abstract class xanmodule(RotationModuleManager manager, Actor player) : LegacyModule(manager, player)
{
    public enum Targeting { Auto, Manual, AutoPrimary }

    protected void PushGCD<AID>(AID aid, Actor? target, int additionalPrio = 0) where AID : Enum
        => Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + 500 + additionalPrio);

    protected void PushOGCD<AID>(AID aid, Actor? target, int additionalPrio = 0) where AID : Enum
        => Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + 500 + additionalPrio);

    protected abstract CommonState GetState();

    protected void QueueOGCD(Action<float, float> ogcdFun)
    {
        var st = GetState();
        var deadline = st.GCD > 0 ? st.GCD : float.MaxValue;
        if (st.CanWeave(deadline - st.OGCDSlotLength))
            ogcdFun(deadline - st.OGCDSlotLength, deadline);
        if (st.CanWeave(deadline))
            ogcdFun(deadline, deadline);
    }

    protected Actor? SelectMeleeTarget(OptionRef track, Actor? primaryTarget) => SelectSingleTarget(track, primaryTarget, 3);

    protected Actor? SelectSingleTarget(OptionRef track, Actor? primaryTarget, float range)
    {
        var tars = track.As<Targeting>();
        if (tars == Targeting.Manual)
            return primaryTarget;

        return Player.DistanceTo(primaryTarget) <= range ? primaryTarget : Hints.PriorityTargets.FirstOrDefault(x => x.Actor.DistanceTo(Player) <= range)?.Actor;
    }

    protected (Actor? Best, int Targets) SelectRangedTarget(OptionRef track, Actor? primaryTarget, float range, Func<Actor, int> priorityFunc) => track.As<Targeting>() switch
    {
        Targeting.Auto => FindBetterTargetBy(primaryTarget, range, priorityFunc),
        Targeting.AutoPrimary => throw new NotImplementedException(),
        _ => (primaryTarget, primaryTarget == null ? 0 : priorityFunc(primaryTarget))
    };

    protected int NumSplashTargets(Actor primary) => Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
    protected int NumMeleeAOETargets() => NumSplashTargets(Player);
}

static class xanextensions
{
    public static void DefineTargeting<Index>(this RotationModuleDefinition def, Index trackname)
         where Index : Enum
    {
        def.Define(trackname).As<xanmodule.Targeting>("Targeting", uiPriority: 80)
            .AddOption(xanmodule.Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(xanmodule.Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(xanmodule.Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit");
    }
}
