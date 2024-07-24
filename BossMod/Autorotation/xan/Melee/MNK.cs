using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class MNK(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("MNK", "Monk", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineShared().AddAssociatedActions(AID.RiddleOfFire, AID.RiddleOfWind, AID.Brotherhood);

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
            return (AID.TornadoKick, true);

        var bc = BeastChakra;
        if (bc[0] == bc[1] && bc[1] == bc[2])
            return (AID.ElixirField, false);
        if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
            return (AID.FlintStrike, false);
        return (AID.CelestialRevolution, true);
    }

    public int BeastCount => BeastChakra.Count(x => x != BeastChakraType.None);
    public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
    public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

    public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

    private (Positional, bool) GetNextPositional() => (CoeurlStacks > 0 ? Positional.Flank : Positional.Rear, EffectiveForm == Form.Coeurl);

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Chakra < 5 && Unlocked(AID.SteeledMeditation))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.SteeledMeditation), Player, ActionQueue.Priority.Minimal + 1);

        if (Unlocked(AID.FormShift) && PerfectBalanceLeft == 0 && FormShiftLeft < 5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.FormShift), Player, ActionQueue.Priority.Minimal);

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 0.2 && _state.RangeToTarget is > 3 and < 25 && Unlocked(AID.Thunderclap))
                PushGCD(AID.Thunderclap, primaryTarget);

            return;
        }

        if (NumBlitzTargets > 0)
            PushGCD(AID.MasterfulBlitz, BestBlitzTarget);

        // demo opener might be optimal sometimes
        //if (FormShiftLeft > _state.GCD && CoeurlStacks == 0)
        //    PushGCD(AID.Demolish, primaryTarget);

        if (PerfectBalanceLeft == 0 && BlitzLeft == 0)
        {
            if (FormShiftLeft == 0 && FiresReplyLeft > _state.GCD)
                PushGCD(AID.FiresReply, BestRangedTarget);

            if (WindsReplyLeft > _state.GCD)
                PushGCD(AID.WindsReply, BestLineTarget);
        }

        if (NumAOETargets > 2 && Unlocked(AID.ArmOfTheDestroyer))
        {
            if (EffectiveForm == Form.Coeurl && Unlocked(AID.Rockbreaker))
                PushGCD(AID.Rockbreaker, Player);

            // TODO this is actually still suboptimal on 3 targets
            if (EffectiveForm == Form.Raptor && Unlocked(AID.FourPointFury))
                PushGCD(AID.FourPointFury, Player);

            PushGCD(AID.ArmOfTheDestroyer, Player);
        }
        else
        {
            switch (EffectiveForm)
            {
                case Form.Coeurl:
                    PushGCD(CoeurlStacks == 0 && Unlocked(AID.Demolish) ? AID.Demolish : AID.SnapPunch, primaryTarget); break;
                case Form.Raptor:
                    PushGCD(RaptorStacks == 0 && Unlocked(AID.TwinSnakes) ? AID.TwinSnakes : AID.TrueStrike, primaryTarget); break;
                default:
                    PushGCD(OpoStacks == 0 && Unlocked(AID.DragonKick) ? AID.DragonKick : AID.Bootshine, primaryTarget); break;
            }
        }
    }

    private Form EffectiveForm
    {
        get
        {
            if (PerfectBalanceLeft == 0)
                return CurrentForm;

            // hack: allow double lunar opener
            var forcedSolar = ForcedSolar || HasLunar && !HasSolar && CombatTimer > 30;

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

    private void QueuePB(StrategyValues strategy, float deadline)
    {
        if (CurrentForm != Form.Raptor
            || !Unlocked(AID.PerfectBalance)
            || !_state.CanWeave(_state.CD(AID.PerfectBalance) - 40, 0.6f, deadline)
            || BeastChakra[0] != BeastChakraType.None
            || FiresReplyLeft > _state.GCD
            )
            return;

        // prevent odd window double blitz
        if (HasBothNadi && FireLeft > 0)
            return;

        // TODO forced solar in strategy
        // default: solar in odd windows only, opener/2m is always lunar
        var wantSolar = HasLunar && !HasSolar && FireLeft == 0;

        // earliest we can press PB before next RoF
        var gcdsAhead = wantSolar ? 1 : 2;

        if (_state.CanWeave(AID.RiddleOfFire, 0.6f, _state.GCD + _state.AttackGCDTime * gcdsAhead))
            PushOGCD(AID.PerfectBalance, Player);

        // can PB if we have 4 GCDs worth of buff remaining
        if (FireLeft > _state.GCD + _state.AttackGCDTime * 3)
            PushOGCD(AID.PerfectBalance, Player);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (Player.InCombat && _state.GCD > 0)
        {
            if (strategy.BuffsOk())
            {
                QueuePB(strategy, deadline);

                if (ShouldUseBH(strategy, deadline))
                    PushOGCD(AID.Brotherhood, Player);

                if (_state.GCD < 0.8f && ShouldUseRoF(strategy, deadline))
                    PushOGCD(AID.RiddleOfFire, Player);

                if (_state.CD(AID.RiddleOfFire) > 0 && _state.CanWeave(AID.RiddleOfWind, 0.6f, deadline))
                    PushOGCD(AID.RiddleOfWind, Player);

                if (ShouldUseTrueNorth(strategy, deadline))
                    PushOGCD(AID.TrueNorth, Player);
            }

            if (Chakra >= 5 && _state.CanWeave(AID.SteelPeak, 0.6f, deadline))
            {
                if (Unlocked(AID.HowlingFist) && NumLineTargets >= 3)
                    PushOGCD(AID.HowlingFist, BestLineTarget);

                PushOGCD(AID.SteelPeak, primaryTarget);
            }
        }
    }

    private bool ShouldUseBH(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.Brotherhood) || !_state.CanWeave(AID.Brotherhood, 0.6f, deadline))
            return false;

        // delay in opener for party cd alignment
        if (CombatTimer < 10)
            return BeastCount == 3;

        return true;
    }

    private bool ShouldUseRoF(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.RiddleOfFire) || !_state.CanWeave(AID.RiddleOfFire, 0.6f, deadline))
            return false;

        return !Unlocked(AID.Brotherhood) || _state.CD(AID.Brotherhood) > 0;
    }

    private bool ShouldUseTrueNorth(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.TrueNorth) || !_state.CanWeave(_state.CD(AID.TrueNorth) - 45, 0.6f, deadline))
            return false;

        var wrong = _state.NextPositionalImminent && !_state.NextPositionalCorrect;

        // always late weave unless it would delay riddle of fire
        if (ShouldUseRoF(strategy, _state.GCD))
            return wrong;
        else
            return wrong && _state.GCD < 0.8f;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

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

        if (BlitzLeft > _state.GCD)
        {
            if (currentBlitzIsTargeted)
            {
                (BestBlitzTarget, NumBlitzTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);
            }
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

        _state.UpdatePositionals(primaryTarget, GetNextPositional(), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private bool IsEnlightenmentTarget(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);

    private (Form, float) DetermineForm()
    {
        if (PerfectBalanceLeft > 0)
            return (Form.None, 0);

        var s = _state.StatusDetails(Player, SID.OpoOpoForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.OpoOpo, s);
        s = _state.StatusDetails(Player, SID.RaptorForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.Raptor, s);
        s = _state.StatusDetails(Player, SID.CoeurlForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.Coeurl, s);
        return (Form.None, 0);
    }
}
