using BossMod.PCT;
using System.Runtime.InteropServices;
using static BossMod.Autorotation.xan.xcommon;

namespace BossMod.Autorotation.xan;
public sealed class PCT(RotationModuleManager manager, Actor player) : xmodule<AID, TraitID>(manager, player)
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
    private CreatureFlags _creatureFlags;
    private CanvasFlags _canvasFlags;

    public int Subtractive;
    public AetherHues Hues;
    public float StarryMuseLeft; // 20s max
    public (float Left, int Stacks) HammerTime;
    public float SpectrumLeft; // 30s max
    public float SwiftcastLeft; // 10s max

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

    private bool ShouldHoly(StrategyValues strategy) => Paint > 0 &&
        (Hues == AetherHues.Two && Paint == 5
            || ForceMovementIn == 0);

    private bool ShouldHammer(StrategyValues strategy) => HammerTime.Stacks > 0 &&
         (_state.RaidBuffsLeft > _state.GCD
             || ForceMovementIn == 0
             || HammerTime.Left < _state.GCD + _state.SpellGCDTime * HammerTime.Stacks);

    private bool PomOnly => _creatureFlags.HasFlag(CreatureFlags.Pom) && !_creatureFlags.HasFlag(CreatureFlags.Wings);

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (_canvasFlags.HasFlag(CanvasFlags.Pom) && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.PomMuse, BestAOETarget);

        if (!Player.InCombat)
            return;

        if (strategy.Option(Track.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay)
        {
            if (Landscape && _state.CanWeave(AID.ScenicMuse, 0.6f, deadline))
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ScenicMuse), Player, ActionQueue.Priority.Low + 500, Player.PosRot.XYZ());
        }

        if (Weapon && _state.CanWeave(_state.CD(AID.SteelMuse) - 60, 0.6f, deadline))
            PushOGCD(AID.SteelMuse, Player);

        if (ShouldSubtract(strategy, deadline))
            PushOGCD(AID.SubtractivePalette, Player);

        if (Creature
            && BestAOETarget != null // FIXME autotarget again
            && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.LivingMuse, BestAOETarget);

        if (Moogle && _state.CanWeave(AID.MogOfTheAges, 0.6f, deadline))
            PushOGCD(AID.MogOfTheAges, BestMogTarget);

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool ShouldSubtract(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.SubtractivePalette) || !_state.CanWeave(AID.SubtractivePalette, 0.6f, deadline) || Subtractive > 0 || (Palette < 50 && SpectrumLeft == 0))
            return false;

        return Palette > 75 || _state.RaidBuffsLeft > 0;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        var track = strategy.Option(Track.Targeting);
        SelectPrimaryTarget(track, ref primaryTarget, 25);
        _state.UpdateCommon(primaryTarget);

        UpdateGauge();

        Subtractive = _state.StatusDetails(Player, SID.SubtractivePalette, Player.InstanceID).Stacks;
        StarryMuseLeft = _state.StatusDetails(Player, SID.StarryMuse, Player.InstanceID).Left;
        HammerTime = _state.StatusDetails(Player, SID.HammerTime, Player.InstanceID);
        SpectrumLeft = _state.StatusDetails(Player, SID.SubtractiveSpectrum, Player.InstanceID).Left;
        SwiftcastLeft = _state.StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;

        var ah1 = _state.StatusDetails(Player, SID.Aetherhues, Player.InstanceID).Left;
        var ah2 = _state.StatusDetails(Player, SID.AetherhuesII, Player.InstanceID).Left;

        Hues = ah1 > _state.GCD ? AetherHues.One : ah2 > _state.GCD ? AetherHues.Two : AetherHues.None;

        (BestAOETarget, (NumAOETargets, _)) = SelectTarget(track, primaryTarget, 25, actor => (NumSplashTargets(actor), actor.HPMP.CurHP));
        if (primaryTarget == null && BestAOETarget != null)
            throw new Exception("uh oh!");

        if (strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.SingleTarget)
            // BestAOETarget still used for hammer, pom, etc
            NumAOETargets = 0;

        BestMogTarget = SelectTarget(track, primaryTarget, 25, act => Hints.NumPriorityTargetsInAOERect(Player.Position, (act.Position - Player.Position).Normalized(), 25, 4)).Best;

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private unsafe void UpdateGauge()
    {
        var gauge = (PictomancerGauge*)Service.JobGauges.Address;

        Palette = gauge->PaletteGauge;
        Paint = gauge->Paint;
        Creature = gauge->CreatureMotifDrawn;
        Weapon = gauge->WeaponMotifDrawn;
        Landscape = gauge->LandscapeMotifDrawn;
        Moogle = gauge->MooglePortraitReady;
        Madeen = gauge->MadeenPortraitReady;
        _creatureFlags = gauge->CreatureFlags;
        _canvasFlags = gauge->CanvasFlags;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct PictomancerGauge
    {
        [FieldOffset(0x08)] public byte PaletteGauge;
        [FieldOffset(0x0A)] public byte Paint;
        [FieldOffset(0x0B)] public CanvasFlags CanvasFlags;
        [FieldOffset(0x0C)] public CreatureFlags CreatureFlags;

        public bool CreatureMotifDrawn => CanvasFlags.HasFlag(CanvasFlags.Pom) || CanvasFlags.HasFlag(CanvasFlags.Wing) || CanvasFlags.HasFlag(CanvasFlags.Claw) || CanvasFlags.HasFlag(CanvasFlags.Maw);
        public bool WeaponMotifDrawn => CanvasFlags.HasFlag(CanvasFlags.Weapon);
        public bool LandscapeMotifDrawn => CanvasFlags.HasFlag(CanvasFlags.Landscape);
        public bool MooglePortraitReady => CreatureFlags.HasFlag(CreatureFlags.MooglePortait);
        public bool MadeenPortraitReady => CreatureFlags.HasFlag(CreatureFlags.MadeenPortrait);
    }

    [Flags]
    public enum CanvasFlags : byte
    {
        Pom = 1,
        Wing = 2,
        Claw = 4,
        Maw = 8,
        Weapon = 16,
        Landscape = 32,
    }

    [Flags]
    public enum CreatureFlags : byte
    {
        Pom = 1,
        Wings = 2,
        Claw = 4,

        MooglePortait = 16,
        MadeenPortrait = 32,
    }

    public enum AetherHues : uint
    {
        None = 0,
        One = 1,
        Two = 2
    }
}
