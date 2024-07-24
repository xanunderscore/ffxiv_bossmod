using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan.Melee;
public sealed class DRG(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("DRG", "Dragoon", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRG, Class.LNC), 100);

        def.DefineShared().AddAssociatedActions(AID.BattleLitany);

        return def;
    }

    public int Eyes;
    public int Focus;
    public float PowerSurge;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane))
            PushGCD(AID.Drakesbane, primaryTarget);

        if (ComboLastMove is AID.ChaosThrust or AID.ChaoticSpring && Unlocked(AID.WheelingThrust))
            PushGCD(AID.WheelingThrust, primaryTarget);

        if (ComboLastMove is AID.HeavensThrust && Unlocked(AID.FangAndClaw))
            PushGCD(AID.FangAndClaw, primaryTarget);

        if (ComboLastMove == AID.Disembowel && Unlocked(AID.ChaosThrust))
            PushGCD(AID.ChaosThrust, primaryTarget);

        if (ComboLastMove is AID.VorpalThrust && Unlocked(AID.HeavensThrust))
            PushGCD(AID.HeavensThrust, primaryTarget);

        if (ComboLastMove is AID.TrueThrust or AID.RaidenThrust)
        {
            if (Unlocked(AID.Disembowel) && PowerSurge < 10)
                PushGCD(AID.Disembowel, primaryTarget);

            if (Unlocked(AID.VorpalThrust))
                PushGCD(AID.VorpalThrust, primaryTarget);
        }

        PushGCD(AID.TrueThrust, primaryTarget);
    }

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

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<DragoonGauge>();

        Eyes = gauge.EyeCount;
        Focus = gauge.FirstmindsFocusCount;
        PowerSurge = StatusLeft(SID.PowerSurge);

        _state.UpdatePositionals(primaryTarget, GetPositional(strategy), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
    }
}
