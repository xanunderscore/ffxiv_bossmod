﻿using BossMod.PCT;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class PCT(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, Motif, Holy, Hammer }
    public enum MotifStrategy { Instant, Downtime, Combat }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("PCT", "Pictomancer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PCT), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.StarryMuse);

        def.Define(Track.Motif).As<MotifStrategy>("Motifs")
            .AddOption(MotifStrategy.Instant, "Instant", "Only cast motifs when they are instant (out of combat)")
            .AddOption(MotifStrategy.Downtime, "Downtime", "Cast motifs in combat if there are no targets nearby")
            .AddOption(MotifStrategy.Combat, "Combat", "Cast motifs in combat, outside of burst window");

        def.DefineSimple(Track.Holy, "Holy").AddAssociatedActions(AID.HolyInWhite);
        def.DefineSimple(Track.Hammer, "Hammer").AddAssociatedActions(AID.HammerStamp);

        return def;
    }

    public int Palette; // 0-100
    public int Paint; // 0-5
    public bool Creature;
    public bool Weapon;
    public bool Landscape;
    public bool Moogle;
    public bool Madeen;
    public bool Monochrome;
    public CreatureFlags CreatureFlags;
    public CanvasFlags CanvasFlags;

    public enum AetherHues : uint
    {
        None = 0,
        One = 1,
        Two = 2
    }

    public AetherHues Hues;
    public int Subtractive;
    public float StarryMuseLeft; // 20s max
    public (float Left, int Stacks) HammerTime;
    public float SpectrumLeft; // 30s max
    public int Hyperphantasia;
    public float RainbowBright;

    public int NumAOETargets;

    private Actor? BestAOETarget;
    private Actor? BestLineTarget;

    private bool WingPlanned => PomOnly && !Creature && _state.CD(AID.LivingMuse) - 80 < _state.GCD + 4;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        var motifOk = IsMotifOk(strategy);

        if (motifOk)
        {
            if (!Creature && Unlocked(AID.CreatureMotif))
                PushGCD(AID.CreatureMotif, Player);

            if (!Weapon && Unlocked(AID.WeaponMotif) && HammerTime.Left == 0)
                PushGCD(AID.WeaponMotif, Player);

            if (!Landscape && Unlocked(AID.LandscapeMotif) && StarryMuseLeft == 0)
                PushGCD(AID.LandscapeMotif, Player);
        }

        if (_state.CountdownRemaining > 0)
        {
            if (Unlocked(AID.RainbowDrip) && _state.CountdownRemaining <= GetCastTime(AID.RainbowDrip))
                PushGCD(AID.RainbowDrip, primaryTarget);

            return;
        }

        if (RainbowBright > _state.GCD)
            PushGCD(AID.RainbowDrip, BestLineTarget);

        // hardcasting wing motif is #1 prio in opener
        if (WingPlanned)
            PushGCD(AID.CreatureMotif, Player);

        if (ShouldHammer(strategy))
            PushGCD(AID.HammerStamp, BestAOETarget);

        if (ShouldHoly(strategy))
            PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, BestAOETarget);

        if (NumAOETargets > 3 && Unlocked(AID.FireIIInRed))
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardIIInCyan, BestAOETarget);

            PushGCD(AID.FireIIInRed, BestAOETarget);
        }
        else
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardInCyan, primaryTarget);

            PushGCD(AID.FireInRed, primaryTarget);
        }
    }

    private bool IsMotifOk(StrategyValues strategy)
    {
        if (!Player.InCombat)
            return true;

        // spend buffs instead of casting motifs
        if (Hyperphantasia > 0 || SpectrumLeft > _state.GCD)
            return false;

        return strategy.Option(Track.Motif).As<MotifStrategy>() switch
        {
            MotifStrategy.Downtime => !Hints.PriorityTargets.Any(),
            MotifStrategy.Combat => _state.RaidBuffsLeft == 0,
            _ => false
        };
    }

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.LandscapeMotif or AID.WeaponMotif or AID.CreatureMotif => SwiftcastLeft > _state.GCD || !Player.InCombat ? 0 : 3,
        _ => base.GetCastTime(aid)
    };

    private bool ShouldHoly(StrategyValues strategy)
    {
        if (Paint == 0)
            return false;

        // use for movement, or to weave raid buff at fight start
        if (ForceMovementIn == 0 || ShouldLandscape(strategy, _state.GCD + _state.SpellGCDTime) || ShouldSubtract(strategy, _state.GCD + _state.SpellGCDTime))
            return true;

        // use comet to prevent overcap or during buffs
        // (we don't use regular holy to prevent overcap, it's a single target dps loss)
        if (Monochrome && (Paint == 5 || _state.RaidBuffsLeft > 0))
            return true;

        return false;
    }

    private bool ShouldHammer(StrategyValues strategy) => HammerTime.Stacks > 0 &&
         (_state.RaidBuffsLeft > _state.GCD
             || ForceMovementIn == 0
             // set to 4s instead of GCD timer in case we end up wanting to hardcast all 3 motifs
             || HammerTime.Left < _state.GCD + 4 * HammerTime.Stacks);

    private bool PomOnly => CreatureFlags.HasFlag(CreatureFlags.Pom) && !CreatureFlags.HasFlag(CreatureFlags.Wings);

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat)
            return;

        if (Weapon && _state.CanWeave(_state.CD(AID.SteelMuse) - 60, 0.6f, deadline))
            PushOGCD(AID.SteelMuse, Player);

        if (CanvasFlags.HasFlag(CanvasFlags.Pom) && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.PomMuse, BestAOETarget);

        if (!WingPlanned && ShouldLandscape(strategy, deadline))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ScenicMuse), Player, ActionQueue.Priority.Low + 500, Player.PosRot.XYZ());

        if (ShouldSubtract(strategy, deadline))
            PushOGCD(AID.SubtractivePalette, Player);

        if (ShouldCreature(strategy, deadline))
            PushOGCD(AID.LivingMuse, BestAOETarget);

        if (ShouldMog(strategy, deadline))
            PushOGCD(AID.MogOfTheAges, BestLineTarget);

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool ShouldCreature(StrategyValues strategy, float deadline)
    {
        // triggers native autotarget if BestAOETarget is null because LivingMuse is self targeted and all the actual muse actions are not
        if (!Creature || BestAOETarget == null)
            return false;

        // use if max charges
        if (_state.CanWeave(AID.LivingMuse, 0.6f, deadline))
            return true;

        if (_state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            return _state.RaidBuffsLeft > 0;

        return false;
    }

    private bool ShouldMog(StrategyValues strategy, float deadline) => Moogle && !ShouldLandscape(strategy, deadline) && _state.CanWeave(AID.MogOfTheAges, 0.6f, deadline);

    private bool ShouldLandscape(StrategyValues strategy, float deadline)
    {
        if (strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        return Landscape && _state.CanWeave(AID.ScenicMuse, 0.6f, deadline);
    }

    private bool ShouldSubtract(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.SubtractivePalette)
            || !_state.CanWeave(AID.SubtractivePalette, 0.6f, deadline)
            || Subtractive > 0
            || ShouldLandscape(strategy, deadline)
            || Palette < 50 && SpectrumLeft == 0)
            return false;

        return Palette > 75 || _state.RaidBuffsLeft > 0 || SpectrumLeft > 0;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        var track = strategy.Option(Track.Targeting);
        SelectPrimaryTarget(track, ref primaryTarget, 25);
        _state.UpdateCommon(primaryTarget);

        var gauge = Service.JobGauges.Get<PCTGauge>();
        Palette = gauge.PalleteGauge;
        Paint = gauge.Paint;
        Creature = gauge.CreatureMotifDrawn;
        Weapon = gauge.WeaponMotifDrawn;
        Landscape = gauge.LandscapeMotifDrawn;
        Moogle = gauge.MooglePortraitReady;
        Madeen = gauge.MadeenPortraitReady;
        CreatureFlags = gauge.CreatureFlags;
        CanvasFlags = gauge.CanvasFlags;

        Subtractive = StatusStacks(SID.SubtractivePalette);
        StarryMuseLeft = StatusLeft(SID.StarryMuse);
        HammerTime = Status(SID.HammerTime);
        SpectrumLeft = StatusLeft(SID.SubtractiveSpectrum);
        Monochrome = Player.FindStatus(SID.MonochromeTones) != null;
        Hyperphantasia = StatusStacks(SID.Hyperphantasia);
        RainbowBright = StatusLeft(SID.RainbowBright);

        var ah1 = StatusLeft(SID.Aetherhues);
        var ah2 = StatusLeft(SID.AetherhuesII);

        Hues = ah1 > 0 ? AetherHues.One : ah2 > 0 ? AetherHues.Two : AetherHues.None;

        (BestAOETarget, (NumAOETargets, _)) = SelectTarget(track, primaryTarget, 25, SplashTargetsPlusHP);

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.SingleTarget)
            NumAOETargets = 0;

        BestLineTarget = SelectTarget(track, primaryTarget, 25, Num25yRectTargets).Best;

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}