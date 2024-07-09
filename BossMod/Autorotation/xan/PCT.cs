using BossMod.PCT;
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

    public int NumAOETargets;

    private Actor? BestAOETarget;
    private Actor? BestMogTarget;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        var motifUse = strategy.Option(Track.Motif).As<MotifStrategy>();

        var motifOk = !Player.InCombat || motifUse switch
        {
            MotifStrategy.Downtime => !Hints.PriorityTargets.Any(),
            MotifStrategy.Combat => _state.RaidBuffsLeft == 0,
            _ => false
        };

        if (ShouldHammer(strategy))
            PushGCD(AID.HammerStamp, BestAOETarget);

        if (ShouldHoly(strategy))
            PushGCD(AID.HolyInWhite, BestAOETarget);

        if (PomOnly && !Creature)
            PushGCD(AID.CreatureMotif, Player);

        if (motifOk)
        {
            if (!Creature && Unlocked(AID.CreatureMotif))
                PushGCD(AID.CreatureMotif, Player);

            if (!Weapon && Unlocked(AID.WeaponMotif) && HammerTime.Left == 0)
                PushGCD(AID.WeaponMotif, Player);

            if (!Landscape && Unlocked(AID.LandscapeMotif) && StarryMuseLeft == 0)
                PushGCD(AID.LandscapeMotif, Player);
        }

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

    protected override float GetCastTime(AID aid) => SwiftcastLeft > _state.GCD ? 0 : aid switch
    {
        AID.LandscapeMotif or AID.WeaponMotif or AID.CreatureMotif => Player.InCombat ? 3 : 0,
        _ => base.GetCastTime(aid)
    };

    private bool ShouldHoly(StrategyValues strategy)
    {
        if (Paint == 0)
            return false;

        // use for movement, or to weave raid buff at fight start
        if (ForceMovementIn == 0 || ShouldLandscape(strategy, _state.GCD + _state.SpellGCDTime))
            return true;

        // use comet to prevent overcap or before raid buffs expire
        // (we don't use regular holy to prevent overcap, it's a single target dps loss)
        if (Monochrome && (Paint == 5 || _state.RaidBuffsLeft > 0 && _state.RaidBuffsLeft < _state.GCD))
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

        if (ShouldLandscape(strategy, deadline))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ScenicMuse), Player, ActionQueue.Priority.Low + 500, Player.PosRot.XYZ());

        Subtract(strategy, deadline);

        if (Creature
            && BestAOETarget != null // this triggers native autotarget if BestAOETarget is null, because Living Muse is self-targeted but all of its transformations are not
            && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.LivingMuse, BestAOETarget);

        if (Moogle && _state.CanWeave(AID.MogOfTheAges, 0.6f, deadline))
            PushOGCD(AID.MogOfTheAges, BestMogTarget);

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool ShouldLandscape(StrategyValues strategy, float deadline)
    {
        if (strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        return Landscape && _state.CanWeave(AID.ScenicMuse, 0.6f, deadline);
    }

    private void Subtract(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.SubtractivePalette)
            || !_state.CanWeave(AID.SubtractivePalette, 0.6f, deadline)
            || Subtractive > 0
            || Palette < 50 && SpectrumLeft == 0)
            return;

        if (Palette > 75 || _state.RaidBuffsLeft > 0)
            PushOGCD(AID.SubtractivePalette, Player);
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

        var ah1 = StatusLeft(SID.Aetherhues);
        var ah2 = StatusLeft(SID.AetherhuesII);

        Hues = ah1 > 0 ? AetherHues.One : ah2 > 0 ? AetherHues.Two : AetherHues.None;

        (BestAOETarget, (NumAOETargets, _)) = SelectTarget(track, primaryTarget, 25, SplashTargetsPlusHP);

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.SingleTarget)
            NumAOETargets = 0;

        BestMogTarget = SelectTarget(track, primaryTarget, 25, Num25yRectTargets).Best;

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
