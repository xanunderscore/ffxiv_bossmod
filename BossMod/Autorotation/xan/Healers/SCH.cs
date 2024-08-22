using BossMod.SCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class SCH(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public enum Track { Place = SharedTrack.Count }
    public enum FairyPlacement
    {
        Manual,
        Heel,
        PlaceArena
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SCH", "Scholar", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SCH, Class.ACN), 100);

        def.DefineShared().AddAssociatedActions(AID.ChainStratagem, AID.Dissipation);

        def.Define(Track.Place).As<FairyPlacement>("FairyPlace", "Fairy placement")
            .AddOption(FairyPlacement.Manual, "Do not automatically move fairy")
            .AddOption(FairyPlacement.Heel, "Order fairy to follow player")
            .AddOption(FairyPlacement.PlaceArena, "Place fairy at current arena center, if one exists");

        return def;
    }

    public int Aetherflow;
    public int FairyGauge;
    public float SeraphTimer;
    public bool FairyGone;

    public float ImpactImminent;
    public float TargetDotLeft;
    public int NumAOETargets;
    public int NumRangedAOETargets;

    public enum PetOrder
    {
        None = 0,
        Follow = 2,
        Place = 3
    }

    public PetOrder FairyOrder;

    private Actor? Eos;
    private Actor? BestDotTarget;
    private Actor? BestRangedAOETarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = GetGauge<ScholarGauge>();
        Aetherflow = gauge.Aetherflow;
        FairyGauge = gauge.FairyGauge;
        SeraphTimer = gauge.SeraphTimer * 0.001f;
        FairyGone = gauge.DismissedFairy > 0;

        var pet = World.Client.ActivePet;

        Eos = pet.InstanceID == 0xE0000000 ? null : World.Actors.Find(pet.InstanceID);

        FairyOrder = (PetOrder)pet.Order;

        ImpactImminent = StatusLeft(SID.ImpactImminent);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        NumAOETargets = NumMeleeAOETargets(strategy);

        if (Eos == null && !FairyGone)
            PushGCD(AID.SummonEos, Player);

        OGCD(strategy, primaryTarget);

        if (Eos != null)
        {
            switch (strategy.Option(Track.Place).As<FairyPlacement>())
            {
                case FairyPlacement.Manual:
                    break;
                case FairyPlacement.Heel:
                    if (FairyOrder != PetOrder.Follow)
                        Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 2), null, ActionQueue.Priority.High);
                    break;
                case FairyPlacement.PlaceArena:
                    if (FairyOrder != PetOrder.Place)
                        Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 3), null, ActionQueue.Priority.High, targetPos: Player.PosRot.XYZ() + new Vector3(20, 0, 0));
                    break;
            }
        }

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.Broil1))
                PushGCD(AID.Broil1, primaryTarget);

            return;
        }

        if (!CanFitGCD(TargetDotLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        if (RaidBuffsLeft > 0 && !CanFitGCD(RaidBuffsLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        if (Unlocked(AID.ArtOfWar1) && !Unlocked(AID.Broil1))
            Hints.RecommendedRangeToTarget = 4.9f - Player.HitboxRadius;

        var needAOETargets = Unlocked(AID.Broil1) ? 2 : 1;

        if (NumAOETargets >= needAOETargets)
            PushGCD(AID.ArtOfWar1, Player);

        PushGCD(AID.Ruin1, primaryTarget);

        // instant cast - fallback for movement
        PushGCD(AID.Ruin2, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.BuffsOk())
        {
            if (Eos != null)
                PushOGCD(AID.Dissipation, Player);

            PushOGCD(AID.ChainStratagem, primaryTarget);
        }

        if (Aetherflow == 0)
            PushOGCD(AID.Aetherflow, Player);

        if (Aetherflow > 0 && CanWeave(AID.Aetherflow, Aetherflow))
            PushOGCD(AID.EnergyDrain, primaryTarget);

        if (ImpactImminent > 0)
            PushOGCD(AID.BanefulImpaction, BestRangedAOETarget);

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);
    }

    static readonly SID[] DotStatus = [SID.Bio1, SID.Bio2, SID.Biolysis];

    private float DotDuration(Actor? x)
    {
        if (x == null)
            return float.MaxValue;

        foreach (var stat in DotStatus)
        {
            var dur = StatusDetails(x, (uint)stat, Player.InstanceID).Left;
            if (dur > 0)
                return dur;
        }

        return 0;
    }
}
