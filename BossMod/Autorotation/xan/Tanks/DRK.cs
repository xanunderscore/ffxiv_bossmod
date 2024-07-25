using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class DRK(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("DRK", "Dark Knight", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRK), 100);

        def.DefineShared().AddAssociatedActions(AID.LivingShadow);

        return def;
    }

    public DarkKnightGauge Gauge;
    public int BloodWeapon;
    public int Delirium;
    public float SaltedEarth;

    public float Darkside => Gauge.DarksideTimer * 0.001f;
    public int Blood => Gauge.Blood;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Delirium > 0)
            PushGCD(AID.Bloodspiller, primaryTarget);

        if (ComboLastMove == AID.SyphonStrike)
            PushGCD(AID.Souleater, primaryTarget);

        if (ComboLastMove == AID.HardSlash)
            PushGCD(AID.SyphonStrike, primaryTarget);

        PushGCD(AID.HardSlash, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (primaryTarget == null)
            return;

        if (Darkside < 20)
            PushOGCD(AID.EdgeOfDarkness, primaryTarget);

        if (_state.CanWeave(AID.LivingShadow, 0.6f, deadline))
            PushOGCD(AID.LivingShadow, Player);

        if (Blood > 0 && _state.CanWeave(AID.Delirium, 0.6f, deadline))
            PushOGCD(AID.Delirium, Player);

        if (_state.CD(AID.Delirium) > 0)
        {
            if (_state.CanWeave(AID.SaltedEarth, 0.6f, deadline))
                PushOGCD(AID.SaltedEarth, Player);

            if (_state.CanWeave(AID.Shadowbringer, 0.6f, deadline))
                PushOGCD(AID.Shadowbringer, primaryTarget);

            if (_state.CanWeave(AID.CarveAndSpit, 0.6f, deadline))
                PushOGCD(AID.CarveAndSpit, primaryTarget);

            if (_state.CanWeave(_state.CD(AID.Shadowbringer) - 60, 0.6f, deadline))
                PushOGCD(AID.Shadowbringer, primaryTarget);

            if (SaltedEarth > 0 && _state.CanWeave(AID.SaltAndDarkness, 0.6f, deadline))
                PushOGCD(AID.SaltAndDarkness, Player);
        }
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        Gauge = GetGauge<DarkKnightGauge>();

        BloodWeapon = StatusStacks(SID.BloodWeapon);
        Delirium = StatusStacks(SID.Delirium);
        SaltedEarth = StatusLeft(SID.SaltedEarth);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
