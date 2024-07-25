using BossMod.SMN;
using System.Runtime.InteropServices;

namespace BossMod.Autorotation.xan;

public sealed class SMN(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("SMN", "Summoner", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.SMN, Class.ACN), 100);

        def.DefineShared().AddAssociatedActions(AID.SearingLight);

        return def;
    }

    public Trance Trance;
    public Arcanum Arcanum;
    public Favor Favor;
    public AttunementType AttunementType;
    public int Attunement;
    public float SummonLeft;
    public float FurtherRuin;
    public int Aetherflow => Trance.HasFlag(Trance.Aetherflow2) ? 2 : Trance.HasFlag(Trance.Aetherflow) ? 1 : 0;

    public int NumAOETargets;
    public int NumMeleeTargets;

    private Actor? Carbuncle;
    private Actor? BestAOETarget;
    private Actor? BestMeleeTarget;

    public bool FirebirdTrance => SummonLeft > 0 && Trance.HasFlag(Trance.Phoenix) && AttunementType == AttunementType.None;
    public bool DreadwyrmTrance => SummonLeft > 0 && !Trance.HasFlag(Trance.Phoenix) && AttunementType == AttunementType.None && Unlocked(AID.DreadwyrmTrance);

    protected override float GetCastTime(AID aid)
    {
        if (aid is AID.Gemshine or AID.PreciousBrilliance)
            return AttunementType == AttunementType.Ruby ? base.GetCastTime(AID.RubyRuin1) : 0;

        if (aid == AID.AstralFlow)
            return AttunementType == AttunementType.Emerald ? base.GetCastTime(AID.Slipstream) : 0;

        // bahamut or phoenix is summoned, main GCD is instant
        if (aid == AID.Ruin1 && SummonLeft > 0 && AttunementType == AttunementType.None && Unlocked(AID.DreadwyrmTrance))
            return 0;

        return base.GetCastTime(aid);
    }

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Carbuncle == null)
            PushGCD(AID.SummonCarbuncle, Player);

        if (Favor == Favor.Garuda)
            PushGCD(AID.Slipstream, BestAOETarget);

        if (AttunementType != AttunementType.None)
        {
            if (Unlocked(AID.PreciousBrilliance) && NumAOETargets > 2)
                PushGCD(AID.PreciousBrilliance, BestAOETarget);

            PushGCD(AID.Gemshine, primaryTarget);
        }

        if (ComboLastMove == AID.CrimsonCyclone)
            PushGCD(AID.CrimsonStrike, BestMeleeTarget);

        if (Favor == Favor.Ifrit && ForceMovementIn > 5) // idk what to put here
            PushGCD(AID.CrimsonCyclone, BestAOETarget);

        if (SummonLeft <= _state.GCD)
        {
            if (Arcanum.HasFlag(Arcanum.Topaz))
                PushGCD(AID.SummonTopaz, primaryTarget);

            if (Arcanum.HasFlag(Arcanum.Emerald))
                PushGCD(AID.SummonEmerald, primaryTarget);

            if (Arcanum.HasFlag(Arcanum.Ruby))
                PushGCD(AID.SummonRuby, primaryTarget);

            if (_state.CD(AID.Aethercharge) < _state.GCD)
            {
                var isTargeted = Unlocked(TraitID.AetherchargeMastery);
                // scarlet flame and wyrmwave are both single target, this is ok
                PushGCD(AID.Aethercharge, isTargeted ? primaryTarget : Player);
            }
        }

        if (FurtherRuin > _state.GCD && SummonLeft == 0)
            PushGCD(AID.Ruin4, BestAOETarget);

        if (Unlocked(AID.Outburst) && NumAOETargets > 2)
            PushGCD(AID.Outburst, BestAOETarget);

        PushGCD(AID.Ruin1, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (strategy.BuffsOk() && Unlocked(AID.SearingLight) && _state.CanWeave(AID.SearingLight, 0.6f, deadline))
            PushOGCD(AID.SearingLight, Player);

        if (Favor == Favor.Titan && _state.CanWeave(AID.MountainBuster, 0.6f, deadline))
            PushOGCD(AID.MountainBuster, BestAOETarget);

        if (DreadwyrmTrance)
        {
            if (Unlocked(AID.EnkindleBahamut) && _state.CanWeave(AID.EnkindleBahamut, 0.6f, deadline))
                PushOGCD(AID.EnkindleBahamut, BestAOETarget);

            if (_state.CanWeave(AID.Deathflare, 0.6f, deadline))
                PushOGCD(AID.Deathflare, BestAOETarget);
        }

        if (FirebirdTrance && _state.CanWeave(AID.EnkindlePhoenix, 0.6f, deadline))
            PushOGCD(AID.EnkindlePhoenix, BestAOETarget);

        if (Aetherflow > 0)
        {
            if (Unlocked(AID.Painflare) && NumAOETargets > 2)
            {
                if (_state.CanWeave(AID.Painflare, 0.6f, deadline))
                    PushOGCD(AID.Painflare, BestAOETarget);
            }
            else if (_state.CanWeave(AID.Fester, 0.6f, deadline))
                PushOGCD(AID.Fester, primaryTarget);
        }

        if (_state.CanWeave(AID.EnergyDrain, 0.6f, deadline))
        {
            if (Unlocked(AID.EnergySiphon) && NumAOETargets > 2)
                PushOGCD(AID.EnergySiphon, BestAOETarget);

            PushOGCD(AID.EnergyDrain, primaryTarget);
        }

        if (MP <= 7000 && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    public override unsafe void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<SmnGauge>();
        Trance = (Trance)(gauge.SummonFlags & 7);
        Arcanum = (Arcanum)(gauge.SummonFlags >> 4);
        SummonLeft = gauge.SummonTimer * 0.001f;
        AttunementType = (AttunementType)(gauge.AttunementFlags & 3);
        Attunement = gauge.AttunementFlags >> 2;

        Carbuncle = World.Actors.FirstOrDefault(x => x.Type == ActorType.Pet && x.OwnerID == Player.InstanceID);

        var favor = Player.Statuses.FirstOrDefault(x => (SID)x.ID is SID.GarudasFavor or SID.IfritsFavor or SID.TitansFavor);

        Favor = (SID)favor.ID switch
        {
            SID.GarudasFavor => Favor.Garuda,
            SID.IfritsFavor => Favor.Ifrit,
            SID.TitansFavor => Favor.Titan,
            _ => Favor.None
        };
        FurtherRuin = StatusLeft(SID.FurtherRuin);

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);
        (BestMeleeTarget, NumMeleeTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
internal unsafe struct SmnGauge
{
    [FieldOffset(0x8)] public ushort SummonTimer;
    [FieldOffset(0xE)] public byte AttunementFlags;
    [FieldOffset(0xF)] public byte SummonFlags;
}

[Flags]
public enum Trance
{
    Bahamut = 0,
    Aetherflow = 1,
    Aetherflow2 = 2,
    Phoenix = 4
}

[Flags]
public enum Arcanum
{
    None = 0,
    Ruby = 2,
    Topaz = 4,
    Emerald = 8
}

public enum AttunementType
{
    None = 0,
    Ruby = 1,
    Topaz = 2,
    Emerald = 3
}

public enum Favor
{
    None,
    Ifrit,
    Titan,
    Garuda
}
