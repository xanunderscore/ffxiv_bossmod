using BossMod.Autorotation.Legacy;
using BossMod.DNC;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class DNC : xanmodule
{
    public enum Track { AOE, Targeting, Buffs, Partner }
    public enum AOEStrategy { AOE, SingleTarget }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum PartnerStrategy { Automatic, Manual }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("DNC", "Dancer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DNC), 100);

        def.Define(Track.AOE).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions");

        def.DefineTargeting(Track.Targeting);

        def.Define(Track.Buffs).As<OffensiveStrategy>("Buffs")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use buffs when optimal")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use buffs")
            .AddOption(OffensiveStrategy.Force, "Force", "Use buffs ASAP");

        def.Define(Track.Partner).As<PartnerStrategy>("Partner")
            .AddOption(PartnerStrategy.Automatic, "Auto", "Choose dance partner automatically (based on job aDPS)")
            .AddOption(PartnerStrategy.Manual, "Manual", "Do not choose dance partner automatically");

        return def;
    }

    public byte Feathers;
    public bool IsDancing;
    public byte CompletedSteps;
    public uint NextStep;
    public byte Esprit;

    public float StandardStepLeft; // 15s max
    public float StandardFinishLeft; // 60s max
    public float TechStepLeft; // 15s max
    public float TechFinishLeft; // 20s max
    public float FlourishingFinishLeft; // 30s max, granted by tech step
    public float ImprovisationLeft; // 15s max
    public float ImprovisedFinishLeft; // 30s max
    public float DevilmentLeft; // 20s max
    public float SymmetryLeft; // 30s max
    public float FlowLeft; // 30s max
    public float FlourishingStarfallLeft; // 20s max
    public float ThreefoldLeft; // 30s max
    public float FourfoldLeft; // 30s max
    public float PelotonLeft;

    private Actor? BestSingleTarget;
    private Actor? BestFan4Target;
    private Actor? BestRangedAOETarget;
    private Actor? BestStarfallTarget;

    public int NumAOETargets;
    public int NumDanceTargets;

    public int NumFan4Targets;
    public int NumRangedAOETargets;
    public int NumStarfallTargets;

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    internal class State(RotationModule module) : CommonState(module) { }

    private readonly State _state;

    public DNC(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    protected override CommonState GetState() => _state;

    private const float FinishDanceWindow = 0.5f;

    private bool HaveTarget => NumAOETargets > 1 || BestSingleTarget != null;

    private void QueueNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        primaryTarget ??= BestSingleTarget;

        if (IsDancing)
        {
            if (NextStep != 0)
                PushGCD((AID)NextStep, Player);

            if (ShouldFinishDance(StandardStepLeft))
                PushGCD(AID.DoubleStandardFinish, Player);

            if (ShouldFinishDance(TechStepLeft))
                PushGCD(AID.QuadrupleTechnicalFinish, Player);

            return;
        }

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining is > 3.5f and < 15.5f && !IsDancing && Unlocked(AID.StandardStep))
                PushGCD(AID.StandardStep, Player);

            return;
        }

        if (ShouldTechStep(strategy))
            PushGCD(AID.TechnicalStep, Player);

        var shouldStdStep = ShouldStdStep(strategy);

        var canStarfall = FlourishingStarfallLeft > _state.GCD && NumStarfallTargets > 0;
        var canFlow = CanFlow(out var flowCombo);
        var canSymmetry = CanSymmetry(out var symmetryCombo);
        var combo2 = NumAOETargets > 1 ? AID.Bladeshower : AID.Fountain;
        var haveCombo2 = Unlocked(combo2) && _state.ComboLastAction == (NumAOETargets > 1 ? (uint)AID.Windmill : (uint)AID.Cascade);

        if (canStarfall && FlourishingStarfallLeft <= _state.AttackGCDTime)
            PushGCD(AID.StarfallDance, BestStarfallTarget);

        // the targets for these two will be auto fixed if they are AOE actions
        if (canFlow && FlowLeft <= _state.AttackGCDTime)
            PushGCD(flowCombo, primaryTarget);

        if (canSymmetry && SymmetryLeft <= _state.AttackGCDTime)
            PushGCD(symmetryCombo, primaryTarget);

        if (ShouldSaberDance(strategy, 85))
            PushGCD(AID.SaberDance, BestRangedAOETarget);

        // TODO combine this with above
        if (canStarfall)
            PushGCD(AID.StarfallDance, BestStarfallTarget);

        if (haveCombo2 && _state.ComboTimeLeft < _state.AttackGCDTime * 2)
        {
            if (canFlow)
                PushGCD(flowCombo, primaryTarget);

            if (_state.ComboTimeLeft < _state.AttackGCDTime)
                PushGCD(combo2, primaryTarget);
        }

        if (FlourishingFinishLeft > _state.GCD && _state.CD(AID.Devilment) > 0 && NumDanceTargets > 0)
            PushGCD(AID.Tillana, Player);

        if (TechFinishLeft > _state.GCD && ShouldSaberDance(strategy, 50))
            PushGCD(AID.SaberDance, BestRangedAOETarget);

        if (TechFinishLeft == 0 && shouldStdStep && (_state.CD(AID.TechnicalStep) > _state.GCD + 5 || !Unlocked(AID.TechnicalStep)))
            PushGCD(AID.StandardStep, Player);

        if (canFlow)
            PushGCD(flowCombo, primaryTarget);

        if (canSymmetry)
            PushGCD(symmetryCombo, primaryTarget);

        if (shouldStdStep)
            PushGCD(AID.StandardStep, Player);

        if (haveCombo2)
            PushGCD(combo2, primaryTarget);

        if (NumAOETargets > 1 && Unlocked(AID.Windmill))
            PushGCD(AID.Windmill, Player);

        PushGCD(AID.Cascade, primaryTarget);
    }

    private void QueueNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining is > 2 and < 10 && NextStep == 0 && PelotonLeft == 0 && Unlocked(AID.Peloton))
                PushOGCD(AID.Peloton, Player);

            return;
        }

        if (IsDancing)
            return;

        if (TechFinishLeft > _state.GCD && Unlocked(AID.Devilment) && _state.CanWeave(AID.Devilment, 0.6f, deadline))
            PushOGCD(AID.Devilment, Player);

        if (_state.CD(AID.Devilment) > 55 && _state.CanWeave(AID.Flourish, 0.6f, deadline))
            PushOGCD(AID.Flourish, Player);

        if ((TechFinishLeft == 0 || _state.CD(AID.Devilment) > 0) && ThreefoldLeft > _state.AnimationLock && NumRangedAOETargets > 0 && _state.CanWeave(AID.FanDanceIII, 0.6f, deadline))
            PushOGCD(AID.FanDanceIII, BestRangedAOETarget);

        var canF1 = ShouldSpendFeathers(strategy);
        var f1ToUse = NumAOETargets > 1 && Unlocked(AID.FanDanceII) ? AID.FanDanceII : AID.FanDance;

        if (Feathers == 4 && canF1)
            PushOGCD(f1ToUse, primaryTarget ?? BestSingleTarget);

        if (_state.CD(AID.Devilment) > 0 && FourfoldLeft > _state.AnimationLock && NumFan4Targets > 0)
            PushOGCD(AID.FanDanceIV, BestFan4Target);

        if (canF1)
            PushOGCD(f1ToUse, primaryTarget ?? BestSingleTarget);
    }

    private bool ShouldStdStep(StrategyValues strategy)
    {
        if (!Unlocked(AID.StandardStep) || _state.CD(AID.StandardStep) > _state.GCD)
            return false;

        return NumDanceTargets > 0 &&
            (TechFinishLeft == 0 || TechFinishLeft > _state.GCD + 3.5 || !Unlocked(AID.TechnicalStep));
    }

    private bool ShouldTechStep(StrategyValues strategy)
    {
        if (!Unlocked(AID.TechnicalStep) || _state.CD(AID.TechnicalStep) > _state.GCD)
            return false;

        return NumDanceTargets > 0 && StandardFinishLeft > _state.GCD + 5.5;
    }

    private bool CanFlow(out AID action)
    {
        var act = NumAOETargets > 1 ? AID.Bloodshower : AID.Fountainfall;
        if (Unlocked(act) && FlowLeft > _state.GCD && HaveTarget)
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool CanSymmetry(out AID action)
    {
        var act = NumAOETargets > 1 ? AID.RisingWindmill : AID.ReverseCascade;
        if (Unlocked(act) && SymmetryLeft > _state.GCD && HaveTarget)
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool ShouldFinishDance(float danceTimeLeft)
    {
        if (NextStep != 0)
            return false;
        if (danceTimeLeft is > 0 and < FinishDanceWindow)
            return true;

        return danceTimeLeft > _state.GCD && NumDanceTargets > 0;
    }

    private bool ShouldSaberDance(StrategyValues strategy, int minimumEsprit)
    {
        if (Esprit < 50 || !Unlocked(AID.SaberDance))
            return false;

        return Esprit >= minimumEsprit && NumRangedAOETargets > 0;
    }

    private bool ShouldSpendFeathers(StrategyValues strategy)
    {
        if (Feathers == 0)
            return false;

        if (Feathers == 4)
            return true;

        return TechFinishLeft > _state.AnimationLock;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        _state.UpdateCommon(primaryTarget);
        _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);

        var gauge = Service.JobGauges.Get<DNCGauge>();

        Feathers = gauge.Feathers;
        IsDancing = gauge.IsDancing;
        CompletedSteps = gauge.CompletedSteps;
        NextStep = (gauge.CompletedSteps == 4 || gauge.NextStep == 15998) ? 0 : gauge.NextStep;
        Esprit = gauge.Esprit;

        StandardStepLeft = StatusLeft(SID.StandardStep);
        StandardFinishLeft = StatusLeft(SID.StandardFinish);
        TechStepLeft = StatusLeft(SID.TechnicalStep);
        TechFinishLeft = StatusLeft(SID.TechnicalFinish);
        FlourishingFinishLeft = StatusLeft(SID.FlourishingFinish);
        ImprovisationLeft = StatusLeft(SID.Improvisation);
        ImprovisedFinishLeft = StatusLeft(SID.ImprovisedFinish);
        DevilmentLeft = StatusLeft(SID.Devilment);
        SymmetryLeft = MathF.Max(StatusLeft(SID.SilkenSymmetry), StatusLeft(SID.FlourishingSymmetry));
        FlowLeft = MathF.Max(StatusLeft(SID.SilkenFlow), StatusLeft(SID.FlourishingFlow));
        FlourishingStarfallLeft = StatusLeft(SID.FlourishingStarfall);
        ThreefoldLeft = StatusLeft(SID.ThreefoldFanDance);
        FourfoldLeft = StatusLeft(SID.FourfoldFanDance);

        BestSingleTarget = SelectSingleTarget(strategy.Option(Track.Targeting), primaryTarget, 25);
        (BestFan4Target, NumFan4Targets) = SelectRangedTarget(strategy.Option(Track.Targeting), primaryTarget, 15, CalcNumFan4Targets);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectRangedTarget(strategy.Option(Track.Targeting), primaryTarget, 25, NumSplashTargets);
        (BestStarfallTarget, NumStarfallTargets) = SelectRangedTarget(strategy.Option(Track.Targeting), primaryTarget, 25, CalcNumStarfallTargets);

        NumDanceTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 15);
        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => NumMeleeAOETargets(),
            _ => 0
        };

        if (Unlocked(AID.ClosedPosition)
            && strategy.Option(Track.Partner).As<PartnerStrategy>() == PartnerStrategy.Automatic
            && StatusLeft(SID.ClosedPosition) == 0
            && _state.CD(AID.ClosedPosition) == 0
            && FindDancePartner() is Actor partner)
            PushOGCD(AID.ClosedPosition, partner);

        QueueNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => QueueNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private int CalcNumFan4Targets(Actor primary) => Hints.NumPriorityTargetsInAOECone(Player.Position, 15, (primary.Position - Player.Position).Normalized(), 60.Degrees());
    private int CalcNumStarfallTargets(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 25, 4);

    private float StatusLeft(SID status) => _state.StatusDetails(Player, status, Player.InstanceID).Left;

    private Actor? FindDancePartner() => World.Party.WithoutSlot().Exclude(Player).MaxBy(p => p.Class switch
        {
            Class.SAM => 100,
            Class.NIN or Class.VPR => 99,
            Class.MNK => 88,
            Class.RPR => 87,
            Class.DRG => 86,
            Class.BLM or Class.PCT => 79,
            Class.SMN => 78,
            Class.RDM => 77,
            Class.MCH => 69,
            Class.BRD => 68,
            Class.DNC => 67,
            _ => 1
        });
}
