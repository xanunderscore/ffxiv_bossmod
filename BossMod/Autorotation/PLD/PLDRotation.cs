namespace BossMod.PLD;

// note: correct up to ~L30
public static class Rotation
{
    // full state needed for determining next action
    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
    {
        public float FightOrFlightLeft; // 0 if buff not up, max 25
        public float DivineMightLeft; // max 30
        public AID ConfiteorCombo;
        public (float Left, int Stacks) Requiescat; // max 30/4 stacks
        public (float Left, int Stacks) SwordOath; // max 30/3 stacks
        public int OathGauge; // 0-100

        public AID ComboLastMove => (AID)ComboLastAction;

        public AID BestRoyalAuthority => Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : AID.RageOfHalone;
        public AID BestExpiacion => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
        public AID BestSheltron => Unlocked(AID.HolySheltron) ? AID.HolySheltron : AID.Sheltron;

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"RB={RaidBuffsLeft:f1}, Combo={ComboLastMove}, FF={FightOrFlightLeft:f1}/{CD(CDGroup.FightOrFlight):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumAOETargets;
    }

    public static AID GetNextRiotBladeComboAction(AID comboLastMove)
    {
        return comboLastMove == AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
    }

    public static AID GetNextSTComboAction(AID comboLastMove, AID finisher)
    {
        return comboLastMove switch
        {
            AID.RiotBlade => finisher,
            AID.FastBlade => AID.RiotBlade,
            _ => AID.FastBlade
        };
    }

    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        if (!state.TargetingEnemy)
            return AID.None;

        if (
            state.Unlocked(AID.GoringBlade)
            && state.CD(CDGroup.GoringBlade) <= state.GCD
            && state.FightOrFlightLeft > state.GCD
            // skip this conditional to allow confiteor/holy spirit if target is out of range
            && state.RangeToTarget <= 3
        )
            return AID.GoringBlade;

        if (state.ConfiteorCombo != AID.None && state.CurMP >= 1000)
            return state.ConfiteorCombo;

        if (state.RangeToTarget > 3 && state.DivineMightLeft > state.GCD)
            return AID.HolySpirit;

        if (state.SwordOath.Left > state.GCD && state.SwordOath.Stacks < 3)
            return AID.Atonement;

        if (strategy.NumAOETargets >= 3 && state.Unlocked(AID.TotalEclipse))
        {
            // TODO change when confiteor is unlocked
            if (state.Requiescat.Left > state.GCD && state.Unlocked(AID.HolyCircle) && state.CurMP >= 1000)
                return AID.HolyCircle;

            if (state.Unlocked(AID.Prominence) && state.ComboLastMove == AID.TotalEclipse)
            {
                if (state.DivineMightLeft > state.GCD && state.Unlocked(AID.HolyCircle) && state.CurMP >= 1000)
                    return AID.HolyCircle;

                return AID.Prominence;
            }

            return AID.TotalEclipse;
        }
        else
        {
            if (
                state.DivineMightLeft > state.GCD
                && state.DivineMightLeft < state.GCD + state.AttackGCDTime
                && state.CurMP >= 1000
            )
                return AID.HolySpirit;

            // TODO change when confiteor is unlocked
            if (state.Requiescat.Left > state.GCD && state.CurMP >= 1000)
                return AID.HolySpirit;

            // use early in FoF window
            if (state.SwordOath.Left > state.GCD && state.FightOrFlightLeft > state.GCD)
                return AID.Atonement;

            if (state.Unlocked(AID.RageOfHalone) && state.ComboLastMove == AID.RiotBlade)
            {
                if (state.DivineMightLeft > state.GCD && state.CurMP >= 1000)
                    return AID.HolySpirit;

                if (state.SwordOath.Left > state.GCD)
                    return AID.Atonement;

                return state.BestRoyalAuthority;
            }

            if (state.Unlocked(AID.RiotBlade) && state.ComboLastMove == AID.FastBlade)
                return AID.RiotBlade;

            return AID.FastBlade;
        }
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
    {
        // 1. potion - TODO

        var aoe = strategy.NumAOETargets >= 3;

        if (
            (state.DivineMightLeft > 0 || state.Requiescat.Left > 0 || state.SwordOath.Left > 0)
            && state.CanWeave(CDGroup.FightOrFlight, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.FightOrFlight);

        if (!state.Unlocked(AID.Requiescat))
        {
            // pre-requiescat unlocked, use during basic GCD combo to buff
            if (
                state.Unlocked(AID.FightOrFlight)
                && state.ComboLastMove == (aoe ? AID.TotalEclipse : AID.FastBlade)
                && state.CanWeave(CDGroup.FightOrFlight, 0.6f, deadline)
                && state.GCD <= 1.0f
            )
                return ActionID.MakeSpell(AID.FightOrFlight);
        }

        if (
            state.FightOrFlightLeft > state.AnimationLock
            && state.Unlocked(AID.Requiescat)
            && state.CanWeave(CDGroup.Requiescat, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Requiescat);

        // 3. spirits within/circle of scorn, delayed until FoF if it's about to be off cooldown (TODO: think more about delay condition...)
        if (
            state.Unlocked(state.BestExpiacion)
            && state.CanWeave(CDGroup.SpiritsWithin, 0.6f, deadline)
            && (state.FightOrFlightLeft > 0 || state.CD(CDGroup.FightOrFlight) > 15)
        )
            return ActionID.MakeSpell(state.BestExpiacion);

        if (
            state.Unlocked(AID.CircleOfScorn)
            && state.CanWeave(CDGroup.CircleOfScorn, 0.6f, deadline)
            && (state.FightOrFlightLeft > 0 || state.CD(CDGroup.FightOrFlight) > 15)
            && strategy.NumAOETargets > 0
        )
            return ActionID.MakeSpell(AID.CircleOfScorn);

        if (
            state.FightOrFlightLeft > 0
            && state.Unlocked(AID.Intervene)
            && state.CanWeave(state.CD(CDGroup.Intervene) - 30, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Intervene);

        if (state.Unlocked(state.BestSheltron) && strategy.CombatTimer > 0 && state.OathGauge >= 95)
            return ActionID.MakeSpell(state.BestSheltron);

        // no suitable oGCDs...
        return new();
    }
}
