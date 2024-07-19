using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan.Melee;
public sealed class SAM(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
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
    public float EnhancedEnpi; // max 15s
    public float Zanshin; // max 30s
    public float Tendo; // max 30s

    public int NumAOECircleTargets; // 5y circle around self, but if fuko isn't unlocked, then...
    public int NumAOETargets; // 8y/120deg cone if we don't have fuko
    public int NumTenkaTargets; // 8y circle instead of 5
    public int NumLineTargets; // shoha+guren
    public int NumOgiTargets; // 8y/120deg cone

    public AID AOEStarter => Unlocked(AID.Fuko) ? AID.Fuko : AID.Fuga;
    public AID STStarter => Unlocked(AID.Gyofu) ? AID.Gyofu : AID.Hakaze;

    private Actor? BestAOETarget; // null if fuko is unlocked since it's self-targeted
    private Actor? BestLineTarget;
    private Actor? BestOgiTarget;
    private Actor? IaiTarget;
    private Actor? EnpiTarget;

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

    private bool HaveFugetsu => FugetsuLeft > _state.GCD + 1.3f;

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
        UseKaeshi();
        UseIaijutsu();

        if (OgiLeft > _state.GCD && TargetDotLeft > 10 && HaveFugetsu)
            PushGCD(AID.OgiNamikiri, BestOgiTarget);

        if (MeikyoLeft > _state.GCD)
            PushGCD(GetMeikyoAction(), NumAOETargets > 2 ? Player : primaryTarget);

        if (NumAOETargets > 2 && Unlocked(AID.Fuga))
        {
            if (ComboLastMove == AOEStarter)
            {
                if (Unlocked(AID.Mangetsu) && FugetsuLeft <= FukaLeft)
                    PushGCD(AID.Mangetsu, Player);
                if (Unlocked(AID.Oka) && FukaLeft <= FugetsuLeft)
                    PushGCD(AID.Oka, Player);
            }

            PushGCD(AOEStarter, BestAOETarget);
        }
        else
        {
            if (ComboLastMove == AID.Jinpu && Unlocked(AID.Gekko))
                PushGCD(AID.Gekko, primaryTarget);
            if (ComboLastMove == AID.Shifu && Unlocked(AID.Kasha))
                PushGCD(AID.Kasha, primaryTarget);

            if (ComboLastMove == STStarter)
                PushGCD(GetHakazeComboAction(strategy), primaryTarget);

            PushGCD(AID.Hakaze, primaryTarget);
        }

        if (Unlocked(AID.Enpi) && EnhancedEnpi > _state.GCD)
            PushGCD(AID.Enpi, EnpiTarget);
    }

    private AID GetHakazeComboAction(StrategyValues strategy)
    {
        if (Unlocked(AID.Jinpu) && FugetsuLeft < _state.AttackGCDTime * 2)
            return AID.Jinpu;

        if (Unlocked(AID.Shifu) && FukaLeft < _state.AttackGCDTime * 2)
            return AID.Shifu;

        // TODO fix loop, can't track tsubame anymore
        // if (NumStickers == 0 && GCDSUntilNextTsubame is 19 or 21)
        //     PushGCD(AID.Yukikaze, primaryTarget);

        // TODO use yukikaze if we need to re apply higanbana?

        if (Unlocked(AID.Shifu) && !Flower && FugetsuLeft > FukaLeft)
            return AID.Shifu;

        if (Unlocked(AID.Jinpu) && !Moon)
            return AID.Jinpu;

        if (Unlocked(AID.Yukikaze) && !Ice)
            return AID.Yukikaze;

        // fallback if we are full on sen but can't use midare bc of movement restrictions or w/e
        return Unlocked(AID.Jinpu) ? AID.Jinpu : AID.None;
    }

    private AID GetMeikyoAction()
    {
        if (NumAOETargets > 2)
        {
            // priority 0: damage buff
            if (FugetsuLeft == 0)
                return AID.Mangetsu;

            return (Moon, Flower) switch
            {
                // refresh buff running out first
                (false, false) => FugetsuLeft <= FukaLeft ? AID.Mangetsu : AID.Oka,
                (true, false) => AID.Oka,
                _ => AID.Mangetsu,
            };
        }
        else
        {
            // priority 0: damage buff
            if (FugetsuLeft == 0)
                return AID.Gekko;

            return (Moon, Flower) switch
            {
                // refresh buff running out first
                (false, false) => FugetsuLeft <= FukaLeft ? AID.Gekko : AID.Kasha,
                (false, true) => AID.Gekko,
                (true, false) => AID.Kasha,
                // only use yukikaze to get sen, as it's the weakest ender
                _ => !Ice ? AID.Yukikaze : AID.Gekko,
            };
        }
    }

    private void UseKaeshi()
    {
        switch (Kaeshi)
        {
            case KaeshiAction.Goken:
                PushGCD(AID.KaeshiGoken, Player);
                break;
            case KaeshiAction.Setsugekka:
                PushGCD(AID.KaeshiSetsugekka, IaiTarget);
                break;
            case KaeshiAction.Namikiri:
                PushGCD(AID.KaeshiNamikiri, BestOgiTarget);
                break;
            case (KaeshiAction)5:
                PushGCD(AID.TendoKaeshiGoken, Player);
                break;
            case (KaeshiAction)6:
                PushGCD(AID.TendoKaeshiSetsugekka, IaiTarget);
                break;
        }
    }

    private void UseIaijutsu()
    {
        if (!HaveFugetsu)
            return;

        if (NumStickers == 1 && TargetDotLeft < 10 && FukaLeft > 0)
            PushGCD(AID.Higanbana, IaiTarget);

        if (NumStickers == 2 && NumTenkaTargets > 2)
            PushGCD(Tendo > _state.GCD ? AID.TendoGoken : AID.TenkaGoken, Player);

        if (NumStickers == 3)
            PushGCD(Tendo > _state.GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, IaiTarget);
    }

    private void EmergencyMeikyo(StrategyValues strategy)
    {
        // special case for if we got thrust into combat with no prep
        if (NumStickers == 0 && Unlocked(AID.MeikyoShisui) && MeikyoLeft == 0 && !HaveFugetsu && CombatTimer < 5 && _state.CD(AID.MeikyoShisui) < 55)
            PushGCD(AID.MeikyoShisui, Player);
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (NumAOETargets > 2)
            return (Positional.Any, false);

        if (MeikyoLeft > _state.GCD)
            return GetMeikyoAction() switch
            {
                AID.Gekko => (Positional.Rear, true),
                AID.Kasha => (Positional.Flank, true),
                _ => (Positional.Any, false)
            };

        if (ComboLastMove == AID.Jinpu)
            return (Positional.Rear, true);

        if (ComboLastMove == AID.Shifu)
            return (Positional.Flank, true);

        if (ComboLastMove == AID.Hakaze)
        {
            var pos = GetHakazeComboAction(strategy) switch
            {
                AID.Jinpu => Positional.Rear,
                AID.Shifu => Positional.Flank,
                _ => Positional.Any
            };
            return (pos, false);
        }

        return (Positional.Any, false);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (EnpiTarget == null || !HaveFugetsu)
            return;

        var buffOk = strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

        if (buffOk)
        {
            if (Unlocked(AID.Ikishoten) && _state.CanWeave(AID.Ikishoten, 0.6f, deadline))
                PushOGCD(AID.Ikishoten, Player);

            if (Zanshin > _state.AnimationLock && Kenki >= 50 && _state.CanWeave(AID.Zanshin, 0.6f, deadline))
                PushOGCD(AID.Zanshin, BestOgiTarget);

            if (Unlocked(AID.HissatsuGuren) && _state.CanWeave(AID.HissatsuGuren, 0.6f, deadline) && Kenki >= 25 && Zanshin == 0)
            {
                if (Unlocked(AID.HissatsuSenei) && NumLineTargets < 2)
                    PushOGCD(AID.HissatsuSenei, primaryTarget);

                PushOGCD(AID.HissatsuGuren, BestLineTarget);
            }
        }

        if (Meditation == 3 && _state.CanWeave(AID.Shoha, 0.6f, deadline))
            PushOGCD(AID.Shoha, BestLineTarget);

        if (Kenki >= 25 && _state.CD(AID.HissatsuGuren) > 10 && Zanshin == 0 && _state.CanWeave(AID.HissatsuShinten, 0.6f, deadline))
        {
            if (Unlocked(AID.HissatsuKyuten) && NumAOECircleTargets > 2)
                PushOGCD(AID.HissatsuKyuten, Player);

            PushOGCD(AID.HissatsuShinten, primaryTarget);
        }

        if (Unlocked(AID.MeikyoShisui) && _state.CanWeave(_state.CD(AID.MeikyoShisui) - 55, 0.6f, deadline) && Kaeshi == 0 && MeikyoLeft == 0 && Tendo == 0 && (NumStickers == 3 || CombatTimer < 30 || NumTenkaTargets > 2))
            PushOGCD(AID.MeikyoShisui, Player);
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();

        IaiTarget = primaryTarget;
        EnpiTarget = primaryTarget;

        SelectPrimaryTarget(targeting, ref primaryTarget, range: 3);
        SelectPrimaryTarget(targeting, ref IaiTarget, range: 6);
        SelectPrimaryTarget(targeting, ref EnpiTarget, range: 20);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<SamuraiGauge>();
        Kaeshi = gauge.Kaeshi;
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;

        Service.Log($"{Kaeshi}");

        FugetsuLeft = StatusLeft(SID.Fugetsu);
        FukaLeft = StatusLeft(SID.Fuka);
        MeikyoLeft = StatusLeft(SID.MeikyoShisui);
        OgiLeft = StatusLeft(SID.OgiNamikiriReady);
        TsubameLeft = StatusLeft(SID.TsubameGaeshiReady);
        EnhancedEnpi = StatusLeft(SID.EnhancedEnpi);
        Zanshin = StatusLeft(SID.ZanshinReady);
        Tendo = StatusLeft(SID.Tendo);

        (BestOgiTarget, NumOgiTargets) = SelectTarget(targeting, primaryTarget, 8, InConeAOE);

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.AOE)
        {
            NumAOECircleTargets = NumMeleeAOETargets();
            if (Unlocked(AID.Fuko))
                (BestAOETarget, NumAOETargets) = (null, NumAOECircleTargets);
            else
                (BestAOETarget, NumAOETargets) = (BestOgiTarget, NumOgiTargets);

            NumTenkaTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
            (BestLineTarget, NumLineTargets) = SelectTarget(targeting, primaryTarget, 10, InLineAOE);
        }
        else
        {
            NumAOECircleTargets = 0;
            NumTenkaTargets = 0;
            (BestAOETarget, NumAOETargets) = (null, 0);
            (BestLineTarget, NumLineTargets) = (primaryTarget, Player.DistanceToHitbox(primaryTarget) <= 10 ? 1 : 0);
        }

        if (Hints.PriorityTargets.Count() >= 3)
            TargetDotLeft = float.MaxValue;
        else
        {
            TargetDotLeft = strategy.Option(Track.Higanbana).As<OffensiveStrategy>() switch
            {
                OffensiveStrategy.Automatic => HiganbanaLeft(primaryTarget),
                OffensiveStrategy.Delay => float.MaxValue,
                OffensiveStrategy.Force => 0,
                _ => throw new NotImplementedException("sigh")
            };
        }

        _state.UpdatePositionals(primaryTarget, GetNextPositional(strategy), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private float HiganbanaLeft(Actor? p) => p == null ? float.MaxValue : _state.StatusDetails(p, SID.Higanbana, Player.InstanceID).Left;

    private bool InConeAOE(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees());
    private bool InLineAOE(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 4);
}
