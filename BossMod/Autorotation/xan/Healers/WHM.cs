using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
namespace BossMod.Autorotation.xan;
public sealed class WHM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public enum Track { Raise = SharedTrack.Count, Assize, Misery }
    public enum RaiseStrategy { None, Fast, Slow }
    public enum AssizeStrategy { HitAny, HitAll, Heal }
    public enum MiseryStrategy { ASAP, BuffedOnly, Delay }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan WHM", "White Mage", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.WHM), 100);

        def.DefineShared().AddAssociatedActions(AID.PresenceOfMind);
        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Off", "Do not automatically raise")
            .AddOption(RaiseStrategy.Fast, "Fast", "Raise if Swiftcast is available")
            .AddOption(RaiseStrategy.Slow, "Slow", "Always raise, hardcast if necessary");
        def.Define(Track.Assize).As<AssizeStrategy>("Assize")
            .AddOption(AssizeStrategy.HitAny, "HitAny", "Use if it would hit any priority target")
            .AddOption(AssizeStrategy.HitAll, "HitAll", "Use if it would hit all priority targets")
            .AddOption(AssizeStrategy.Heal, "Heal", "Use to heal teammates");
        def.Define(Track.Misery).As<MiseryStrategy>("Afflatus Misery")
            .AddOption(MiseryStrategy.ASAP, "ASAP", "Use on best target at 3 Blood Lilies")
            .AddOption(MiseryStrategy.BuffedOnly, "Buffs", "Use during raid buffs")
            .AddOption(MiseryStrategy.Delay, "Delay", "Do not use");

        return def;
    }

    public uint Lily;
    public uint BloodLily;
    public float NextLily;

    public float TargetDotLeft;

    public int NumHolyTargets;
    public int NumAssizeTargets;
    public int NumMiseryTargets;

    private Actor? BestMiseryTarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = GetGauge<WhiteMageGauge>();

        NextLily = 20f - gauge.LilyTimer * 0.001f;
        Lily = gauge.Lily;
        BloodLily = gauge.BloodLily;

        NumHolyTargets = NumNearbyTargets(strategy, 8);
        NumAssizeTargets = NumNearbyTargets(strategy, 15);
        (BestMiseryTarget, NumMiseryTargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        TargetDotLeft = DotLeft(primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 1.7)
                PushGCD(AID.Stone1, primaryTarget);

            return;
        }

        if (!CanFitGCD(TargetDotLeft, 1) && NumHolyTargets <= 2)
            PushGCD(AID.Aero1, primaryTarget);

        if (BloodLily == 3 && NumMiseryTargets > 0)
        {
            switch (strategy.Option(Track.Misery).As<MiseryStrategy>())
            {
                case MiseryStrategy.ASAP:
                    PushGCD(AID.AfflatusMisery, BestMiseryTarget);
                    break;
                case MiseryStrategy.BuffedOnly:
                    if (RaidBuffsLeft > GCD)
                        PushGCD(AID.AfflatusMisery, BestMiseryTarget);
                    break;
            }
        }

        if (NumHolyTargets > 2)
            PushGCD(AID.Holy1, Player);

        // TODO make a track for this
        if (Lily == 3 || !CanFitGCD(NextLily, 1) && Lily == 2)
        {
            var healTarget = World.Party.WithoutSlot().MinBy(x => x.HPMP.CurHP / x.HPMP.MaxHP);
            PushGCD(AID.AfflatusSolace, healTarget);
        }

        PushGCD(AID.Stone1, primaryTarget);

        if (RaidBuffsLeft >= 15 || RaidBuffsIn > 9000)
            PushOGCD(AID.PresenceOfMind, Player);

        if (CD(AID.Assize) < GCD)
        {
            switch (strategy.Option(Track.Assize).As<AssizeStrategy>())
            {
                case AssizeStrategy.HitAll:
                    if (NumAssizeTargets == Hints.PriorityTargets.Count())
                        PushOGCD(AID.Assize, Player);
                    break;
                case AssizeStrategy.HitAny:
                    if (NumAssizeTargets > 0)
                        PushOGCD(AID.Assize, Player);
                    break;
                case AssizeStrategy.Heal:
                    // implement me!
                    break;
            }
        }

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);
    }

    static readonly SID[] DotStatus = [SID.Aero1, SID.Aero2, SID.Dia];

    private float DotLeft(Actor? x)
    {
        if (x == null)
            return 0;

        foreach (var stat in DotStatus)
        {
            var dur = StatusDetails(x, (uint)stat, Player.InstanceID).Left;
            if (dur > 0)
                return dur;
        }

        return 0;
    }
}
