﻿using BossMod.VPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System.Runtime.InteropServices;

namespace BossMod.Autorotation.xan;

public sealed class VPR(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("VPR", "Viper", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.VPR), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs");

        return def;
    }

    public enum TwinType
    {
        None,
        SingleTarget,
        AOE
    }

    public DreadCombo DreadCombo;
    public int Coil; // max 3
    public int Offering; // max 100
    public int Anguine; // 0-4
    public AID CurSerpentsTail; // adjusted during reawaken and after basic combos
    public int TwinStacks; // max 2, granted by using "coil" or "den" gcds
    public TwinType TwinCombo;

    public float TargetGnashLeft;
    public float SwiftscaledLeft;
    public float InstinctLeft;
    public float FlankstungVenomLeft;
    public float FlanksbaneVenomLeft;
    public float HindstungVenomLeft;
    public float HindsbaneVenomLeft;
    public float HuntersVenomLeft;
    public float SwiftskinsVenomLeft;
    public float FellhuntersVenomLeft;
    public float FellskinsVenomLeft;
    public float GrimhuntersVenomLeft;
    public float GrimskinsVenomLeft;
    public float ReawakenReady;
    public float ReawakenLeft;

    public int NumNearbyGnashlessEnemies;
    public int NumAOETargets;
    public int NumRangedAOETargets;

    private Actor? BestRangedAOETarget;
    private Actor? BestGenerationTarget;

    private int CoilMax => Unlocked(TraitID.EnhancedVipersRattle) ? 3 : 2;
    private float GnashRefreshTimer => 8 + _state.GCD;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Unlocked(AID.Reawaken) && (ReawakenReady > _state.GCD || Offering >= 50) && NumAOETargets > 0 && ReawakenLeft == 0 && InstinctLeft > 10 && SwiftscaledLeft > 10)
        {
            var needRefresh = NumAOETargets > 2 ? NumNearbyGnashlessEnemies <= 2 : TargetGnashLeft < GnashRefreshTimer;
            if (!needRefresh)
                PushGCD(AID.Reawaken, Player);
        }

        if (DreadCombo == DreadCombo.HuntersCoil)
            PushGCD(AID.SwiftskinsCoil, primaryTarget);

        if (DreadCombo == DreadCombo.SwiftskinsCoil)
            PushGCD(AID.HuntersCoil, primaryTarget);

        if (DreadCombo == DreadCombo.Dreadwinder)
        {
            if (SwiftscaledLeft < InstinctLeft)
                PushGCD(AID.SwiftskinsCoil, primaryTarget);

            PushGCD(AID.HuntersCoil, primaryTarget);
        }

        if (DreadCombo == DreadCombo.HuntersDen)
            PushGCD(AID.SwiftskinsDen, Player);

        if (DreadCombo == DreadCombo.SwiftskinsDen)
            PushGCD(AID.HuntersDen, Player);

        if (DreadCombo == DreadCombo.PitOfDread)
        {
            if (SwiftscaledLeft < InstinctLeft)
                PushGCD(AID.SwiftskinsDen, Player);

            PushGCD(AID.HuntersDen, Player);
        }

        if (Anguine > 0)
            PushGCD(Anguine switch
            {
                1 => AID.FourthGeneration,
                2 => AID.ThirdGeneration,
                3 => AID.SecondGeneration,
                4 => AID.FirstGeneration,
                _ => AID.None
            }, BestGenerationTarget);

        if (Coil == CoilMax)
            PushGCD(AID.UncoiledFury, BestRangedAOETarget);

        // 123 combos
        // 1. 34606 steel fangs (left)
        //    34607 dread fangs (right)
        //   use right to refresh debuff, otherwise left
        //
        // 2. 34608 hunter (left) damage buff
        //    34609 swiftskin (right) haste buff
        //   pick one based on buff timer, if both are 0 then choose your favorite
        //
        // 3. 34610 flank strike (left) (combos from hunter)
        //    34612 hind strike (left) (combos from swift)
        //    34611 flank fang (right) (combos from hunter)
        //    34613 hind fang (right) (combos from swift)
        //   each action buffs the next one in a loop

        if (NumAOETargets > 2)
        {
            if (Unlocked(AID.PitOfDread) && _state.CD(AID.PitOfDread) - 40 <= _state.GCD)
                PushGCD(AID.PitOfDread, Player);

            if (ComboLastMove is AID.HuntersBite or AID.SwiftskinsBite)
            {
                if (GrimskinsVenomLeft > _state.GCD)
                    PushGCD(AID.BloodiedMaw, Player);

                PushGCD(AID.JaggedMaw, Player);
            }

            if (ComboLastMove is AID.SteelMaw or AID.DreadMaw)
            {
                if (SwiftscaledLeft < InstinctLeft)
                    PushGCD(AID.SwiftskinsBite, Player);

                PushGCD(AID.HuntersBite, Player);
            }

            if (NumNearbyGnashlessEnemies > 2 && Unlocked(AID.DreadFangs))
                PushGCD(AID.DreadMaw, primaryTarget);

            PushGCD(AID.SteelMaw, Player);
        }
        else
        {
            if (Unlocked(AID.Dreadwinder) && _state.CD(AID.Dreadwinder) - 40 <= _state.GCD)
            {
                PushGCD(AID.Dreadwinder, primaryTarget);
            }

            if (ComboLastMove is AID.HuntersSting)
            {
                if (FlankstungVenomLeft > _state.GCD)
                    PushGCD(AID.FlankstingStrike, primaryTarget);

                PushGCD(AID.FlanksbaneFang, primaryTarget);
            }

            if (ComboLastMove is AID.SwiftskinsSting)
            {
                if (HindstungVenomLeft > _state.GCD)
                    PushGCD(AID.HindstingStrike, primaryTarget);

                PushGCD(AID.HindsbaneFang, primaryTarget);
            }

            if (ComboLastMove is AID.SteelFangs or AID.DreadFangs)
            {
                if (SwiftscaledLeft < InstinctLeft)
                    PushGCD(AID.SwiftskinsSting, primaryTarget);

                PushGCD(AID.HuntersSting, primaryTarget);
            }

            if (TargetGnashLeft < GnashRefreshTimer && Unlocked(AID.DreadFangs))
                PushGCD(AID.DreadFangs, primaryTarget);

            PushGCD(AID.SteelFangs, primaryTarget);
        }

        // fallback for out of range
        if (Coil > 0)
            PushGCD(AID.UncoiledFury, BestRangedAOETarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (CurSerpentsTail != AID.SerpentsTail && _state.CanWeave(AID.SerpentsTail, 0.6f, deadline))
            PushOGCD(CurSerpentsTail, primaryTarget);

        if (TwinStacks > 0)
        {
            if (FellhuntersVenomLeft > deadline && _state.CanWeave(AID.TwinfangThresh, 0.6f, deadline))
                PushOGCD(AID.TwinfangThresh, Player);

            if (FellskinsVenomLeft > deadline && _state.CanWeave(AID.TwinbloodThresh, 0.6f, deadline))
                PushOGCD(AID.TwinbloodThresh, Player);

            if (HuntersVenomLeft > deadline && _state.CanWeave(AID.TwinfangBite, 0.6f, deadline))
                PushOGCD(AID.TwinfangBite, primaryTarget);

            if (SwiftskinsVenomLeft > deadline && _state.CanWeave(AID.TwinbloodBite, 0.6f, deadline))
                PushOGCD(AID.TwinbloodBite, primaryTarget);
        }

        if (Unlocked(AID.SerpentsIre) && Coil < CoilMax && _state.CanWeave(AID.SerpentsIre, 0.6f, deadline))
            PushOGCD(AID.SerpentsIre, Player);
    }

    private (Positional, bool) GetPositional()
    {
        if (!Unlocked(AID.FlankstingStrike))
            return (Positional.Any, false);

        if (DreadCombo == DreadCombo.Dreadwinder)
            return (SwiftscaledLeft < InstinctLeft ? Positional.Rear : Positional.Flank, true);

        if (DreadCombo == DreadCombo.HuntersCoil)
            return (Positional.Rear, true);

        if (DreadCombo == DreadCombo.SwiftskinsCoil)
            return (Positional.Flank, true);

        return ComboLastMove switch
        {
            AID.HuntersSting => (Positional.Flank, true),
            AID.SwiftskinsSting => (Positional.Rear, true),
            _ => (SwiftscaledLeft < InstinctLeft ? Positional.Rear : Positional.Flank, false)
        };
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x11)]
    private struct ViperGaugeEx
    {
        [FieldOffset(0x08)] public byte RattlingCoilStacks;
        [FieldOffset(0x0A)] public byte SerpentOffering;
        [FieldOffset(0x09)] public byte AnguineTribute;
        [FieldOffset(0x0B)] public DreadCombo DreadCombo; //Shows the previously used action of the secondary combo(s) whilst it's active
        [FieldOffset(0x10)] public byte ComboEx; // extra combo stuff
    }

    public override unsafe void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var track = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(track, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<ViperGaugeEx>();
        DreadCombo = gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;

        CurSerpentsTail = (gauge.ComboEx >> 2) switch
        {
            1 => AID.DeathRattle,
            2 => AID.LastLash,
            3 => AID.FirstLegacy,
            4 => AID.SecondLegacy,
            5 => AID.ThirdLegacy,
            6 => AID.FourthLegacy,
            _ => AID.SerpentsTail,
        };
        // this doesn't really matter because the GCDs grant unique statuses, but might as well track regardless
        TwinCombo = (gauge.ComboEx >> 2) switch
        {
            7 => TwinType.SingleTarget,
            8 => TwinType.AOE,
            _ => TwinType.None
        };

        TwinStacks = gauge.ComboEx & 3;

        FlanksbaneVenomLeft = StatusLeft(SID.FlanksbaneVenom);
        FlankstungVenomLeft = StatusLeft(SID.FlankstungVenom);
        HindsbaneVenomLeft = StatusLeft(SID.HindsbaneVenom);
        HindstungVenomLeft = StatusLeft(SID.HindstungVenom);
        SwiftscaledLeft = StatusLeft(SID.Swiftscaled);
        InstinctLeft = StatusLeft(SID.HuntersInstinct);
        HuntersVenomLeft = StatusLeft(SID.HuntersVenom);
        SwiftskinsVenomLeft = StatusLeft(SID.SwiftskinsVenom);
        FellhuntersVenomLeft = StatusLeft(SID.FellhuntersVenom);
        FellskinsVenomLeft = StatusLeft(SID.FellskinsVenom);
        GrimhuntersVenomLeft = StatusLeft(SID.GrimhuntersVenom);
        GrimskinsVenomLeft = StatusLeft(SID.GrimskinsVenom);
        ReawakenReady = StatusLeft(SID.ReawakenReady);
        ReawakenLeft = StatusLeft(SID.Reawakened);

        TargetGnashLeft = GnashLeft(primaryTarget);
        NumNearbyGnashlessEnemies = Hints.PriorityTargets.Count(x => x.Actor.DistanceToHitbox(Player) <= 5 && GnashLeft(x.Actor) < GnashRefreshTimer);

        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(track, primaryTarget, 20, IsSplashTarget);
        BestGenerationTarget = SelectTarget(track, primaryTarget, 3, IsSplashTarget).Best;
        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => Unlocked(AID.SteelMaw) ? NumMeleeAOETargets() : 0,
            _ => _state.RangeToTarget <= 5 ? 1 : 0
        };

        _state.UpdatePositionals(primaryTarget, GetPositional(), false);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private float GnashLeft(Actor? a) => _state.StatusDetails(a, SID.NoxiousGnash, Player.InstanceID).Left;
}
