using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class BLM(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("BLM", "Black Mage", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BLM, Class.THM), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.LeyLines);

        return def;
    }

    public int Element; // -3 (ice) <=> 3 (fire), 0 for none
    public float ElementLeft;
    public float NextPolyglot; // max 30
    public int Hearts; // max 3
    public int Polyglot;
    public int AstralSoul; // max 6
    public bool Paradox;

    public float Triplecast;
    public float Thunderhead;
    public float Firestarter;
    public bool InLeyLines;

    public float TargetThunderLeft;

    public int Fire => Math.Max(0, Element);
    public int Ice => Math.Max(0, -Element);

    public int PolyglotMax => Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;

    private Actor? BestAOETarget;
    private int NumAOETargets;

    private enum Aspect
    {
        None,
        Ice,
        Fire
    }

    private Aspect GetAspect(AID aid) => aid switch
    {
        AID.Fire1 or AID.Fire2 or AID.Fire3 or AID.Fire4 or AID.Flare => Aspect.Fire,
        AID.Blizzard1 or AID.Blizzard2 or AID.Blizzard3 or AID.Blizzard4 or AID.Freeze => Aspect.Ice,
        _ => Aspect.None
    };

    protected override float GetCastTime(AID aid)
    {
        if (Triplecast > _state.GCD)
            return 0;

        var castTime = base.GetCastTime(aid);
        if (castTime == 0)
            return 0;

        if (aid == AID.Fire3 && Firestarter > _state.GCD)
            return 0;

        if (Element == -3 && GetAspect(aid) == Aspect.Fire || Element == 3 && GetAspect(aid) == Aspect.Ice)
            castTime *= 0.5f;

        return castTime;
    }

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < GetCastTime(AID.Fire3))
                PushGCD(AID.Fire3, primaryTarget);

            return;
        }

        if (primaryTarget == null)
        {
            if (Unlocked(AID.UmbralSoul) && Ice > 0 && (Ice < 3 || Hearts < 3 || ElementLeft < 3))
                PushGCD(AID.UmbralSoul, Player);

            return;
        }

        if (Fire > 0)
            GetFireGCD(strategy, primaryTarget);
        else if (Ice > 0)
            GetIceGCD(strategy, primaryTarget);
        else if (!Player.InCombat && MP >= 9600)
            Choose(AID.Fire3, AID.Fire2, primaryTarget);
        else
            Choose(AID.Blizzard3, AID.Blizzard2, primaryTarget);

        if (Polyglot > 0)
            Choose(AID.Xenoglossy, AID.Foul, primaryTarget);
    }

    private void GetFireGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Thunderhead > _state.GCD && TargetThunderLeft < 5)
            Choose(AID.Thunder1, AID.Thunder2, primaryTarget);

        if (Fire < 3)
            Choose(AID.Fire3, AID.Fire2, primaryTarget);

        if (NumAOETargets > 2)
        {
            if (Hearts is 3 or 0)
                PushGCD(AID.Fire2, BestAOETarget);

            PushGCD(AID.Flare, BestAOETarget);
        }
        else if (Unlocked(AID.Fire4))
        {
            var minF4Time = MathF.Max(_state.SpellGCDTime, GetCastTime(AID.Fire4) + _state.AnimationLockDelay);

            if (Fire == 3)
            {
                if (_state.CanWeave(AID.LeyLines, 0.6f, _state.GCD + _state.SpellGCDTime) && GetCastTime(AID.Fire4) > 0)
                    TryInstantCast(strategy, primaryTarget);

                // despair requires 800 MP
                if (MP < 800)
                    PushGCD(AID.Blizzard3, primaryTarget);
                // breakpoint at which despair is more damage than f1 despair, because it speeds up next fire phase
                else if (MP <= 2400 && ElementLeft > GetSlidecastEnd(AID.Despair))
                    PushGCD(AID.Despair, primaryTarget);
                // AF3 will last *at least* another two F4s, ok to cast
                // TODO in the case where we have one triplecast stack left, this will end up checking (timer > 2.5 + 2.5) instead of (timer > 2.5 + 3.1) - i think it's ok?
                else if (ElementLeft > NextCastStart + minF4Time * 2)
                {
                    if (Polyglot == PolyglotMax && NextPolyglot < 5)
                        PushGCD(AID.Xenoglossy, primaryTarget);

                    PushGCD(AID.Fire4, primaryTarget);
                }
                // AF3 will last long enough for us to refresh using Paradox
                else if (ElementLeft > NextCastStart + minF4Time && Paradox)
                    PushGCD(AID.Fire4, primaryTarget);
                else
                    PushGCD(AID.Fire1, primaryTarget);
            }
        }
        else
        {
            if (MP < 1600)
                PushGCD(AID.Blizzard3, primaryTarget);

            PushGCD(Firestarter > _state.GCD ? AID.Fire3 : AID.Fire1, primaryTarget);
        }
    }

    private void GetIceGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Ice < 3)
            Choose(AID.Blizzard3, AID.Blizzard2, primaryTarget);

        if (MP < 10000)
            Choose(Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Freeze, AID.Freeze, primaryTarget);

        var nextGCD = _state.GCD + _state.SpellGCDTime;

        if (ElementLeft > nextGCD && Firestarter > nextGCD && _state.CanWeave(AID.Transpose, 0.6f, nextGCD) && SwiftcastLeft == 0 && Triplecast == 0 && MP == 10000)
            TryInstantCast(strategy, primaryTarget, useFirestarter: false);

        Choose(AID.Fire3, AID.Fire2, primaryTarget);
    }

    private void Choose(AID st, AID aoe, Actor? primaryTarget, int additionalPrio = 0)
    {
        if (NumAOETargets > 2)
            PushGCD(aoe, BestAOETarget, additionalPrio);
        else
            PushGCD(st, primaryTarget, additionalPrio);
    }

    private void TryInstantCast(StrategyValues strategy, Actor? primaryTarget, bool useFirestarter = true, bool useThunderhead = true, bool usePolyglot = true)
    {
        var tp = useThunderhead && Thunderhead > _state.GCD;

        if (tp && TargetThunderLeft < 5)
            Choose(AID.Thunder1, AID.Thunder2, primaryTarget);

        if (usePolyglot && Polyglot > 0)
            Choose(AID.Xenoglossy, AID.Foul, primaryTarget);

        if (tp)
            Choose(AID.Thunder1, AID.Thunder2, primaryTarget, TargetThunderLeft < 5 ? 20 : 0);

        if (useFirestarter && Firestarter > _state.GCD)
            PushGCD(AID.Fire3, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat || primaryTarget == null)
        {
            if (Fire > 0 && Unlocked(AID.Transpose) && _state.CD(AID.Transpose) == 0)
                PushOGCD(AID.Transpose, Player);

            return;
        }

        if (Unlocked(AID.Swiftcast) && _state.CanWeave(AID.Swiftcast, 0.6f, deadline))
            PushOGCD(AID.Swiftcast, Player);

        if (Unlocked(AID.Amplifier) && _state.CanWeave(AID.Amplifier, 0.6f, deadline) && Polyglot < PolyglotMax)
            PushOGCD(AID.Amplifier, Player);

        if (ShouldTriplecast(strategy, deadline))
            PushOGCD(AID.Triplecast, Player);

        if (ShouldUseLeylines(strategy, deadline))
            PushOGCD(AID.LeyLines, Player);

        if (Unlocked(AID.Manafont) && MP == 0 && Fire > 0 && _state.CanWeave(AID.Manafont, 0.6f, deadline))
            PushOGCD(AID.Manafont, Player);

        if (Firestarter > _state.GCD && Ice > 0 && Hearts == 3 && _state.CanWeave(AID.Transpose, 0.6f, deadline))
            PushOGCD(AID.Transpose, Player);
    }

    private bool ShouldTriplecast(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.Triplecast) || !_state.CanWeave(_state.CD(AID.Triplecast) - 60, 0.6f, deadline) || Triplecast > 0)
            return false;

        return ShouldUseLeylines(strategy, _state.GCD) || InLeyLines;
    }

    private bool ShouldUseLeylines(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.LeyLines) || !_state.CanWeave(AID.LeyLines, 0.6f, deadline) || ForceMovementIn < 30)
            return false;

        return strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<BlackMageGauge>();

        Element = gauge.ElementStance;
        ElementLeft = gauge.ElementTimeRemaining * 0.001f;
        NextPolyglot = gauge.EnochianTimer * 0.001f;
        Hearts = gauge.UmbralHearts;
        Polyglot = gauge.PolyglotStacks;
        Paradox = gauge.ParadoxActive;
        AstralSoul = gauge.AstralSoulStacks;

        Triplecast = StatusLeft(SID.Triplecast);
        Thunderhead = StatusLeft(SID.Thunderhead);
        Firestarter = Player.FindStatus((uint)SID.Firestarter) is ActorStatus s ? _state.StatusDuration(s.ExpireAt) : 0;
        InLeyLines = Player.FindStatus(SID.CircleOfPower) != null;

        TargetThunderLeft = _state.StatusDetails(primaryTarget, SID.Thunder3, Player.InstanceID, 27).Left;

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.AOE)
            (BestAOETarget, NumAOETargets) = SelectTarget(targeting, primaryTarget, 25, IsSplashTarget);
        else
            (BestAOETarget, NumAOETargets) = (null, 0);

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
