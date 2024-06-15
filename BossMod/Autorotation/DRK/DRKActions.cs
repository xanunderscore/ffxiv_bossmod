using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.DRK;

class Actions : TankActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private WPos _saltedEarthPosition;

    private readonly ConfigListener<DRKConfig> _config;
    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.GetAndSubscribe<DRKConfig>(OnConfigModified);
        _state = new(autorot.WorldState);
        _strategy = new();

        SupportedSpell(AID.SaltedEarth).TransformAction = () => ActionID.MakeSpell(_state.BestSalt);
        SupportedSpell(AID.Oblation).Condition = (tar) => tar?.FindStatus((uint)SID.Oblation) == null;
        SupportedSpell(AID.Grit).TransformAction = SupportedSpell(AID.ReleaseGrit).TransformAction = () =>
            ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseGrit : AID.Grit);
    }

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;

    public override CommonRotation.Strategy GetStrategy() => _strategy;

    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
    {
        if (_state.Unlocked(AID.AbyssalDrain) && _state.CD(CDGroup.AbyssalDrain) <= 0.6f)
        {
            (var newTarget, var newPrio) =
                FindBetterTargetBy(initial, 20, act => NumDrainTargets(act.Actor));
            if (newPrio > 2)
                return new(newTarget, 3);
        }

        return new(initial);
    }



    protected override void UpdateInternalState(int autoAction)
    {
        base.UpdateInternalState(autoAction);
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(
            Autorot
                .Bossmods.ActiveModule?.PlanExecution
                ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? [],
            _config.Data.AutomaticTBNFallback
        );

        if (!_config.Data.AutoPlunge)
            _strategy.PlungeStrategy = Rotation.Strategy.PlungeUse.Delay;

        _strategy.NumSaltTargets = Autorot.Hints.NumPriorityTargetsInAOECircle(
            _saltedEarthPosition == default ? Player.Position : _saltedEarthPosition,
            5
        );
        _strategy.NumAOETargets =
            autoAction == AutoActionST ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        _strategy.NumSHBTargets =
            Autorot.PrimaryTarget == null ? 0 : NumCleaveTargets(Autorot.PrimaryTarget);
        _strategy.NumFloodTargets =
            (autoAction == AutoActionST && _state.Unlocked(AID.EdgeOfDarkness)) ? 0 : _strategy.NumSHBTargets;
        _strategy.NumDrainTargets =
            autoAction == AutoActionST || Autorot.PrimaryTarget == null
                ? 0
                : NumDrainTargets(Autorot.PrimaryTarget);
    }

    private int NumDrainTargets(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);

    private int NumCleaveTargets(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);
        if (_state.AnimationLockDelay < 0.1f)
            _state.AnimationLockDelay = 0.1f; // TODO: reconsider; we generally don't want triple weaves or extra-late proc weaves

        _state.HaveTankStance = Player.FindStatus(SID.Grit) != null;

        var gauge = Service.JobGauges.Get<DRKGauge>();
        _state.Blood = gauge.Blood;
        _state.ShadowLeft = gauge.ShadowTimeRemaining / 1000f;
        _state.DarksideLeft = gauge.DarksideTimeRemaining / 1000f;
        _state.HaveDarkArts = gauge.HasDarkArts;
        _state.Delirium = StatusDetails(Player, SID.Delirium, Player.InstanceID);
        _state.BloodWeapon = StatusDetails(Player, SID.BloodWeapon, Player.InstanceID);
        _state.SaltedEarthLeft = StatusDetails(Player, SID.SaltedEarth, Player.InstanceID).Left;
        if (_state.SaltedEarthLeft == 0)
            _saltedEarthPosition = default;
    }

    protected override void OnActionSucceeded(ActorCastEvent ev)
    {
        if (ev.Action.ID == (uint)AID.SaltedEarth)
            _saltedEarthPosition = Player.Position;
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.Interject))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Interject), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.Grit))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Grit), Player, ShouldSwapStance());
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (AutoAction < AutoActionAIFight)
            return default;

        if (AutoAction == AutoActionAIFight && Autorot.PrimaryTarget != null && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.Unmend))
            return MakeResult(AID.Unmend, Autorot.PrimaryTarget); // TODO: reconsider...

        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, false);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline, true);
        return MakeResult(res, Autorot.PrimaryTarget);
    }

    private void OnConfigModified(DRKConfig config)
    {
        SupportedSpell(AID.HardSlash).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Unleash).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        SupportedSpell(AID.Shirk).TransformTarget = config.SmartTargetShirk ? SmartTargetCoTank : null;
        SupportedSpell(AID.TheBlackestNight).TransformTarget = SupportedSpell(AID.Oblation).TransformTarget =
            config.SmartTargetFriendly ? SmartTargetFriendlyOrSelf : null;
    }
}
