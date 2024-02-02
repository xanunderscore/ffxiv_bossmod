using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace BossMod.NIN
{
    public static class Rotation
    {
        public class State(float[] cooldowns) : CommonRotation.PlayerState(cooldowns)
        {
            public float KassatsuLeft;
            public float HutonLeft;
            public float SuitonLeft;
            public float DotonLeft;
            public float TargetMugLeft;
            public float TargetTrickLeft;
            public float MeisuiLeft;
            public float HiddenLeft;
            public float KamaitachiLeft;

            public (float Left, int Combo) TenChiJin;
            public (float Left, int Combo) Mudra;
            public (float Left, int Stacks) Bunshin;
            public (float Left, int Stacks) RaijuReady;

            public byte Ninki;

            public AID ComboLastMove => ComboTimeLeft > GCD ? (AID)ComboLastAction : AID.None;

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public float NextMudraCD => KassatsuLeft > 0 ? 0 : MathF.Max(CD(CDGroup.Ten) - 20, 0);

            private static readonly string[] MudraNames = ["", "Ten", "Chi", "Jin"];

            public static string ShowMudra(int combo)
            {
                var mudraDescription = new StringBuilder("[");
                if (combo > 0)
                    mudraDescription.Append(MudraNames[combo & 3]);
                if (combo > 4)
                    mudraDescription.Append($",{MudraNames[(combo >> 2) & 3]}");
                if (combo > 16)
                    mudraDescription.Append($",{MudraNames[(combo >> 4) & 3]}");
                mudraDescription.Append(']');
                return mudraDescription.ToString();
            }

            public override string ToString()
            {
                return $"M={ShowMudra(Mudra.Combo)},{Mudra.Left:f3}";
            }
        }

        public class Strategy : CommonRotation.Strategy
        {
            public bool NonCombatHide;
            public bool AllowDashRaiju;

            public void ApplyStrategyOverrides(uint[] overrides) { }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            AID act;

            if (strategy.CombatTimer < 0)
            {
                if (strategy.CombatTimer < -100 && state.HutonLeft < 5 && DoNinjutsu(state, AID.Huton, out act))
                    return act;

                if (
                    strategy.CombatTimer > -9.5
                    && strategy.CombatTimer < -6
                    && state.HutonLeft < 50
                    && DoNinjutsu(state, AID.Huton, out act)
                )
                    return act;

                if (strategy.CombatTimer > -6 && DoNinjutsu(state, AID.Suiton, out act))
                {
                    // delay suiton
                    if (act == AID.Suiton && strategy.CombatTimer < -1)
                        return AID.None;

                    return act;
                }
            }

            if (!state.TargetingEnemy)
                return AID.None;

            if (
                state.TargetTrickLeft == 0
                && state.CD(CDGroup.TrickAttack) < 20
                && state.SuitonLeft == 0
                && DoNinjutsu(state, AID.Suiton, out act)
            )
                return act;

            if (state.CD(CDGroup.Mug) > 0 && state.KamaitachiLeft > state.GCD)
                return AID.PhantomKamaitachi;

            if (state.TenChiJin.Left > 0)
                return state.TenChiJin.Combo switch
                {
                    0 => AID.FumaTen,
                    1 => AID.TCJRaiton,
                    _ => AID.TCJSuiton
                };

            if (state.RaijuReady.Left > state.GCD)
            {
                if (state.RangeToTarget <= 3)
                    return AID.FleetingRaiju;
                else if (strategy.AllowDashRaiju)
                    return AID.ForkedRaiju;
            }

            if (ShouldUseDamageNinjutsu(state, strategy))
            {
                if (state.KassatsuLeft > state.GCD && DoNinjutsu(state, AID.HyoshoRanryu, out act))
                    return act;

                if (DoNinjutsu(state, AID.Raiton, out act))
                    return act;
            }

            if (state.ComboLastMove == AID.GustSlash)
                return state.HutonLeft < 30 ? AID.ArmorCrush : AID.AeolianEdge;

            if (state.ComboLastMove == AID.SpinningEdge)
                return AID.GustSlash;

            return AID.SpinningEdge;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool lastSlot)
        {
            // don't use anything during ninjutsu as it breaks combo
            if (state.Mudra.Left > 0 || state.TenChiJin.Left > 0)
                return new();

            if (
                state.CD(CDGroup.Ten) > 0
                && strategy.NonCombatHide
                && state.CanWeave(CDGroup.Hide, 0.6f, deadline)
                && strategy.CombatTimer <= 0
            )
                return ActionID.MakeSpell(AID.Hide);

            if (
                strategy.CombatTimer > -1
                && state.CanWeave(CDGroup.Kassatsu, 0.6f, deadline)
                && state.Unlocked(AID.Kassatsu)
            )
                return ActionID.MakeSpell(AID.Kassatsu);

            if (state.TargetTrickLeft > 0)
            {
                if (state.CanWeave(CDGroup.DreamWithinADream, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.DreamWithinADream);

                if (state.RaijuReady.Left > state.GCD && state.CanWeave(CDGroup.TenChiJin, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.TenChiJin);

                if (state.SuitonLeft > state.GCD && state.CanWeave(CDGroup.Meisui, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Meisui);

                if (
                    state.CD(CDGroup.Meisui) > 0
                    && state.Ninki >= 50
                    && state.CanWeave(CDGroup.HellfrogMedium, 0.6f, deadline)
                )
                    return ActionID.MakeSpell(AID.Bhavacakra);
            }

            if (state.ComboLastMove == AID.GustSlash && state.CanWeave(CDGroup.Mug, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Mug);

            if (ShouldUseBunshin(state, strategy) && state.CanWeave(CDGroup.Bunshin, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Bunshin);

            if (ShouldUseTrick(state, strategy) && state.CanWeave(CDGroup.TrickAttack, 0.6f, deadline))
                return ActionID.MakeSpell(AID.TrickAttack);

            return new();
        }

        private static bool DoNinjutsu(State state, AID ninjutsu, out AID act)
        {
            act = AID.None;

            if (!state.Unlocked(ninjutsu) || (state.Mudra.Left == 0 && state.NextMudraCD > state.GCD))
                return false;

            Service.Log($"trying to cast {ninjutsu}");

            act = Combos.GetNextAction(ninjutsu, state.Mudra.Combo);

            if (act == AID.None)
            {
                act = AID.RabbitMedium;
            }
            else
            {
                if (act == AID.Hyoton && ninjutsu == AID.HyoshoRanryu)
                    act = AID.HyoshoRanryu;
                if (act == AID.Katon && ninjutsu == AID.GokaMekkyaku)
                    act = AID.GokaMekkyaku;
            }

            return true;
        }

        private static bool ShouldUseDamageNinjutsu(State state, Strategy strategy)
        {
            // spam raiton in trick windows
            if (state.TargetTrickLeft > state.GCD && state.Bunshin.Stacks < 5)
                return true;

            // prevent charge overcap (TODO: make sure we are saving for trick)
            // (note that the first mudra use increases the cooldown by 20s)
            if (state.CD(CDGroup.Ten) < (state.Mudra.Left > 0 ? 25 : 5))
                return true;

            return false;
        }

        private static bool ShouldUseTrick(State state, Strategy strategy)
        {
            if (!state.Unlocked(AID.TrickAttack) || (strategy.CombatTimer > 0 && state.SuitonLeft == 0))
                return false;

            if (strategy.CombatTimer < 30)
                return state.TargetMugLeft > 0 && state.CD(CDGroup.Bunshin) > 0 && state.GCD > 0.800;

            return true;
        }

        private static bool ShouldUseBunshin(State state, Strategy strategy)
        {
            if (!state.Unlocked(AID.Bunshin) || state.Ninki < 50)
                return false;

            if (strategy.CombatTimer < 30)
                return state.TargetMugLeft > 0;

            return true;
        }
    }
}
