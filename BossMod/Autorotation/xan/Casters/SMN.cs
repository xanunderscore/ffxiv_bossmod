using BossMod.SMN;
using System.Runtime.InteropServices;

namespace BossMod.Autorotation.xan;

public sealed class SMN(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { Cyclone = SharedTrack.Count }
    public enum CycloneUse
    {
        Automatic,
        Delay,
        DelayMove,
        SkipMove,
        Skip
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("SMN", "Summoner", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.SMN, Class.ACN), 100);

        def.DefineShared().AddAssociatedActions(AID.SearingLight);

        def.Define(Track.Cyclone).As<CycloneUse>("Cyclone")
            .AddOption(CycloneUse.Automatic, "Auto", "Use when Ifrit is summoned")
            .AddOption(CycloneUse.Delay, "Delay", "Delay automatic use, but do not overwrite Ifrit with any other summon")
            .AddOption(CycloneUse.DelayMove, "DelayMove", "Delay automatic use until player is not holding a movement key - do not overwrite Ifrit with any other summon")
            .AddOption(CycloneUse.SkipMove, "SkipMove", "Skip if a movement key is held, otherwise use")
            .AddOption(CycloneUse.Skip, "Skip", "Do not use at all");

        return def;
    }

    public TranceFlags TranceFlags;
    public GemFlags GemFlags;
    public Favor Favor;
    public AttunementType AttunementType;
    public int Attunement;
    public float SummonLeft;
    public float FurtherRuin;
    public float SearingLightLeft;
    public float SearingFlash;

    public int Aetherflow => TranceFlags.HasFlag(TranceFlags.Aetherflow2) ? 2 : TranceFlags.HasFlag(TranceFlags.Aetherflow) ? 1 : 0;

    public int NumAOETargets;
    public int NumMeleeTargets;

    private Actor? Carbuncle;
    private Actor? BestAOETarget;
    private Actor? BestMeleeTarget;

    public Trance Trance
    {
        get
        {
            if (SummonLeft > 0 && AttunementType == AttunementType.None)
            {
                if (TranceFlags.HasFlag(TranceFlags.Phoenix))
                    return Trance.Phoenix;

                if (Unlocked(AID.DreadwyrmTrance))
                    return Trance.Dreadwyrm;
            }

            return Trance.None;
        }
    }

    private static readonly AID[] Gemshines = [
        AID.RubyRuin1, AID.TopazRuin1, AID.EmeraldRuin1,
        AID.RubyRuin2, AID.TopazRuin2, AID.EmeraldRuin2,
        AID.RubyRuin3, AID.TopazRuin3, AID.EmeraldRuin3,
        AID.RubyRite, AID.TopazRite, AID.EmeraldRite
    ];

    private static readonly AID[] Brilliances = [
        AID.RubyOutburst, AID.TopazOutburst, AID.EmeraldOutburst,
        AID.RubyDisaster, AID.TopazDisaster, AID.EmeraldDisaster,
        AID.RubyCatastrophe, AID.TopazCatastrophe, AID.EmeraldCatastrophe
    ];

    public AID BestGemshine
    {
        get
        {
            var offset = AttunementType switch
            {
                AttunementType.Ruby => 0,
                AttunementType.Topaz => 1,
                AttunementType.Emerald => 2,
                _ => -1
            };
            if (offset < 0)
                return AID.None;

            if (Unlocked(TraitID.RuinMastery1))
                offset += 3;
            if (Unlocked(TraitID.RuinMastery2))
                offset += 3;
            if (Unlocked(TraitID.RuinMastery3))
                offset += 3;

            return Gemshines[offset];
        }
    }

    public AID BestBrilliance
    {
        get
        {
            var offset = AttunementType switch
            {
                AttunementType.Ruby => 0,
                AttunementType.Topaz => 1,
                AttunementType.Emerald => 2,
                _ => -1
            };
            if (offset < 0)
                return AID.None;

            if (Unlocked(TraitID.OutburstMastery1))
                offset += 3;
            if (Unlocked(TraitID.OutburstMastery2))
                offset += 3;

            return Brilliances[offset];
        }
    }

    public AID BestRuin
    {
        get
        {
            if (Trance == Trance.Phoenix)
                return AID.FountainOfFire;

            if (Trance == Trance.Dreadwyrm)
                return AID.AstralImpulse;

            return Unlocked(AID.Ruin3)
                ? AID.Ruin3
                : Unlocked(AID.Ruin2)
                    ? AID.Ruin2
                    : AID.Ruin1;
        }
    }

    public AID BestOutburst
    {
        get
        {
            if (Trance == Trance.Phoenix)
                return AID.BrandOfPurgatory;

            if (Trance == Trance.Dreadwyrm)
                return AID.AstralFlare;

            return Unlocked(AID.TriDisaster) ? AID.TriDisaster : AID.Outburst;
        }
    }

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Carbuncle == null)
            PushGCD(AID.SummonCarbuncle, Player);

        if (primaryTarget == null)
            return;

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 1.5)
                PushGCD(AID.Ruin1, primaryTarget);

            return;
        }

        if (ComboLastMove == AID.CrimsonCyclone)
            PushGCD(AID.CrimsonStrike, BestMeleeTarget);

        if (Favor == Favor.Garuda)
            PushGCD(AID.Slipstream, BestAOETarget);

        if (AttunementType != AttunementType.None)
        {
            if (NumAOETargets > 2)
                PushGCD(BestBrilliance, BestAOETarget);

            PushGCD(BestGemshine, primaryTarget);
        }

        if (Favor == Favor.Ifrit)
        {
            switch (strategy.Option(Track.Cyclone).As<CycloneUse>())
            {
                case CycloneUse.Automatic:
                    PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.Delay: // do nothing, pause rotation
                    return;
                case CycloneUse.DelayMove:
                    if (ForceMovementIn == 0)
                        return;
                    else
                        PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.SkipMove:
                    if (ForceMovementIn > 0)
                        PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.Skip:
                    break;
            }
        }

        if (SummonLeft <= _state.GCD)
        {
            // TODO make this configurable - this will summon baha/phoenix and ignore current gems
            // balance says to default to summons if you don't know whether you will lose a usage or not
            if (_state.CD(AID.Aethercharge) <= _state.GCD)
            {
                var isTargeted = Unlocked(TraitID.AetherchargeMastery);
                // scarlet flame and wyrmwave are both single target, this is ok
                PushGCD(AID.Aethercharge, isTargeted ? primaryTarget : Player);
            }

            if (GemFlags.HasFlag(GemFlags.Topaz))
                PushGCD(AID.SummonTopaz, primaryTarget);

            if (GemFlags.HasFlag(GemFlags.Emerald))
                PushGCD(AID.SummonEmerald, primaryTarget);

            if (GemFlags.HasFlag(GemFlags.Ruby))
                PushGCD(AID.SummonRuby, primaryTarget);
        }

        if (FurtherRuin > _state.GCD && SummonLeft == 0)
            PushGCD(AID.Ruin4, BestAOETarget);

        if (NumAOETargets > 2)
            PushGCD(BestOutburst, BestAOETarget);

        PushGCD(BestRuin, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        // don't overwrite other player's searing light in roulettes lol i guess
        if (strategy.BuffsOk() && SearingLightLeft == 0)
            PushOGCD(AID.SearingLight, Player);

        if (Favor == Favor.Titan)
            PushOGCD(AID.MountainBuster, BestAOETarget);

        if (Trance == Trance.Dreadwyrm)
        {
            PushOGCD(AID.EnkindleBahamut, BestAOETarget);
            PushOGCD(AID.Deathflare, BestAOETarget);
        }

        if (Trance == Trance.Phoenix)
        {
            PushOGCD(AID.EnkindlePhoenix, BestAOETarget);

            if (_state.CanWeave(AID.Rekindle, 0.6f, deadline))
            {
                static float HPRatio(Actor a) => (float)a.HPMP.CurHP / a.HPMP.MaxHP;

                var rekindleTarget = World.Party.WithoutSlot().Where(x => HPRatio(x) < 1).MinBy(HPRatio);
                if (rekindleTarget is Actor a)
                    PushOGCD(AID.Rekindle, a);
            }
        }

        if (Aetherflow > 0)
        {
            // have to separate these because they don't share a cdgroup, meaning you can accidentally do painflare and fester in one window when 2 festers in 2 windows is optimal
            if (Unlocked(AID.Painflare) && NumAOETargets > 2)
                PushOGCD(AID.Painflare, BestAOETarget);
            else
                PushOGCD(AID.Fester, primaryTarget);
        }

        if (NumAOETargets > 2)
            PushOGCD(AID.EnergySiphon, BestAOETarget);

        PushOGCD(AID.EnergyDrain, primaryTarget);

        if (SearingFlash > 0)
            PushOGCD(AID.SearingFlash, BestAOETarget);

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<SmnGauge>();
        TranceFlags = (TranceFlags)(gauge.SummonFlags & 7);
        GemFlags = (GemFlags)(gauge.SummonFlags >> 4);
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
        SearingFlash = StatusLeft(SID.RubysGlimmer);
        SearingLightLeft = _state.StatusDetails(Player, SID.SearingLight, pendingDuration: 20).Left;

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);
        (BestMeleeTarget, NumMeleeTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
internal struct SmnGauge
{
    [FieldOffset(0x8)] public ushort SummonTimer;
    [FieldOffset(0xE)] public byte AttunementFlags;
    [FieldOffset(0xF)] public byte SummonFlags;
}

[Flags]
public enum TranceFlags
{
    Bahamut = 0,
    Aetherflow = 1 << 0,
    Aetherflow2 = 1 << 1,
    Phoenix = 1 << 2
}

[Flags]
public enum GemFlags
{
    None = 0,
    Ruby = 1 << 1,
    Topaz = 1 << 2,
    Emerald = 1 << 3
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

public enum Trance
{
    None,
    Dreadwyrm,
    Phoenix
}
