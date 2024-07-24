using BossMod.Autorotation.Legacy;

namespace BossMod.Autorotation.xan;

public enum Targeting { Auto, Manual, AutoPrimary }
public enum OffensiveStrategy { Automatic, Delay, Force }
public enum AOEStrategy { AOE, SingleTarget, AOEForced }

public abstract class Basexan<AID, TraitID> : LegacyModule where AID : Enum where TraitID : Enum
{
    public class State(RotationModule module) : CommonState(module) { }

    protected State _state;

    protected float PelotonLeft { get; private set; }
    protected float SwiftcastLeft { get; private set; }
    protected float TrueNorthLeft { get; private set; }
    protected float CombatTimer { get; private set; }

    protected uint MP;

    protected AID ComboLastMove => (AID)(object)_state.ComboLastAction;

    protected Basexan(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    protected void PushGCD(AID aid, Actor? target, int additionalPrio = 0)
        => PushAction(aid, target, ActionQueue.Priority.High + 500 + additionalPrio);

    protected void PushOGCD(AID aid, Actor? target, int additionalPrio = 0)
        => PushAction(aid, target, ActionQueue.Priority.Low + 500 + additionalPrio);

    protected void PushAction(AID aid, Actor? target, float priority)
    {
        if ((uint)(object)aid == 0)
            return;

        if (!CanCast(aid))
            return;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return;

        if (def.Range != 0 && target == null)
        {
            // Service.Log($"Queued targeted action ({aid}) with no target");
            return;
        }

        Vector3 targetPos = default;
        if (def.ID.ID is (uint)BossMod.BLM.AID.LeyLines or (uint)BossMod.BLM.AID.Retrace or (uint)BossMod.PCT.AID.StarryMuse or (uint)BossMod.PCT.AID.ScenicMuse)
            targetPos = Player.PosRot.XYZ();

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, targetPos: targetPos);
    }

    protected void QueueOGCD(Action<float> ogcdFun)
    {
        var deadline = _state.GCD > 0 ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength))
            ogcdFun(deadline - _state.OGCDSlotLength);
        if (_state.CanWeave(deadline))
            ogcdFun(deadline);
    }

    /// <summary>
    /// Tries to select a suitable primary target.<br/>
    ///
    /// If the provided <paramref name="primaryTarget"/> is null, an NPC, or non-enemy object; it will be reset to <c>null</c>.<br/>
    ///
    /// Additionally, if <paramref name="range"/> is set to <c>Targeting.Auto</c>, and the user's current target is more than <paramref name="range"/> yalms from the player, this function attempts to find a closer one. No prioritization is done; if any target is returned, it is simply the actor that was earliest in the object table. If no closer target is found, <paramref name="primaryTarget"/> will remain unchanged.
    /// </summary>
    /// <param name="strategy">Targeting strategy</param>
    /// <param name="primaryTarget">Player's current target - may be null</param>
    /// <param name="range">Maximum distance from the player to search for a candidate target</param>
    protected void SelectPrimaryTarget(Targeting strategy, ref Actor? primaryTarget, float range)
    {
        if (!IsEnemy(primaryTarget))
            primaryTarget = null;

        if (strategy != Targeting.Auto)
            return;

        if (Player.DistanceToHitbox(primaryTarget) > range)
        {
            var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range)?.Actor;
            if (newTarget != null)
                primaryTarget = newTarget;
            // Hints.ForcedTarget = primaryTarget;
        }
    }

    protected (Actor? Best, int Targets) SelectTarget(
        Targeting track,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(track, primaryTarget, range, isInAOE, (numTargets, _) => numTargets);

    protected (Actor? Best, P Priority) SelectTarget<P>(
        Targeting track,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize
    ) where P : struct, IComparable
    {
        P targetPrio(Actor potentialTarget) => prioritize(Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor)), potentialTarget);

        switch (track)
        {
            case Targeting.Auto:
                return FindBetterTargetBy(primaryTarget, range, targetPrio);
            case Targeting.AutoPrimary:
                if (primaryTarget == null)
                    return (null, default);
                return FindBetterTargetBy(
                    primaryTarget,
                    range,
                    targetPrio,
                    enemy => isInAOE(enemy.Actor, primaryTarget)
                );
            default:
                if (primaryTarget == null)
                    return (null, default);
                // if AOE action on primary target would hit a forbidden target, return null
                var cnt = Hints.NumPriorityTargetsInAOE(enemy => isInAOE(primaryTarget, enemy.Actor));
                return cnt > 0 ? (primaryTarget, prioritize(cnt, primaryTarget)) : (null, default);
        }
    }

    /// <summary>
    /// Get <em>effective</em> cast time for the provided action.<br/>
    /// The default implementation returns the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like Leylines) and slow debuffs. It also accounts for Swiftcast.<br/>
    /// Subclasses should handle job-specific cast speed adjustments, such as RDM's Dualcast or PCT's motifs.
    /// </summary>
    /// <param name="aid"></param>
    /// <returns></returns>
    protected virtual float GetCastTime(AID aid) => SwiftcastLeft > _state.GCD ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * _state.SpellGCDTime / 2.5f;

    protected float NextCastStart => _state.AnimationLock > _state.GCD ? _state.AnimationLock + _state.AnimationLockDelay : _state.GCD;

    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);
    protected float GetSlidecastEnd(AID aid) => NextCastStart + GetSlidecastTime(aid);

    protected bool CanCast(AID aid)
    {
        var t = GetSlidecastTime(aid);
        if (t == 0)
            return true;

        return NextCastStart + t <= ForceMovementIn;
    }

    protected float ForceMovementIn;

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    private static bool IsEnemy(Actor? actor) => actor != null && actor.Type is ActorType.Enemy or ActorType.Part && !actor.IsAlly;

    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.707167f => Positional.Rear,
        < 0.707167f => Positional.Flank,
        _ => Positional.Front
    };

    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);

    protected int NumMeleeAOETargets() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    protected PositionCheck IsSplashTarget => (Actor primary, Actor other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    protected PositionCheck Is25yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 25, 4);

    public sealed override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        var pelo = Player.FindStatus(BRD.SID.Peloton);
        PelotonLeft = pelo != null ? _state.StatusDuration(pelo.Value.ExpireAt) : 0;
        SwiftcastLeft = StatusLeft(WHM.SID.Swiftcast);
        TrueNorthLeft = StatusLeft(DRG.SID.TrueNorth);

        ForceMovementIn = forceMovementIn;

        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;

        MP = (uint)Math.Clamp(Player.HPMP.CurMP + World.PendingEffects.PendingMPDifference(Player.InstanceID), 0, 10000);

        Exec(strategy, primaryTarget, MathF.Max(estimatedAnimLockDelay, 0.1f));
    }

    public abstract void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay);

    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => Player.FindStatus(status) is ActorStatus s ? (StatusDuration(s.ExpireAt), s.Extra & 0xFF) : (0, 0);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;
}

static class Extendxan
{
    public static RotationModuleDefinition.ConfigRef<Targeting> DefineTargeting<Index>(this RotationModuleDefinition def, Index trackname)
         where Index : Enum
    {
        return def.Define(trackname).As<Targeting>("Targeting")
            .AddOption(Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit");
    }

    public static RotationModuleDefinition.ConfigRef<AOEStrategy> DefineAOE<Index>(this RotationModuleDefinition def, Index trackname) where Index : Enum
    {
        return def.Define(trackname).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AOEForced, "AOEF", "Always use AOE actions");
    }

    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name) where Index : Enum
    {
        return def.Define(track).As<OffensiveStrategy>(name)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use when optimal")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use")
            .AddOption(OffensiveStrategy.Force, "Force", "Use ASAP");
    }
}
