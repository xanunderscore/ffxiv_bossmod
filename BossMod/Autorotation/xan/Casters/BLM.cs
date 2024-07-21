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

    public uint MP => Player.HPMP.CurMP;
    public int Fire => Math.Max(0, Element);
    public int Ice => Math.Max(0, -Element);

    public uint PendingMP;

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
        if (Fire > 0)
        {
            GetFireGCD(strategy, primaryTarget);
            return;
        }

        if (Ice > 0)
        {
            GetIceGCD(strategy, primaryTarget);
            return;
        }

        PushGCD(AID.Fire3, primaryTarget);
    }

    private void GetFireGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Thunderhead > _state.GCD)
            PushGCD(AID.Thunder3, primaryTarget);

        if (Fire == 3)
        {
            // despair requires 800 MP
            if (MP < 800)
                PushGCD(AID.Blizzard3, primaryTarget);
            // breakpoint at which despair is more damage than f1 despair, because it speeds up next fire phase
            else if (MP <= 2400 && ElementLeft > GetSlidecastEnd(AID.Despair))
                PushGCD(AID.Despair, primaryTarget);
            else if (ElementLeft > GetSlidecastEnd(AID.Fire4) + GetCastTime(AID.Fire4))
                PushGCD(AID.Fire4, primaryTarget);
            else if (ElementLeft > GetSlidecastEnd(AID.Fire4) && Paradox)
                PushGCD(AID.Fire4, primaryTarget);
            else
                PushGCD(AID.Fire1, primaryTarget);
        }
    }

    private void GetIceGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Ice < 3)
            PushGCD(AID.Blizzard3, primaryTarget);

        if (Hearts < 3)
            PushGCD(AID.Blizzard4, primaryTarget);

        PushGCD(AID.Fire3, primaryTarget);
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

        CalcNextBestGCD(strategy, primaryTarget);
    }
}
