namespace BossMod.NIN;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Chimatsuri = 4243, // LB3, 4.5s cast, range 8, single-target, targets=Hostile, animLock=3.700s?
    SpinningEdge = 2240, // L1, instant, GCD, range 3, single-target, targets=Hostile
    ShadeShift = 2241, // L2, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    GustSlash = 2242, // L4, instant, GCD, range 3, single-target, targets=Hostile
    Hide = 2245, // L10, instant, 20.0s CD (group 2), range 0, single-target, targets=Self
    ThrowingDagger = 2247, // L15, instant, GCD, range 20, single-target, targets=Hostile
    Mug = 2248, // L15, instant, 120.0s CD (group 21), range 3, single-target, targets=Hostile
    TrickAttack = 2258, // L18, instant, 60.0s CD (group 8), range 3, single-target, targets=Hostile
    AeolianEdge = 2255, // L26, instant, GCD, range 3, single-target, targets=Hostile
    Ninjutsu = 2260, // L30, instant, GCD, range 0, single-target, targets=Self
    Ten = 2259, // L30, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350
    Chi = 2261, // L35, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350s?
    Jin = 2263, // L45, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350
    FumaShuriken = 2265, // L30, instant, GCD, range 25, single-target, targets=Hostile
    Katon = 2266, // L35, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    Raiton = 2267, // L35, instant, GCD, range 20, single-target, targets=Hostile
    Hyoton = 2268, // L45, instant, GCD, range 25, single-target, targets=Hostile
    Huton = 2269, // L45, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    Doton = 2270, // L45, instant, GCD, range 0, ???, targets=Self
    Suiton = 2271, // L45, instant, GCD, range 20, single-target, targets=Hostile
    RabbitMedium = 2272, // L30, instant, GCD, range 0, single-target, targets=Self
    DeathBlossom = 2254, // L38, instant, GCD, range 0, AOE 5 circle, targets=Self
    Assassinate = 2246, // L40, instant, 60.0s CD (group 9), range 3, single-target, targets=Hostile
    Shukuchi = 2262, // L40, instant, 60.0s CD (group 15/70), range 20, ???, targets=Area, animLock=0.800
    TenCombo = 18805, // L30, instant, GCD, range 0, single-target, targets=Self, animLock=0.350
    ChiCombo = 18806, // L35, instant, GCD, range 0, single-target, targets=Self, animLock=0.350
    JinCombo = 18807, // L45, instant, GCD, range 0, single-target, targets=Self, animLock=0.350
    FumaShurikenTen = 18873, // L30, instant, GCD, range 25, single-target, targets=Hostile
    FumaShurikenChi = 18874, // L30, instant, GCD, range 25, single-target, targets=Hostile
    FumaShurikenJin = 18875, // L30, instant, GCD, range 25, single-target, targets=Hostile
    TCJKaton = 18876, // L35, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    TCJRaiton = 18877, // L35, instant, GCD, range 20, single-target, targets=Hostile
    TCJHyoton = 18878, // L45, instant, GCD, range 25, single-target, targets=Hostile
    TCJHuton = 18879, // L45, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    TCJDoton = 18880, // L45, instant, GCD, range 0, ???, targets=Self
    TCJSuiton = 18881, // L45, instant, GCD, range 20, single-target, targets=Hostile
    Kassatsu = 2264, // L50, instant, 60.0s CD (group 10), range 0, single-target, targets=Self
    HakkeMujinsatsu = 16488, // L52, instant, GCD, range 0, AOE 5 circle, targets=Self
    ArmorCrush = 3563, // L54, instant, GCD, range 3, single-target, targets=Hostile
    DreamWithinADream = 3566, // L56, instant, 60.0s CD (group 11), range 3, single-target, targets=Hostile
    HellfrogMedium = 7401, // L62, instant, 1.0s CD (group 0), range 25, AOE 6 circle, targets=Hostile
    Dokumori = 36957, // L66, instant, 120.0s CD (group 21), range 3, single-target, targets=Hostile
    Bhavacakra = 7402, // L68, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    TenChiJin = 7403, // L70, instant, 120.0s CD (group 19), range 0, single-target, targets=Self
    Meisui = 16489, // L72, instant, 120.0s CD (group 18), range 0, single-target, targets=Self
    HyoshoRanryu = 16492, // L76, instant, GCD, range 25, single-target, targets=Hostile
    GokaMekkyaku = 16491, // L76, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    Bunshin = 16493, // L80, instant, 90.0s CD (group 14), range 0, single-target, targets=Self
    PhantomKamaitachi = 25774, // L82, instant, GCD, range 20, single-target, targets=Hostile
    HollowNozuchi = 25776, // L86, instant, range 100, AOE 5 circle, targets=Self/Area
    FleetingRaiju = 25778, // L90, instant, GCD, range 3, single-target, targets=Hostile
    ForkedRaiju = 25777, // L90, instant, GCD, range 20, single-target, targets=Hostile
    KunaisBane = 36958, // L92, instant, 60.0s CD (group 8), range 3, AOE 5 circle, targets=Hostile, animLock=???
    DeathfrogMedium = 36959, // L96, instant, 1.0s CD (group 0), range 25, AOE 6 circle, targets=Hostile, animLock=???
    ZeshoMeppo = 36960, // L96, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile, animLock=???
    TenriJindo = 36961, // L100, instant, 1.0s CD (group 1), range 20, AOE 5 circle, targets=Hostile, animLock=???

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=Self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 43), range 3, single-target, targets=Hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=Self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    AllFours = 90, // L14
    FleetOfFoot = 93, // L20
    IncreaseAttackSpeed = 584, // L45
    AdeptAssassination = 515, // L56
    Shukiho = 165, // L62
    EnhancedShukuchi = 166, // L64
    MugMastery = 585, // L66
    EnhancedShukuchiII = 279, // L74
    MeleeMastery = 516, // L74
    EnhancedKassatsu = 250, // L76
    ShukihoII = 280, // L78
    MeleeMasteryII = 522, // L84
    ShukihoIII = 439, // L84
    EnhancedMeisui = 440, // L88
    EnhancedRaiton = 441, // L90
    TrickAttackMastery = 586, // L92
    EnhancedSecondWind = 642, // L94
    MeleeMasteryIII = 661, // L94
    EnhancedDokumori = 587, // L96
    EnhancedFeint = 641, // L98
    EnhancedTenChiJin = 588, // L100
}

public enum SID : uint
{
    None = 0,
    ShadeShift = 488, // applied by Shade Shift to self
    Hidden = 614, // applied by Hide to self
    Bloodbath = 84, // applied by Bloodbath to self
    TrickAttack = 3254, // applied by Trick Attack to target
    Feint = 1195, // applied by Feint to target
    TenChiJin = 1186, // applied by Fuma Shuriken, Raiton, Ten Chi Jin to self
    Mudra = 496, // applied by Ten, Ten, Chi, Jin, Jin to self
    ArmsLength = 1209, // applied by Arm's Length to self
    RaijuReady = 2690, // applied by Raiton, Raiton to self
    ShadowWalker = 3848, // applied by Suiton, Huton, Suiton to self
    Kassatsu = 497, // applied by Kassatsu to self
    TrueNorth = 1250, // applied by True North to self
    Dokumori = 3849, // applied by Dokumori to target
    Meisui = 2689, // applied by Meisui to self
    Bunshin = 1954, // applied by Bunshin to self
    PhantomKamaitachiReady = 2723, // applied by Bunshin to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Chimatsuri, castAnimLock: 3.70f); // animLock=3.700s?
        d.RegisterSpell(AID.SpinningEdge);
        d.RegisterSpell(AID.ShadeShift);
        d.RegisterSpell(AID.GustSlash);
        d.RegisterSpell(AID.Hide);
        d.RegisterSpell(AID.ThrowingDagger);
        d.RegisterSpell(AID.Mug);
        d.RegisterSpell(AID.TrickAttack);
        d.RegisterSpell(AID.AeolianEdge);
        d.RegisterSpell(AID.FumaShuriken);
        d.RegisterSpell(AID.Ninjutsu);
        d.RegisterSpell(AID.FumaShurikenJin);
        d.RegisterSpell(AID.FumaShurikenChi);
        d.RegisterSpell(AID.FumaShurikenTen);
        d.RegisterSpell(AID.Ten, maxCharges: 2, instantAnimLock: 0.35f); // animLock=0.350
        d.RegisterSpell(AID.RabbitMedium);
        d.RegisterSpell(AID.TenCombo, instantAnimLock: 0.35f); // animLock=0.350
        d.RegisterSpell(AID.TCJRaiton);
        d.RegisterSpell(AID.ChiCombo, instantAnimLock: 0.35f); // animLock=0.350
        d.RegisterSpell(AID.Raiton);
        d.RegisterSpell(AID.TCJKaton);
        d.RegisterSpell(AID.Chi, maxCharges: 2, instantAnimLock: 0.35f); // animLock=0.350s?
        d.RegisterSpell(AID.Katon);
        d.RegisterSpell(AID.DeathBlossom);
        d.RegisterSpell(AID.Assassinate);
        d.RegisterSpell(AID.Shukuchi, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.Jin, maxCharges: 2, instantAnimLock: 0.35f); // animLock=0.350
        d.RegisterSpell(AID.TCJSuiton);
        d.RegisterSpell(AID.TCJDoton);
        d.RegisterSpell(AID.TCJHuton);
        d.RegisterSpell(AID.TCJHyoton);
        d.RegisterSpell(AID.Hyoton);
        d.RegisterSpell(AID.Huton);
        d.RegisterSpell(AID.Suiton);
        d.RegisterSpell(AID.JinCombo, instantAnimLock: 0.35f); // animLock=0.350
        d.RegisterSpell(AID.Doton);
        d.RegisterSpell(AID.Kassatsu);
        d.RegisterSpell(AID.HakkeMujinsatsu);
        d.RegisterSpell(AID.ArmorCrush);
        d.RegisterSpell(AID.DreamWithinADream);
        d.RegisterSpell(AID.HellfrogMedium);
        d.RegisterSpell(AID.Dokumori);
        d.RegisterSpell(AID.Bhavacakra);
        d.RegisterSpell(AID.TenChiJin);
        d.RegisterSpell(AID.Meisui);
        d.RegisterSpell(AID.HyoshoRanryu);
        d.RegisterSpell(AID.GokaMekkyaku);
        d.RegisterSpell(AID.Bunshin);
        d.RegisterSpell(AID.PhantomKamaitachi);
        d.RegisterSpell(AID.HollowNozuchi);
        d.RegisterSpell(AID.FleetingRaiju);
        d.RegisterSpell(AID.ForkedRaiju);
        d.RegisterSpell(AID.KunaisBane); // animLock=???
        d.RegisterSpell(AID.DeathfrogMedium); // animLock=???
        d.RegisterSpell(AID.ZeshoMeppo); // animLock=???
        d.RegisterSpell(AID.TenriJindo); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}

