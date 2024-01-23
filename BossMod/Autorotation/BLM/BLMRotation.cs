using System;
using System.Net.NetworkInformation;
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
            public int UmbralHearts; // max 3
            public int Polyglot; // max 2
            public float EnochianTimer;
            public float NextPolyglot => EnochianTimer;

            public float InstantCastLeft => MathF.Max(SwiftcastLeft, TriplecastLeft);

            // upgrade paths
            public AID BestThunder3 => Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;

            // statuses
            public SID ExpectedThunder3 => Unlocked(AID.Thunder3) ? SID.Thunder3 : SID.Thunder1;

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

            public override string ToString()
            {
                return $"MP={CurMP} (tick={TimeToManaTick:f1}), RB={RaidBuffsLeft:f1}, E={EnochianTimer}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, TC={ThundercloudLeft:f1}, FS={FirestarterLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumAOETargets;
        }

        private static unsafe bool CanCast(
            State state,
            Strategy strategy,
            float castTime,
            int mpCost
        )
        {
            if (state.InstantCastLeft > state.GCD)
                return true;
            if (mpCost > state.CurMP)
                return false;

            return strategy.ForceMovementIn >= state.GCD + castTime;
        }

        private static unsafe bool CanCast(
            State state,
            Strategy strategy,
            AID action,
            int mpCost
        ) => CanCast(state, strategy, GetCastTime(state, action), mpCost);

        private static uint ExpectedMPAfter(State state, float timeRemaining)
        {
            var expected = state.CurMP;

            var gainedPerTick = MPTick(state.ElementalLevel);
            timeRemaining -= state.TimeToManaTick;

            while (timeRemaining > 0)
            {
                expected += gainedPerTick;
                timeRemaining -= 3f;
            }
            return Math.Min(10000, expected);
        }

        public static float GetCastTime(State state, AID action)
        {
            if (
                (state.ElementalLevel < 0 && action == AID.Paradox)
                || (state.FirestarterLeft > state.GCD && action == AID.Fire3)
                || (state.ThundercloudLeft > state.GCD && action.Aspect() == BLM.Aspect.Thunder)
            )
                return 0f;

            var spsFactor = state.SpellGCDTime / 2.5f;

            var iceAdjust = state.ElementalLevel == 3 ? 0.5f : 1f;
            var fireAdjust = state.ElementalLevel == -3 ? 0.5f : 1f;

            var castTime = action.CastTime();
            var aspect = action.Aspect();

            if (aspect == BLM.Aspect.Ice)
                castTime *= iceAdjust;
            if (aspect == BLM.Aspect.Fire)
                castTime *= fireAdjust;

            return castTime * spsFactor;
        }

        public static float GetSlidecastTime(State state, AID action) =>
            MathF.Max(0f, GetCastTime(state, action) - 0.5f);

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
                if (
                    strategy.CombatTimer > -GetCastTime(state, AID.Fire3)
                    && state.ElementalLevel == 0
                )
                    return AID.Fire3;

                return AID.None;
            }

            if (!state.TargetingEnemy)
                return AID.None;

            if (state.ElementalLevel == 3)
                return GetFireGCD(state, strategy);

            if (state.ElementalLevel == -3)
                return GetIceGCD(state, strategy);

            if (state.ElementalLevel == 0)
            {
                // 2000 MP for fire 3 + at least one fire IV
                if (CanCast(state, strategy, AID.Fire3, 3600))
                    return AID.Fire3;

                if (CanCast(state, strategy, AID.Blizzard3, 800))
                    return AID.Blizzard3;

                if (CanCast(state, strategy, AID.Blizzard1, 400))
                    return AID.Blizzard1;

                if (state.Polyglot > 0)
                    return AID.Xenoglossy;
            }

            return AID.None;
        }

        public static AID GetFireGCD(State state, Strategy strategy)
        {
            var f4CastTime = GetCastTime(state, AID.Fire4);

            if (state.CurMP < 800)
            {
                if (
                    state.Polyglot > 0
                    && state.CanWeave(CDGroup.Manafont, 0.6f, state.GCD + state.SpellGCDTime)
                )
                    return AID.Xenoglossy;

                if (CanCast(state, strategy, AID.Blizzard3, 0))
                    return AID.Blizzard3;
            }

            if (
                state.CurMP <= 1600
                && state.Unlocked(AID.Despair)
                && CanCast(state, strategy, AID.Despair, 1)
            )
                return AID.Despair;

            if (
                state.Unlocked(AID.Fire4)
                && state.ElementalLeft < f4CastTime + GetCastTime(state, AID.Fire1)
            )
                return AID.Fire1;

            if (state.Polyglot == 2 && state.NextPolyglot < 5)
                return AID.Xenoglossy;

            if (state.TargetThunderLeft < 5 && CanCast(state, strategy, AID.Thunder3, 400))
                return AID.Thunder3;

            if (
                state.InstantCastLeft == 0
                && state.Unlocked(AID.Triplecast)
                && state.CD(CDGroup.Triplecast) == 0
            )
                return AID.Triplecast;

            if (
                state.Unlocked(AID.Fire4)
                && CanCast(state, strategy, f4CastTime, state.GetAdjustedFireCost(800))
            )
                return AID.Fire4;

            if (CanCast(state, strategy, AID.Fire1, state.GetAdjustedFireCost(800)))
                return AID.Fire1;

            return AID.None;
        }

        public static AID GetIceGCD(State state, Strategy strategy)
        {
            if (state.UmbralHearts == 0 && CanCast(state, strategy, AID.Blizzard4, 0))
                return AID.Blizzard4;

            if (state.Polyglot == 2 && state.NextPolyglot < 5)
                return AID.Xenoglossy;

            if (ExpectedMPAfter(state, state.GCD + GetSlidecastTime(state, AID.Fire3)) == 10000)
                return AID.Fire3;

            if (state.TargetThunderLeft < 5 && CanCast(state, strategy, AID.Thunder3, 400))
                return AID.Thunder3;

            if (state.Polyglot > 0 && state.Unlocked(AID.Xenoglossy))
                return AID.Xenoglossy;

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

                if (state.CanWeave(CDGroup.LeyLines, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.LeyLines);
            }

            if (state.InLeyLines && state.InstantCastLeft < state.GCD)
            {
                if (state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Swiftcast);

                if (state.CanWeave(state.CD(CDGroup.Triplecast) - 60, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Triplecast);
            }

            return new();
        }
    }
}
