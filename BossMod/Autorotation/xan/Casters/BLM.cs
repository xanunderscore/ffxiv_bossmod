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

    public float TriplecastLeft;
    public float ThunderheadLeft;

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
        if (TriplecastLeft > _state.GCD)
            return 0;

        var castTime = base.GetCastTime(aid);
        if (castTime == 0)
            return 0;

        if (Element == -3 && GetAspect(aid) == Aspect.Fire)
            castTime *= 0.5f;

        if (Element == 3 && GetAspect(aid) == Aspect.Ice)
            castTime *= 0.5f;

        return castTime;
    }

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
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
        Hearts = gauge.UmbralStacks;
        Polyglot = gauge.PolyglotStacks;
        Paradox = gauge.ParadoxActive;
        AstralSoul = gauge.AstralSoulStacks;

        TriplecastLeft = StatusLeft(SID.Triplecast);
        ThunderheadLeft = StatusLeft(SID.Thunderhead);

        CalcNextBestGCD(strategy, primaryTarget);
    }
}
