using BossMod.Autorotation.Legacy;
using BossMod.PCT;
using System.Runtime.InteropServices;

namespace BossMod.Autorotation.xan;
public sealed class PCT : xanmodule
{
    public enum Track { AOE, Targeting, Buffs, Motif }
    public enum AOEStrategy { AOE, SingleTarget }
    public enum MotifStrategy { Instant, Downtime, Combat }
    public enum OffensiveStrategy { Automatic, Delay, Force }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("PCT", "Pictomancer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PCT), 100);

        def.Define(Track.AOE).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions");

        def.DefineTargeting(Track.Targeting);

        def.Define(Track.Buffs).As<OffensiveStrategy>("Buffs")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use buffs when optimal")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use buffs")
            .AddOption(OffensiveStrategy.Force, "Force", "Use buffs ASAP");

        def.Define(Track.Motif).As<MotifStrategy>("Motifs")
            .AddOption(MotifStrategy.Instant, "Instant", "Only cast motifs when they are instant (out of combat)")
            .AddOption(MotifStrategy.Downtime, "Downtime", "Cast motifs in combat if there are no targets nearby")
            .AddOption(MotifStrategy.Combat, "Combat", "Cast motifs in combat, outside of burst window");

        return def;
    }

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    public int Palette; // 0-100
    public int Paint; // 0-5
    public bool Creature;
    public bool Weapon;
    public bool Landscape;
    public bool Moogle;
    public bool Madeen;

    public int Subtractive;
    public AetherHues Hues;
    public float StarryMuseLeft; // 20s max
    public (float Left, int Stacks) HammerTime;
    public float SpectrumLeft; // 30s max

    public int NumAOETargets;

    private Actor? BestAOETarget;
    private Actor? BestMogTarget;

    internal class State(RotationModule module) : CommonState(module)
    {
    }

    private readonly State _state;

    public PCT(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    protected override CommonState GetState() => _state;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        var motifUse = strategy.Option(Track.Motif).As<MotifStrategy>();

        var motifOk = !Player.InCombat || motifUse switch
        {
            MotifStrategy.Downtime => primaryTarget == null,
            MotifStrategy.Combat => _state.RaidBuffsLeft == 0,
            _ => false
        };

        if (_state.RaidBuffsLeft > _state.GCD || !Unlocked(AID.LandscapeMotif))
        {
            if (HammerTime.Stacks > 0)
                PushGCD(AID.HammerStamp, BestAOETarget);

            if (Paint > 0)
                PushGCD(AID.HolyInWhite, BestAOETarget);
        }

        if (HammerTime.Stacks > 0 && HammerTime.Left < _state.GCD + _state.SpellGCDTime * HammerTime.Stacks)
            PushGCD(AID.HammerStamp, BestAOETarget);

        if (Hues == AetherHues.Two && Paint == 5)
            PushGCD(AID.HolyInWhite, BestAOETarget);

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

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat)
            return;

        if (Landscape && _state.CanWeave(AID.ScenicMuse, 0.6f, deadline))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ScenicMuse), Player, ActionQueue.Priority.Low + 500, Player.PosRot.XYZ());

        if (_state.CanWeave(AID.SubtractivePalette, 0.6f, deadline) && Subtractive == 0 && (Palette > 75 || SpectrumLeft > 0))
            PushOGCD(AID.SubtractivePalette, Player);

        if (Weapon && _state.CanWeave(_state.CD(AID.SteelMuse) - 60, 0.6f, deadline))
            PushOGCD(AID.SteelMuse, Player);

        if (Creature && _state.CanWeave(_state.CD(AID.LivingMuse) - 80, 0.6f, deadline))
            PushOGCD(AID.LivingMuse, BestAOETarget);

        if (Moogle && _state.CanWeave(AID.MogOfTheAges, 0.6f, deadline))
            PushOGCD(AID.MogOfTheAges, BestMogTarget);
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

        var ah1 = _state.StatusDetails(Player, SID.Aetherhues, Player.InstanceID).Left;
        var ah2 = _state.StatusDetails(Player, SID.AetherhuesII, Player.InstanceID).Left;

        if (ah1 > _state.GCD)
            Hues = AetherHues.One;
        else if (ah2 > _state.GCD)
            Hues = AetherHues.Two;
        else
            Hues = AetherHues.None;

        (BestAOETarget, NumAOETargets) = SelectTarget(track, primaryTarget, 25, NumSplashTargets);
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

        Service.Log($"{Palette}, {Paint}, {Creature}, {Weapon}, {Landscape}, {Moogle}, {Madeen}, {_state.CD(AID.LivingMuse):f2}, {_state.CD(AID.SteelMuse):f2}");
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
