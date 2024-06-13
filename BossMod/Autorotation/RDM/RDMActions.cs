using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.RDM;

class Actions : HealerActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<RDMConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.GetAndSubscribe<RDMConfig>(OnConfigModified);
        _state = new(autorot.WorldState);
        _strategy = new();

        SupportedSpell(AID.Riposte).TransformAction = () => ActionID.MakeSpell(_state.BestRiposte);
        SupportedSpell(AID.Zwerchhau).TransformAction = () => ActionID.MakeSpell(_state.BestZwerchhau);
        SupportedSpell(AID.Redoublement).TransformAction = () => ActionID.MakeSpell(_state.BestRedoublement);
        SupportedSpell(AID.Moulinet).TransformAction = () => ActionID.MakeSpell(_state.BestMoulinet);
        SupportedSpell(AID.Reprise).TransformAction = () => ActionID.MakeSpell(_state.BestReprise);
        SupportedSpell(AID.Veraero).TransformAction = SupportedSpell(AID.VeraeroIII).TransformAction = () =>
            ActionID.MakeSpell(_state.BestAero);
        SupportedSpell(AID.VeraeroII).TransformAction = () => ActionID.MakeSpell(_state.BestAero2);
        SupportedSpell(AID.Verthunder).TransformAction = SupportedSpell(AID.VerthunderIII).TransformAction = () =>
            ActionID.MakeSpell(_state.BestThunder);
        SupportedSpell(AID.VerthunderII).TransformAction = () => ActionID.MakeSpell(_state.BestThunder2);
        SupportedSpell(AID.Jolt).TransformAction = () => ActionID.MakeSpell(_state.BestJolt);
        SupportedSpell(AID.Scatter).TransformAction = () => ActionID.MakeSpell(_state.BestScatter);
    }

    public override CommonRotation.PlayerState GetState() => _state;

    public override CommonRotation.Strategy GetStrategy() => _strategy;

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (AutoAction < AutoActionAIFight)
            return default;

        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

        return MakeResult(res, Autorot.PrimaryTarget);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
        _strategy.ApplyStrategyOverrides(
            Autorot
                .Bossmods.ActiveModule?.PlanExecution
                ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []
        );

        _strategy.NumManastackAOETargets = NumCircleTargets(Autorot.PrimaryTarget, 5);
        _strategy.NumAOETargets = autoAction == AutoActionST ? 0 : _strategy.NumManastackAOETargets;
        _strategy.NumC6Targets = NumCircleTargets(Autorot.PrimaryTarget, 6);
        _strategy.NumMoulinetTargets = autoAction == AutoActionST ? 0 : NumMoulinetTargets(Autorot.PrimaryTarget);
        _strategy.NumResolutionTargets = NumResolutionTargets(Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);

        var gauge = Service.JobGauges.Get<RDMGauge>();

        _state.DualcastLeft = Math.Max(
            StatusDetails(Player, SID.Dualcast, Player.InstanceID).Left,
            StatusDetails(Player, SID.LostChainspell, Player.InstanceID).Left
        );
        _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
        _state.AccelerationLeft = StatusDetails(Player, SID.Acceleration, Player.InstanceID).Left;
        _state.ManaficationLeft = StatusDetails(Player, SID.Manafication, Player.InstanceID).Left;
        _state.VerstoneReadyLeft = StatusDetails(Player, SID.VerstoneReady, Player.InstanceID).Left;
        _state.VerfireReadyLeft = StatusDetails(Player, SID.VerfireReady, Player.InstanceID).Left;
        _state.WhiteMana = gauge.WhiteMana;
        _state.BlackMana = gauge.BlackMana;
        _state.ManaStacks = gauge.ManaStacks;
    }

    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
    {
        var tar = initial;
        var range = initial.StayAtLongRange ? 25 : 15;

        if (_state.ResolutionReady)
        {
            tar = FindBetterTargetBy(initial, 25, (e) => NumResolutionTargets(e.Actor)).Target;
            range = tar.StayAtLongRange ? 25 : 15;
        }
        else if (_state.MinMana >= 60 - _state.ManaStacks * 20 && _strategy.NumMoulinetTargets >= 3)
        {
            tar = FindBetterTargetBy(initial, 8, (e) => NumMoulinetTargets(e.Actor)).Target;
            range = 8;
        }
        else if (_state.InMeleeCombo || _state.MinMana >= 50 && _strategy.NumMoulinetTargets < 3)
            range = 3;
        else
        {
            tar = FindBetterTargetBy(
                initial,
                25,
                (e) => NumCircleTargets(e.Actor, 5) * 1000000 + (int)e.Actor.HPMP.CurHP
            ).Target;
            range = tar.StayAtLongRange ? 25 : 15;
        }

        return new(tar, range);
    }

    protected override void QueueAIActions() { }

    private void OnConfigModified(RDMConfig config)
    {
        SupportedSpell(AID.Jolt).PlaceholderForAuto = SupportedSpell(AID.JoltII).PlaceholderForAuto =
            config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Scatter).PlaceholderForAuto = SupportedSpell(AID.Impact).PlaceholderForAuto =
            config.FullRotation ? AutoActionAOE : AutoActionNone;

        SupportedSpell(AID.Vercure).TransformTarget = SmartTargetFriendlyOrSelf;
        SupportedSpell(AID.Verraise).TransformAction = config.SmartRaise ? SmartVerraise : null;
        SupportedSpell(AID.Verraise).TransformTarget = config.SmartRaise ? SmartTargetDead : null;
    }

    private ActionID SmartVerraise()
    {
        if (_state.DualcastLeft > 0 || _state.SwiftcastLeft > 0)
            return ActionID.MakeSpell(AID.Verraise);

        if (_state.CD(CDGroup.Swiftcast) == 0)
            return ActionID.MakeSpell(AID.Swiftcast);

        if (_state.TargetingEnemy && _state.RangeToTarget <= 25)
            return ActionID.MakeSpell(_state.BestJolt);

        return ActionID.MakeSpell(AID.Vercure);
    }

    private int NumCircleTargets(Actor? primary, float radius)
        => primary == null ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, radius);

    private int NumMoulinetTargets(Actor? primary)
        => primary == null
            ? 0
            : Autorot.Hints.NumPriorityTargetsInAOECone(
                Player.Position,
                8,
                (primary.Position - Player.Position).Normalized(),
                60.Degrees()
            );

    private int NumResolutionTargets(Actor? primary)
        => primary == null
            ? 0
            : Autorot.Hints.NumPriorityTargetsInAOERect(
                Player.Position,
                (primary.Position - Player.Position).Normalized(),
                25,
                2
            );
}
