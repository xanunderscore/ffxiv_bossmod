using System.Linq;

namespace BossMod.BLU
{
    public static class Rotation
    {
        public class State(float[] cooldowns) : CommonRotation.PlayerState(cooldowns)
        {
            public bool TargetMortalFlame;
            public float TargetBoMLeft;

            public AID[] BLUSlots = new AID[24];

            public bool OnSlot(AID act) => BLUSlots.Contains(act);

            public override string ToString()
            {
                var slots = string.Join(", ", BLUSlots.Select(x => x.ToString()));
                return $"actions=[{slots}]";
            }
        }

        public class Strategy : CommonRotation.Strategy { }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (!state.TargetMortalFlame && state.OnSlot(AID.MortalFlame))
                return AID.MortalFlame;

            if (
                state.TargetBoMLeft <= state.GCD
                && state.OnSlot(AID.BreathOfMagic)
                && state.RangeToTarget <= 10
            )
                return AID.BreathOfMagic;

            return AID.SonicBoom;
        }
    }
}
