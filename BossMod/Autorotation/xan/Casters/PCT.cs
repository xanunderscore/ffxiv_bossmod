using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan.Casters;
public sealed class PCT(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
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
    public float Starstruck;

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

        if (CombatTimer < 1 && Weapon)
            PushGCD(AID.SteelMuse, Player);

        if (Starstruck > _state.GCD)
            PushGCD(AID.StarPrism, BestAOETarget);

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
        AID.RainbowDrip => RainbowBright > _state.GCD ? 0 : base.GetCastTime(aid),
        _ => base.GetCastTime(aid)
    };

    private bool ShouldHoly(StrategyValues strategy)
    {
        if (Paint == 0)
            return false;

        // use for movement, or to weave raid buff at fight start
        if (ForceMovementIn == 0 || ShouldSubtract(strategy, _state.GCD + _state.SpellGCDTime))
            return true;

        if (CombatTimer < 10 && !CreatureFlags.HasFlag(CreatureFlags.Pom))
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
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (ShouldWeapon(strategy, deadline))
            PushOGCD(AID.SteelMuse, Player);

        if (CanvasFlags.HasFlag(CanvasFlags.Pom) && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.PomMuse, BestAOETarget);

        if (ShouldLandscape(strategy, deadline))
            // TODO figure out why it won't weave (in opener) if using ogcd priority
            // motifs have 3s cast with 4s GCD, giving 1s window
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ScenicMuse), Player, ActionQueue.Priority.High + 600, targetPos: Player.PosRot.XYZ());

        if (ShouldSubtract(strategy, deadline))
            PushOGCD(AID.SubtractivePalette, Player);

        if (ShouldCreature(strategy, deadline))
            PushOGCD(AID.LivingMuse, BestAOETarget);

        if (ShouldMog(strategy, deadline))
            PushOGCD(AID.MogOfTheAges, BestLineTarget);

        if (Madeen && _state.CanWeave(AID.RetributionOfTheMadeen, 0.6f, deadline))
            PushOGCD(AID.RetributionOfTheMadeen, BestLineTarget);

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool ShouldWeapon(StrategyValues strategy, float deadline)
    {
        if (!Weapon || !_state.CanWeave(_state.CD(AID.SteelMuse) - 60, 0.6f, deadline))
            return false;

        // ensure muse alignment
        return !Unlocked(AID.StarryMuse) || _state.CD(AID.StarryMuse) is < 10 or > 60;
    }

    private bool ShouldCreature(StrategyValues strategy, float deadline)
    {
        // triggers native autotarget if BestAOETarget is null because LivingMuse is self targeted and all the actual muse actions are not
        if (!Creature || BestAOETarget == null)
            return false;

        return _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline);

        /*
        var singleChargeTime = Unlocked(TraitID.EnhancedPictomancyIV) ? 0 : 40;

        // use if max charges
        if (_state.CanWeave(_state.CD(AID.LivingMuse) - singleChargeTime, 0.6f, deadline))
            return true;

        // TODO these conditions are wrong, figure out the actual CD usage at level 100 when you get 3 charges
        if (_state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            return _state.RaidBuffsLeft > 0;

        return false;
        */
    }

    private bool ShouldMog(StrategyValues strategy, float deadline)
    {
        if (!Moogle || !_state.CanWeave(AID.MogOfTheAges, 0.6f, deadline))
            return false;

        // ensure muse alignment - moogle takes two 40s charges to rebuild
        return !Unlocked(AID.StarryMuse) || _state.RaidBuffsLeft > 0 || _state.CD(AID.StarryMuse) > 80;
    }

    private bool ShouldLandscape(StrategyValues strategy, float deadline)
    {
        if (strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        if (CombatTimer < 10 && !CanvasFlags.HasFlag(CanvasFlags.Wing))
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

    public override string DescribeState()
        => $"Canvas={CanvasFlags},Creature={CreatureFlags},W={Weapon},L={Landscape},PT={Paint},PL={Palette}";

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var track = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(track, ref primaryTarget, 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<PictomancerGauge>();
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
        Starstruck = StatusLeft(SID.Starstruck);

        var ah1 = StatusLeft(SID.Aetherhues);
        var ah2 = StatusLeft(SID.AetherhuesII);

        Hues = ah1 > 0 ? AetherHues.One : ah2 > 0 ? AetherHues.Two : AetherHues.None;

        (BestAOETarget, (NumAOETargets, _)) = SelectTarget(track, primaryTarget, 25, IsSplashTarget,
            (numTargets, target) => (numTargets, target.HPMP.CurHP)
        );

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.SingleTarget)
            NumAOETargets = 0;

        BestLineTarget = SelectTarget(track, primaryTarget, 25, Is25yRectTarget).Best;

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
