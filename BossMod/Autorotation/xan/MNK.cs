using BossMod.MNK;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class MNK(RotationModuleManager manager, Actor player) : xanmodule(manager, player)
{
    public enum Track { AOE, Targeting, Buffs }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("MNK", "Monk", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.RiddleOfFire, AID.RiddleOfWind, AID.Brotherhood);

        return def;
    }

    public enum Form { None, OpoOpo, Raptor, Coeurl }

    public int Chakra; // 0-5 (0-10 during Brotherhood)
    public BeastChakra[] BeastChakra = [];
    public int OpoStacks; // 0-1
    public int RaptorStacks; // 0-2
    public int CoeurlStacks; // 0-3
    public NadiFlags Nadi;

    public Form CurrentForm;
    public float FormLeft; // 0 if no form, 30 max

    public float BlitzLeft; // 20 max
    public float PerfectBalanceLeft; // 20 max
    public float FormShiftLeft; // 30 max
    public float BrotherhoodLeft; // 20 max
    public float FireLeft; // 20 max
    public float TrueNorthLeft; // 10 max
    public float EarthsReplyLeft; // 30 max, probably doesnt belong in autorotation
    public float FiresReplyLeft; // 20 max
    public float WindsReplyLeft; // 15 max

    public int NumBlitzTargets;
    public int NumAOETargets;
    public int NumLineTargets;

    private Actor? BestBlitzTarget;
    private Actor? BestRangedTarget; // fire's reply
    private Actor? BestLineTarget; // enlightenment, wind's reply

    public bool HasLunar => Nadi.HasFlag(NadiFlags.LUNAR);
    public bool HasSolar => Nadi.HasFlag(NadiFlags.SOLAR);
    public bool HasBothNadi => HasLunar && HasSolar;

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

    public int BeastCount => BeastChakra.Count(x => x != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE);
    public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
    public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

    public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    private (Positional, bool) GetNextPositional() => (CoeurlStacks > 0 ? Positional.Flank : Positional.Rear, CurrentForm == Form.Coeurl);

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

        // TODO fix when they fix the potency in 7.0.1
        if (NumAOETargets > 3 && Unlocked(AID.ArmOfTheDestroyer))
        {
            if (EffectiveForm == Form.Coeurl && Unlocked(AID.Rockbreaker))
                PushGCD(AID.Rockbreaker, Player);

            if (EffectiveForm == Form.Raptor && Unlocked(AID.FourPointFury))
                PushGCD(AID.FourPointFury, Player);

            PushGCD(AID.ArmOfTheDestroyer, Player);
        }
        else
        {
            if (CurrentForm == Form.Raptor && OpoStacks == 0 && PerfectBalanceLeft == 0 && Unlocked(AID.DragonKick))
                PushGCD(AID.DragonKick, primaryTarget);

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

            var forcedSolar = ForcedSolar || HasLunar && !HasSolar && FireLeft == 0;

            var canCoeurl = forcedSolar;
            var canRaptor = forcedSolar;
            var canOpo = true;

            foreach (var chak in BeastChakra)
            {
                // why the hell did they do this
                canCoeurl &= chak != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR;
                canRaptor &= chak != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.OPOOPO;
                if (ForcedSolar)
                    canOpo &= chak != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.COEURL;
            }

            if (FireLeft > _state.GCD || _state.CD(AID.RiddleOfFire) == 0)
                return canOpo ? Form.OpoOpo : canCoeurl ? Form.Coeurl : Form.Raptor;

            return canRaptor ? Form.Raptor : canCoeurl ? Form.Coeurl : Form.OpoOpo;
        }
    }

    private void QueuePB(StrategyValues strategy, float deadline, float finalDeadline)
    {
        if (CurrentForm != Form.Raptor
            || !Unlocked(AID.PerfectBalance)
            || !_state.CanWeave(_state.CD(AID.PerfectBalance) - 40, 0.6f, deadline)
            || BeastChakra[0] != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE
            || FiresReplyLeft > _state.GCD
            )
            return;

        // TODO forced solar in strategy
        // default: solar in odd windows only, opener/2m is always lunar
        var wantSolar = HasLunar && !HasSolar && FireLeft == 0;

        if (wantSolar)
        {
            if (_state.CanWeave(AID.RiddleOfFire, 0.6f, finalDeadline + _state.AttackGCDTime * 2))
                PushOGCD(AID.PerfectBalance, Player);
        }
        else if (_state.CanWeave(AID.RiddleOfFire, 0.6f, finalDeadline + _state.AttackGCDTime * 2) || FireLeft > _state.GCD + _state.AttackGCDTime * 2)
            PushOGCD(AID.PerfectBalance, Player);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline, float finalDeadline)
    {
        var buff = strategy.Option(Track.Buffs).As<OffensiveStrategy>();
        if (Player.InCombat)
        {
            if (buff != OffensiveStrategy.Delay)
            {
                QueuePB(strategy, deadline, finalDeadline);

                if (Unlocked(AID.Brotherhood) && _state.CanWeave(AID.Brotherhood, 0.6f, deadline))
                    PushOGCD(AID.Brotherhood, Player);

                if (_state.GCD < 0.8f && ShouldUseRoF(strategy, deadline))
                    PushOGCD(AID.RiddleOfFire, Player);

                if (_state.CD(AID.RiddleOfFire) > 0 && _state.CanWeave(AID.RiddleOfWind, 0.6f, deadline))
                    PushOGCD(AID.RiddleOfWind, Player);

                if (ShouldUseTrueNorth(strategy, deadline, finalDeadline))
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

    private bool ShouldUseRoF(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.RiddleOfFire) || !_state.CanWeave(AID.RiddleOfFire, 0.6f, deadline))
            return false;

        return !Unlocked(AID.Brotherhood) || _state.CD(AID.Brotherhood) > 0;
    }

    private bool ShouldUseTrueNorth(StrategyValues strategy, float deadline, float finalDeadline)
    {
        if (!Unlocked(AID.TrueNorth) || !_state.CanWeave(_state.CD(AID.TrueNorth), 0.6f, deadline))
            return false;

        var wrong = _state.NextPositionalImminent && !_state.NextPositionalCorrect;

        // always late weave unless it would delay riddle of fire
        if (ShouldUseRoF(strategy, finalDeadline))
            return wrong;
        else
            return wrong && _state.GCD < 0.8f;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        var targeting = strategy.Option(Track.Targeting);
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 3);
        _state.UpdateCommon(primaryTarget);

        UpdateGauge();

        PerfectBalanceLeft = _state.StatusDetails(Player, SID.PerfectBalance, Player.InstanceID).Left;
        FormShiftLeft = _state.StatusDetails(Player, SID.FormlessFist, Player.InstanceID).Left;
        FireLeft = _state.StatusDetails(Player, SID.RiddleOfFire, Player.InstanceID).Left;
        TrueNorthLeft = _state.StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
        WindsReplyLeft = _state.StatusDetails(Player, SID.WindsRumination, Player.InstanceID).Left;
        FiresReplyLeft = _state.StatusDetails(Player, SID.FiresRumination, Player.InstanceID).Left;
        EarthsReplyLeft = _state.StatusDetails(Player, SID.EarthsRumination, Player.InstanceID).Left;
        BrotherhoodLeft = _state.StatusDetails(Player, SID.Brotherhood, Player.InstanceID).Left;
        (var currentBlitz, var currentBlitzIsTargeted) = GetCurrentBlitz();

        if (BlitzLeft > _state.GCD)
        {
            if (currentBlitzIsTargeted)
            {
                (BestBlitzTarget, NumBlitzTargets) = SelectTarget(targeting, primaryTarget, 3, NumSplashTargets);
            }
            else
            {
                BestBlitzTarget = Player;
                NumBlitzTargets = NumMeleeAOETargets();
            }
        }
        else
        {
            BestBlitzTarget = Player;
            NumBlitzTargets = 0;
        }

        (CurrentForm, FormLeft) = DetermineForm();

        BestRangedTarget = SelectTarget(targeting, primaryTarget, 20, NumSplashTargets).Best;
        (BestLineTarget, NumLineTargets) = SelectTarget(targeting, primaryTarget, 10, NumEnlightenmentTargets);
        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => NumMeleeAOETargets(),
            _ => 0
        };

        _state.UpdatePositionals(primaryTarget, GetNextPositional(), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, finalDeadline) => CalcNextBestOGCD(strategy, primaryTarget, deadline, finalDeadline));
    }

    private int NumEnlightenmentTargets(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);

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

    // FIXME when CS is updated
    private unsafe void UpdateGauge()
    {
        var gauge = Service.JobGauges.Get<MNKGauge>();
        Service.Log($"{gauge.Address:X8}");

        Chakra = gauge.Chakra;
        BeastChakra = gauge.BeastChakra;
        BlitzLeft = gauge.BlitzTimeRemaining / 1000f;
        Nadi = *(NadiFlags*)(gauge.Address + 0x0D);

        var formFlags = *(byte*)(gauge.Address + 0x0C);
        OpoStacks = formFlags & 0x01;
        RaptorStacks = (formFlags & 0xF) / 0x04;
        CoeurlStacks = formFlags / 0x10;
    }

    [Flags]
    public enum NadiFlags : byte
    {
        LUNAR = 1,
        SOLAR = 2
    }
}
