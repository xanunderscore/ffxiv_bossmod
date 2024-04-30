using System.Linq;

namespace BossMod.BLU;

public static class Rotation
{
    public enum Mimic : uint
    {
        None = 0,
        DPS = 1,
        Healer = 2,
        Tank = 3
    }

    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
    {
        public bool TargetMortalFlame;
        public float TargetBoMLeft; // 60s max
        public float TargetDropsyLeft;
        public float TargetSlowLeft;
        public float TargetBindLeft;
        public float TargetLightheadedLeft;
        public float TargetBegrimedLeft;
        public float TargetBleedingLeft; // 30s from song of torment
        public Mimic Mimic;

        public (float Left, int Stacks) SurpanakhasFury; // 3/3 max
        public int ReproStacks;

        public float WaxingLeft; // 15s max
        public float HarmonizedLeft; // 30s max
        public float TinglingLeft; // 15s max
        public float BoostLeft; // 30s max
        public float BrushWithDeathLeft;
        public float ToadOilLeft;
        public float VeilLeft;
        public float ApokalypsisLeft;
        public float FlurryLeft;

        public bool TargetingBoss;
        public Actor? Chocobo;

        public AID[] BLUSlots = new AID[24];

        public bool OnSlot(AID act) => BLUSlots.Contains(act);

        public override string ToString()
        {
            var slots = string.Join(", ", BLUSlots.Select(x => x.ToString()));
            return $"actions=[{slots}]";
        }
    }

    public class Strategy : CommonRotation.Strategy
    {
        public int Num15yTargets; // hydro pull
        public int Num10yTargets;
        public int Num6yTargets; // ram's voice
        public int NumFrozenTargets; // all 6y circle targets with Deep Freeze
        public int NumSurpanakhaTargets;
    }

    public static bool CanCast(State state, Strategy strategy, AID action)
    {
        if (!state.OnSlot(action))
            return false;

        var castTime = Definitions.SupportedActions[ActionID.MakeSpell(action)].CastTime * state.SpellGCDTime / 2.5f;

        return strategy.ForceMovementIn > castTime;
    }

    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        if (!state.TargetMortalFlame && state.TargetingBoss && CanCast(state, strategy, AID.MortalFlame))
            return AID.MortalFlame;

        if (state.TargetingBoss && state.TargetBleedingLeft < state.GCD && CanCast(state, strategy, AID.SongOfTorment))
            return AID.SongOfTorment;

        if (strategy.NumSurpanakhaTargets > 0 && state.OnSlot(AID.Surpanakha))
        {
            if (state.SurpanakhasFury.Left > 0 && state.CD(CDGroup.Surpanakha) < 90)
                return AID.Surpanakha;
            else if (state.SurpanakhasFury.Left == 0 && state.CD(CDGroup.Surpanakha) == 0)
                return AID.Surpanakha;
        }

        if (state.CD(CDGroup.WingedReprobation) < state.GCD && CanCast(state, strategy, AID.WingedReprobation))
            return AID.WingedReprobation;

        if (
            state.TargetBoMLeft <= state.GCD + state.SpellGCDTime
            && state.TargetingBoss
            && state.OnSlot(AID.BreathOfMagic)
            && CanCast(state, strategy, AID.Bristle)
            && state.RangeToTarget <= 10
            && state.BoostLeft == 0
        )
            return AID.Bristle;

        if (
            state.TargetBoMLeft <= state.GCD
            && CanCast(state, strategy, AID.BreathOfMagic)
            && state.RangeToTarget <= 10
            && state.TargetingBoss
        )
            return AID.BreathOfMagic;

        if (strategy.Num15yTargets > 1)
        {
            if (
                strategy.NumFrozenTargets == strategy.Num15yTargets
                && CanCast(state, strategy, AID.Ultravibration)
                && state.CD(CDGroup.Level5Death) <= state.GCD
            )
                return AID.Ultravibration;

            if (strategy.Num6yTargets == strategy.Num15yTargets && CanCast(state, strategy, AID.TheRamsVoice))
                return AID.TheRamsVoice;

            if (CanCast(state, strategy, AID.HydroPull))
                return AID.HydroPull;
        }

        if (
            CanCast(state, strategy, AID.MatraMagic)
            && state.CD(CDGroup.AngelsSnack) <= state.GCD
            && state.Mimic == Mimic.DPS
        )
            return AID.MatraMagic;

        if (CanCast(state, strategy, AID.TheRoseOfDestruction) && state.CD(CDGroup.TheRoseOfDestruction) <= state.GCD)
            return AID.TheRoseOfDestruction;

        if (state.OnSlot(AID.TripleTrident) && state.RangeToTarget <= 3)
        {
            if (
                CanCast(state, strategy, AID.Whistle)
                && state.CD(CDGroup.TripleTrident) <= state.GCD + state.SpellGCDTime
                && state.HarmonizedLeft <= state.GCD + state.SpellGCDTime
            )
                return AID.Whistle;

            if (state.CD(CDGroup.TripleTrident) <= state.GCD && CanCast(state, strategy, AID.TripleTrident))
                return AID.TripleTrident;
        }

        if (CanCast(state, strategy, AID.SonicBoom))
            return AID.SonicBoom;

        if (state.TargetingBoss && CanCast(state, strategy, AID.Missile))
            return AID.Missile;

        if (state.Chocobo != null && CanCast(state, strategy, AID.ChocoMeteor))
            return AID.ChocoMeteor;

        return AID.None;
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
    {
        if (state.CurMP <= 7000 && state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
            return ActionID.MakeSpell(AID.LucidDreaming);

        if (
            state.OnSlot(AID.BeingMortal)
            && state.CanWeave(CDGroup.Apokalypsis, 0.6f, deadline)
            && strategy.Num10yTargets > 0
        )
            return ActionID.MakeSpell(AID.BeingMortal);

        if (
            state.OnSlot(AID.Nightbloom)
            && state.CanWeave(CDGroup.Nightbloom, 0.6f, deadline)
            && strategy.Num10yTargets > 0
        )
            return ActionID.MakeSpell(AID.Nightbloom);

        if (state.OnSlot(AID.ShockStrike) && state.CanWeave(CDGroup.ShockStrike, 0.6f, deadline))
            return ActionID.MakeSpell(AID.ShockStrike);

        if (state.OnSlot(AID.FeatherRain) && state.CanWeave(CDGroup.FeatherRain, 0.6f, deadline))
            return ActionID.MakeSpell(AID.FeatherRain);

        if (state.OnSlot(AID.JKick) && state.CanWeave(CDGroup.Quasar, 0.6f, deadline))
            return ActionID.MakeSpell(AID.JKick);

        return new();
    }
}
