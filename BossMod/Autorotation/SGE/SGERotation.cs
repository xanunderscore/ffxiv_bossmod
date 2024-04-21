using System.Linq;
using static BossMod.CommonActions;

namespace BossMod.SGE;

public static class Rotation
{
    // full state needed for determining next action
    public class State : CommonRotation.PlayerState
    {
        public int Gall; // 3 max
        public float NextGall; // 20 max
        public int Sting; // 3 max
        public bool Eukrasia;
        public float ZoeLeft; // max 30

        public float SwiftcastLeft; // 0 if buff not up, max 10
        public float TargetDotLeft; // i am not typing that shit out. max 30

        public AID BestDosis =>
            Eukrasia
                ? FindUnlocked(AID.EukrasianDosisIII, AID.EukrasianDosisII, AID.EukrasianDosis)
                : FindUnlocked(AID.DosisIII, AID.DosisII, AID.Dosis);
        public AID BestPhlegma => FindUnlocked(AID.PhlegmaIII, AID.PhlegmaII, AID.Phlegma);
        public AID BestDyskrasia => FindUnlocked(AID.DyskrasiaII, AID.Dyskrasia);
        public AID BestToxikon => FindUnlocked(AID.ToxikonII, AID.Toxikon);

        public SID ExpectedEudosis =>
            Unlocked(AID.EukrasianDosisIII)
                ? SID.EukrasianDosisIII
                : Unlocked(AID.EukrasianDosisII)
                    ? SID.EukrasianDosisII
                    : SID.EukrasianDosis;

        public CDGroup PhlegmaCD =>
            Unlocked(AID.PhlegmaIII)
                ? CDGroup.PhlegmaIII
                : Unlocked(AID.PhlegmaII)
                    ? CDGroup.PhlegmaII
                    : CDGroup.Phlegma;

        public State(WorldState ws)
            : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        private AID FindUnlocked(params AID[] actions)
        {
            var act = actions.FirstOrDefault(Unlocked);
            return act == AID.None ? actions.Last() : act;
        }

        public override string ToString()
        {
            return $"E={Eukrasia}, S={Sting}, G={Gall}/{NextGall}, RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumDyskrasiaTargets; // 5y around self
        public int NumToxikonTargets; // 5y around target
        public int NumPhlegmaTargets; // 5y around target
        public int NumPneumaTargets; // 25y/4y rect

        public int NumNearbyUnshieldedAllies; // up to 8 including self

        public enum GCDShieldStrategy : uint
        {
            [PropertyDisplay("Manual shielding only")]
            Manual = 0,

            [PropertyDisplay("EukProg")]
            Prog = 1,

            [PropertyDisplay("Zoe + EukProg")]
            ProgZoe = 2,
        }

        public GCDShieldStrategy GCDShieldUse;

        public OffensiveAbilityUse PneumaUse;

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 2)
            {
                GCDShieldUse = (GCDShieldStrategy)overrides[0];
                PneumaUse = (OffensiveAbilityUse)overrides[1];
            }
            else
            {
                GCDShieldUse = GCDShieldStrategy.Manual;
                PneumaUse = OffensiveAbilityUse.Automatic;
            }
        }

        public override string ToString()
        {
            return $"AOE={NumDyskrasiaTargets}/{NumToxikonTargets}, no-dots={ForbidDOTs}, movement-in={ForceMovementIn:f3}";
        }
    }

    public static bool CanCast(State state, Strategy strategy, float castTime) =>
        state.SwiftcastLeft > state.GCD || strategy.ForceMovementIn >= state.GCD + castTime;

    public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        if (strategy.NumNearbyUnshieldedAllies > 0)
        {
            switch (strategy.GCDShieldUse)
            {
                case Strategy.GCDShieldStrategy.Prog:
                    return state.Eukrasia ? AID.EukrasianPrognosis : AID.Eukrasia;
                case Strategy.GCDShieldStrategy.ProgZoe:
                    if (state.ZoeLeft > state.GCD)
                        return state.Eukrasia ? AID.EukrasianPrognosis : AID.Eukrasia;
                    break;
                case Strategy.GCDShieldStrategy.Manual:
                    break;
            }
        }

        if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0)
            return AID.None;

        var canCast = CanCast(state, strategy, 1.5f);

        // planned pneuma
        if (
            strategy.PneumaUse == CommonRotation.Strategy.OffensiveAbilityUse.Force
            && state.CD(CDGroup.Pneuma) <= state.GCD
            && state.TargetingEnemy
        )
        {
            if (canCast)
                return AID.Pneuma;
            else if (state.CD(CDGroup.Swiftcast) == 0)
                return AID.Swiftcast;
        }

        // dot refresh - this is instant cast so no check needed
        if (RefreshDOT(state, state.TargetDotLeft) && state.Unlocked(AID.Eukrasia))
            return state.Eukrasia ? state.BestDosis : AID.Eukrasia;

        // phlegma in raid buff window
        if (
            state.RangeToTarget <= 6
            && state.CD(state.PhlegmaCD) <= 40
            && ShouldUseBurst(state, strategy)
            && strategy.NumPhlegmaTargets > 0
        )
            return state.BestPhlegma;

        // dyskrasia is a gain on 2
        if (strategy.NumDyskrasiaTargets > 1 && state.Unlocked(state.BestDyskrasia))
            return state.BestDyskrasia;

        if (canCast)
        {
            // aoe pneuma for big damage
            if (
                strategy.NumPneumaTargets >= 3
                && state.Unlocked(AID.Pneuma)
                && state.CD(CDGroup.Pneuma) <= state.GCD
                && strategy.PneumaUse != CommonRotation.Strategy.OffensiveAbilityUse.Delay
            )
                return AID.Pneuma;

            if (state.TargetingEnemy)
                return state.BestDosis;
        } else {
            if (state.Unlocked(AID.Toxikon) && state.Sting > 0 && strategy.NumToxikonTargets > 0)
                return state.BestToxikon;

            if (strategy.NumDyskrasiaTargets > 0 && state.Unlocked(state.BestDyskrasia))
                return state.BestDyskrasia;
        }

        if (!state.TargetingEnemy && state.Unlocked(AID.Eukrasia))
            return state.Eukrasia ? AID.None : AID.Eukrasia;

        return AID.None;
    }

    private static bool ShouldUseBurst(State state, Strategy strategy)
    {
        return state.RaidBuffsLeft > state.GCD || strategy.RaidBuffsIn > 9000;
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
    {
        if (
            strategy.GCDShieldUse == Strategy.GCDShieldStrategy.ProgZoe
            && strategy.NumNearbyUnshieldedAllies > 0
            && state.CanWeave(CDGroup.Zoe, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Zoe);

        if (
            state.CurMP <= 7000
            && state.Unlocked(AID.LucidDreaming)
            && state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.LucidDreaming);

        if (
            state.Unlocked(AID.Rhizomata)
            && state.Gall < 2
            && state.NextGall > 10
            && state.CanWeave(CDGroup.Rhizomata, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Rhizomata);

        return new();
    }
}
