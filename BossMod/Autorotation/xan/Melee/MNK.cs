using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class MNK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Potion = SharedTrack.Count, SSS }
    public enum PotionStrategy
    {
        Manual,
        PreBuffs,
        Now
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan MNK", "Monk", "xan", RotationModuleQuality.Good, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineShared().AddAssociatedActions(AID.RiddleOfFire, AID.RiddleOfWind, AID.Brotherhood);
        def.Define(Track.Potion).As<PotionStrategy>("Pot")
            .AddOption(PotionStrategy.Manual, "Do not automatically use")
            .AddOption(PotionStrategy.PreBuffs, "Use ~4 GCDs before raid buff window")
            .AddOption(PotionStrategy.Now, "Use ASAP");

        def.DefineSimple(Track.SSS, "SixSidedStar");

        return def;
    }

    public enum Form { None, OpoOpo, Raptor, Coeurl }

    public int Chakra; // 0-5 (0-10 during Brotherhood)
    public BeastChakraType[] BeastChakra = [];
    public int OpoStacks; // 0-1
    public int RaptorStacks; // 0-1
    public int CoeurlStacks; // 0-2
    public NadiFlags Nadi;

    public Form CurrentForm;
    public float FormLeft; // 0 if no form, 30 max

    public float BlitzLeft; // 20 max
    public float PerfectBalanceLeft; // 20 max
    public float FormShiftLeft; // 30 max
    public float BrotherhoodLeft; // 20 max
    public float FireLeft; // 20 max
    public float EarthsReplyLeft; // 30 max, probably doesnt belong in autorotation
    public float FiresReplyLeft; // 20 max
    public float WindsReplyLeft; // 15 max

    public int NumBlitzTargets;
    public int NumAOETargets;
    public int NumLineTargets;

    private Actor? BestBlitzTarget;
    private Actor? BestRangedTarget; // fire's reply
    private Actor? BestLineTarget; // enlightenment, wind's reply

    public bool HasLunar => Nadi.HasFlag(NadiFlags.Lunar);
    public bool HasSolar => Nadi.HasFlag(NadiFlags.Solar);
    public bool HasBothNadi => HasLunar && HasSolar;

    protected override float GetCastTime(AID aid) => 0;

    private (AID action, bool isTargeted) GetCurrentBlitz()
    {
        if (BeastCount != 3)
            return (AID.None, false);

        if (HasBothNadi)
            return (Unlocked(AID.PhantomRush) ? AID.PhantomRush : AID.TornadoKick, true);

        var bc = BeastChakra;
        if (bc[0] == bc[1] && bc[1] == bc[2])
            return (Unlocked(AID.ElixirBurst) ? AID.ElixirBurst : AID.ElixirField, false);
        if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
            return (Unlocked(AID.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike, false);
        return (AID.CelestialRevolution, true);
    }

    public int BeastCount => BeastChakra.Count(x => x != BeastChakraType.None);
    public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
    public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

    public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

    // TODO incorporate crit calculation - rockbreaker is a gain on 3 at 22.1% crit
    public int AOEBreakpoint => EffectiveForm == Form.OpoOpo ? 3 : 4;
    public bool UseAOE => NumAOETargets >= AOEBreakpoint;

    private (Positional, bool) NextPositional
    {
        get
        {
            if (UseAOE)
                return (Positional.Any, false);

            var pos = CoeurlStacks > 0 ? Positional.Flank : Positional.Rear;
            var imm = EffectiveForm == Form.Coeurl && NextGCD != AID.WindsReply;

            return (pos, imm);
        }
    }

    public enum GCDPriority
    {
        None = -1,
        WindRanged = 100,
        Basic = 300,
        AOE = 400,
        SSS = 500,
        FiresReply = 700,
        WindsReply = 800,
        Blitz = 900,
    }

    // some monk OGCDs will be queued with higher prio than what user presses manually - the rotation is very drift-sensitive and monk has much less time to weave than other classes do
    public enum OGCDPriority
    {
        None = -1,
        TrueNorth = 100,
        TFC = 150,
        Potion = 200,
        RiddleOfWind = 300,
        ManualOGCD = 1901, // included for reference, not used here - actual value is 1901 + Low (2000) + 100 (in base class) = 4001
        RiddleOfFire = 1910,
        Brotherhood = 1915,
        PerfectBalance = 1920
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = GetGauge<MonkGauge>();

        Chakra = gauge.Chakra;
        BeastChakra = gauge.BeastChakra;
        BlitzLeft = gauge.BlitzTimeRemaining / 1000f;
        Nadi = gauge.Nadi;

        OpoStacks = gauge.OpoOpoStacks;
        RaptorStacks = gauge.RaptorStacks;
        CoeurlStacks = gauge.CoeurlStacks;

        PerfectBalanceLeft = StatusLeft(SID.PerfectBalance);
        FormShiftLeft = StatusLeft(SID.FormlessFist);
        FireLeft = StatusLeft(SID.RiddleOfFire);
        WindsReplyLeft = StatusLeft(SID.WindsRumination);
        FiresReplyLeft = StatusLeft(SID.FiresRumination);
        EarthsReplyLeft = StatusLeft(SID.EarthsRumination);
        BrotherhoodLeft = StatusLeft(SID.Brotherhood);
        (var currentBlitz, var currentBlitzIsTargeted) = GetCurrentBlitz();

        NumAOETargets = NumMeleeAOETargets(strategy);

        if (BlitzLeft > GCD)
        {
            if (currentBlitzIsTargeted)
                (BestBlitzTarget, NumBlitzTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);
            else
            {
                BestBlitzTarget = Player;
                NumBlitzTargets = NumAOETargets;
            }
        }
        else
        {
            BestBlitzTarget = Player;
            NumBlitzTargets = 0;
        }

        (CurrentForm, FormLeft) = DetermineForm();

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget).Best;
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, IsEnlightenmentTarget);

        UpdatePositionals(primaryTarget, NextPositional, TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (Chakra < 5 && Unlocked(AID.SteeledMeditation) && (!Player.InCombat || primaryTarget == null))
            PushGCD(AID.SteeledMeditation, Player);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 2 and < 11.8f && FormShiftLeft == 0)
                PushGCD(AID.FormShift, Player);

            if (CountdownRemaining < 0.4 && Player.DistanceToHitbox(primaryTarget) is > 3 and < 25)
                PushGCD(AID.Thunderclap, primaryTarget);

            return;
        }

        if (NumBlitzTargets > 0)
            PushGCD(currentBlitz, BestBlitzTarget, GCDPriority.Blitz);

        if (FiresReplyLeft > GCD && CurrentForm == Form.Raptor)
            PushGCD(AID.FiresReply, BestRangedTarget, GCDPriority.FiresReply);

        WindsReply();

        if (UseAOE)
        {
            if (EffectiveForm == Form.Coeurl)
                PushGCD(AID.Rockbreaker, Player, GCDPriority.AOE);

            if (EffectiveForm == Form.Raptor)
                PushGCD(AID.FourPointFury, Player, GCDPriority.AOE);

            PushGCD(AID.ArmOfTheDestroyer, Player, GCDPriority.AOE);
        }

        switch (EffectiveForm)
        {
            case Form.Coeurl:
                PushGCD(CoeurlStacks == 0 && Unlocked(AID.Demolish) ? AID.Demolish : AID.SnapPunch, primaryTarget, GCDPriority.Basic); break;
            case Form.Raptor:
                PushGCD(RaptorStacks == 0 && Unlocked(AID.TwinSnakes) ? AID.TwinSnakes : AID.TrueStrike, primaryTarget, GCDPriority.Basic); break;
            default:
                PushGCD(OpoStacks == 0 && Unlocked(AID.DragonKick) ? AID.DragonKick : AID.Bootshine, primaryTarget, GCDPriority.Basic); break;
        }

        switch (strategy.Simple(Track.SSS))
        {
            case OffensiveStrategy.Force:
                PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS);
                break;
            case OffensiveStrategy.Automatic:
                if (!CanFitGCD(DowntimeIn - SSSApplicationDelay, 1))
                    PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS);
                break;
        }
    }

    private const float SSSApplicationDelay = 0.62f;

    private Form EffectiveForm
    {
        get
        {
            if (PerfectBalanceLeft == 0)
                return CurrentForm;

            // hack: allow double lunar opener - only in boss fights
            // trash packs should get regular lunar solar
            var forceDoubleLunar = CombatTimer < 30 && NumAOETargets < 3;
            var forcedSolar = ForcedSolar || HasLunar && !HasSolar && !forceDoubleLunar;

            var canCoeurl = forcedSolar;
            var canRaptor = forcedSolar;
            var canOpo = true;

            foreach (var chak in BeastChakra)
            {
                canCoeurl &= chak != BeastChakraType.Coeurl;
                canRaptor &= chak != BeastChakraType.Raptor;
                if (ForcedSolar)
                    canOpo &= chak != BeastChakraType.OpoOpo;
            }

            return canRaptor ? Form.Raptor : canCoeurl ? Form.Coeurl : Form.OpoOpo;
        }
    }

    private void QueuePB(StrategyValues strategy)
    {
        if (CurrentForm != Form.Raptor || BeastChakra[0] != BeastChakraType.None || FiresReplyLeft > GCD)
            return;

        // prevent odd window double blitz
        // TODO figure out the actual mathematical equation that differentiates odd windows, this is stupid
        if (BrotherhoodLeft == 0 && CD(AID.PerfectBalance) > 30)
            return;

        if (CanWeave(AID.RiddleOfFire, 3) || CanFitGCD(FireLeft, 3))
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || GCD == 0 || primaryTarget == null)
            return;

        if (strategy.Option(Track.Potion).As<PotionStrategy>() == PotionStrategy.Now)
            Potion();

        if (strategy.BuffsOk())
        {
            if (strategy.Option(Track.Potion).As<PotionStrategy>() == PotionStrategy.PreBuffs && CanWeave(AID.Brotherhood, 4))
                Potion();

            QueuePB(strategy);

            if (CombatTimer >= 10 || BeastCount == 2)
                PushOGCD(AID.Brotherhood, Player, OGCDPriority.Brotherhood);

            if (ShouldRoF)
                PushOGCD(AID.RiddleOfFire, Player, OGCDPriority.RiddleOfFire, GCD - EarliestRoF(AnimationLockDelay));

            if (CD(AID.RiddleOfFire) > 0)
                PushOGCD(AID.RiddleOfWind, Player, OGCDPriority.RiddleOfWind);

            if (NextPositionalImminent && !NextPositionalCorrect)
                PushOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth, ShouldRoF ? 0 : GCD - 0.8f);
        }

        if (Chakra >= 5)
        {
            if (NumLineTargets >= 3)
                PushOGCD(AID.HowlingFist, BestLineTarget, OGCDPriority.TFC);

            PushOGCD(AID.SteelPeak, primaryTarget, OGCDPriority.TFC);
        }
    }

    private void WindsReply()
    {
        if (WindsReplyLeft <= GCD || PerfectBalanceLeft > GCD || BlitzLeft > GCD)
            return;

        var prio = GCDPriority.WindRanged;

        // use early during buffs, or use now if about to expire
        if (FireLeft > GCD || !CanFitGCD(WindsReplyLeft, 1))
            prio = GCDPriority.WindsReply;

        PushGCD(AID.WindsReply, BestLineTarget, prio);
    }

    private float DesiredFireWindow => GCDLength * 10;
    private float EarliestRoF(float estimatedDelay) => MathF.Max(estimatedDelay + 0.6f, 20.6f - DesiredFireWindow);

    private void Potion() => Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Low + 100 + (float)OGCDPriority.Potion);

    private bool ShouldRoF => CanWeave(AID.RiddleOfFire) && !CanWeave(AID.Brotherhood);

    private bool IsEnlightenmentTarget(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);

    private (Form, float) DetermineForm()
    {
        if (PerfectBalanceLeft > 0)
            return (Form.None, 0);

        var s = StatusLeft(SID.OpoOpoForm);
        if (s > 0)
            return (Form.OpoOpo, s);
        s = StatusLeft(SID.RaptorForm);
        if (s > 0)
            return (Form.Raptor, s);
        s = StatusLeft(SID.CoeurlForm);

        return s > 0 ? (Form.Coeurl, s) : (Form.None, 0);
    }
}
