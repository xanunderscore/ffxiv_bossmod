using BossMod.NIN;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class NIN(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, Hide, ForkedRaiju }
    public enum HideStrategy { Automatic, Manual }
    public enum RaijuStrategy { Manual, Automatic }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("NIN", "Ninja", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ROG, Class.NIN), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.Dokumori);

        def.Define(Track.Hide).As<HideStrategy>("Hide")
            .AddOption(HideStrategy.Automatic, "Auto", "Use when out of combat to restore charges")
            .AddOption(HideStrategy.Manual, "Manual", "Do not use automatically");

        def.Define(Track.ForkedRaiju).As<RaijuStrategy>("Forked Raiju")
            .AddOption(RaijuStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(RaijuStrategy.Automatic, "Auto", "Use when out of melee range");

        return def;
    }

    public int Ninki;
    public int Kazematoi;
    public (float Left, int Param) Mudra;
    public (float Left, int Param) TenChiJin;
    public bool HiddenStatus; // no max, ends when combat starts
    public float ShadowWalker; // max 20
    public float Kassatsu; // max 15
    public float PhantomKamaitachi; // max 45
    public float Meisui; // max 30
    public (float Left, int Stacks) Raiju;

    public float TargetTrickLeft; // max 15

    public int NumAOETargets;
    public int NumRangedAOETargets;

    // 25y for hellfrog - ninjutsu have a range of 20y
    private Actor? BestRangedAOETarget;

    private int[] Mudras => [Mudra.Param & 3, (Mudra.Param >> 2) & 3, (Mudra.Param >> 4) & 3];

    private readonly Dictionary<AID, (int Len, int Last)> Combos = new()
    {
        [AID.FumaShuriken] = (1, 0),
        [AID.Katon] = (2, 1),
        [AID.GokaMekkyaku] = (2, 1),
        [AID.Raiton] = (2, 2),
        [AID.Hyoton] = (2, 3),
        [AID.HyoshoRanryu] = (2, 3),
        [AID.Huton] = (3, 1),
        [AID.Doton] = (3, 2),
        [AID.Suiton] = (3, 3)
    };

    private bool Hidden => HiddenStatus || ShadowWalker > _state.AnimationLock;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 6)
                UseMudra(AID.Suiton, primaryTarget, endCondition: _state.CountdownRemaining < 1);

            return;
        }

        if (TenChiJin.Left > _state.GCD)
        {
            if (NumRangedAOETargets > 2)
            {
                PushGCD(TenChiJin.Param switch
                {
                    0 => AID.FumaJin,
                    1 => AID.TCJHyoton,
                    2 or 3 => AID.TCJKaton,
                    _ => AID.TCJDoton
                }, BestRangedAOETarget);
            }
            else
            {
                PushGCD(TenChiJin.Param switch
                {
                    0 => AID.FumaTen,
                    1 or 3 => AID.TCJRaiton,
                    2 => AID.TCJKaton,
                    _ => AID.TCJSuiton
                }, primaryTarget);
            }
            return;
        }

        if (PhantomKamaitachi > _state.GCD && Mudra.Left == 0)
            PushGCD(AID.PhantomKamaitachi, BestRangedAOETarget);

        if (Raiju.Stacks > 0 && Mudra.Left == 0)
        {
            if (strategy.Option(Track.ForkedRaiju).As<RaijuStrategy>() == RaijuStrategy.Automatic && Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20)
                PushGCD(AID.ForkedRaiju, primaryTarget);

            if (_state.CD(AID.TenChiJin) > 0)
                PushGCD(AID.FleetingRaiju, primaryTarget);
        }

        if (NumRangedAOETargets > 2 && Unlocked(AID.Katon))
        {
            if (_state.CD(AID.TrickAttack) < 15 && ShadowWalker == 0)
                UseMudra(AID.Huton, BestRangedAOETarget);

            // this will automatically adjust to goka mekkyaku
            if (_state.CD(AID.Kassatsu) > 0 && _state.CD(AID.DreamWithinADream) > 0)
                UseMudra(AID.Katon, BestRangedAOETarget);

            // fallthrough - condition changed while trying to execute an earlier case
            if (Mudra.Left > 0)
                PushGCD(AID.Katon, BestRangedAOETarget);
        }
        else if (Unlocked(AID.Raiton))
        {
            if (_state.CD(AID.TrickAttack) < 15 && ShadowWalker == 0)
                UseMudra(AID.Suiton, primaryTarget);

            if (_state.CD(AID.Kassatsu) > 0 && _state.CD(AID.DreamWithinADream) > 15 && _state.CD(AID.TrickAttack) > Kassatsu)
                UseMudra(Kassatsu > 0 && Unlocked(AID.HyoshoRanryu) ? AID.HyoshoRanryu : AID.Raiton, primaryTarget);

            // see above
            if (Mudra.Left > 0)
                PushGCD(AID.Raiton, primaryTarget);
        }
        else if (Unlocked(AID.FumaShuriken))
            UseMudra(AID.FumaShuriken, primaryTarget);

        if (NumAOETargets > 2 && Unlocked(AID.DeathBlossom))
        {
            if (ComboLastMove == AID.DeathBlossom && Unlocked(AID.HakkeMujinsatsu))
                PushGCD(AID.HakkeMujinsatsu, Player);

            PushGCD(AID.DeathBlossom, Player);
        }
        else
        {
            if (ComboLastMove == AID.GustSlash && Unlocked(AID.AeolianEdge) && primaryTarget != null)
                PushGCD(GetComboEnder(primaryTarget), primaryTarget);

            if (ComboLastMove == AID.SpinningEdge && Unlocked(AID.GustSlash))
                PushGCD(AID.GustSlash, primaryTarget);

            PushGCD(AID.SpinningEdge, primaryTarget);
        }
    }

    private AID GetComboEnder(Actor primaryTarget)
    {
        if (!Unlocked(AID.ArmorCrush))
            return AID.AeolianEdge;

        if (Kazematoi == 0)
            return AID.ArmorCrush;

        if (Kazematoi >= 4)
            return AID.AeolianEdge;

        return GetCurrentPositional(primaryTarget) == Positional.Rear ? AID.AeolianEdge : AID.ArmorCrush;
    }

    private void UseMudra(AID mudra, Actor? target, bool startCondition = true, bool endCondition = true)
    {
        (var aid, var tar) = PickMudra(mudra, target, startCondition, endCondition);
        if (aid != AID.None)
            PushGCD(aid, tar);
    }

    private (AID action, Actor? target) PickMudra(AID mudra, Actor? target, bool startCondition, bool endCondition)
    {
        if (!Unlocked(mudra) || target == null)
            return (AID.None, null);

        // no charges remaining and no kassatsu = we can't use it
        if (Mudra.Param == 0 && _state.CD(AID.Ten1) - 20 > _state.GCD && Kassatsu == 0)
            return (AID.None, null);

        // do nothing if start condition failed - since this could be something like checking ninjutsu CD, we skip it otherwise
        // (since ninjutsu goes on CD as soon as you press the first one)
        if (Mudra.Param == 0 && !startCondition)
            return (AID.None, null);

        // unrecognized action - this really shouldn't happen
        if (!Combos.TryGetValue(mudra, out var combo))
            return (AID.None, null);

        var (len, last) = combo;

        var ten1 = Kassatsu > 0 ? AID.Ten2 : AID.Ten1;
        var chi1 = Kassatsu > 0 ? AID.Chi2 : AID.Chi1;

        if (len == 1)
        {
            if (Mudras[0] == 0)
                return (ten1, Player);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        if (len == 2)
        {
            // early exit
            if (Mudras[0] == last)
                return (AID.Ninjutsu, target);

            if (Mudras[0] == 0)
                return (last == 1 ? chi1 : ten1, Player);

            if (Mudras[1] == 0)
                return (last == 1 ? AID.Ten2 : last == 2 ? AID.Chi2 : AID.Jin2, Player);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        if (len == 3)
        {
            // early exit
            if (Mudras[0] == last || Mudras[1] == last)
                return (AID.Ninjutsu, target);

            if (Mudras[0] == 0)
                return (last == 1 ? chi1 : ten1, Player);

            if (Mudras[1] == 0)
                return (Mudras[0] switch
                {
                    1 => last == 3 ? AID.Chi2 : AID.Jin2,
                    2 => last == 3 ? AID.Ten2 : AID.Jin2,
                    3 => last == 1 ? AID.Chi2 : AID.Ten2,
                    _ => AID.None
                }, Player);

            if (Mudras[2] == 0)
                return (last == 1 ? AID.Ten2 : last == 2 ? AID.Chi2 : AID.Jin2, Player);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        return (AID.None, null);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
        {
            if (strategy.Option(Track.Hide).As<HideStrategy>() == HideStrategy.Automatic
                && Mudra.Left == 0
                && _state.GCD == 0
                && _state.CD(AID.Ten1) > 0
                && _state.CanWeave(AID.Hide, 0.6f, deadline))
                PushOGCD(AID.Hide, Player);

            return;
        }

        if (Unlocked(AID.Meisui) && _state.CanWeave(AID.Meisui, 0.6f, deadline) && _state.CD(AID.TrickAttack) > 0 && ShadowWalker > 0)
            PushOGCD(AID.Meisui, Player);

        if (Unlocked(AID.Kassatsu) && _state.CanWeave(AID.Kassatsu, 0.6f, deadline) && _state.CD(AID.TrickAttack) < 5)
            PushOGCD(AID.Kassatsu, Player);

        var buffsOk = strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

        if (buffsOk && Unlocked(AID.Mug))
        {
            if ((!Unlocked(TraitID.Shukiho) || Ninki >= 10) && _state.CanWeave(AID.Mug, 0.6f, deadline))
                PushOGCD(AID.Mug, primaryTarget);

            if (Unlocked(AID.TenChiJin) && _state.CD(AID.Ten1) > 20 && Mudra.Left == 0 && Kassatsu == 0 && _state.CanWeave(AID.TenChiJin, 0.6f, deadline) && ForceMovementIn > _state.GCD + 2)
                PushOGCD(AID.TenChiJin, Player);

            if (Unlocked(AID.Bunshin) && Ninki >= 50 && _state.CanWeave(AID.Bunshin, 0.6f, deadline))
                PushOGCD(AID.Bunshin, Player);
        }

        if (_state.GCD < 0.8f && Unlocked(AID.TrickAttack) && _state.CanWeave(AID.TrickAttack, 0.6f, deadline) && Hidden && (_state.CD(AID.Mug) > 0 || !buffsOk))
            PushOGCD(AID.TrickAttack, primaryTarget);

        if (Unlocked(AID.DreamWithinADream) && _state.CD(AID.TrickAttack) > 10 && _state.CanWeave(AID.DreamWithinADream, 0.6f, deadline))
            PushOGCD(AID.DreamWithinADream, primaryTarget);

        if (ShouldBhava(strategy, primaryTarget, deadline))
        {
            if (NumRangedAOETargets > 2 || !Unlocked(AID.Bhavacakra))
                PushOGCD(AID.HellfrogMedium, BestRangedAOETarget);

            PushOGCD(AID.Bhavacakra, primaryTarget, Meisui > 0 ? 50 : 0);
        }
    }

    private bool ShouldBhava(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (Ninki < 50 || !Unlocked(AID.HellfrogMedium) || !_state.CanWeave(AID.Bhavacakra, 0.6f, deadline))
            return false;

        if (Meisui > 0 || TargetTrickLeft > _state.AnimationLock)
            return true;

        return Ninki > 85;
    }

    private (Positional, bool) GetNextPositional(Actor? primaryTarget)
    {
        if (!Unlocked(AID.AeolianEdge) || primaryTarget == null)
            return (Positional.Any, false);

        return (GetComboEnder(primaryTarget) == AID.AeolianEdge ? Positional.Rear : Positional.Flank, ComboLastMove == AID.GustSlash);
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();

        SelectPrimaryTarget(targeting, ref primaryTarget, range: 3);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<NinjaGauge>();
        Ninki = gauge.Ninki;
        Kazematoi = gauge.Kazematoi;

        var mudra = Player.FindStatus(SID.Mudra);
        if (mudra == null)
            Mudra = (0, 0);
        else
            Mudra = (_state.StatusDuration(mudra.Value.ExpireAt), mudra.Value.Extra);

        ShadowWalker = StatusLeft(SID.ShadowWalker);
        Kassatsu = StatusLeft(SID.Kassatsu);
        PhantomKamaitachi = StatusLeft(SID.PhantomKamaitachiReady);
        HiddenStatus = StatusStacks(SID.Hidden) > 0;
        TargetTrickLeft = MathF.Max(
            _state.StatusDetails(primaryTarget, SID.TrickAttack, Player.InstanceID).Left,
            _state.StatusDetails(primaryTarget, SID.KunaisBane, Player.InstanceID).Left
        );
        Raiju = Status(SID.RaijuReady);
        TenChiJin = Status(SID.TenChiJin);
        Meisui = StatusLeft(SID.Meisui);

        if (HiddenStatus)
            Hints.StatusesToCancel.Add(((uint)SID.Hidden, Player.InstanceID));

        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(targeting, primaryTarget, 20, IsSplashTarget);

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.AOE)
            NumAOETargets = NumMeleeAOETargets();
        else
        {
            NumAOETargets = 0;
            NumRangedAOETargets = BestRangedAOETarget == null ? 0 : 1;
        }

        _state.UpdatePositionals(primaryTarget, GetNextPositional(primaryTarget), TrueNorthLeft > _state.GCD);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
