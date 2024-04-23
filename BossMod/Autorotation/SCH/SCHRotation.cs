namespace BossMod.SCH;

public static class Rotation
{
    // full state needed for determining next action
    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
    {
        public Actor? Fairy;
        public int AetherflowStacks; // 3 max
        public float SwiftcastLeft; // 0 if buff not up, max 10
        public float TargetBioLeft; // 0 if debuff not up, max 30
        public float DissipationLeft; // 30 max

        // upgrade paths
        public AID BestBroil => Unlocked(AID.Broil4) ? AID.Broil4 : Unlocked(AID.Broil3) ? AID.Broil3 : Unlocked(AID.Broil2) ? AID.Broil2 : Unlocked(AID.Broil1) ? AID.Broil1 : AID.Ruin1;
        public AID BestBio => Unlocked(AID.Biolysis) ? AID.Biolysis : Unlocked(AID.Bio2) ? AID.Bio2 : AID.Bio1;
        public AID BestArtOfWar => Unlocked(AID.ArtOfWar2) ? AID.ArtOfWar2 : AID.ArtOfWar1;

        // statuses
        public SID ExpectedBio => Unlocked(AID.Biolysis) ? SID.Biolysis : Unlocked(AID.Bio2) ? SID.Bio2 : SID.Bio1;

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"AF={AetherflowStacks}, RB={RaidBuffsLeft:f1}, Bio={TargetBioLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumWhisperingDawnTargets; // how many targets would whispering dawn heal (15y around fairy)
        public int NumSuccorTargets; // how many targets would succor heal (15y around self)
        public int NumArtOfWarTargets; // how many targets art of war would hit (5y around self)
        public (Actor? Target, float HPRatio) BestSTHeal;

        public override string ToString()
        {
            return $"AOE={NumArtOfWarTargets}, SH={BestSTHeal.Target?.Name[..4]}={BestSTHeal.HPRatio:f2}, AH={NumSuccorTargets}/{NumWhisperingDawnTargets}, no-dots={ForbidDOTs}, movement-in={ForceMovementIn:f3}";
        }
    }

    public static bool CanCast(State state, Strategy strategy, float castTime) => state.SwiftcastLeft > state.GCD || strategy.ForceMovementIn >= state.GCD + castTime;
    public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

    public static AID GetNextBestSTHealGCD(State state, Strategy strategy)
    {
        return state.Unlocked(AID.Adloquium) && state.CurMP >= 1000 ? AID.Adloquium : AID.Physick;
    }

    public static AID GetNextBestDamageGCD(State state, Strategy strategy)
    {
        if (strategy.NumArtOfWarTargets >= 3)
            return state.BestArtOfWar;

        if (!strategy.ForbidDOTs && RefreshDOT(state, state.TargetBioLeft))
            return state.BestBio;

        // yes, art of war is a gain on 1 until broil is unlocked at level 54
        var minAoeTargets = state.Unlocked(AID.Broil1) ? 2 : 1;

        if (strategy.NumArtOfWarTargets >= minAoeTargets)
            return state.BestArtOfWar;

        // TODO: priorities change at L54, L64, L72, L82
        bool allowRuin = CanCast(state, strategy, 1.5f);

        if (allowRuin)
            return state.BestBroil;

        return AID.Ruin2;
    }
}
