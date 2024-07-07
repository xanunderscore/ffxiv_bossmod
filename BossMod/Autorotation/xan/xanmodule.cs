using BossMod.Autorotation.Legacy;
using static BossMod.Autorotation.StrategyValues;

namespace BossMod.Autorotation.xan;

// frick you i'll name my class whatever i want
#pragma warning disable CS8981
#pragma warning disable IDE1006

public abstract class xanmodule : LegacyModule
{
    public enum Targeting { Auto, Manual, AutoPrimary }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum AOEStrategy { AOE, SingleTarget }

    public class State(RotationModule module) : CommonState(module) { }

    protected State _state;

    protected xanmodule(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    protected void PushGCD<AID>(AID aid, Actor? target, int additionalPrio = 0) where AID : Enum
        => PushAction(aid, target, ActionQueue.Priority.High + 500 + additionalPrio);

    protected void PushOGCD<AID>(AID aid, Actor? target, int additionalPrio = 0) where AID : Enum
        => PushAction(aid, target, ActionQueue.Priority.Low + 500 + additionalPrio);

    protected void PushAction<AID>(AID aid, Actor? target, float priority) where AID : Enum
    {
        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return;

        if (def.Range != 0 && target == null)
        {
            // Service.Log($"Queued targeted action ({aid}) with no target");
            return;
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority);
    }

    protected void QueueOGCD(Action<float, float> ogcdFun)
    {
        var deadline = _state.GCD > 0 ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength))
            ogcdFun(deadline - _state.OGCDSlotLength, deadline);
        if (_state.CanWeave(deadline))
            ogcdFun(deadline, deadline);
    }

    /// <summary>
    /// If the user's current target is more than <paramref name="range"/> yalms from the player, this function attempts to find a closer one. No prioritization is done; if any target is returned, it is simply the actor that was earliest in the object table.<br/>
    ///
    /// It is guaranteed that <paramref name="primaryTarget"/> will be set to either <c>null</c> or an attackable enemy (not, for example, an ally or event object).<br/>
    ///
    /// If the provided Targeting strategy is Manual, this function is otherwise a <strong>no-op</strong>.
    /// </summary>
    /// <param name="track">Reference to the Targeting track of the active strategy</param>
    /// <param name="primaryTarget">Player's current target - may be null</param>
    /// <param name="range">Maximum distance from the player to search for a candidate target</param>
    protected void SelectPrimaryTarget(OptionRef track, ref Actor? primaryTarget, float range)
    {
        if (!IsEnemy(primaryTarget))
            primaryTarget = null;

        var tars = track.As<Targeting>();
        if (tars == Targeting.Manual)
        {
            return;
        }

        if (Player.DistanceTo(primaryTarget) > range)
        {
            primaryTarget = Hints.PriorityTargets.Where(x => x.Actor.DistanceTo(Player) <= range).MaxBy(x => x.Actor.HPMP.CurHP)?.Actor;
            // Hints.ForcedTarget = primaryTarget;
        }
    }

    private static bool IsEnemy(Actor? actor) => actor != null && actor.Type is ActorType.Enemy or ActorType.Part && !actor.IsAlly;

    protected (Actor? Best, int Targets) SelectTarget(OptionRef track, Actor? primaryTarget, float range, Func<Actor, int> priorityFunc) => track.As<Targeting>() switch
    {
        Targeting.Auto => FindBetterTargetBy(primaryTarget, range, priorityFunc),
        Targeting.AutoPrimary => throw new NotImplementedException(),
        _ => (primaryTarget, primaryTarget == null ? 0 : priorityFunc(primaryTarget))
    };

    protected int NumSplashTargets(Actor primary) => Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
    protected int NumMeleeAOETargets() => NumSplashTargets(Player);

    protected int Num25yRectTargets(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 25, 4);
}

static class xanextensions
{
    public static RotationModuleDefinition.ConfigRef<xanmodule.Targeting> DefineTargeting<Index>(this RotationModuleDefinition def, Index trackname)
         where Index : Enum
    {
        return def.Define(trackname).As<xanmodule.Targeting>("Targeting")
            .AddOption(xanmodule.Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(xanmodule.Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(xanmodule.Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit");
    }

    public static RotationModuleDefinition.ConfigRef<xanmodule.AOEStrategy> DefineAOE<Index>(this RotationModuleDefinition def, Index trackname) where Index : Enum
    {
        return def.Define(trackname).As<xanmodule.AOEStrategy>("AOE")
            .AddOption(xanmodule.AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(xanmodule.AOEStrategy.SingleTarget, "ST", "Use single-target actions");
    }

    public static RotationModuleDefinition.ConfigRef<xanmodule.OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name) where Index : Enum
    {
        return def.Define(track).As<xanmodule.OffensiveStrategy>(name)
            .AddOption(xanmodule.OffensiveStrategy.Automatic, "Auto", "Use when optimal")
            .AddOption(xanmodule.OffensiveStrategy.Delay, "Delay", "Don't use")
            .AddOption(xanmodule.OffensiveStrategy.Force, "Force", "Use ASAP");
    }
}
