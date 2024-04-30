using System;

namespace BossMod.BLU;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;
    public const int AutoMimicD = AutoActionFirstCustom + 2;
    public const int AutoMimicH = AutoActionFirstCustom + 3;
    public const int AutoMimicT = AutoActionFirstCustom + 4;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly BLUConfig _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.Get<BLUConfig>();
        _state = new(autorot.WorldState);
        _strategy = new();

        _config.Modified += OnConfigModified;
        OnConfigModified();

        SupportedSpell(AID.AethericMimicry).TransformAction = () =>
            ActionID.MakeSpell(
                _state.Mimic switch
                {
                    Rotation.Mimic.DPS => AID.AethericMimicryReleaseDamage,
                    Rotation.Mimic.Healer => AID.AethericMimicryReleaseHealer,
                    Rotation.Mimic.Tank => AID.AethericMimicryReleaseTank,
                    _ => AID.AethericMimicry
                }
            );
        SupportedSpell(AID.AethericMimicry).TransformTarget = SmartMimic;
    }

    protected override void Dispose(bool disposing)
    {
        _config.Modified -= OnConfigModified;
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;

    public override CommonRotation.Strategy GetStrategy() => _strategy;

    protected override void QueueAIActions()
    {
        SimulateManualActionForAI(
            ActionID.MakeSpell(AID.AethericMimicry),
            SmartMimic(Autorot.PrimaryTarget),
            !Player.InCombat && _state.Mimic == Rotation.Mimic.None && _state.OnSlot(AID.AethericMimicry)
        );
    }

    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
    {
        var t = base.SelectBetterTarget(initial);
        if (_state.HarmonizedLeft > _state.GCD)
            t.PreferredRange = 3;
        else
            t.PreferredRange = initial.StayAtLongRange ? 25 : 15;
        return t;
    }

    protected override NextAction CalculateAutomaticGCD()
    {
        if (AutoAction < AutoActionAIFight || !_state.TargetingEnemy)
            return new();

        if (Player.HP.Cur * 2 < Player.HP.Max && _state.OnSlot(AID.Rehydration))
            return MakeResult(AID.Rehydration, Player);

        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override NextAction CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return new();

        deadline += 0.4f;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

        var next = MakeResult(res, Autorot.PrimaryTarget);

        if (res.ID == (uint)AID.FeatherRain && next.Target != null)
            next.TargetPos = next.Target.PosRot.XYZ();

        return next;
    }

    protected override unsafe void UpdateInternalState(int autoAction)
    {
        FillCommonPlayerState(_state);
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);

        _strategy.Num15yTargets = Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 15);
        _strategy.Num10yTargets = Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 10);
        _strategy.Num6yTargets = Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 6);
        _strategy.NumSurpanakhaTargets = Autorot.Hints.NumPriorityTargetsInAOECone(
            Player.Position,
            16,
            Player.Rotation.ToDirection(),
            60.Degrees()
        );
        _strategy.NumFrozenTargets = Autorot.Hints.NumPriorityTargetsInAOE(
            e =>
                e.Actor.Position.InCircle(Player.Position, 6 + Player.HitboxRadius)
                && e.Actor.FindStatus(SID.DeepFreeze) != null
        );

        _state.TargetingBoss = Utils.IsBoss(Autorot.PrimaryTarget);

        if (_state.Chocobo is null || _state.Chocobo.IsDestroyed)
            _state.Chocobo = Autorot.WorldState.Actors.FirstOrDefault(
                a => a.Type == ActorType.Chocobo && a.OwnerID == Player.InstanceID
            );

        for (var i = 0; i < 24; i++)
            _state.BLUSlots[i] = (AID)
                FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetActiveBlueMageActionInSlot(i);

        _state.TargetMortalFlame = Autorot.PrimaryTarget?.FindStatus(SID.MortalFlame) != null;
        var bomExpire = Autorot.PrimaryTarget?.FindStatus(SID.BreathOfMagic)?.ExpireAt;
        _state.TargetBoMLeft = bomExpire == null ? 0f : StatusDuration(bomExpire.Value);
        _state.TargetDropsyLeft = StatusDetails(Autorot.PrimaryTarget, SID.Dropsy, Player.InstanceID).Left;
        _state.TargetSlowLeft = StatusDetails(Autorot.PrimaryTarget, SID.Slow, Player.InstanceID).Left;
        _state.TargetBindLeft = StatusDetails(Autorot.PrimaryTarget, SID.Bind, Player.InstanceID).Left;
        _state.TargetLightheadedLeft = StatusDetails(Autorot.PrimaryTarget, SID.Lightheaded, Player.InstanceID).Left;
        _state.TargetBegrimedLeft = StatusDetails(Autorot.PrimaryTarget, SID.Begrimed, Player.InstanceID).Left;
        _state.TargetBleedingLeft = StatusDetails(Autorot.PrimaryTarget, SID.Bleeding, Player.InstanceID).Left;

        _state.SurpanakhasFury = StatusDetails(Player, SID.SurpanakhasFury, Player.InstanceID);
        _state.WaxingLeft = StatusDetails(Player, SID.WaxingNocturne, Player.InstanceID).Left;
        _state.HarmonizedLeft = StatusDetails(Player, SID.Harmonized, Player.InstanceID).Left;
        _state.TinglingLeft = StatusDetails(Player, SID.Tingling, Player.InstanceID).Left;
        _state.BoostLeft = StatusDetails(Player, SID.Boost, Player.InstanceID).Left;
        _state.BrushWithDeathLeft = StatusDetails(Player, SID.BrushWithDeath, Player.InstanceID).Left;
        _state.ToadOilLeft = StatusDetails(Player, SID.ToadOil, Player.InstanceID).Left;
        _state.VeilLeft = StatusDetails(Player, SID.VeilOfTheWhorl, Player.InstanceID).Left;
        _state.ApokalypsisLeft = StatusDetails(Player, SID.Apokalypsis, Player.InstanceID).Left;
        _state.FlurryLeft = StatusDetails(Player, SID.PhantomFlurry, Player.InstanceID).Left;

        _state.Mimic = Rotation.Mimic.None;
        if (Player.FindStatus(SID.AethericMimicryDPS) != null)
            _state.Mimic = Rotation.Mimic.DPS;
        if (Player.FindStatus(SID.AethericMimicryHealer) != null)
            _state.Mimic = Rotation.Mimic.Healer;
        if (Player.FindStatus(SID.AethericMimicryTank) != null)
            _state.Mimic = Rotation.Mimic.Tank;
    }

    private void OnConfigModified()
    {
        SupportedSpell(AID.SonicBoom).PlaceholderForAuto =
            SupportedSpell(AID.WaterCannon).PlaceholderForAuto =
            SupportedSpell(AID.ChocoMeteor).PlaceholderForAuto =
                _config.FullRotation ? AutoActionST : AutoActionNone;
    }

    private Actor? SmartMimic(Actor? target)
    {
        if (_state.Mimic != Rotation.Mimic.None)
            return null;

        if (target != null && target.IsAlly)
            return target;

        return Autorot.WorldState.Actors.FirstOrDefault(
            x => x.Class.IsDD() && x.Position.InCircle(Player.Position, 25)
        );
    }
}
