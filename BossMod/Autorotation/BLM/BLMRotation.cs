using System;
using System.Data;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using BossMod.Components;
using BossMod.ReplayAnalysis;
using Dalamud.Game.ClientState.Structs;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace BossMod.BLM
{
    public static class Rotation
    {
        public enum Aspect
        {
            None = 0,
            Ice = 1,
            Fire = 2
        }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public float TimeToManaTick; // we assume mana tick happens every 3s
            public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
            public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float TriplecastLeft; // max 15
            public float SharpcastLeft; // max 30
            public float ThundercloudLeft;
            public float FirestarterLeft;
            public float TargetThunderLeft; // TODO: this shouldn't be here...
            public float LeyLinesLeft; // max 30
            public bool InLeyLines;
            public bool Paradox;
            public int UmbralHearts; // max 3
            public int Polyglot; // max 2
            public float EnochianTimer;
            public float NextPolyglot => EnochianTimer;

            public float InstantCastLeft => MathF.Max(SwiftcastLeft, TriplecastLeft);

            // upgrade paths
            public AID BestThunder1 => Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
            public AID BestThunder2 => Unlocked(AID.Thunder4) ? AID.Thunder4 : AID.Thunder2;

            public AID BestFire1 => Paradox ? AID.Paradox : AID.Fire1;
            public AID BestBlizzard1 => Paradox ? AID.Paradox : AID.Blizzard1;
            public AID BestFire2 => Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2;
            public AID BestBlizzard2 =>
                Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : AID.Blizzard2;

            public AID BestSTPolyglotSpell => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;

            // statuses
            public SID ExpectedThunder1 => Unlocked(AID.Thunder3) ? SID.Thunder3 : SID.Thunder1;
            public SID ExpectedThunder2 => Unlocked(AID.Thunder4) ? SID.Thunder4 : SID.Thunder2;

            public State(float[] cooldowns)
                : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public int GetAdjustedFireCost(int mpCost) =>
                ElementalLevel switch
                {
                    0 => mpCost,
                    > 0 => UmbralHearts > 0 ? mpCost : mpCost * 2,
                    < 0 => 0
                };

            public int GetAdjustedIceCost(int mpCost) =>
                ElementalLevel switch
                {
                    -3 => 0,
                    -2 => mpCost / 2,
                    -1 => mpCost / 4 * 3,
                    _ => mpCost
                };

            public uint MPPerTick =>
                ElementalLeft switch
                {
                    -3 => 6200,
                    -2 => 4700,
                    -1 => 3200,
                    0 => 200,
                    _ => 0
                };

            public uint ExpectedMPAfter(float delay)
            {
                var expected = CurMP;
                var perTick = MPPerTick;
                delay -= TimeToManaTick;

                while (delay > 0)
                {
                    expected += perTick;
                    delay -= 3f;
                }
                return Math.Min(10000, expected);
            }

            public float GetCastTime(AID action)
            {
                if (
                    action == AID.Paradox && ElementalLevel < 0
                    || InstantCastLeft > GCD
                    // todo: this is wrong if firestarter/thundercloud will expire *during* the next cast
                    || action == AID.Fire3 && FirestarterLeft > GCD
                    || action.Aspect() == BLM.Aspect.Thunder && ThundercloudLeft > GCD
                )
                    return 0f;

                var spsFactor = SpellGCDTime / 2.5f;
                var castTime = action.BaseCastTime();
                var aspect = action.Aspect();

                if (
                    aspect == BLM.Aspect.Ice && ElementalLevel == 3
                    || aspect == BLM.Aspect.Fire && ElementalLevel == -3
                )
                    castTime *= 0.5f;

                return castTime * spsFactor;
            }

            // in addition to slidecasting, this is also the time until the provided spell spends MP
            public float GetSlidecastTime(AID action) =>
                MathF.Max(0f, GetCastTime(action) - 0.500f);

            public override string ToString()
            {
                return $"MP={CurMP} (tick={TimeToManaTick:f1}), RB={RaidBuffsLeft:f1}, E={EnochianTimer}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, TC={ThundercloudLeft:f1}, FS={FirestarterLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public enum TriplecastUse : uint
            {
                // forced clip in opener, otherwise use when inside leylines
                Automatic = 0,
            }

            public OffensiveAbilityUse TriplecastStrategy;
            public OffensiveAbilityUse LeylinesStrategy;
            public bool UseAOERotation;
        }

        private static bool CanCast(State state, Strategy strategy, float castTime, int mpCost)
        {
            var castEndIn = state.GCD + castTime;

            if (mpCost > state.ExpectedMPAfter(castEndIn))
                return false;

            return strategy.ForceMovementIn >= castEndIn;
        }

        private static bool CanCast(
            State state,
            Strategy strategy,
            AID action,
            int mpCost
        ) =>
            state.Unlocked(action)
            && CanCast(state, strategy, state.GetSlidecastTime(action), mpCost);

        public static uint MPTick(int elementalLevel)
        {
            return elementalLevel switch
            {
                -3 => 6200,
                -2 => 4700,
                -1 => 3200,
                0 => 200,
                _ => 0
            };
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0)
            {
                if (strategy.CombatTimer > -state.GetSlidecastTime(AID.Fire3))
                    return AID.Fire3;

                return AID.None;
            }

            if (!state.TargetingEnemy)
                return AID.None;

            // if fire timer will run out soon...
            if (
                !strategy.UseAOERotation
                && state.Unlocked(AID.Fire4)
                && state.ElementalLevel == 3
                && state.ElementalLeft
                    < state.GetCastTime(AID.Fire4) + state.GetCastTime(AID.Fire1)
            )
            {
                // use despair now if we don't have enough mana for f1/paradox -> despair
                if (
                    state.CurMP - 800 < state.GetAdjustedFireCost(800)
                    && CanCast(state, strategy, AID.Despair, 800)
                )
                    return AID.Despair;

                // refresh timer
                return state.BestFire1;
            }

            // polyglot overcap
            if (state.Polyglot == 2 && state.NextPolyglot > 0 && state.NextPolyglot < 10)
                return strategy.UseAOERotation ? AID.Foul : state.BestSTPolyglotSpell;

            if (state.TargetThunderLeft < 5 && CanCast(state, strategy, state.BestThunder1, 400))
                return strategy.UseAOERotation && state.Unlocked(state.BestThunder2)
                    ? state.BestThunder2
                    : state.BestThunder1;

            var damageGCD =
                state.ElementalLevel > 0 ? GetFireGCD(state, strategy) : GetIceGCD(state, strategy);

            if (damageGCD == AID.None && state.Polyglot > 0)
                return strategy.UseAOERotation ? AID.Foul : state.BestSTPolyglotSpell;

            return damageGCD;
        }

        public static AID GetFireGCD(State state, Strategy strategy)
        {
            // despair/flare require at least 800 mp even though they only say all
            if (state.CurMP < 800)
            {
                // use instant spell for manafont weave
                if (
                    state.Polyglot > 0
                    && state.CanWeave(CDGroup.Manafont, 0.6f, state.GCD + state.SpellGCDTime)
                )
                    return state.BestSTPolyglotSpell;

                // otherwise swap to ice
                if (strategy.UseAOERotation && CanCast(state, strategy, state.BestBlizzard2, 0))
                    return state.BestBlizzard2;

                if (CanCast(state, strategy, AID.Blizzard3, 0))
                    return AID.Blizzard3;

                if (CanCast(state, strategy, state.BestBlizzard1, 0))
                    return state.BestBlizzard1;
            }

            if (
                strategy.UseAOERotation
                && state.UmbralHearts == 1
                && CanCast(state, strategy, AID.Flare, 1)
            )
                return AID.Flare;

            if (state.CurMP < state.GetAdjustedFireCost(strategy.UseAOERotation ? 1500 : 800))
            {
                if (
                    (strategy.UseAOERotation || !state.Unlocked(AID.Despair))
                    && CanCast(state, strategy, AID.Flare, 1)
                )
                    return AID.Flare;

                if (CanCast(state, strategy, AID.Despair, 1))
                    return AID.Despair;
            }

            // intentional gcd clip in opener
            if (
                state.InstantCastLeft == 0
                && strategy.TriplecastStrategy != CommonRotation.Strategy.OffensiveAbilityUse.Delay
                && state.Unlocked(AID.Triplecast)
                && state.CD(CDGroup.Triplecast) == 0
            )
                return AID.Triplecast;

            if (
                strategy.UseAOERotation
                && CanCast(state, strategy, state.BestFire2, state.GetAdjustedFireCost(1500))
            )
                return state.BestFire2;

            if (CanCast(state, strategy, AID.Fire4, state.GetAdjustedFireCost(800)))
                return AID.Fire4;
            // before F4 unlock, use firestarter proc for damage
            else if (state.FirestarterLeft > state.GCD && state.Unlocked(AID.Fire3))
            {
                return AID.Fire3;
            }

            if (CanCast(state, strategy, AID.Fire1, state.GetAdjustedFireCost(800)))
                return state.BestFire1;

            return AID.None;
        }

        public static AID GetIceGCD(State state, Strategy strategy)
        {
            if (
                state.UmbralHearts == 0
                && state.Unlocked(TraitID.EnhancedFreeze)
                && state.ElementalLevel < 0
            )
            {
                if (strategy.UseAOERotation && CanCast(state, strategy, AID.Freeze, 0))
                    return AID.Freeze;

                if (CanCast(state, strategy, AID.Blizzard4, 0))
                    return AID.Blizzard4;
            }

            if (state.Polyglot == 2 && state.NextPolyglot < 10)
                return strategy.UseAOERotation ? AID.Foul : state.BestSTPolyglotSpell;

            if (
                strategy.UseAOERotation
                && CanCast(state, strategy, state.BestFire2, 10000 - state.GetAdjustedIceCost(800))
                && (!state.Unlocked(TraitID.EnhancedFreeze) || state.UmbralHearts == 3)
            )
                return state.BestFire2;

            if (CanCast(state, strategy, AID.Fire3, 9600))
                return AID.Fire3;

            if (CanCast(state, strategy, AID.Fire1, 9600))
                return AID.Fire1;

            if (
                strategy.UseAOERotation
                && CanCast(state, strategy, state.BestBlizzard2, state.GetAdjustedIceCost(800))
            )
                return state.BestBlizzard2;

            if (
                CanCast(state, strategy, AID.Blizzard3, state.GetAdjustedIceCost(800))
                && state.ElementalLevel > -3
            )
                return AID.Blizzard3;

            if (state.Paradox)
                return AID.Paradox;

            if (CanCast(state, strategy, AID.Blizzard1, state.GetAdjustedIceCost(400)))
                return AID.Blizzard1;

            return AID.None;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0)
            {
                if (strategy.CombatTimer > -12 && state.SharpcastLeft == 0)
                    return ActionID.MakeSpell(AID.Sharpcast);

                return new();
            }

            if (
                state.CurMP < 800
                && state.ElementalLevel == 3
                && state.CanWeave(CDGroup.Manafont, 0.6f, deadline)
            )
                return ActionID.MakeSpell(AID.Manafont);

            if (state.TriplecastLeft > state.GCD)
            {
                if (state.CanWeave(CDGroup.Amplifier, 0.6f, deadline) && state.Polyglot < 2)
                    return ActionID.MakeSpell(AID.Amplifier);

                if (
                    state.CanWeave(CDGroup.LeyLines, 0.6f, deadline)
                    && strategy.LeylinesStrategy
                        != CommonRotation.Strategy.OffensiveAbilityUse.Delay
                    // don't place leylines after opener, let the player do it
                    && strategy.CombatTimer < 60
                )
                    return ActionID.MakeSpell(AID.LeyLines);
            }

            if (state.InLeyLines && state.InstantCastLeft < state.GCD)
            {
                if (state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Swiftcast);

                if (state.CanWeave(state.CD(CDGroup.Triplecast) - 60, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Triplecast);
            }

            // TODO: what we actually want to do is sharpcast *only* if we will refresh thunder using thundercloud, otherwise it will proc on paradox/f1
            if (
                state.ThundercloudLeft > 0
                && state.SharpcastLeft < state.GCD
                && state.CanWeave(state.CD(CDGroup.Sharpcast) - 30, 0.6f, deadline)
            )
                return ActionID.MakeSpell(AID.Sharpcast);

            return new();
        }
    }
}
