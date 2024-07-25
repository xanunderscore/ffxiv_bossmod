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
    public float Reign;

    public float SonicBreak;
    public bool Continuation;
    public float NoMercy;

    public int NumAOETargets;
    public int NumReignTargets;

    private Actor? BestReignTarget;

    public bool FastGCD => _state.AttackGCDTime <= 2.47f;
    public int MaxAmmo => Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
            return;

        if (_state.CD(AID.NoMercy) > 20 && Ammo > 0 && Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) < _state.GCD)
            PushGCD(AID.GnashingFang, primaryTarget);

        if (NumAOETargets > 0 && Ammo >= 2 && _state.CD(AID.DoubleDown) < _state.GCD && NoMercy > _state.GCD)
            PushGCD(AID.DoubleDown, Player);

        if (SonicBreak > _state.GCD)
            PushGCD(AID.SonicBreak, primaryTarget);

        switch (AmmoCombo)
        {
            case 1:
                PushGCD(AID.SavageClaw, primaryTarget);
                return;
            case 2:
                PushGCD(AID.WickedTalon, primaryTarget);
                return;
            case 3:
                PushGCD(AID.NobleBlood, BestReignTarget);
                return;
            case 4:
                PushGCD(AID.LionHeart, BestReignTarget);
                return;
        }

        if (Reign > _state.GCD && _state.CD(AID.GnashingFang) > 0 && _state.CD(AID.DoubleDown) > 0 && SonicBreak == 0)
            PushGCD(AID.ReignOfBeasts, BestReignTarget);

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

    // TODO handle forced 2 cartridge burst
    private bool ShouldBust(StrategyValues strategy, AID spend)
    {
        if (!Unlocked(spend) || Ammo == 0)
            return false;

        if (NoMercy > _state.GCD)
            return _state.CD(AID.DoubleDown) > NoMercy || Ammo == MaxAmmo || (Ammo == 1 && _state.CD(AID.DoubleDown) < NoMercy);

        return ComboLastMove is AID.BrutalShell or AID.DemonSlice && Ammo == MaxAmmo;
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (Continuation)
            PushOGCD(AID.Continuation, primaryTarget);

        if (strategy.BuffsOk() && Unlocked(AID.Bloodfest) && _state.CanWeave(AID.Bloodfest, 0.6f, deadline) && Ammo == 0)
            PushOGCD(AID.Bloodfest, primaryTarget);

        var usedNM = _state.CD(AID.NoMercy) > 20;

        if (ShouldNoMercy(strategy, deadline))
            PushOGCD(AID.NoMercy, Player);

        if (usedNM)
        {
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

        Reign = StatusLeft(SID.ReadyToReign);
        SonicBreak = StatusLeft(SID.ReadyToBreak);
        Continuation = Player.Statuses.Any(s => IsContinuationStatus((SID)s.ID));
        NoMercy = StatusLeft(SID.NoMercy);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestReignTarget, NumReignTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private bool IsContinuationStatus(SID sid) => sid is SID.ReadyToBlast or SID.ReadyToRaze or SID.ReadyToGouge or SID.ReadyToTear or SID.ReadyToRip;
}
