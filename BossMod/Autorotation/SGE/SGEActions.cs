using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SGE;

// TODO: this is shit, like all healer modules...
class Actions : HealerActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly ConfigListener<SGEConfig> _config;
    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.GetAndSubscribe<SGEConfig>(OnConfigModified);
        _state = new(autorot.WorldState);
        _strategy = new();
    }

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;

    public override CommonRotation.Strategy GetStrategy() => _strategy;

    private Actor? _autoRaiseTarget;
    private Actor? _kardiaTarget;

    private IEnumerable<Actor> AlliesNeedShield
        => Autorot.WorldState.Party.WithoutSlot(partyOnly: true).Where(NeedShield);
    private IEnumerable<Actor> NearbyAlliesNeedShield
        => AlliesNeedShield.Where(x => x.Position.InCircle(Player.Position, 15));

    protected override void UpdateInternalState(int autoAction)
    {
        base.UpdateInternalState(autoAction);
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
        _strategy.ApplyStrategyOverrides(
            Autorot
                .Bossmods.ActiveModule?.PlanExecution
                ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []
        );
        _strategy.NumDyskrasiaTargets =
            autoAction == AutoActionST ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5f);
        _strategy.NumToxikonTargets = _strategy.NumPhlegmaTargets =
            Autorot.PrimaryTarget == null
                ? 0
                : Autorot.Hints.NumPriorityTargetsInAOECircle(Autorot.PrimaryTarget.Position, 5f);
        _strategy.NumPneumaTargets = Autorot.PrimaryTarget == null ? 0 : NumPneumaTargets(Autorot.PrimaryTarget);
    }

    private static bool NeedShield(Actor act)
    {
        // already shielded
        if (act.Statuses.Any(s => (SID)s.ID is SID.EukrasianDiagnosis or SID.EukrasianPrognosis))
            return false;

        // tanks generally won't need GCD shield
        return act.Role != Role.Tank;
    }

    private int NumPneumaTargets(Actor primary)
        => Autorot.Hints.NumPriorityTargetsInAOERect(
            Player.Position,
            (primary.Position - Player.Position).Normalized(),
            25,
            2
        );

    protected override void QueueAIActions() { }

    protected override NextAction CalculateAutomaticGCD()
    {
        var shieldGcd = GetShieldGCD();
        if (shieldGcd != default)
            return shieldGcd;

        if (_config.Data.AutoEsuna && Rotation.CanCast(_state, _strategy, 1))
        {
            var esunaTarget = FindEsunaTarget();
            if (esunaTarget != null)
                return MakeResult(AID.Esuna, esunaTarget);
        }

        if (_autoRaiseTarget != null && _state.Unlocked(AID.Egeiro))
        {
            if (_config.Data.AutoRaise == SGEConfig.RaiseBehavior.Auto && _state.SwiftcastLeft > _state.GCD)
                return MakeResult(AID.Egeiro, _autoRaiseTarget);

            if (_config.Data.AutoRaise == SGEConfig.RaiseBehavior.AutoSlow)
                return MakeResult(AID.Egeiro, _autoRaiseTarget);
        }

        return MakeResult(Rotation.GetNextBestGCD(_state, _strategy), Autorot.PrimaryTarget);
    }

    protected override NextAction CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return new();

        // just use zoe asap, it has a very long buff window
        if (_strategy.GCDShieldUse == Rotation.Strategy.GCDShieldStrategy.ProgZoe && _state.CanWeave(CDGroup.Zoe, 0.6f, deadline))
            return MakeResult(AID.Zoe, Player);

        NextAction res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
        if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
            res = GetNextBestOGCD(deadline);

        return res;
    }

    private NextAction GetShieldGCD()
    {
        if (_strategy.GCDShieldUse == Rotation.Strategy.GCDShieldStrategy.Manual)
            return default;

        var zoe = _strategy.GCDShieldUse == Rotation.Strategy.GCDShieldStrategy.ProgZoe;
        // if we haven't used zoe yet, wait for it to be used
        // if zoe isn't active but is on cooldown, i.e. it was spent on some other heal GCD, then fall through, unboosted shield is more valuable than nothing
        if (zoe && _state.ZoeLeft == 0 && _state.CD(CDGroup.Zoe) == 0)
            return default;

        // TODO: tweak threshold?
        if (NearbyAlliesNeedShield.Count() > 3)
            return _state.Eukrasia ? MakeResult(AID.EukrasianPrognosis, Player) : MakeResult(AID.Eukrasia, Player);

        var nextAlly = AlliesNeedShield.FirstOrDefault();
        if (nextAlly != null)
            return _state.Eukrasia ? MakeResult(AID.EukrasianDiagnosis, nextAlly) : MakeResult(AID.Eukrasia, Player);

        return default;
    }

    private NextAction GetNextBestOGCD(float deadline)
    {
        if (
            _kardiaTarget != null
            && _state.Unlocked(AID.Kardia)
            && StatusDetails(_kardiaTarget, (uint)SID.Kardion, Player.InstanceID).Left == 0
            && _state.CanWeave(CDGroup.Kardia, 0.6f, deadline)
        )
            return MakeResult(ActionID.MakeSpell(AID.Kardia), _kardiaTarget);

        if (_autoRaiseTarget != null && _state.SwiftcastLeft == 0 && _state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
            return MakeResult(ActionID.MakeSpell(AID.Swiftcast), Player);

        var ogcd = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

        if (
            !ogcd
            && _config.Data.PreventGallOvercap
            && (_state.Gall == 3 || _state.Gall == 2 && _state.NextGall < 2.5)
            && _state.CurMP <= 9000
        )
            return MakeResult(ActionID.MakeSpell(AID.Druochole), FindBestSTHealTarget(1).Target ?? Player);

        return MakeResult(ogcd, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);

        var gauge = Service.JobGauges.Get<SGEGauge>();
        _state.Gall = gauge.Addersgall;
        _state.Sting = gauge.Addersting;
        _state.NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        _state.Eukrasia = gauge.Eukrasia;

        _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
        _state.TargetDotLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedEudosis, Player.InstanceID).Left;

        _autoRaiseTarget = _config.Data.AutoRaise is SGEConfig.RaiseBehavior.Auto or SGEConfig.RaiseBehavior.AutoSlow
            ? FindRaiseTarget()
            : null;
        _kardiaTarget = _config.Data.AutoKardia ? FindKardiaTarget() : null;
    }

    private void OnConfigModified(SGEConfig config)
    {
        // placeholders
        SupportedSpell(AID.Dosis).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Dyskrasia).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.Diagnosis).TransformTarget =
            SupportedSpell(AID.Druochole).TransformTarget =
            SupportedSpell(AID.Kardia).TransformTarget =
            SupportedSpell(AID.Taurochole).TransformTarget =
            SupportedSpell(AID.Krasis).TransformTarget =
            SupportedSpell(AID.Haima).TransformTarget =
            SupportedSpell(AID.Esuna).TransformTarget =
            SupportedSpell(AID.Rescue).TransformTarget =
                config.MouseoverFriendly ? SmartTargetFriendlyOrSelf : null;

        SupportedSpell(AID.Icarus).TransformTarget = config.MouseoverIcarus
            ? (act) => Autorot.SecondaryTarget ?? act
            : null;

        SupportedSpell(AID.Egeiro).TransformTarget =
            config.AutoRaise == SGEConfig.RaiseBehavior.SmartManual
                ? ((act) => Autorot.SecondaryTarget ?? FindRaiseTarget())
                : config.MouseoverFriendly
                    ? SmartTargetFriendly
                    : null;
    }

    private Actor? FindKardiaTarget()
    {
        if (!_config.Data.AutoKardia)
            return null;

        var party = Autorot.WorldState.Party.WithoutSlot(partyOnly: true);

        if (party.Count(x => x.Type == ActorType.Player) == 1)
            return Player;

        var tanks = party.Where(x => x.Class.GetRole() == Role.Tank);
        if (tanks.Count() == 1)
            return tanks.First();

        return null;
    }
}
