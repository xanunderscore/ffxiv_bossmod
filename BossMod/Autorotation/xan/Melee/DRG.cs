using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan.Melee;
public sealed class DRG(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { Dive = SharedTrack.Count }

    public enum DiveStrategy
    {
        Allow,
        DenyMove,
        DenyLock
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("DRG", "Dragoon", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRG, Class.LNC), 100);

        def.DefineShared().AddAssociatedActions(AID.BattleLitany, AID.LanceCharge);

        def.Define(Track.Dive).As<DiveStrategy>("Dive")
            .AddOption(DiveStrategy.Allow, "Allow", "Use dives according to standard rotation")
            .AddOption(DiveStrategy.DenyMove, "NoMove", "Disallow dive actions that move you to the target")
            .AddOption(DiveStrategy.DenyLock, "NoLock", "Disallow dive actions that prevent you from moving (all except Mirage Dive)");

        return def;
    }

    public int Eyes;
    public int Focus;
    public float LotD;
    public float PowerSurge;
    public float LanceCharge;
    public float DiveReady;
    public float NastrondReady;
    public float LifeSurge;
    public float DraconianFire;
    public float DragonsFlight;

    public int NumAOETargets; // standard combo (10x4 rect)
    public int NumLongAOETargets; // GSK, nastrond (15x4 rect)
    public int NumDiveTargets; // dragonfire, stardiver, etc

    private Actor? BestAOETarget;
    private Actor? BestLongAOETarget;
    private Actor? BestDiveTarget;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (NumAOETargets > 2)
        {
            switch (ComboLastMove)
            {
                case AID.SonicThrust:
                    PushGCD(AID.CoerthanTorment, BestAOETarget);
                    break;
                case AID.DoomSpike:
                case AID.DraconianFury:
                    PushGCD(AID.SonicThrust, BestAOETarget);
                    break;
            }

            PushGCD(DraconianFire > _state.GCD ? AID.DraconianFury : AID.DoomSpike, BestAOETarget);
        }
        else
        {
            switch (ComboLastMove)
            {
                case AID.WheelingThrust:
                case AID.FangAndClaw:
                    PushGCD(AID.Drakesbane, primaryTarget);
                    break;
                case AID.ChaosThrust:
                case AID.ChaoticSpring:
                    PushGCD(AID.WheelingThrust, primaryTarget);
                    break;
                case AID.HeavensThrust:
                    PushGCD(AID.FangAndClaw, primaryTarget);
                    break;
                case AID.Disembowel:
                    PushGCD(AID.ChaosThrust, primaryTarget);
                    break;
                case AID.VorpalThrust:
                    PushGCD(AID.HeavensThrust, primaryTarget);
                    break;
                case AID.TrueThrust:
                case AID.RaidenThrust:
                    if (PowerSurge < 10)
                        PushGCD(AID.Disembowel, primaryTarget);
                    PushGCD(AID.VorpalThrust, primaryTarget);
                    break;
            }
        }

        PushGCD(DraconianFire > _state.GCD ? AID.RaidenThrust : AID.TrueThrust, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (primaryTarget == null || !Player.InCombat || PowerSurge == 0)
            return;

        var moveOk = MoveOk(strategy);
        var posOk = PosLockOk(strategy);

        if (LotD > 0 && moveOk)
            PushOGCD(AID.Stardiver, BestDiveTarget);

        if (strategy.BuffsOk())
        {
            PushOGCD(AID.LanceCharge, Player);
            PushOGCD(AID.BattleLitany, Player);
        }

        if (NastrondReady == 0)
            PushOGCD(AID.Geirskogul, BestLongAOETarget);

        if (DiveReady == 0 && posOk)
            PushOGCD(AID.HighJump, primaryTarget);

        if (LanceCharge > _state.GCD && LifeSurge == 0)
        {
            if (NumAOETargets > 2)
            {
                if (ComboLastMove is AID.SonicThrust || DraconianFire > _state.GCD)
                    PushOGCD(AID.LifeSurge, Player);
            }
            else if (ComboLastMove is AID.WheelingThrust or AID.VorpalThrust)
                PushOGCD(AID.LifeSurge, Player);
        }

        if (moveOk)
            PushOGCD(AID.DragonfireDive, BestDiveTarget);

        if (NastrondReady > 0)
            PushOGCD(AID.Nastrond, BestLongAOETarget);

        if (DragonsFlight > 0)
            PushOGCD(AID.RiseOfTheDragon, BestDiveTarget);

        if (DiveReady > 0)
            PushOGCD(AID.MirageDive, primaryTarget);

        if (Focus == 2)
            PushOGCD(AID.WyrmwindThrust, BestLongAOETarget);
    }

    private bool MoveOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() == DiveStrategy.Allow;
    private bool PosLockOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() != DiveStrategy.DenyLock;

    private (Positional, bool) GetPositional(StrategyValues strategy) => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => (PowerSurge < 10 ? Positional.Rear : Positional.Flank, false),
        AID.Disembowel or AID.ChaosThrust or AID.ChaoticSpring => (Positional.Rear, true),
        AID.FangAndClaw => (Positional.Rear, false),
        AID.WheelingThrust => (Positional.Flank, false),
        AID.VorpalThrust => (Positional.Flank, false),
        AID.HeavensThrust => (Positional.Flank, true),
        AID.Drakesbane => (PowerSurge < 12.5 ? Positional.Rear : Positional.Flank, false),
        _ => (Positional.Any, false)
    };

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget, AnimationLockDelay);

        var gauge = GetGauge<DragoonGauge>();

        Eyes = gauge.EyeCount;
        Focus = gauge.FirstmindsFocusCount;
        LotD = gauge.LotdTimer * 0.001f;

        PowerSurge = StatusLeft(SID.PowerSurge);
        DiveReady = StatusLeft(SID.DiveReady);
        NastrondReady = StatusLeft(SID.NastrondReady);
        LifeSurge = StatusLeft(SID.LifeSurge);
        LanceCharge = StatusLeft(SID.LanceCharge);
        DraconianFire = StatusLeft(SID.DraconianFire);
        DragonsFlight = StatusLeft(SID.DragonsFlight);

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));
        (BestLongAOETarget, NumLongAOETargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestDiveTarget, NumDiveTargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);

        _state.UpdatePositionals(primaryTarget, GetPositional(strategy), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
