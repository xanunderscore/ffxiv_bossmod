using BossMod.VPR;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;

public sealed class VPR(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
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

    public DreadCombo DreadCombo;
    public int Coil; // max 3
    public int Offering; // max 100
    public int Anguine; // 0-4

    public float DeathRattleLeft;
    public float LastLashLeft;
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
    private float GnashRefreshTimer => _state.AttackGCDTime * 3;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
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

        if (Coil == CoilMax)
            PushGCD(AID.UncoiledFury, BestRangedAOETarget);

        if (Anguine > 0)
            PushGCD(Anguine switch
            {
                1 => AID.FourthGeneration,
                2 => AID.ThirdGeneration,
                3 => AID.SecondGeneration,
                4 => AID.FirstGeneration,
                _ => throw new NotImplementedException("unreachable")
            }, BestGenerationTarget);

        if (Unlocked(AID.Reawaken) && (ReawakenReady > _state.GCD || Offering >= 50) && NumAOETargets > 0)
            PushGCD(AID.Reawaken, Player);

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

            if (_state.ComboLastAction is (uint)AID.HuntersBite or (uint)AID.SwiftskinsBite)
            {
                if (GrimskinsVenomLeft > _state.GCD)
                    PushGCD(AID.BloodiedMaw, Player);

                PushGCD(AID.JaggedMaw, Player);
            }

            if (_state.ComboLastAction is (uint)AID.SteelMaw or (uint)AID.DreadMaw)
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

            if (_state.ComboLastAction is (uint)AID.HuntersSting)
            {
                if (FlankstungVenomLeft > _state.GCD)
                    PushGCD(AID.FlankstingStrike, primaryTarget);

                PushGCD(AID.FlanksbaneFang, primaryTarget);
            }

            if (_state.ComboLastAction is (uint)AID.SwiftskinsSting)
            {
                if (HindstungVenomLeft > _state.GCD)
                    PushGCD(AID.HindstingStrike, primaryTarget);

                PushGCD(AID.HindsbaneFang, primaryTarget);
            }

            if (_state.ComboLastAction is (uint)AID.SteelFangs or (uint)AID.DreadFangs)
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
        if (LastLashLeft > deadline && _state.CanWeave(AID.SerpentsTail, 0.6f, deadline))
            PushOGCD(AID.LastLash, Player);

        if (DeathRattleLeft > deadline && _state.CanWeave(AID.SerpentsTail, 0.6f, deadline))
            PushOGCD(AID.DeathRattle, primaryTarget);

        if (FellhuntersVenomLeft > deadline && _state.CanWeave(AID.TwinfangThresh, 0.6f, deadline))
            PushOGCD(AID.TwinfangThresh, Player);

        if (FellskinsVenomLeft > deadline && _state.CanWeave(AID.TwinbloodThresh, 0.6f, deadline))
            PushOGCD(AID.TwinbloodThresh, Player);

        if (HuntersVenomLeft > deadline && _state.CanWeave(AID.TwinfangBite, 0.6f, deadline))
            PushOGCD(AID.TwinfangBite, primaryTarget);

        if (SwiftskinsVenomLeft > deadline && _state.CanWeave(AID.TwinbloodBite, 0.6f, deadline))
            PushOGCD(AID.TwinbloodBite, primaryTarget);

        if (Unlocked(AID.SerpentsIre) && Coil < CoilMax && _state.CanWeave(AID.SerpentsIre, 0.6f, deadline))
            PushOGCD(AID.SerpentsIre, Player);
    }

    private (Positional, bool) GetPositional()
    {
        if (DreadCombo == DreadCombo.Dreadwinder)
            return (SwiftscaledLeft < InstinctLeft ? Positional.Rear : Positional.Flank, true);

        if (DreadCombo == DreadCombo.HuntersCoil)
            return (Positional.Rear, true);

        if (DreadCombo == DreadCombo.SwiftskinsCoil)
            return (Positional.Flank, true);

        return (AID)_state.ComboLastAction switch
        {
            AID.HuntersSting => (Positional.Flank, true),
            AID.SwiftskinsSting => (Positional.Rear, true),
            _ => (SwiftscaledLeft < InstinctLeft ? Positional.Rear : Positional.Flank, false)
        };
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        var track = strategy.Option(Track.Targeting);
        SelectPrimaryTarget(track, ref primaryTarget, 3);
        _state.UpdateCommon(primaryTarget);

        var gauge = Service.JobGauges.Get<VPRGauge>();
        DreadCombo = gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;

        DeathRattleLeft = StatusLeft(SID.DeathRattleReady);
        LastLashLeft = StatusLeft(SID.LastLashReady);
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
        NumNearbyGnashlessEnemies = Hints.PriorityTargets.Count(x => x.Actor.DistanceTo(Player) <= 5 && GnashLeft(x.Actor) < GnashRefreshTimer);

        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(track, primaryTarget, 20, NumSplashTargets);
        BestGenerationTarget = SelectTarget(track, primaryTarget, 3, NumSplashTargets).Best;
        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => NumMeleeAOETargets(),
            _ => 0
        };

        _state.UpdatePositionals(primaryTarget, GetPositional(), false);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private float GnashLeft(Actor? a) => _state.StatusDetails(a, SID.NoxiousGnash, Player.InstanceID).Left;
}
