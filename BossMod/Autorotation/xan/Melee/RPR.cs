using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class RPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("RPR", "Reaper", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.RPR), 100);

        def.DefineShared().AddAssociatedActions(AID.ArcaneCircle);

        return def;
    }

    public int RedGauge;
    public int BlueGauge;
    public bool Soulsow;
    public float EnshroudLeft;
    public int BlueSouls;
    public int PurpleSouls;
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
    public float ShortestNearbyDDLeft;

    public int NumAOETargets; // melee
    public int NumRangedAOETargets; // gluttony, communio
    public int NumConeTargets; // grim swathe, guillotine
    public int NumLineTargets; // plentiful harvest

    private Actor? BestRangedAOETarget;
    private Actor? BestConeTarget;
    private Actor? BestLineTarget;

    public enum GCDPriority
    {
        None = 0,
        Soulsow = 1,
        EnhancedHarpe = 100,
        HarvestMoon = 150,
        Filler = 200,
        FillerAOE = 201,
        SoulSlice = 300,
        DDExtend = 600,
        Harvest = 650,
        Reaver = 700,
        Lemure = 700,
        Communio = 750,
        DDExpiring = 900,
    }

    private bool Enshrouded => BlueSouls > 0;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = GetGauge<ReaperGauge>();

        RedGauge = gauge.Soul;
        BlueGauge = gauge.Shroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;
        BlueSouls = gauge.LemureShroud;
        PurpleSouls = gauge.VoidShroud;

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

        var primaryEnemy = Hints.PotentialTargets.FirstOrDefault(x => x.Actor.InstanceID == primaryTarget?.InstanceID);

        TargetDDLeft = DDLeft(primaryEnemy);
        ShortestNearbyDDLeft = float.MaxValue;

        switch (strategy.AOE())
        {
            case AOEStrategy.AOE:
            case AOEStrategy.ForceAOE:
                var nearbyDD = Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 5).Select(DDLeft);
                var minNeeded = strategy.AOE() == AOEStrategy.ForceAOE ? 1 : 2;
                if (nearbyDD.Count(x => x < 30) >= minNeeded)
                    ShortestNearbyDDLeft = nearbyDD.Min();
                break;
        }

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees()));
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        UpdatePositionals(primaryTarget, GetNextPositional(primaryTarget), TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (Soulsow)
            PushGCD(AID.HarvestMoon, BestRangedAOETarget, GCDPriority.HarvestMoon);
        else
            PushGCD(AID.SoulSow, Player, GCDPriority.Soulsow);

        if (EnhancedHarpe > GCD)
            PushGCD(AID.Harpe, primaryTarget, GCDPriority.EnhancedHarpe);

        DDRefresh(primaryTarget);

        if (SoulReaver > GCD)
        {
            if (NumConeTargets > 2)
                PushGCD(AID.Guillotine, BestConeTarget, GCDPriority.Reaver);

            if (primaryTarget != null)
            {
                if (EnhancedGallows > GCD)
                    PushGCD(AID.Gallows, primaryTarget, GCDPriority.Reaver);
                else if (EnhancedGibbet > GCD)
                    PushGCD(AID.Gibbet, primaryTarget, GCDPriority.Reaver);
                else if (GetCurrentPositional(primaryTarget!) == Positional.Rear)
                    PushGCD(AID.Gallows, primaryTarget, GCDPriority.Reaver);
                else
                    PushGCD(AID.Gibbet, primaryTarget, GCDPriority.Reaver);
            }
        }

        EnshroudGCDs(strategy, primaryTarget);

        if (ImmortalSacrifice.Stacks > 0 && BloodsownCircle == 0)
            PushGCD(AID.PlentifulHarvest, BestLineTarget, GCDPriority.Harvest);

        if (BlueSouls > 0)
            return;

        if (RedGauge <= 50)
        {
            if (NumConeTargets > 2)
                PushGCD(AID.SoulScythe, BestConeTarget, GCDPriority.SoulSlice);

            PushGCD(AID.SoulSlice, primaryTarget, GCDPriority.SoulSlice);
        }

        if (NumAOETargets > 2)
        {
            if (ComboLastMove == AID.SpinningScythe)
                PushGCD(AID.NightmareScythe, Player, GCDPriority.FillerAOE);

            PushGCD(AID.SpinningScythe, Player, GCDPriority.FillerAOE);
        }

        if (ComboLastMove == AID.WaxingSlice)
            PushGCD(AID.InfernalSlice, primaryTarget, GCDPriority.Filler);

        if (ComboLastMove == AID.Slice)
            PushGCD(AID.WaxingSlice, primaryTarget, GCDPriority.Filler);

        PushGCD(AID.Slice, primaryTarget, GCDPriority.Filler);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.BuffsOk())
        {
            // wait for soul slice in opener
            if (CD(AID.SoulSlice) > 0 || CombatTimer > 20)
                PushOGCD(AID.ArcaneCircle, Player, delay: GCD - 1.6f);
        }

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, delay: GCD - 0.8f);

        if (SoulReaver > 0)
            return;

        if (Oblatio > 0 && (RaidBuffsLeft > 0 || CD(AID.ArcaneCircle) > EnshroudLeft))
            PushOGCD(AID.Sacrificium, BestRangedAOETarget);

        if (PurpleSouls > 1)
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.LemuresScythe, BestConeTarget);

            PushOGCD(AID.LemuresSlice, primaryTarget);
        }

        if (ShouldEnshroud(strategy))
            PushOGCD(AID.Enshroud, Player);

        UseSoul(strategy, primaryTarget);
    }

    private void DDRefresh(Actor? primaryTarget)
    {
        void Extend(float timer, AID action, Actor? target)
        {
            if (!CanFitGCD(timer, CanWeave(AID.Gluttony) ? 2 : 1))
                PushGCD(action, target, GCDPriority.DDExpiring);

            if (timer < 30 && CanWeave(AID.ArcaneCircle, 1) && CD(AID.SoulSlice) > 0)
                PushGCD(action, target, GCDPriority.DDExtend);
        }

        Extend(ShortestNearbyDDLeft, AID.WhorlofDeath, Player);
        Extend(TargetDDLeft, AID.ShadowofDeath, primaryTarget);
    }

    private bool ShouldEnshroud(StrategyValues strategy)
    {
        if (Enshrouded)
            return false;

        if (IdealHost > 0)
            return true;

        if (BlueGauge < 50)
            return false;

        if (RaidBuffsLeft > GCD + 6)
            return true;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f)) // stupid fixed recast timer
            return true;

        // TODO tweak deadline, i need a simulator or something
        return CD(AID.ArcaneCircle) > 65;
    }

    private void UseSoul(StrategyValues strategy, Actor? primaryTarget)
    {
        if (RedGauge < 50 || Enshrouded)
            return;

        if (ImmortalSacrifice.Stacks > 0 && CanWeave(BloodsownCircle, 0.6f, 1))
            return;

        var debuffLeft = Math.Min(TargetDDLeft, ShortestNearbyDDLeft);

        if (CanFitGCD(debuffLeft, 2))
            PushOGCD(AID.Gluttony, BestRangedAOETarget);

        if (CanFitGCD(debuffLeft, 1) && (RaidBuffsLeft > 0 || BlueGauge < 50) && !CanWeave(AID.Gluttony, 5))
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.GrimSwathe, BestConeTarget);

            PushOGCD(AID.BloodStalk, primaryTarget);
        }
    }

    private void EnshroudGCDs(StrategyValues strategy, Actor? primaryTarget)
    {
        if (BlueSouls == 0)
            return;

        if (BlueSouls == 1)
            PushGCD(AID.Communio, BestRangedAOETarget, GCDPriority.Communio);

        var prio = GCDPriority.Lemure;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f) && BlueSouls < 5)
            prio = GCDPriority.None;

        if (RaidBuffsLeft > 0)
            prio = GCDPriority.Lemure;

        if (NumConeTargets > 2 && prio > 0)
            PushGCD(AID.GrimReaping, BestConeTarget, prio + 1);

        PushGCD(EnhancedCrossReaping > GCD ? AID.CrossReaping : AID.VoidReaping, primaryTarget, prio);
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

    private float DDLeft(AIHints.Enemy? target)
        => (target?.ForbidDOTs ?? false)
            ? float.MaxValue
            : StatusDetails(target?.Actor, SID.DeathsDesign, Player.InstanceID, 30).Left;
}
