using System;
using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SAM
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private SAMConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        private DateTime _lastTsubame;
        private float _tsubameCooldown = 0;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<SAMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override CommonRotation.PlayerState GetState() => _state;

        public override CommonRotation.Strategy GetStrategy() => _strategy;

        private void OnConfigModified(object? sender, EventArgs args)
        {
            SupportedSpell(AID.Hakaze).PlaceholderForAuto = _config.FullRotation
                ? AutoActionST
                : AutoActionNone;
            SupportedSpell(AID.Fuga).PlaceholderForAuto = SupportedSpell(
                AID.Fuko
            ).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

            return MakeResult(res, Autorot.PrimaryTarget);
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.SecondWind))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.SecondWind),
                    Player,
                    Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f
                );
            if (_state.Unlocked(AID.Bloodbath))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.Bloodbath),
                    Player,
                    Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.8f
                );
            if (_state.Unlocked(AID.MeikyoShisui))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.MeikyoShisui),
                    Player,
                    !_state.HasCombatBuffs
                        && _strategy.CombatTimer > 0
                        && _strategy.CombatTimer < 5
                        && _state.MeikyoLeft == 0
                );
            // TODO: true north...
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.ApplyStrategyOverrides(
                Autorot
                    .Bossmods.ActiveModule?.PlanExecution
                    ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []
            );

            _strategy.NumAOETargets =
                autoAction == AutoActionST
                    ? 0
                    : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
            _strategy.NumTenkaTargets =
                autoAction == AutoActionST
                    ? 0
                    : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
            _strategy.NumOgiTargets = NumConeTargets(Autorot.PrimaryTarget);
            _strategy.NumGurenTargets = NumGurenTargets(Autorot.PrimaryTarget);

            if (autoAction == AutoActionST)
                _strategy.NumFufuTargets = 0;
            else if (_state.Unlocked(AID.Fuko))
                _strategy.NumFufuTargets = _strategy.NumAOETargets;
            else
                _strategy.NumFufuTargets = NumConeTargets(Autorot.PrimaryTarget);

            FillStrategyPositionals(
                _strategy,
                Rotation.GetNextPositional(_state, _strategy),
                _state.TrueNorthLeft > _state.GCD
            );
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var newTsubameCooldown = _state.CD(CDGroup.TsubameGaeshi);
            if (newTsubameCooldown > _tsubameCooldown + 10) // eliminate variance, cd increment is 60s
                _lastTsubame = Autorot.WorldState.CurrentTime;

            _tsubameCooldown = newTsubameCooldown;

            var gauge = Service.JobGauges.Get<SAMGauge>();

            _state.HasIceSen = gauge.HasSetsu;
            _state.HasMoonSen = gauge.HasGetsu;
            _state.HasFlowerSen = gauge.HasKa;
            _state.MeditationStacks = gauge.MeditationStacks;
            _state.Kenki = gauge.Kenki;
            _state.Kaeshi = gauge.Kaeshi;
            _state.FukaLeft = StatusDetails(Player, SID.Fuka, Player.InstanceID).Left;
            _state.FugetsuLeft = StatusDetails(Player, SID.Fugetsu, Player.InstanceID).Left;
            _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
            _state.MeikyoLeft = StatusDetails(Player, SID.MeikyoShisui, Player.InstanceID).Left;
            _state.OgiNamikiriLeft = StatusDetails(
                Player,
                SID.OgiNamikiriReady,
                Player.InstanceID
            ).Left;

            _state.TargetHiganbanaLeft = _strategy.ForbidDOTs
                ? float.MaxValue
                : StatusDetails(Autorot.PrimaryTarget, SID.Higanbana, Player.InstanceID).Left;

            _state.GCDTime = ActionManagerEx.Instance!.GCDTime();
            _state.LastTsubame =
                _lastTsubame == default
                    ? float.MaxValue
                    : (float)(Autorot.WorldState.CurrentTime - _lastTsubame).TotalSeconds;

            _state.ClosestPositional = GetClosestPositional();
        }

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            if (Rotation.ShouldUseGuren(_state, _strategy))
            {
                var bestGuren = initial;
                var hit = NumGurenTargets(bestGuren.Actor);

                foreach (
                    var enemy in Autorot.Hints.PriorityTargets.Where(
                        e => e != initial && e.Actor.Position.InCircle(Player.Position, 10)
                    )
                )
                {
                    var newHit = NumGurenTargets(enemy.Actor);
                    if (newHit > hit)
                        bestGuren = enemy;
                }

                return new(initial, 10);
            }

            if (_state.OgiNamikiriLeft > 0 || !_state.Unlocked(AID.Fuko))
            {
                var bestAOE = initial;
                var hit = NumConeTargets(bestAOE.Actor);

                foreach (
                    var enemy in Autorot.Hints.PriorityTargets.Where(
                        e => e != initial && e.Actor.Position.InCircle(Player.Position, 8)
                    )
                )
                {
                    var newHit = NumConeTargets(enemy.Actor);
                    if (newHit > hit)
                        bestAOE = enemy;
                }

                return new(bestAOE, 8);
            }

            return new(initial, 3, _strategy.NextPositionalImminent ? _strategy.NextPositional : Positional.Any);
        }

        private Positional GetClosestPositional()
        {
            var tar = Autorot.PrimaryTarget;
            if (tar == null)
                return Positional.Any;

            return (Player.Position - tar.Position)
                .Normalized()
                .Dot(tar.Rotation.ToDirection()) switch
            {
                < -0.707167f => Positional.Rear,
                < 0.707167f => Positional.Flank,
                _ => Positional.Front
            };
        }

        private int NumGurenTargets(Actor? primary) =>
            primary == null
                ? 0
                : Autorot.Hints.NumPriorityTargetsInAOERect(
                    Player.Position,
                    (primary.Position - Player.Position).Normalized(),
                    10,
                    4
                );

        private int NumConeTargets(Actor? primary) =>
            primary == null
                ? 0
                : Autorot.Hints.NumPriorityTargetsInAOECone(
                    Player.Position,
                    8,
                    (primary.Position - Player.Position).Normalized(),
                    60.Degrees()
                );
    }
}
