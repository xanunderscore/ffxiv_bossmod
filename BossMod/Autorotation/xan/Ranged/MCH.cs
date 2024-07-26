﻿using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class MCH(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, Queen }
    public enum QueenStrategy
    {
        MinGauge,
        FullGauge,
        RaidBuffsOnly,
        Never
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("MCH", "Machinist", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.MCH), 100);

        def.DefineShared().AddAssociatedActions(AID.BarrelStabilizer, AID.Wildfire);

        def.Define(Track.Queen).As<QueenStrategy>("Queen", "Automaton Queen")
            .AddOption(QueenStrategy.MinGauge, "Min", "Summon at 50+ gauge")
            .AddOption(QueenStrategy.FullGauge, "Full", "Summon at 100 gauge")
            .AddOption(QueenStrategy.RaidBuffsOnly, "Buffed", "Delay summon until raid buffs, regardless of gauge")
            .AddOption(QueenStrategy.Never, "Never", "Do not automatically summon Queen at all")
            .AddAssociatedActions(AID.AutomatonQueen, AID.RookAutoturret);

        return def;
    }

    public int Heat; // max 100
    public int Battery; // max 100
    public float OverheatLeft; // max 10s
    public bool Overheated;
    public bool HasMinion;

    public float ReassembleLeft; // max 5s
    public float WildfireLeft; // max 10s
    public float HyperchargedLeft; // max 30s
    public float ExcavatorLeft; // max 30s
    public float FMFLeft; // max 30s

    public bool Flamethrower;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumSawTargets;
    public int NumFlamethrowerTargets;

    private Actor? BestAOETarget;
    private Actor? BestRangedAOETarget;
    private Actor? BestChainsawTarget;

    private bool IsPausedForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && Flamethrower;

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
            if (FMFLeft > _state.GCD)
                PushGCD(AID.FullMetalField, BestRangedAOETarget);

            if (NumAOETargets > 3 && Unlocked(AID.AutoCrossbow))
                PushGCD(AID.AutoCrossbow, BestAOETarget);

            if (Unlocked(AID.HeatBlast))
                PushGCD(AID.HeatBlast, primaryTarget);

            // we don't use any other gcds during overheat
            return;
        }

        if (ExcavatorLeft > _state.GCD)
            PushGCD(AID.Excavator, BestRangedAOETarget);

        if (Unlocked(AID.AirAnchor) && _state.CD(AID.AirAnchor) <= _state.GCD)
            PushGCD(AID.AirAnchor, primaryTarget, 20);

        if (Unlocked(AID.ChainSaw) && _state.CD(AID.ChainSaw) <= _state.GCD)
            PushGCD(AID.ChainSaw, BestChainsawTarget, 10);

        if (Unlocked(AID.Drill) && _state.CD(AID.Drill) - 20 <= _state.GCD)
        {
            if (Unlocked(AID.Bioblaster) && NumAOETargets > 2)
                PushGCD(AID.Bioblaster, BestAOETarget);

            PushGCD(AID.Drill, primaryTarget, _state.CD(AID.Drill) <= _state.GCD ? 20 : 0);
        }

        // TODO work out priorities
        if (FMFLeft > _state.GCD && ExcavatorLeft == 0)
            PushGCD(AID.FullMetalField, BestRangedAOETarget);

        if (ReassembleLeft > _state.GCD && NumAOETargets > 3)
            PushGCD(AID.Scattergun, BestAOETarget);

        if (!Unlocked(AID.AirAnchor) && Unlocked(AID.HotShot) && _state.CD(AID.HotShot) <= _state.GCD)
            PushGCD(AID.HotShot, primaryTarget);

        if (NumAOETargets > 2 && Unlocked(AID.SpreadShot))
            PushGCD(AID.SpreadShot, BestAOETarget);
        else
        {
            if (ComboLastMove == AID.SlugShot && Unlocked(AID.CleanShot))
                PushGCD(AID.CleanShot, primaryTarget);

            if (ComboLastMove == AID.SplitShot && Unlocked(AID.SlugShot))
                PushGCD(AID.SlugShot, primaryTarget);

            PushGCD(AID.SplitShot, primaryTarget);
        }
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (_state.CountdownRemaining is > 0 and < 5 && ReassembleLeft == 0 && _state.CD(AID.Reassemble) < 55)
            PushOGCD(AID.Reassemble, Player);

        if (IsPausedForFlamethrower || !Player.InCombat || primaryTarget == null)
            return;

        if (ShouldWildfire(strategy, deadline))
            PushOGCD(AID.Wildfire, primaryTarget);

        if (ShouldReassemble(strategy, primaryTarget) && _state.CanWeave(_state.CD(AID.Reassemble) - 55, 0.6f, deadline))
            PushOGCD(AID.Reassemble, Player);

        if (ShouldStabilize(strategy, deadline))
            PushOGCD(AID.BarrelStabilizer, Player);

        UseCharges(strategy, primaryTarget, deadline);

        if (ShouldMinion(strategy, primaryTarget) && _state.CanWeave(AID.RookAutoturret, 0.6f, deadline))
            PushOGCD(AID.RookAutoturret, Player);

        if (ShouldHypercharge(strategy, deadline))
            PushOGCD(AID.Hypercharge, Player);
    }

    // TODO this argument name sucks ass
    private float NextToolCD(bool untilCap)
        => MathF.Min(
            Unlocked(AID.Drill) ? _state.CD(AID.Drill) - (untilCap && Unlocked(TraitID.EnhancedMultiweapon) ? 0 : 20) : float.MaxValue,
            MathF.Min(
                Unlocked(AID.AirAnchor) ? _state.CD(AID.AirAnchor) : float.MaxValue,
                Unlocked(AID.ChainSaw) ? _state.CD(AID.ChainSaw) : float.MaxValue
            )
        );

    // cooldown at max charges: <=30 up to level 73, 0 otherwise
    private float MaxGaussCD => _state.CD(AID.GaussRound) - (Unlocked(TraitID.ChargedActionMastery) ? 0 : 30);
    private float MaxRicochetCD => _state.CD(AID.Ricochet) - (Unlocked(TraitID.ChargedActionMastery) ? 0 : 30);

    private void UseCharges(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        var gaussRoundCD = _state.CD(AID.GaussRound) - 60;
        var ricochetCD = _state.CD(AID.Ricochet) - 60;

        var canGauss = Unlocked(AID.GaussRound) && _state.CanWeave(gaussRoundCD, 0.6f, deadline);
        var canRicochet = Unlocked(AID.Ricochet) && _state.CanWeave(ricochetCD, 0.6f, deadline);

        if (canGauss && _state.CanWeave(MaxGaussCD, 0.6f, deadline))
            PushOGCD(AID.GaussRound, Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget);

        if (canRicochet && _state.CanWeave(MaxRicochetCD, 0.6f, deadline))
            PushOGCD(AID.Ricochet, BestRangedAOETarget);

        var useAllCharges = _state.RaidBuffsLeft > 0 || _state.RaidBuffsIn > 9000 || Overheated || !Unlocked(AID.Hypercharge);
        if (!useAllCharges)
            return;

        // this is a little awkward but we want to try to keep the cooldowns of both actions within range of each other
        if (canGauss && canRicochet)
        {
            if (gaussRoundCD > ricochetCD)
                UseRicochet(primaryTarget);
            else
                UseGauss(primaryTarget);
        }
        else if (canGauss)
            UseGauss(primaryTarget);
        else if (canRicochet)
            UseRicochet(primaryTarget);
    }

    private void UseGauss(Actor? primaryTarget) => PushOGCD(AID.GaussRound, Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget, -50);
    private void UseRicochet(Actor? primaryTarget) => PushOGCD(AID.Ricochet, BestRangedAOETarget, -50);

    private bool ShouldReassemble(StrategyValues strategy, Actor? primaryTarget)
    {
        if (ReassembleLeft > 0 || !Unlocked(AID.Reassemble) || Overheated || primaryTarget == null)
            return false;

        if (NumAOETargets > 3 && Unlocked(AID.SpreadShot))
            return true;

        if (_state.RaidBuffsIn < 10 && _state.RaidBuffsIn > _state.GCD)
            return false;

        if (!Unlocked(AID.Drill))
            return ComboLastMove == AID.SlugShot;

        return NextToolCD(untilCap: false) <= _state.GCD;
    }

    private bool ShouldMinion(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Unlocked(AID.RookAutoturret) || primaryTarget == null || HasMinion || Battery < 50 || ShouldWildfire(strategy, _state.GCD))
            return false;

        return strategy.Option(Track.Queen).As<QueenStrategy>() switch
        {
            QueenStrategy.MinGauge => true,
            QueenStrategy.FullGauge => Battery == 100,
            // allow early summon, queen doesn't start autoing for 5 seconds
            QueenStrategy.RaidBuffsOnly => _state.RaidBuffsLeft > 10 || _state.RaidBuffsIn < 5,
            _ => false,
        };
    }

    private bool ShouldHypercharge(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.Hypercharge) || HyperchargedLeft == 0 && Heat < 50 || Overheated || ReassembleLeft > _state.GCD || !_state.CanWeave(AID.Hypercharge, 0.6f, deadline))
            return false;

        // avoid delaying wildfire
        if (_state.CD(AID.Wildfire) < 10 && !ShouldWildfire(strategy, _state.GCD))
            return false;

        // we can't early weave if the overheat window will contain a regular GCD, because then it will expire before last HB
        if (FMFLeft > 0 && _state.GCD > 1.1f)
            return false;

        /* A full segment of Hypercharge is exactly three GCDs worth of time, or 7.5 seconds. Because of this, you should never enter Hypercharge if Chainsaw, Drill or Air Anchor has less than eight seconds on their cooldown timers. Doing so will cause the Chainsaw, Drill or Air Anchor cooldowns to drift, which leads to a loss of DPS and will more than likely cause issues down the line in your rotation when you reach your rotational reset at Wildfire.
         */
        return NextToolCD(untilCap: true) > _state.GCD + 7.5f;
    }

    private bool ShouldWildfire(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.Wildfire) || !_state.CanWeave(AID.Wildfire, 0.6f, deadline) || strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        // hack for opener - delay until all 4 tool charges are used
        if (CombatTimer < 60)
            return NextToolCD(untilCap: false) > _state.GCD;

        return FMFLeft == 0;
    }

    private bool ShouldStabilize(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.BarrelStabilizer) || !_state.CanWeave(AID.BarrelStabilizer, 0.6f, deadline) || strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        return _state.CD(AID.Drill) > 0;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<MachinistGauge>();

        Heat = gauge.Heat;
        Battery = gauge.Battery;
        Overheated = (gauge.TimerActive & 1) != 0;
        OverheatLeft = gauge.OverheatTimeRemaining / 1000f;
        HasMinion = (gauge.TimerActive & 2) != 0;

        ReassembleLeft = StatusLeft(SID.Reassembled);
        WildfireLeft = StatusLeft(SID.WildfirePlayer);
        HyperchargedLeft = StatusLeft(SID.Hypercharged);
        ExcavatorLeft = StatusLeft(SID.ExcavatorReady);
        FMFLeft = StatusLeft(SID.FullMetalMachinist);

        Flamethrower = StatusLeft(SID.Flamethrower) > 0;

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 12, IsConeAOETarget);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestChainsawTarget, NumSawTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 8, Player.Rotation.ToDirection(), 45.Degrees());

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private PositionCheck IsConeAOETarget => (playerTarget, targetToTest) => Hints.TargetInAOECone(targetToTest, Player.Position, 12, Player.DirectionTo(playerTarget), 60.Degrees());
}