﻿namespace BossMod.WAR;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    LandWaker = 4240, // LB3, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    HeavySwing = 31, // L1, instant, GCD, range 3, single-target, targets=hostile
    Maim = 37, // L4, instant, GCD, range 3, single-target, targets=hostile
    Berserk = 38, // L6, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    Overpower = 41, // L10, instant, GCD, range 0, AOE 5 circle, targets=self
    Defiance = 48, // L10, instant, 2.0s CD (group 2), range 0, single-target, targets=self
    ReleaseDefiance = 32066, // L10, instant, 1.0s CD (group 2), range 0, single-target, targets=self
    Tomahawk = 46, // L15, instant, GCD, range 20, single-target, targets=hostile
    StormPath = 42, // L26, instant, GCD, range 3, single-target, targets=hostile
    ThrillOfBattle = 40, // L30, instant, 90.0s CD (group 16), range 0, single-target, targets=self
    InnerBeast = 49, // L35, instant, GCD, range 3, single-target, targets=hostile
    Vengeance = 44, // L38, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    MythrilTempest = 16462, // L40, instant, GCD, range 0, AOE 5 circle, targets=self
    Holmgang = 43, // L42, instant, 240.0s CD (group 23), range 6, single-target, targets=self/hostile
    SteelCyclone = 51, // L45, instant, GCD, range 0, AOE 5 circle, targets=self
    StormEye = 45, // L50, instant, GCD, range 3, single-target, targets=hostile
    Infuriate = 52, // L50, instant, 60.0s CD (group 19/70) (2 charges), range 0, single-target, targets=self
    FellCleave = 3549, // L54, instant, GCD, range 3, single-target, targets=hostile
    RawIntuition = 3551, // L56, instant, 25.0s CD (group 3), range 0, single-target, targets=self
    Equilibrium = 3552, // L58, instant, 60.0s CD (group 14), range 0, single-target, targets=self
    Decimate = 3550, // L60, instant, GCD, range 0, AOE 5 circle, targets=self
    Onslaught = 7386, // L62, instant, 30.0s CD (group 9/71) (2-3 charges), range 20, single-target, targets=hostile
    Upheaval = 7387, // L64, instant, 30.0s CD (group 5), range 3, single-target, targets=hostile
    ShakeItOff = 7388, // L68, instant, 90.0s CD (group 17), range 0, AOE 30 circle, targets=self
    InnerRelease = 7389, // L70, instant, 60.0s CD (group 11), range 0, single-target, targets=self
    ChaoticCyclone = 16463, // L72, instant, GCD, range 0, AOE 5 circle, targets=self
    NascentFlash = 16464, // L76, instant, 25.0s CD (group 3), range 30, single-target, targets=party
    InnerChaos = 16465, // L80, instant, GCD, range 3, single-target, targets=hostile
    Bloodwhetting = 25751, // L82, instant, 25.0s CD (group 3), range 0, single-target, targets=self
    Orogeny = 25752, // L86, instant, 30.0s CD (group 5), range 0, AOE 5 circle, targets=self
    PrimalRend = 25753, // L90, instant, GCD, range 20, AOE 5 circle, targets=hostile, animLock=1.150

    // Shared
    ShieldWall = ClassShared.AID.ShieldWall, // LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
    Stronghold = ClassShared.AID.Stronghold, // LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    Rampart = ClassShared.AID.Rampart, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    LowBlow = ClassShared.AID.LowBlow, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
    Provoke = ClassShared.AID.Provoke, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    Interject = ClassShared.AID.Interject, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
    Reprisal = ClassShared.AID.Reprisal, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Shirk = ClassShared.AID.Shirk, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=party
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 318, // L1
    TheBeastWithin = 249, // L35, gauge generation
    InnerBeastMastery = 265, // L54, IB->FC upgrade
    SteelCycloneMastery = 266, // L60,  steel cyclone -> decimate upgrade
    EnhancedInfuriate = 157, // L66, gauge spenders reduce cd by 5
    BerserkMastery = 218, // L70, berserk -> IR upgrade
    NascentChaos = 267, // L72, decimate -> chaotic cyclone after infuriate
    MasteringTheBeast = 268, // L74, mythril tempest gives gauge
    EnhancedShakeItOff = 417, // L76, adds heal
    EnhancedThrillOfBattle = 269, // L78, adds incoming heal buff
    RawIntuitionMastery = 418, // L82, raw intuition -> bloodwhetting
    EnhancedNascentFlash = 419, // L82, duration increase
    EnhancedEquilibrium = 420, // L84, adds hot
    MeleeMastery = 505, // L84, potency increase
    EnhancedOnslaught = 421, // L88, 3rd onslaught charge
}

public sealed class Definitions : IDisposable
{
    public static readonly uint[] UnlockQuests = [65852, 65855, 66586, 66587, 66589, 66590, 66124, 66132, 66134, 66137, 68440];

    public static bool Unlocked(AID id, int level, int questProgress) => id switch
    {
        AID.Maim => level >= 4,
        AID.Berserk => level >= 6,
        AID.Rampart => level >= 8,
        AID.Overpower => level >= 10,
        AID.Defiance => level >= 10,
        AID.ReleaseDefiance => level >= 10,
        AID.LowBlow => level >= 12,
        AID.Tomahawk => level >= 15 && questProgress > 0,
        AID.Provoke => level >= 15,
        AID.Interject => level >= 18,
        AID.Reprisal => level >= 22,
        AID.StormPath => level >= 26,
        AID.ThrillOfBattle => level >= 30 && questProgress > 1,
        AID.ArmsLength => level >= 32,
        AID.InnerBeast => level >= 35 && questProgress > 2,
        AID.Vengeance => level >= 38,
        AID.MythrilTempest => level >= 40 && questProgress > 3,
        AID.Holmgang => level >= 42,
        AID.SteelCyclone => level >= 45 && questProgress > 4,
        AID.Shirk => level >= 48,
        AID.StormEye => level >= 50,
        AID.Infuriate => level >= 50 && questProgress > 5,
        AID.FellCleave => level >= 54 && questProgress > 6,
        AID.RawIntuition => level >= 56 && questProgress > 7,
        AID.Equilibrium => level >= 58 && questProgress > 8,
        AID.Decimate => level >= 60 && questProgress > 9,
        AID.Onslaught => level >= 62,
        AID.Upheaval => level >= 64,
        AID.ShakeItOff => level >= 68,
        AID.InnerRelease => level >= 70 && questProgress > 10,
        AID.ChaoticCyclone => level >= 72,
        AID.NascentFlash => level >= 76,
        AID.InnerChaos => level >= 80,
        AID.Bloodwhetting => level >= 82,
        AID.Orogeny => level >= 86,
        AID.PrimalRend => level >= 90,
        _ => true
    };

    public static bool Unlocked(TraitID id, int level, int questProgress) => id switch
    {
        TraitID.TheBeastWithin => level >= 35 && questProgress > 2,
        TraitID.InnerBeastMastery => level >= 54 && questProgress > 6,
        TraitID.SteelCycloneMastery => level >= 60 && questProgress > 9,
        TraitID.EnhancedInfuriate => level >= 66,
        TraitID.BerserkMastery => level >= 70 && questProgress > 10,
        TraitID.NascentChaos => level >= 72,
        TraitID.MasteringTheBeast => level >= 74,
        TraitID.EnhancedShakeItOff => level >= 76,
        TraitID.EnhancedThrillOfBattle => level >= 78,
        TraitID.RawIntuitionMastery => level >= 82,
        TraitID.EnhancedNascentFlash => level >= 82,
        TraitID.EnhancedEquilibrium => level >= 84,
        TraitID.MeleeMastery => level >= 84,
        TraitID.EnhancedOnslaught => level >= 88,
        _ => true
    };

    private readonly WARConfig _config = Service.Config.Get<WARConfig>();

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.LandWaker, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.HeavySwing);
        d.RegisterSpell(AID.Maim);
        d.RegisterSpell(AID.Berserk);
        d.RegisterSpell(AID.Overpower);
        d.RegisterSpell(AID.Defiance);
        d.RegisterSpell(AID.ReleaseDefiance);
        d.RegisterSpell(AID.Tomahawk);
        d.RegisterSpell(AID.StormPath);
        d.RegisterSpell(AID.ThrillOfBattle);
        d.RegisterSpell(AID.InnerBeast);
        d.RegisterSpell(AID.Vengeance);
        d.RegisterSpell(AID.MythrilTempest);
        d.RegisterSpell(AID.Holmgang);
        d.RegisterSpell(AID.SteelCyclone);
        d.RegisterSpell(AID.StormEye);
        d.RegisterSpell(AID.Infuriate, maxCharges: 2);
        d.RegisterSpell(AID.FellCleave);
        d.RegisterSpell(AID.RawIntuition);
        d.RegisterSpell(AID.Equilibrium);
        d.RegisterSpell(AID.Decimate);
        d.RegisterSpell(AID.Onslaught, maxCharges: 3);
        d.RegisterSpell(AID.Upheaval);
        d.RegisterSpell(AID.ShakeItOff);
        d.RegisterSpell(AID.InnerRelease);
        d.RegisterSpell(AID.ChaoticCyclone);
        d.RegisterSpell(AID.NascentFlash);
        d.RegisterSpell(AID.InnerChaos);
        d.RegisterSpell(AID.Bloodwhetting);
        d.RegisterSpell(AID.Orogeny);
        d.RegisterSpell(AID.PrimalRend, instantAnimLock: 1.15f);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        //d.Spell(AID.Berserk)!.EffectDuration = 15;
        d.Spell(AID.Tomahawk)!.Condition = (ws, player, _, _) => !_config.ForbidEarlyTomahawk || player.InCombat || ws.Client.CountdownRemaining is null or <= 0.7f;
        d.Spell(AID.Holmgang)!.SmartTarget = (_, player, target, _) => _config.HolmgangSelf ? player : target;
        d.Spell(AID.Equilibrium)!.Condition = (_, player, _, _) => player.HPMP.CurHP < player.HPMP.MaxHP; // don't use equilibrium at full hp
        //d.Spell(AID.InnerRelease)!.EffectDuration = 15;
        d.Spell(AID.NascentFlash)!.SmartTarget = ActionDefinitions.SmartTargetCoTank;

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.Defiance)!.TransformAction = d.Spell(AID.ReleaseDefiance)!.TransformAction = () => ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseDefiance : AID.Defiance);
        //d.Spell(AID.InnerBeast)!.TransformAction = d.Spell(AID.FellCleave)!.TransformAction = d.Spell(AID.InnerChaos)!.TransformAction = () => ActionID.MakeSpell(_state.BestFellCleave);
        //d.Spell(AID.SteelCyclone)!.TransformAction = d.Spell(AID.Decimate)!.TransformAction = d.Spell(AID.ChaoticCyclone)!.TransformAction = () => ActionID.MakeSpell(_state.BestDecimate);
        //d.Spell(AID.Berserk)!.TransformAction = d.Spell(AID.InnerRelease)!.TransformAction = () => ActionID.MakeSpell(_state.BestInnerRelease);
        //d.Spell(AID.RawIntuition)!.TransformAction = d.Spell(AID.Bloodwhetting)!.TransformAction = () => ActionID.MakeSpell(_state.BestBloodwhetting);
        // combo replacement (TODO: don't think we actually care...)
        //d.Spell(AID.Maim)!.TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextMaimComboAction(ComboLastMove)) : null;
        //d.Spell(AID.StormEye)!.TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormEye)) : null;
        //d.Spell(AID.StormPath)!.TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormPath)) : null;
        //d.Spell(AID.MythrilTempest)!.TransformAction = config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;
    }
}