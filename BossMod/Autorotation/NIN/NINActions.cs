using System;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.NIN
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private Rotation.State _state;
        private CommonRotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _state = new(autorot.Cooldowns);
            _strategy = new();

            AID[] allJutsu =
            [
                AID.Ninjutsu,
                AID.FumaShuriken,
                AID.Katon,
                AID.Raiton,
                AID.Hyoton,
                AID.Huton,
                AID.Doton,
                AID.Suiton,
                AID.RabbitMedium,
                AID.HyoshoRanryu,
                AID.GokaMekkyaku
            ];
            foreach (AID jutsu in allJutsu)
            {
                SupportedSpell(jutsu).TransformAction = () =>
                    ActionID.MakeSpell(_state.CurNinjutsu);
            }
            SupportedSpell(AID.Ten).TransformAction = () => ActionID.MakeSpell(_state.CurTen);
            SupportedSpell(AID.Chi).TransformAction = () => ActionID.MakeSpell(_state.CurChi);
            SupportedSpell(AID.Jin).TransformAction = () => ActionID.MakeSpell(_state.CurJin);
        }

        public override CommonRotation.PlayerState GetState()
        {
            return _state;
        }

        public override CommonRotation.Strategy GetStrategy()
        {
            return _strategy;
        }

        public override void Dispose() { }

        protected override NextAction CalculateAutomaticGCD()
        {
            return new();
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            return new();
        }

        protected override void UpdateInternalState(int autoAction)
        {
            FillCommonPlayerState(_state);
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionDex);

            var gauge = Service.JobGauges.Get<NINGauge>();
            _state.HutonLeft = gauge.HutonTimer / 1000f;
            _state.Ninki = gauge.Ninki;

            _state.CurMudra = StatusDetails(Player, SID.Mudra, Player.InstanceID);
            _state.KassatsuLeft = StatusDetails(Player, SID.Kassatsu, Player.InstanceID).Left;
            _state.TCJLeft = StatusDetails(Player, SID.TenChiJin, Player.InstanceID).Left;
            _state.SuitonLeft = StatusDetails(Player, SID.Suiton, Player.InstanceID).Left;
            _state.DotonLeft = StatusDetails(Player, SID.Doton, Player.InstanceID).Left;
            _state.MeisuiLeft = StatusDetails(Player, SID.Meisui, Player.InstanceID).Left;
            _state.RaijuReadyLeft = StatusDetails(Player, SID.RaijuReady, Player.InstanceID).Left;
            _state.HiddenLeft = StatusDetails(Player, SID.Hidden, Player.InstanceID).Left;
            _state.BunshinLeft = StatusDetails(Player, SID.Bunshin, Player.InstanceID).Left;
            _state.KamaitachiLeft = StatusDetails(
                Player,
                SID.PhantomKamaitachiReady,
                Player.InstanceID
            ).Left;
            _state.TargetMugLeft = StatusDetails(
                Autorot.PrimaryTarget,
                SID.VulnerabilityUp,
                Player.InstanceID
            ).Left;
            _state.TargetTrickLeft = StatusDetails(
                Autorot.PrimaryTarget,
                SID.TrickAttack,
                Player.InstanceID
            ).Left;
        }

        protected override void QueueAIActions() { }
    }
}
