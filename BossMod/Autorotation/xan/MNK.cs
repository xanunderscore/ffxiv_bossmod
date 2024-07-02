using BossMod.Autorotation.Legacy;
using BossMod.MNK;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class MNK : RotationModule
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("MNK", "Monk", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.MNK | (int)Class.PGL), 100);
    }

    public class State(RotationModule module) : CommonState(module)
    {
        public int Chakra; // 0-5 (0-10 during Brotherhood)
        public BeastChakra[] BeastChakra = [];
        public Nadi Nadi;
        public float BlitzLeft; // 20 max
        public float FormLeft; // 0 if no form, 30 max
        public float PerfectBalanceLeft; // 20 max
        public float FormShiftLeft; // 30 max
        public float FireLeft; // 20 max
        public float TrueNorthLeft; // 10 max
        public float FiresReplyLeft; // 20 max
        public float WindsReplyLeft; // 15 max

        public int NumBlitzTargets; // 5y around self OR 5y around target (phantom rush/tornado kick only)
        public int NumPointBlankAOETargets; // 5y around self
        public int NumLineAOETargets; // 10y/4y rect, Enlightenment, Wind's Reply
        public int NumFireTargets; // 5y around target, Fire's Reply

        public bool HasLunar => Nadi.HasFlag(Nadi.LUNAR);
        public bool HasSolar => Nadi.HasFlag(Nadi.SOLAR);
        public bool HasBothNadi => HasLunar && HasSolar;

        public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

        public bool Unlocked(AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(TraitID tid) => Module.TraitUnlocked((uint)tid);
    }

    private readonly State _state;

    public MNK(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        _state.UpdateCommon(primaryTarget);

        var gauge = Service.JobGauges.Get<MNKGauge>();
        _state.Chakra = gauge.Chakra;
        _state.BeastChakra = gauge.BeastChakra;
        _state.Nadi = gauge.Nadi;
        _state.BlitzLeft = gauge.BlitzTimeRemaining / 1000f;

        _state.PerfectBalanceLeft = _state.StatusDetails(Player, SID.PerfectBalance, Player.InstanceID).Left;
        _state.FormShiftLeft = _state.StatusDetails(Player, SID.FormlessFist, Player.InstanceID).Left;
        _state.FireLeft = _state.StatusDetails(Player, SID.RiddleOfFire, Player.InstanceID).Left;
        _state.TrueNorthLeft = _state.StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
    }
}
