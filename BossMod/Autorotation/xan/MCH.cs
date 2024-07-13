using BossMod.ActionTweaks.ClassActions;
using BossMod.MCH;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class MCH(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("MCH", "Machinist", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.MCH), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.BarrelStabilizer, AID.Wildfire);

        return def;
    }

    public int Heat; // max 100
    public int Battery; // max 100
    public float OverheatLeft; // max 10s
    public bool Overheated;
    public bool HasMinion;

    public float ReassembleLeft; // max 5s
    public float WildfireLeft; // max 10s
    public float HyperchargedLeft; // max 30

    public bool Flamethrower;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumSawTargets;
    public int NumFlamethrowerTargets;

    private Actor? BestAOETarget;
    private Actor? BestRangedAOETarget;
    private Actor? BestChainsawTarget;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (IsPausedForFlamethrower)
            return;

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 0.4f && Unlocked(AID.AirAnchor))
                PushGCD(AID.AirAnchor, primaryTarget);

            return;
        }

        if (Overheated)
        {
            if (NumAOETargets > 2 && Unlocked(AID.AutoCrossbow))
                PushGCD(AID.AutoCrossbow, BestAOETarget);

            if (Unlocked(AID.HeatBlast))
                PushGCD(AID.HeatBlast, primaryTarget);
        }

        if (Unlocked(AID.AirAnchor) && _state.CD(AID.AirAnchor) <= _state.GCD)
            PushGCD(AID.AirAnchor, primaryTarget);

        if (Unlocked(AID.Drill) && _state.CD(AID.Drill) - 20 <= _state.GCD)
            PushGCD(AID.Drill, primaryTarget);

        if (Unlocked(AID.ChainSaw) && _state.CD(AID.ChainSaw) <= _state.GCD)
            PushGCD(AID.ChainSaw, BestChainsawTarget);

        if (ReassembleLeft > _state.GCD && NumAOETargets > 3)
            PushGCD(AID.Scattergun, BestAOETarget);

        if (NumAOETargets > 2 && Unlocked(AID.SpreadShot))
        {
            if (!Overheated && NumFlamethrowerTargets > 2 && Unlocked(AID.Flamethrower))
            {
                PushGCD(AID.Flamethrower, Player);
                return;
            }

            PushGCD(AID.SpreadShot, BestAOETarget);
        }
        else
        {
            if ((AID)_state.ComboLastAction == AID.SlugShot && Unlocked(AID.CleanShot))
                PushGCD(AID.CleanShot, primaryTarget);

            if ((AID)_state.ComboLastAction == AID.SplitShot && Unlocked(AID.SlugShot))
                PushGCD(AID.SlugShot, primaryTarget);

            PushGCD(AID.SplitShot, primaryTarget);
        }
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (_state.CountdownRemaining is > 0 and < 5 && ReassembleLeft == 0 && _state.CD(AID.Reassemble) < 55)
            PushOGCD(AID.Reassemble, Player);

        if (IsPausedForFlamethrower || !Player.InCombat)
            return;

        var buffOk = strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

        if (buffOk)
        {
            if (_state.CD(AID.Drill) > 0 && Unlocked(AID.BarrelStabilizer) && _state.CanWeave(AID.BarrelStabilizer, 0.6f, deadline))
                PushOGCD(AID.BarrelStabilizer, Player);

            // FIXME need to check that we can overheat first
            if (Unlocked(AID.Wildfire) && _state.CD(AID.RookAutoturret) > 0 && _state.CanWeave(AID.Wildfire, 0.6f, deadline))
                PushOGCD(AID.Wildfire, primaryTarget);
        }

        // prevent overcap
        if (Unlocked(AID.GaussRound) && _state.CanWeave(AID.GaussRound, 0.6f, deadline))
            PushOGCD(AID.GaussRound, primaryTarget);

        // prevent overcap
        if (Unlocked(AID.Ricochet) && _state.CanWeave(AID.Ricochet, 0.6f, deadline))
            PushOGCD(AID.Ricochet, BestRangedAOETarget);

        if (ShouldReassemble(strategy) && _state.CanWeave(_state.CD(AID.Reassemble) - 55, 0.6f, deadline))
            PushOGCD(AID.Reassemble, Player);

        if (Unlocked(AID.RookAutoturret) && Battery >= 50 && !HasMinion && _state.CanWeave(AID.RookAutoturret, 0.6f, deadline))
            PushOGCD(AID.RookAutoturret, Player);

        if (Unlocked(AID.Hypercharge) && (Heat >= 50 || HyperchargedLeft > 0) && !Overheated && _state.CanWeave(AID.Hypercharge, 0.6f, deadline))
            PushOGCD(AID.Hypercharge, Player);
    }

    private bool ShouldReassemble(StrategyValues strategy)
    {
        if (ReassembleLeft > 0 || !Unlocked(AID.Reassemble) || Overheated)
            return false;

        if (NumAOETargets > 3 && Unlocked(AID.SpreadShot))
            return true;

        return _state.Level switch
        {
            < 26 => _state.CD(AID.HotShot) <= _state.GCD,
            < 58 => (AID)_state.ComboLastAction == AID.SlugShot,
            < 76 => _state.CD(AID.Drill) <= _state.GCD,
            < 90 => _state.CD(AID.AirAnchor) <= _state.GCD,
            _ => _state.CD(AID.AirAnchor) <= _state.GCD || _state.CD(AID.ChainSaw) <= _state.GCD,
        };
    }

    private bool IsPausedForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && Flamethrower;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();

        var wildfireTarget = Hints.PriorityTargets.FirstOrDefault(x => x.Actor.FindStatus(SID.WildfireTarget, Player.InstanceID) != null)?.Actor;

        // if autotarget enabled, force all weaponskills to hit wildfire'd target during effect to maximize potency
        if (wildfireTarget != null && targeting == Targeting.Auto)
        {
            primaryTarget = wildfireTarget;
            targeting = Targeting.AutoPrimary;
        }
        else
            SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);

        _state.UpdateCommon(primaryTarget);

        var gauge = Service.JobGauges.Get<MCHGauge>();

        Heat = gauge.Heat;
        Battery = gauge.Battery;
        Overheated = gauge.IsOverheated;
        OverheatLeft = gauge.OverheatTimeRemaining / 1000f;
        HasMinion = gauge.IsRobotActive;

        ReassembleLeft = StatusLeft(SID.Reassembled);
        WildfireLeft = StatusLeft(SID.WildfirePlayer);
        HyperchargedLeft = StatusLeft(SID.Hypercharged);

        Flamethrower = StatusLeft(SID.Flamethrower) > 0;

        (BestAOETarget, NumAOETargets) = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => SelectTarget(targeting, primaryTarget, 12, IsConeAOETarget),
            _ => (primaryTarget, 0)
        };
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(targeting, primaryTarget, 25, IsSplashTarget);
        (BestChainsawTarget, NumSawTargets) = SelectTarget(targeting, primaryTarget, 25, Is25yRectTarget);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 8, Player.Rotation.ToDirection(), 45.Degrees());

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private PositionCheck IsConeAOETarget => (playerTarget, targetToTest) => Hints.TargetInAOECone(targetToTest, Player.Position, 12, Player.DirectionTo(playerTarget), 60.Degrees());
}
