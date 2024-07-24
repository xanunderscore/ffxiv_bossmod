using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class GNB(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("GNB", "Gunbreaker", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.GNB), 100);

        def.DefineShared().AddAssociatedActions(AID.Bloodfest);

        return def;
    }

    public int Ammo;
    public byte AmmoCombo;

    public float SonicBreak;
    public bool Continuation;
    public float NoMercy;

    public int NumAOETargets;

    public bool FastGCD => _state.AttackGCDTime <= 2.47f;
    public int MaxAmmo => Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
        {
            return;
        }

        if (_state.CD(AID.GnashingFang) > 0 && NumAOETargets > 0 && Ammo >= 2 && _state.CD(AID.DoubleDown) < _state.GCD)
            PushGCD(AID.DoubleDown, Player);

        if (_state.CD(AID.GnashingFang) > 0 && SonicBreak > _state.GCD)
            PushGCD(AID.SonicBreak, primaryTarget);

        if (AmmoCombo == 2)
        {
            PushGCD(AID.WickedTalon, primaryTarget);
            return;
        }

        if (AmmoCombo == 1)
        {
            PushGCD(AID.SavageClaw, primaryTarget);
            return;
        }

        if (_state.CD(AID.NoMercy) > 20 && Ammo > 0 && Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) < _state.GCD)
            PushGCD(AID.GnashingFang, primaryTarget);

        if (NumAOETargets > 1 && Unlocked(AID.DemonSlice))
        {
            if (ShouldBust(strategy, AID.BurstStrike))
            {
                if (Unlocked(AID.FatedCircle))
                    PushGCD(AID.FatedCircle, Player);

                PushGCD(AID.BurstStrike, primaryTarget);
            }

            if (ComboLastMove == AID.BrutalShell && Unlocked(AID.SolidBarrel))
                PushGCD(AID.SolidBarrel, primaryTarget);

            if (ComboLastMove == AID.DemonSlice && Unlocked(AID.DemonSlaughter))
                PushGCD(AID.DemonSlaughter, Player);

            PushGCD(AID.DemonSlice, Player);
        }
        else
        {
            if (ShouldBust(strategy, AID.BurstStrike))
                PushGCD(AID.BurstStrike, primaryTarget);

            if (ComboLastMove == AID.DemonSlice && Unlocked(AID.DemonSlaughter) && NumAOETargets > 0)
                PushGCD(AID.DemonSlaughter, Player);

            if (ComboLastMove == AID.BrutalShell && Unlocked(AID.SolidBarrel))
                PushGCD(AID.SolidBarrel, primaryTarget);

            if (ComboLastMove == AID.KeenEdge && Unlocked(AID.BrutalShell))
                PushGCD(AID.BrutalShell, primaryTarget);

            PushGCD(AID.KeenEdge, primaryTarget);
        }
    }

    private bool ShouldBust(StrategyValues strategy, AID spend)
    {
        if (!Unlocked(spend) || Ammo == 0)
            return false;

        if (NoMercy > _state.GCD)
            return _state.CD(AID.DoubleDown) > NoMercy || Ammo == MaxAmmo;

        return ComboLastMove is AID.BrutalShell or AID.DemonSlice && Ammo == MaxAmmo;
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (Continuation)
            PushOGCD(AID.Continuation, primaryTarget);

        var usedNM = _state.CD(AID.NoMercy) > 20;

        if (ShouldNoMercy(strategy, deadline))
            PushOGCD(AID.NoMercy, Player);

        if (usedNM)
        {
            if (Unlocked(AID.Bloodfest) && _state.CanWeave(AID.Bloodfest, 0.6f, deadline) && Ammo == 0)
                PushOGCD(AID.Bloodfest, primaryTarget);

            if (Unlocked(AID.BlastingZone) && _state.CanWeave(AID.BlastingZone, 0.6f, deadline))
                PushOGCD(AID.BlastingZone, primaryTarget);

            if (Unlocked(AID.BowShock) && _state.CanWeave(AID.BowShock, 0.6f, deadline) && NumAOETargets > 0)
                PushOGCD(AID.BowShock, Player);
        }
    }

    private bool ShouldNoMercy(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.NoMercy) || !_state.CanWeave(AID.NoMercy, 0.6f, deadline))
            return false;

        if (FastGCD)
        {
            if (CombatTimer < 10)
                return ComboLastMove == AID.BrutalShell || NumAOETargets > 1;

            return true;
        }
        else
        {
            return Ammo > 0 && _state.GCD < 1.1f;
        }
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        AmmoCombo = gauge.AmmoComboStep;

        SonicBreak = StatusLeft(SID.ReadyToBreak);
        Continuation = Player.Statuses.Any(s => IsContinuationStatus((SID)s.ID));
        NoMercy = StatusLeft(SID.NoMercy);

        NumAOETargets = NumMeleeAOETargets(strategy);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private bool IsContinuationStatus(SID sid) => sid is SID.ReadyToBlast or SID.ReadyToRaze or SID.ReadyToGouge or SID.ReadyToTear or SID.ReadyToRip;
}
