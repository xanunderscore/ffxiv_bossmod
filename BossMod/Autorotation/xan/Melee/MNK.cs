using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class MNK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Potion = SharedTrack.Buffs, SSS, Meditation, FiresReply, Nadi, RoF, RoW, PB, BH, TC, Blitz, Engage }
    public enum PotionStrategy
    {
        Manual,
        PreBuffs,
        Now
    }
    public enum MeditationStrategy
    {
        Safe,
        Greedy,
        Force,
        Delay
    }
    public enum FRStrategy
    {
        Automatic,
        Ranged,
        Force,
        Delay
    }
    public enum NadiStrategy
    {
        Automatic,
        [PropertyDisplay("Lunar", 0xFFDB8BCA)]
        Lunar,
        [PropertyDisplay("Solar", 0xFF8EE6FA)]
        Solar,
        [PropertyDisplay("Lunar (downtime)", 0xFFDB8BCA)]
        LunarDowntime,
        [PropertyDisplay("Solar (downtime)", 0xFF8EE6FA)]
        SolarDowntime,
    }
    public enum RoWStrategy
    {
        Automatic,
        Force,
        Delay
    }
    public enum PBStrategy
    {
        Automatic,
        ForceOpo,
        Force,
        Delay
    }
    public enum TCStrategy
    {
        None,
        GapClose
    }
    public enum BlitzStrategy
    {
        Automatic,
        RoF,
        Multi,
        MultiRoF,
        Delay
    }
    public enum EngageStrategy
    {
        TC,
        Sprint,
        FacepullDK,
        FacepullDemo
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan MNK", "Monk", "xan", RotationModuleQuality.Good, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineSharedTA();
        def.Define(Track.Potion).As<PotionStrategy>("Pot")
            .AddOption(PotionStrategy.Manual, "Do not automatically use")
            .AddOption(PotionStrategy.PreBuffs, "Use ~4 GCDs before raid buff window")
            .AddOption(PotionStrategy.Now, "Use ASAP");

        def.DefineSimple(Track.SSS, "SixSidedStar", minLevel: 80).AddAssociatedActions(AID.SixSidedStar);

        def.Define(Track.Meditation).As<MeditationStrategy>("Meditate")
            .AddOption(MeditationStrategy.Safe, "Use out of combat, during countdown, or if no enemies are targetable")
            .AddOption(MeditationStrategy.Greedy, "Allow using when primary enemy is targetable, but out of range")
            .AddOption(MeditationStrategy.Force, "Use even if enemy is in melee range")
            .AddOption(MeditationStrategy.Delay, "Do not use")
            .AddAssociatedActions(AID.SteeledMeditation);

        def.Define(Track.FiresReply).As<FRStrategy>("FiresReply")
            .AddOption(FRStrategy.Automatic, "Use after Opo GCD", minLevel: 100)
            .AddOption(FRStrategy.Ranged, "Use when out of melee range, or if about to expire", minLevel: 100)
            .AddOption(FRStrategy.Force, "Use ASAP", minLevel: 100)
            .AddOption(FRStrategy.Delay, "Do not use", minLevel: 100)
            .AddAssociatedActions(AID.FiresReply);

        def.Define(Track.Nadi).As<NadiStrategy>("Nadi")
            .AddOption(NadiStrategy.Automatic, "Automatically choose best nadi (double lunar opener, otherwise alternate)", minLevel: 60)
            .AddOption(NadiStrategy.Lunar, "Lunar", minLevel: 60)
            .AddOption(NadiStrategy.Solar, "Solar", minLevel: 60)
            .AddOption(NadiStrategy.LunarDowntime, "Build lunar nadi if no current target", minLevel: 60)
            .AddOption(NadiStrategy.SolarDowntime, "Build solar nadi if no current target", minLevel: 60);

        def.DefineSimple(Track.RoF, "RoF", minLevel: 68).AddAssociatedActions(AID.RiddleOfFire);
        def.DefineSimple(Track.RoW, "RoW", minLevel: 72).AddAssociatedActions(AID.RiddleOfWind);
        def.Define(Track.PB).As<PBStrategy>("PB")
            .AddOption(PBStrategy.Automatic, "Automatically use after Opo before or during Riddle of Fire", minLevel: 50)
            .AddOption(PBStrategy.ForceOpo, "Use ASAP after next Opo", minLevel: 50)
            .AddOption(PBStrategy.Force, "Use ASAP", minLevel: 50)
            .AddOption(PBStrategy.Delay, "Do not use", minLevel: 50)
            .AddAssociatedActions(AID.PerfectBalance);
        def.DefineSimple(Track.BH, "BH", minLevel: 70).AddAssociatedActions(AID.Brotherhood);
        def.Define(Track.TC).As<TCStrategy>("TC")
            .AddOption(TCStrategy.None, "Do not use", minLevel: 35)
            .AddOption(TCStrategy.GapClose, "Use if outside melee range", minLevel: 35)
            .AddAssociatedActions(AID.Thunderclap);

        def.Define(Track.Blitz).As<BlitzStrategy>("Blitz")
            .AddOption(BlitzStrategy.Automatic, "Use ASAP", minLevel: 60)
            .AddOption(BlitzStrategy.RoF, "Hold blitz until Riddle of Fire is active", minLevel: 60)
            .AddOption(BlitzStrategy.Multi, "Hold blitz until at least two targets will be hit", minLevel: 60)
            .AddOption(BlitzStrategy.MultiRoF, "Hold blitz until Riddle of Fire and 2+ targets", minLevel: 60)
            .AddOption(BlitzStrategy.Delay, "Do not use", minLevel: 60)
            .AddAssociatedActions(AID.ElixirField, AID.FlintStrike, AID.TornadoKick, AID.ElixirBurst, AID.RisingPhoenix, AID.PhantomRush);

        def.Define(Track.Engage).As<EngageStrategy>("Engage")
            .AddOption(EngageStrategy.TC, "Thunderclap to target")
            .AddOption(EngageStrategy.Sprint, "Sprint to melee range")
            .AddOption(EngageStrategy.FacepullDK, "Precast Dragon Kick from melee range")
            .AddOption(EngageStrategy.FacepullDemo, "Precast Demolish from melee range");

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
    public Form EffectiveForm;
    public float FormLeft; // 0 if no form, 30 max

    public float BlitzLeft; // 20 max
    public float PerfectBalanceLeft => PerfectBalance.Left;
    public (float Left, int Stacks) PerfectBalance;
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

    public int BuffedGCDsLeft => FireLeft > GCD ? (int)MathF.Floor((FireLeft - GCD) / AttackGCDLength) + 1 : 0;
    public int PBGCDsLeft => PerfectBalance.Stacks + (ReadyIn(AID.PerfectBalance) <= GCD ? 3 : 0);

    private (Positional, bool) NextPositional
    {
        get
        {
            if (UseAOE || !Unlocked(AID.SnapPunch))
                return (Positional.Any, false);

            var pos = Unlocked(AID.Demolish) && CoeurlStacks == 0 ? Positional.Rear : Positional.Flank;
            var imm = EffectiveForm == Form.Coeurl && NextGCD is not AID.WindsReply and not AID.FiresReply;

            return (pos, imm);
        }
    }

    public enum GCDPriority
    {
        None = 0,
        Meditate = 50,
        WindRanged = 100,
        FireRanged = 200,
        Basic = 300,
        AOE = 400,
        SSS = 500,
        Blitz = 600,
        FiresReply = 700,
        WindsReply = 800,
        PR = 900,
        MeditateForce = 950,
    }

    // some monk OGCDs will be queued with higher prio than what user presses manually - the rotation is very drift-sensitive and monk has much less time to weave than other classes do
    public enum OGCDPriority
    {
        None = 0,
        TrueNorth = 100,
        TFC = 150,
        Potion = 200,
        RiddleOfWind = 300,
        ManualOGCD = 2001, // included for reference, not used here - actual value is 2001 + Low (2000) = 4001
        RiddleOfFire = 2002,
        Brotherhood = 2003,
        PerfectBalance = 2004
    }

    private float GetApplicationDelay(AID action) => action switch
    {
        AID.SixSidedStar => 0.62f,
        AID.DragonKick => 1.29f,
        AID.ForbiddenChakra => 1.48f,
        AID.Demolish => 1.60f,
        // add more if needed
        _ => 0
    };

    public override string DescribeState() => $"F={BuffedGCDsLeft}, PB={PBGCDsLeft}";

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = World.Client.GetGauge<MonkGauge>();

        Chakra = gauge.Chakra;
        BeastChakra = gauge.BeastChakra;
        BlitzLeft = gauge.BlitzTimeRemaining / 1000f;
        Nadi = gauge.Nadi;

        OpoStacks = gauge.OpoOpoStacks;
        RaptorStacks = gauge.RaptorStacks;
        CoeurlStacks = gauge.CoeurlStacks;

        PerfectBalance = Status(SID.PerfectBalance);
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

        EffectiveForm = GetEffectiveForm(strategy);

        UpdatePositionals(primaryTarget, NextPositional, TrueNorthLeft > GCD);

        Meditate(strategy, primaryTarget);

        var sprint = StatusLeft(BossMod.SGE.SID.Sprint);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 3 and < 15 && FormShiftLeft == 0)
                PushGCD(AID.FormShift, Player);

            SmartEngage(strategy, primaryTarget);
            return;
        }

        UseBlitz(strategy, currentBlitz);
        FiresReply(strategy);
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
                if (DowntimeIn > 0 && !CanFitGCD(DowntimeIn - GetApplicationDelay(AID.SixSidedStar), 1))
                    PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS);
                break;
        }

        OGCD(strategy, primaryTarget);
    }

    private Form GetEffectiveForm(StrategyValues strategy)
    {
        if (PerfectBalanceLeft == 0)
            return CurrentForm;

        var nadi = strategy.Option(Track.Nadi).As<NadiStrategy>();

        // TODO throw away all this crap and fix odd lunar PB (it should not be used before rof)

        // force lunar PB iff we are in opener, have lunar nadi already, and this is our last PB charge, aka double lunar opener
        // if we have lunar but this is NOT our last charge, it means we came out of downtime with lunar nadi (i.e. dungeon), so solar -> pr is optimal
        // this condition is unfortunately a little contrived. there are no other general cases in the monk rotation where we want to overwrite a lunar, as it's overall a dps loss
        // NextChargeIn(PerfectBalance) > GCD is also not quite correct. ideally this would test whether a PB charge will come up during the riddle of fire window
        // but in fights with extended downtime, nadis will already be explicitly planned out, so this isn't super important
        var forcedDoubleLunar = CombatTimer < 30 && HasLunar && ReadyIn(AID.PerfectBalance) > GCD && CanFitGCD(FireLeft, 3);
        var forcedSolar = nadi is NadiStrategy.Solar or NadiStrategy.SolarDowntime
            || ForcedSolar
            || HasLunar && !HasSolar && !forcedDoubleLunar;

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

    private void QueuePB(StrategyValues strategy)
    {
        var pbstrat = strategy.Option(Track.PB).As<PBStrategy>();

        if (BeastChakra[0] != BeastChakraType.None || NextGCD == AID.FiresReply || pbstrat == PBStrategy.Delay)
            return;

        if (pbstrat == PBStrategy.Force)
        {
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
            return;
        }

        if (CurrentForm != Form.Raptor)
            return;

        if (pbstrat == PBStrategy.ForceOpo || !Unlocked(AID.RiddleOfFire))
        {
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
            return;
        }

        // prevent odd window double blitz
        // TODO figure out the actual mathematical equation that differentiates odd windows, this is stupid
        if (BrotherhoodLeft == 0 && MaxChargesIn(AID.PerfectBalance) > 30)
            return;

        if (ShouldRoF(strategy, 3) || CanFitGCD(FireLeft, 3))
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        var bhUse = strategy.Simple(Track.BH);
        if (bhUse == OffensiveStrategy.Force)
            PushOGCD(AID.Brotherhood, Player, OGCDPriority.Brotherhood);

        if (!Player.InCombat || GCD == 0 || primaryTarget == null)
            return;

        if (strategy.Option(Track.Potion).As<PotionStrategy>() == PotionStrategy.Now)
            Potion();

        if (strategy.Option(Track.Potion).As<PotionStrategy>() == PotionStrategy.PreBuffs && CanWeave(AID.Brotherhood, 4))
            Potion();

        QueuePB(strategy);

        if (bhUse == OffensiveStrategy.Automatic && (CombatTimer >= 10 || BeastCount == 2) && DowntimeIn > World.Client.AnimationLock + 20)
            PushOGCD(AID.Brotherhood, Player, OGCDPriority.Brotherhood);

        var useRof = ShouldRoF(strategy);

        if (useRof)
            PushOGCD(AID.RiddleOfFire, Player, OGCDPriority.RiddleOfFire, GCD - EarliestRoF(AnimationLockDelay));

        if (ShouldRoW(strategy))
            PushOGCD(AID.RiddleOfWind, Player, OGCDPriority.RiddleOfWind);

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth, useRof ? 0 : GCD - 0.8f);

        if (Chakra >= 5 && !CanWeave(AID.RiddleOfFire))
        {
            if (NumLineTargets >= 3)
                PushOGCD(AID.HowlingFist, BestLineTarget, OGCDPriority.TFC);

            PushOGCD(AID.SteelPeak, primaryTarget, OGCDPriority.TFC);
        }

        if (strategy.Option(Track.TC).As<TCStrategy>() == TCStrategy.GapClose && Player.DistanceToHitbox(primaryTarget) is > 3 and < 25)
            PushOGCD(AID.Thunderclap, primaryTarget, OGCDPriority.TrueNorth);
    }

    private void Meditate(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Chakra >= 5 || !Unlocked(AID.SteeledMeditation) || Player.MountId > 0)
            return;

        var prio = strategy.Option(Track.Meditation).As<MeditationStrategy>() switch
        {
            MeditationStrategy.Force => GCDPriority.MeditateForce,
            MeditationStrategy.Safe => Player.InCombat && primaryTarget != null ? GCDPriority.None : GCDPriority.Meditate,
            MeditationStrategy.Greedy => Player.DistanceToHitbox(primaryTarget) > 3 ? GCDPriority.Meditate : GCDPriority.None,
            _ => GCDPriority.None,
        };

        PushGCD(AID.SteeledMeditation, Player, prio);
    }

    private void UseBlitz(StrategyValues strategy, AID currentBlitz)
    {
        var should = NumBlitzTargets > 0;
        should &= strategy.Option(Track.Blitz).As<BlitzStrategy>() switch
        {
            BlitzStrategy.Automatic => true,
            BlitzStrategy.RoF => FireLeft > GCD,
            BlitzStrategy.Multi => NumBlitzTargets > 1,
            BlitzStrategy.MultiRoF => FireLeft > GCD && NumBlitzTargets > 1,
            _ => false
        };

        if (should)
            PushGCD(currentBlitz, BestBlitzTarget, currentBlitz is AID.TornadoKick or AID.PhantomRush ? GCDPriority.PR : GCDPriority.Blitz);
    }

    private void FiresReply(StrategyValues strategy)
    {
        if (FiresReplyLeft <= GCD)
            return;

        var prio = strategy.Option(Track.FiresReply).As<FRStrategy>() switch
        {
            FRStrategy.Automatic => CurrentForm == Form.Raptor ? GCDPriority.FiresReply : GCDPriority.None,
            FRStrategy.Ranged => CanFitGCD(FiresReplyLeft, 1) ? GCDPriority.FireRanged : GCDPriority.FiresReply,
            FRStrategy.Force => GCDPriority.FiresReply,
            _ => GCDPriority.None
        };

        PushGCD(AID.FiresReply, BestRangedTarget, prio);
    }

    private void WindsReply()
    {
        if (WindsReplyLeft <= GCD)
            return;

        // always queue with low prio, this lets us fallback to winds reply when out of range for melee GCDs
        var prio = GCDPriority.WindRanged;

        // if riddle of fire is about to expire, or if the WR buff itself is about to expire, use ASAP
        if (FireLeft > GCD && !CanFitGCD(FireLeft, 1) || !CanFitGCD(WindsReplyLeft, 1))
            prio = GCDPriority.WindsReply;

        PushGCD(AID.WindsReply, BestLineTarget, prio);
    }

    private float DesiredFireWindow => GCDLength * 10;
    private float EarliestRoF(float estimatedDelay) => MathF.Max(estimatedDelay + 0.8f, 20.6f - DesiredFireWindow);

    private void Potion() => Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Low + 100 + (float)OGCDPriority.Potion);

    private bool ShouldRoF(StrategyValues strategy, int extraGCDs = 0)
    {
        if (!CanWeave(AID.RiddleOfFire, extraGCDs))
            return false;

        return strategy.Simple(Track.RoF) switch
        {
            OffensiveStrategy.Automatic => (extraGCDs > 0 || !CanWeave(AID.Brotherhood)) && DowntimeIn > World.Client.AnimationLock + 20,
            OffensiveStrategy.Force => true,
            _ => false
        };
    }

    private bool ShouldRoW(StrategyValues strategy) => strategy.Simple(Track.RoW) switch
    {
        OffensiveStrategy.Automatic => !CanWeave(AID.RiddleOfFire) && DowntimeIn > World.Client.AnimationLock + 15,
        OffensiveStrategy.Force => true,
        _ => false
    };

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

    private void SmartEngage(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;
        var facepullAction = AID.None;

        // invariant: countdown is > 0
        switch (strategy.Option(Track.Engage).As<EngageStrategy>())
        {
            case EngageStrategy.TC:
                if (CountdownRemaining < 0.7f && Player.DistanceToHitbox(primaryTarget) > 3)
                    PushGCD(AID.Thunderclap, primaryTarget);

                if (CountdownRemaining < GetApplicationDelay(AID.DragonKick))
                    PushGCD(AID.DragonKick, primaryTarget);
                return;

            case EngageStrategy.Sprint:
                if (CountdownRemaining < 10)
                    PushGCD(AID.Sprint, Player);

                var dist = Player.DistanceToHitbox(primaryTarget);
                var secToMelee = dist / 7.8f;
                // TODO account for acceleration
                if (CountdownRemaining < secToMelee)
                    Hints.ForcedMovement = Player.DirectionTo(primaryTarget).ToVec3();

                if (CountdownRemaining < GetApplicationDelay(AID.DragonKick))
                    PushGCD(AID.DragonKick, primaryTarget);
                return;

            // TODO delay autoattacks to prevent early pull
            case EngageStrategy.FacepullDK:
                facepullAction = AID.DragonKick;
                break;
            case EngageStrategy.FacepullDemo:
                facepullAction = AID.Demolish;
                break;
        }

        if (facepullAction == default)
            return;

        if (Player.DistanceToHitbox(primaryTarget) > 3)
            Hints.ForcedMovement = Player.DirectionTo(primaryTarget).ToVec3();

        if (CountdownRemaining < GetApplicationDelay(facepullAction))
            PushGCD(facepullAction, primaryTarget);
    }
}

public sealed class ValigarmexMNK(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("EX1-MNK", "EX1 MNK", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.MNK), 100, 1, typeof(Dawntrail.Extreme.Ex1Valigarmanda.Ex1Valigarmanda));
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        var boss = Bossmods.ActiveModule?.PrimaryActor;
        if (boss != null && World.Client.CountdownRemaining > 0 && Player.DistanceToHitbox(boss) > 3)
            Hints.ForcedMovement = Player.DirectionTo(boss).ToVec3();
    }
}
