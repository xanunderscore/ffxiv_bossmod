using BossMod.Autorotation.Legacy;
using BossMod.PLD;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class PLD : LegacyModule
{
    public enum Track { AOE }
    public enum AOEStrategy { AOE, SingleTarget }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("PLD", "Paladin", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.PLD | (int)Class.GLA), 100);

        def.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 100)
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions");

        return def;
    }

    public float FightOrFlightLeft; // max 20
    public float GoringBladeReady; // max 30
    public float DivineMightLeft; // max 30
    public float AtonementReady; // max 30
    public float SupplicationReady; // max 30
    public float SepulchreReady; // max 30
    public (float Left, int Stacks) Requiescat; // max 30/4 stacks

    public int OathGauge; // 0-100

    public AID ConfiteorCombo;

    public int NumAOETargets;
    public int NumScornTargets; // Circle of Scorn is part of single target rotation
    public int NumConfiteorTargets;
    public int NumExpiacionTargets;
    public int NumRequiescatTargets;

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    public class State(RotationModule module) : CommonState(module) { }

    private readonly State _state;

    public PLD(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    private AID GetNextBestGCD(StrategyValues strategy)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 2 && Unlocked(AID.HolySpirit))
                return AID.HolySpirit;

            return AID.None;
        }

        if (
            GoringBladeReady > _state.GCD
            // skip this conditional to allow confiteor/holy spirit if target is out of range
            && _state.RangeToTarget <= 3
        )
            return AID.GoringBlade;

        if (ConfiteorCombo != AID.None && _state.CurMP >= 1000)
            return ConfiteorCombo;

        if (_state.RangeToTarget > 3 && DivineMightLeft > _state.GCD && NumAOETargets < 3)
            return AID.HolySpirit;

        if (SepulchreReady > _state.GCD)
            return AID.Sepulchre;

        if (SupplicationReady > _state.GCD)
            return AID.Supplication;

        if (NumAOETargets >= 3 && Unlocked(AID.TotalEclipse))
        {
            if (Requiescat.Left > _state.GCD && Unlocked(AID.HolyCircle) && _state.CurMP >= 1000)
                return AID.HolyCircle;

            if (DivineMightLeft > _state.GCD && _state.CurMP >= 1000 && FightOrFlightLeft > _state.GCD)
                return AID.HolyCircle;

            if (Unlocked(AID.Prominence) && _state.ComboLastAction == (uint)AID.TotalEclipse)
            {
                if (DivineMightLeft > _state.GCD && Unlocked(AID.HolyCircle) && _state.CurMP >= 1000)
                    return AID.HolyCircle;

                return AID.Prominence;
            }

            return AID.TotalEclipse;
        }
        else if (_state.TargetingEnemy)
        {
            if (
                DivineMightLeft > _state.GCD
                && DivineMightLeft < _state.GCD + _state.AttackGCDTime
                && _state.CurMP >= 1000
            )
                return AID.HolySpirit;

            // confiteor handled above
            if (Requiescat.Left > _state.GCD && _state.CurMP >= 1000)
                return AID.HolySpirit;

            if (DivineMightLeft > _state.GCD && _state.CurMP >= 1000 && FightOrFlightLeft > _state.GCD)
                return AID.HolySpirit;

            // use early in FoF window
            if (AtonementReady > _state.GCD && FightOrFlightLeft > _state.GCD)
                return AID.Atonement;

            if (Unlocked(AID.RageOfHalone) && _state.ComboLastAction == (uint)AID.RiotBlade)
            {
                if (DivineMightLeft > _state.GCD && _state.CurMP >= 1000)
                    return AID.HolySpirit;

                if (AtonementReady > _state.GCD)
                    return AID.Atonement;

                return AID.RoyalAuthority;
            }

            if (Unlocked(AID.RiotBlade) && _state.ComboLastAction == (uint)AID.FastBlade)
                return AID.RiotBlade;

            return AID.FastBlade;
        }

        return AID.None;
    }

    private ActionID GetNextBestOGCD(float deadline)
    {
        if ((AtonementReady > 0 || Requiescat.Left > 0 || DivineMightLeft > 0) && _state.CanWeave(AID.FightOrFlight, 0.6f, deadline))
            return ActionID.MakeSpell(AID.FightOrFlight);

        if (FightOrFlightLeft > 0 && Unlocked(AID.Requiescat) && _state.CanWeave(AID.Requiescat, 0.6f, deadline) && _state.RangeToTarget <= 3 && NumRequiescatTargets > 0)
            return ActionID.MakeSpell(AID.Requiescat);

        if (FightOrFlightLeft > 0 || _state.CD(AID.FightOrFlight) > 15)
        {
            if (Unlocked(AID.SpiritsWithin) && _state.CanWeave(AID.SpiritsWithin, 0.6f, deadline) && _state.RangeToTarget <= 3)
                return ActionID.MakeSpell(AID.SpiritsWithin);

            if (Unlocked(AID.CircleOfScorn) && _state.CanWeave(AID.CircleOfScorn, 0.6f, deadline) && NumScornTargets > 0)
                return ActionID.MakeSpell(AID.CircleOfScorn);
        }

        if (Unlocked(AID.Intervene) && FightOrFlightLeft > 0 && _state.CanWeave(_state.CD(AID.Intervene) - 30, 0.6f, deadline))
            return ActionID.MakeSpell(AID.Intervene);

        if (Unlocked(AID.Sheltron) && !Unlocked(AID.HolySheltron) && OathGauge >= 95 && Player.InCombat)
            return ActionID.MakeSpell(AID.Sheltron);

        return default;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        _state.UpdateCommon(primaryTarget);

        if (_state.AnimationLockDelay < 0.1f)
            _state.AnimationLockDelay = 0.1f;

        var gauge = Service.JobGauges.Get<PLDGauge>();
        OathGauge = gauge.OathGauge;
        FightOrFlightLeft = _state.StatusDetails(Player, SID.FightOrFlight, Player.InstanceID).Left;
        GoringBladeReady = _state.StatusDetails(Player, SID.GoringBladeReady, Player.InstanceID).Left;
        DivineMightLeft = _state.StatusDetails(Player, SID.DivineMight, Player.InstanceID).Left;
        AtonementReady = _state.StatusDetails(Player, SID.AtonementReady, Player.InstanceID).Left;
        SupplicationReady = _state.StatusDetails(Player, SID.SupplicationReady, Player.InstanceID).Left;
        SepulchreReady = _state.StatusDetails(Player, SID.SepulchreReady, Player.InstanceID).Left;
        Requiescat = _state.StatusDetails(Player, SID.Requiescat, Player.InstanceID);

        var confiteorId = Manager.ActionManager.GetAdjustedActionID((uint)AID.Confiteor);

        if (confiteorId == (uint)AID.Confiteor)
            ConfiteorCombo = _state.StatusDetails(Player, SID.ConfiteorReady, Player.InstanceID).Left > _state.GCD ? AID.Confiteor : AID.None;
        else
            ConfiteorCombo = (AID)confiteorId;

        var aoeType = strategy.Option(Track.AOE).As<AOEStrategy>();

        NumScornTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        NumAOETargets = aoeType == AOEStrategy.SingleTarget ? 0 : NumScornTargets;

        (var bestConfiteorTarget, NumConfiteorTargets) = MaybeFindBetterTarget(aoeType, primaryTarget, 25, act => Hints.NumPriorityTargetsInAOECircle(act.Position, 5));

        (var bestExpTarget, NumExpiacionTargets) = Unlocked(AID.Expiacion)
              ? MaybeFindBetterTarget(aoeType, primaryTarget, 3, act => Hints.NumPriorityTargetsInAOECircle(act.Position, 5))
              : (primaryTarget, primaryTarget == null ? 0 : 1);

        if (Unlocked(AID.Imperator))
            NumRequiescatTargets = NumExpiacionTargets;
        else
            NumRequiescatTargets = primaryTarget == null ? 0 : 1;

        var gcd = GetNextBestGCD(strategy);
        if (gcd != AID.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(gcd), gcd is AID.Confiteor or AID.BladeOfFaith or AID.BladeOfTruth or AID.BladeOfValor ? bestConfiteorTarget : primaryTarget, ActionQueue.Priority.High + 500);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(deadline);

        if (ogcd)
            Hints.ActionsToExecute.Push(ogcd, ogcd.ID is (uint)AID.Expiacion or (uint)AID.Imperator ? bestExpTarget : primaryTarget, ActionQueue.Priority.Low + 500);
    }

    private (Actor?, int) MaybeFindBetterTarget(AOEStrategy strat, Actor? primaryTarget, float range, Func<Actor, int> numTargets) => strat switch
    {
        AOEStrategy.AOE => FindBetterTargetBy(primaryTarget, range, numTargets),
        AOEStrategy.SingleTarget => (primaryTarget, primaryTarget == null ? 0 : numTargets(primaryTarget)),
        _ => (null, 0)
    };
}
