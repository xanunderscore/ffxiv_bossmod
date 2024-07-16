using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class SAM(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, Higanbana }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("SAM", "Samurai", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.SAM), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.Ikishoten, AID.HissatsuSenei);

        def.Define(Track.Higanbana).As<OffensiveStrategy>("Higanbana")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Keep Higanbana uptime against 1 or 2 targets")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Do not apply Higanbana")
            .AddOption(OffensiveStrategy.Force, "Force", "Always apply Higanbana to target");

        return def;
    }

    public KaeshiAction Kaeshi;
    public byte Kenki;
    public byte Meditation;
    public SenFlags Sen;

    public float FugetsuLeft; // damage buff, max 40s
    public float FukaLeft; // haste buff, max 40s
    public float MeikyoLeft; // max 20s
    public float OgiLeft; // max 30s
    public float TsubameLeft; // max 30s

    public int NumAOETargets;
    public int NumTenkaTargets; // 8y circle instead of 5
    public int NumLineTargets; // shoha+guren
    public int NumOgiTargets; // 8y/120deg cone

    private Actor? BestAOETarget; // null if fuko is unlocked since it's self-targeted
    private Actor? BestLineTarget;
    private Actor? BestOgiTarget;

    private float TargetDotLeft;

    // TODO multitarget
    //private float LowestTargetDotLeft = float.MaxValue;
    //private Actor? BestDotTarget; // null right now, idk how to do this

    protected override float GetCastTime(AID aid)
        => base.GetCastTime(aid) == 0
            ? 0
            : Unlocked(TraitID.EnhancedIaijutsu) ? 1.3f : 1.5f;

    private int NumStickers => (Ice ? 1 : 0) + (Moon ? 1 : 0) + (Flower ? 1 : 0);

    private bool Ice => Sen.HasFlag(SenFlags.Setsu);
    private bool Moon => Sen.HasFlag(SenFlags.Getsu);
    private bool Flower => Sen.HasFlag(SenFlags.Ka);

    // ensure that buffs will cover iaijutsu
    private bool HaveBuffs => FukaLeft > _state.GCD + 1.3f && FugetsuLeft > _state.GCD + 1.3f;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (Unlocked(AID.MeikyoShisui) && MeikyoLeft == 0 && _state.CountdownRemaining < 14)
                PushGCD(AID.MeikyoShisui, Player);

            if (Unlocked(AID.TrueNorth) && TrueNorthLeft == 0 && Hints.PotentialTargets.Any(x => !x.Actor.Omnidirectional) && _state.CountdownRemaining < 5)
                PushGCD(AID.TrueNorth, Player);

            return;
        }

        EmergencyMeikyo(strategy);
        UseKaeshi(primaryTarget);
        UseIaijutsu(primaryTarget);

        if (OgiLeft > _state.GCD && TargetDotLeft > 30)
            PushGCD(AID.OgiNamikiri, BestOgiTarget);

        if (MeikyoLeft > _state.GCD)
        {
            if (!Moon)
                PushGCD(AID.Gekko, primaryTarget);
            if (!Flower)
                PushGCD(AID.Kasha, primaryTarget);
            if (!Ice)
                PushGCD(AID.Yukikaze, primaryTarget);
        }

        if (ComboLastMove == AID.Jinpu && Unlocked(AID.Gekko))
            PushGCD(AID.Gekko, primaryTarget);
        if (ComboLastMove == AID.Shifu && Unlocked(AID.Kasha))
            PushGCD(AID.Kasha, primaryTarget);

        if (ComboLastMove == AID.Hakaze)
            UseHakazeComboAction(strategy, primaryTarget);

        PushGCD(AID.Hakaze, primaryTarget);
    }

    private void UseHakazeComboAction(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Unlocked(AID.Jinpu) && FugetsuLeft < _state.AttackGCDTime * 2)
            PushGCD(AID.Jinpu, primaryTarget);

        if (Unlocked(AID.Shifu) && FukaLeft < _state.AttackGCDTime * 2)
            PushGCD(AID.Shifu, primaryTarget);

        // if (NumStickers == 0 && GCDSUntilNextTsubame is 19 or 21)
        //     PushGCD(AID.Yukikaze, primaryTarget);

        if (Unlocked(AID.OgiNamikiri) && !Ice)
            PushGCD(AID.Yukikaze, primaryTarget);

        if (Unlocked(AID.Shifu) && !Flower && FugetsuLeft >= FukaLeft)
            PushGCD(AID.Shifu, primaryTarget);

        if (Unlocked(AID.Jinpu) && !Moon)
            PushGCD(AID.Jinpu, primaryTarget);

        if (Unlocked(AID.Yukikaze) && !Ice)
            PushGCD(AID.Yukikaze, primaryTarget);
    }

    private void UseKaeshi(Actor? primaryTarget)
    {
        switch (Kaeshi)
        {
            case KaeshiAction.Setsugekka:
                PushGCD(AID.KaeshiSetsugekka, primaryTarget);
                break;
            case KaeshiAction.Goken:
                PushGCD(AID.KaeshiGoken, Player);
                break;
            case KaeshiAction.Namikiri:
                PushGCD(AID.KaeshiNamikiri, BestOgiTarget);
                break;
        }
    }

    private void UseIaijutsu(Actor? primaryTarget)
    {
        if (!HaveBuffs)
            return;

        if (NumStickers == 1 && TargetDotLeft < 10)
            PushGCD(AID.Higanbana, primaryTarget);

        if (NumStickers == 2 && NumTenkaTargets > 2)
            PushGCD(AID.TenkaGoken, Player);

        if (NumStickers == 3)
            PushGCD(AID.MidareSetsugekka, primaryTarget);
    }

    private void EmergencyMeikyo(StrategyValues strategy)
    {
        // special case for if we got thrust into combat with no prep
        if (NumStickers == 0 && Unlocked(AID.MeikyoShisui) && MeikyoLeft == 0 && !HaveBuffs && CombatTimer < 5 && _state.CD(AID.MeikyoShisui) < 55)
            PushGCD(AID.MeikyoShisui, Player);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        var buffOk = strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

        if (buffOk)
        {
            if (Unlocked(AID.Ikishoten) && _state.CanWeave(AID.Ikishoten, 0.6f, deadline))
                PushOGCD(AID.Ikishoten, Player);

            if (Unlocked(AID.HissatsuGuren) && _state.CanWeave(AID.HissatsuGuren, 0.6f, deadline) && Kenki >= 25)
            {
                if (Unlocked(AID.HissatsuSenei) && NumLineTargets < 2)
                    PushOGCD(AID.HissatsuSenei, primaryTarget);

                PushOGCD(AID.HissatsuGuren, BestLineTarget);
            }
        }
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 3);
        _state.UpdateCommon(primaryTarget);

        var gauge = GetGauge<SamuraiGauge>();
        Kaeshi = gauge.Kaeshi;
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;

        FugetsuLeft = StatusLeft(SID.Fugetsu);
        FukaLeft = StatusLeft(SID.Fuka);
        MeikyoLeft = StatusLeft(SID.MeikyoShisui);
        OgiLeft = StatusLeft(SID.OgiNamikiriReady);
        TsubameLeft = StatusLeft(SID.TsubameGaeshiReady);

        (BestOgiTarget, NumOgiTargets) = SelectTarget(targeting, primaryTarget, 8, InConeAOE);

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.AOE)
        {
            if (Unlocked(AID.Fuko))
                (BestAOETarget, NumAOETargets) = (null, NumMeleeAOETargets());
            else
                (BestAOETarget, NumAOETargets) = (BestOgiTarget, NumOgiTargets);

            NumTenkaTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
            (BestLineTarget, NumLineTargets) = SelectTarget(targeting, primaryTarget, 10, InLineAOE);
        }
        else
        {
            NumTenkaTargets = 0;
            (BestAOETarget, NumAOETargets) = (null, 0);
            (BestLineTarget, NumLineTargets) = (primaryTarget, Player.DistanceToHitbox(primaryTarget) <= 10 ? 1 : 0);
        }

        TargetDotLeft = strategy.Option(Track.Higanbana).As<OffensiveStrategy>() switch
        {
            OffensiveStrategy.Automatic => HiganbanaLeft(primaryTarget),
            OffensiveStrategy.Delay => float.MaxValue,
            OffensiveStrategy.Force => 0,
            _ => throw new NotImplementedException("sigh")
        };

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private float HiganbanaLeft(Actor? p) => p == null ? float.MaxValue : _state.StatusDetails(p, SID.Higanbana, Player.InstanceID).Left;

    private bool InConeAOE(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees());
    private bool InLineAOE(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 4);
}
