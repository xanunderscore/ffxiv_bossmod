using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.PLD;

class Actions : TankActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<PLDConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        SupportedSpell(AID.Reprisal).Condition = _ => Autorot.Hints.PotentialTargets.Any(e => e.Actor.Position.InCircle(Player.Position, 5 + e.Actor.HitboxRadius)); // TODO: consider checking only target?..
        SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;
        SupportedSpell(AID.IronWill).TransformAction = () => ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseIronWill : AID.IronWill);
        SupportedSpell(AID.Sheltron).TransformAction = () => ActionID.MakeSpell(_state.BestSheltron);
        SupportedSpell(AID.SpiritsWithin).TransformAction = () => ActionID.MakeSpell(_state.BestExpiacion);
        SupportedSpell(AID.RageOfHalone).TransformAction = () => ActionID.MakeSpell(_state.BestRoyalAuthority);

        SupportedSpell(AID.Clemency).TransformTarget =
            SupportedSpell(AID.Cover).TransformTarget =
            SupportedSpell(AID.Intervention).TransformTarget = SmartTargetFriendly;

        _config = Service.Config.GetAndSubscribe<PLDConfig>(OnConfigModified);
    }

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;
    public override CommonRotation.Strategy GetStrategy() => _strategy;

    protected override void UpdateInternalState(int autoAction)
    {
        base.UpdateInternalState(autoAction);

        _strategy.NumAOETargets = Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        _strategy.NumConfiteorTargets = Autorot.PrimaryTarget is null ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(Autorot.PrimaryTarget.Position, 5);

        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
    }

    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
    {
        var bestTarget = initial;
        var range = 3;
        if (_state.ConfiteorCombo != AID.None)
        {
            bestTarget = FindBetterTargetBy(
                initial,
                25,
                e => Autorot.Hints.NumPriorityTargetsInAOECircle(e.Actor.Position, 5)
            ).Target;
            range = bestTarget.StayAtLongRange ? 25 : 15;
        }
        return new(bestTarget, range);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.Interject))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Interject), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.IronWill))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.IronWill), Player, ShouldSwapStance());
        if (_state.Unlocked(AID.Provoke))
        {
            var provokeEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeTanked && e.PreferProvoking && e.Actor.TargetID != Player.InstanceID && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Provoke), provokeEnemy?.Actor, provokeEnemy != null);
        }
    }

    protected override NextAction CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return new();
        if (AutoAction == AutoActionAIFight && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.ShieldLob) && _state.DivineMightLeft < _state.GCD && _state.ConfiteorCombo == AID.None)
            return MakeResult(AID.ShieldLob, Autorot.PrimaryTarget); // TODO: reconsider...
        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override NextAction CalculateAutomaticOGCD(float deadline)
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return new();

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);
        return MakeResult(res, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);
        if (_state.AnimationLockDelay < 0.1f)
            _state.AnimationLockDelay = 0.1f;

        _state.HaveTankStance = Player.FindStatus(SID.IronWill) != null;

        var gauge = Service.JobGauges.Get<PLDGauge>();

        // TODO: worldstate doesn't track this type of buff-conditional combo at the moment
        // Confiteor is usable when the Confiteor status is active, but no such corresponding status exists for the
        // Blade of X combo actions, it's somewhere inside actionmanager
        var confiteorID = ActionManagerEx.Instance!.GetAdjustedActionID((uint)AID.Confiteor);

        _state.OathGauge = gauge.OathGauge;

        _state.FightOrFlightLeft = StatusDetails(Player, SID.FightOrFlight, Player.InstanceID).Left;
        _state.DivineMightLeft = StatusDetails(Player, SID.DivineMight, Player.InstanceID).Left;
        _state.Requiescat = StatusDetails(Player, SID.Requiescat, Player.InstanceID);
        _state.SwordOath = StatusDetails(Player, SID.SwordOath, Player.InstanceID);
        if (confiteorID == (uint)AID.Confiteor)
            _state.ConfiteorCombo = StatusDetails(Player, SID.ConfiteorReady, Player.InstanceID).Left > _state.GCD ? AID.Confiteor : AID.None;
        else
            _state.ConfiteorCombo = (AID)confiteorID;
    }

    private void OnConfigModified(PLDConfig config)
    {
        // placeholders
        SupportedSpell(AID.FastBlade).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.TotalEclipse).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.Shirk).TransformTarget = config.SmartShirkTarget ? SmartTargetCoTank : null;
        SupportedSpell(AID.Provoke).TransformTarget = config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
    }
}
