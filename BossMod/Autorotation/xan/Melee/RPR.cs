using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public enum GCDPriority
{
    None = 0
}

public sealed class RPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, GCDPriority>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("RPR", "Reaper", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.RPR), 100);

        def.DefineShared().AddAssociatedActions(AID.ArcaneCircle);

        return def;
    }

    public int Soul;
    public int Shroud;
    public bool Soulsow;
    public float EnshroudLeft;
    public int Lemure;
    public int Void;
    public float SoulReaver;
    public float EnhancedGallows;
    public float EnhancedGibbet;
    public (float Left, int Stacks) ImmortalSacrifice;
    public float BloodsownCircle;
    public float IdealHost;
    public float EnhancedVoidReaping;
    public float EnhancedCrossReaping;
    public float EnhancedHarpe;
    public float Oblatio;

    public float TargetDDLeft;
    public int NumNearbyUndeathedEnemies;

    public int NumAOETargets; // melee
    public int NumRangedAOETargets; // gluttony, communio
    public int NumConeTargets; // grim swathe, guillotine
    public int NumLineTargets; // plentiful harvest

    private Actor? BestRangedAOETarget;
    private Actor? BestConeTarget;
    private Actor? BestLineTarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = GetGauge<ReaperGauge>();

        Soul = gauge.Soul;
        Shroud = gauge.Shroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;
        Lemure = gauge.LemureShroud;
        Void = gauge.VoidShroud;

        Soulsow = Player.FindStatus(SID.Soulsow) != null;
        SoulReaver = StatusLeft(SID.SoulReaver);
        EnhancedGallows = StatusLeft(SID.EnhancedGallows);
        EnhancedGibbet = StatusLeft(SID.EnhancedGibbet);
        ImmortalSacrifice = Status(SID.ImmortalSacrifice);
        BloodsownCircle = StatusLeft(SID.BloodsownCircle);
        IdealHost = StatusLeft(SID.IdealHost);
        EnhancedVoidReaping = StatusLeft(SID.EnhancedVoidReaping);
        EnhancedCrossReaping = StatusLeft(SID.EnhancedCrossReaping);
        EnhancedHarpe = StatusLeft(SID.EnhancedHarpe);
        Oblatio = StatusLeft(SID.Oblatio);

        TargetDDLeft = DDLeft(primaryTarget);
        NumNearbyUndeathedEnemies = AdjustNumTargets(strategy, Hints.PriorityTargets.Count(x => Player.DistanceToHitbox(x.Actor) <= 5 && !CanFitGCD(DDLeft(x.Actor), 1)));

        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees()));
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        UpdatePositionals(primaryTarget, GetNextPositional(primaryTarget), TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (!Soulsow && !Hints.PriorityTargets.Any())
            PushGCD(AID.SoulSow, Player);

        // dd refresh
        if (NumNearbyUndeathedEnemies > 1)
            PushGCD(AID.WhorlofDeath, Player);

        if (!CanFitGCD(TargetDDLeft, 1))
            PushGCD(AID.ShadowofDeath, primaryTarget);

        if (SoulReaver > GCD)
        {
            if (NumConeTargets > 2)
                PushGCD(AID.Guillotine, BestConeTarget);

            if (primaryTarget != null)
            {
                if (EnhancedGallows > GCD)
                    PushGCD(AID.Gallows, primaryTarget);
                else if (EnhancedGibbet > GCD)
                    PushGCD(AID.Gibbet, primaryTarget);
                else if (GetCurrentPositional(primaryTarget!) == Positional.Rear)
                    PushGCD(AID.Gallows, primaryTarget);
                else
                    PushGCD(AID.Gibbet, primaryTarget);
            }
        }

        if (Lemure > 0)
        {
            if (Lemure == 1 && Unlocked(AID.Communio))
                PushGCD(AID.Communio, BestRangedAOETarget);
            else
            {
                if (NumConeTargets > 2)
                    PushGCD(AID.GrimReaping, BestConeTarget);

                PushGCD(EnhancedCrossReaping > GCD ? AID.CrossReaping : AID.VoidReaping, primaryTarget);
            }
        }

        if (ImmortalSacrifice.Stacks > 0 && BloodsownCircle == 0)
            PushGCD(AID.PlentifulHarvest, BestLineTarget);

        if (Soul <= 50)
        {
            if (NumConeTargets > 2)
                PushGCD(AID.SoulScythe, BestConeTarget);

            PushGCD(AID.SoulSlice, primaryTarget);
        }

        if (NumAOETargets > 2 && Unlocked(AID.SpinningScythe))
        {
            if (ComboLastMove == AID.SpinningScythe)
                PushGCD(AID.NightmareScythe, Player);

            PushGCD(AID.SpinningScythe, Player);
        }
        else
        {
            if (ComboLastMove == AID.WaxingSlice)
                PushGCD(AID.InfernalSlice, primaryTarget);

            if (ComboLastMove == AID.Slice)
                PushGCD(AID.WaxingSlice, primaryTarget);

            PushGCD(AID.Slice, primaryTarget);
        }

        if (Soulsow)
            PushGCD(AID.HarvestMoon, BestRangedAOETarget);

        if (EnhancedHarpe > GCD)
            PushGCD(AID.Harpe, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (strategy.BuffsOk())
        {
            // wait for soul slice in opener
            if (CD(AID.SoulSlice) > 0 || CombatTimer > 20)
                PushOGCD(AID.ArcaneCircle, Player, delay: GCD - 1.8f);
        }

        if (SoulReaver > 0)
            return;

        if (Oblatio > 0)
            PushOGCD(AID.Sacrificium, BestRangedAOETarget);

        if (Void >= 2)
            PushOGCD(AID.LemuresSlice, primaryTarget);

        // TODO: if Perfectio Occulta or Perfectio Parata, add 1
        var shroudComboEndLength = 1;

        if ((Shroud >= 50 || IdealHost > 0) && CanFitGCD(TargetDDLeft - 6, shroudComboEndLength))
            PushOGCD(AID.Enshroud, Player);

        if (Soul >= 50 && CanFitGCD(TargetDDLeft, 2))
            PushOGCD(AID.Gluttony, BestRangedAOETarget);

        if (Soul >= 50 && CanFitGCD(TargetDDLeft, 1))
        {
            // terrible abuse of "CanWeave", treating the Bloodsown Circle status as if it were a cooldown, since it prevents us from casting Plentiful Harvest
            var willHarvest = ImmortalSacrifice.Stacks > 0 && CanWeave(BloodsownCircle, 0.6f, 1);
            if (!willHarvest)
                PushOGCD(AID.BloodStalk, primaryTarget);
        }
    }

    protected override float GetCastTime(AID aid) => aid == AID.Harpe && EnhancedHarpe > GCD ? 0 : base.GetCastTime(aid);

    private (Positional, bool) GetNextPositional(Actor? primaryTarget)
    {
        if (primaryTarget == null || !Unlocked(AID.Gibbet) || NumConeTargets > 2)
            return (Positional.Any, false);

        Positional nextPos;

        if (EnhancedGallows > 0)
            nextPos = Positional.Rear;
        else if (EnhancedGibbet > 0)
            nextPos = Positional.Flank;
        else
        {
            var closest = GetCurrentPositional(primaryTarget);
            nextPos = closest == Positional.Front ? Positional.Flank : closest;
        }

        return (nextPos, SoulReaver > GCD);
    }

    private float DDLeft(Actor? target) => StatusDetails(target, SID.DeathsDesign, Player.InstanceID, 30).Left;
}
