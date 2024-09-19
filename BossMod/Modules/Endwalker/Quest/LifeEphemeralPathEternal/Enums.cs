namespace BossMod.Endwalker.Quest.LifeEphemeralPathEternal;

public enum OID : uint
{
    Boss = 0x35C5,
    BossP2 = 0x35C6,
    Helper = 0x233C,
    MahaudFlamehand = 0x35C4, // R0.500, x1
    _Gen_ChiBomb = 0x35C7, // R1.000, x0 (spawn during fight)
    Lalah = 0x35C2,
    Loifa = 0x35C3,
    Mahaud = 0x361C,
    Ancel = 0x361D,
    _Gen_StrengthenedNoulith = 0x35C8, // R1.000, x0 (spawn during fight)
    _Gen_EnhancedNoulith = 0x3859, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->35C2, no cast, single-target
    _Weaponskill_ChiBlast = 26838, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ChiBlast1 = 26839, // Helper->self, 5.0s cast, range 100 circle
    _Weaponskill_ChiBomb = 26835, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ = 26868, // Boss->location, no cast, single-target
    _Weaponskill_Explosion = 26837, // 35C7->self, 5.0s cast, range 6 circle
    _Weaponskill_ArmOfTheScholar = 26836, // Boss->self, 5.0s cast, range 5 circle
    _Weaponskill_RawRockbreaker = 26832, // Boss->self, 5.0s cast, single-target
    _Weaponskill_RawRockbreaker1 = 26833, // Helper->self, 4.0s cast, range 10 circle
    _Weaponskill_RawRockbreaker2 = 26834, // Helper->self, 4.0s cast, range 10-20 donut
    _Weaponskill_DemifireII = 26842, // _Gen_MahaudFlamehand->Lalah, 8.0s cast, single-target
    _Weaponskill_Demiburst = 26843, // _Gen_MahaudFlamehand->self, 7.0s cast, single-target
    _Weaponskill_ElectrogeneticForce = 26844, // Helper->self, 8.0s cast, range 6 circle
    _Weaponskill_ElectrogeneticBlast = 26845, // Helper->self, 1.0s cast, range 80 circle
    _Weaponskill_DemifireIII = 26841, // _Gen_MahaudFlamehand->Lalah, 3.0s cast, single-target
    _Weaponskill_FourElements = 26846, // MahaudFlamehand->self, 8.0s cast, single-target
    _Weaponskill_ClassicalFire = 26847, // Helper->Lalah, 8.0s cast, range 6 circle
    _Weaponskill_ClassicalThunder = 26848, // Helper->player/Loifa/Lalah, 5.0s cast, range 6 circle
    _Weaponskill_ClassicalBlizzard = 26849, // Helper->location, 5.0s cast, range 6 circle
    _Weaponskill_ClassicalStone = 26850, // Helper->self, 9.0s cast, range 50 circle

    _Weaponskill_Nouliths = 26851, // BossP2->self, 5.0s cast, single-target
    _Weaponskill_AetherstreamTank = 26852, // 35C8->Lalah, no cast, range 50 width 4 rect
    _Weaponskill_AetherstreamPlayer = 26853, // 35C8->players/Loifa, no cast, range 50 width 4 rect
    _Weaponskill_Tracheostomy = 26854, // BossP2->self, 5.0s cast, range 10-20 donut
    _Weaponskill_RightScalpel = 26855, // BossP2->self, 5.0s cast, range 15 210-degree cone
    _Weaponskill_LeftScalpel = 26856, // BossP2->self, 5.0s cast, range 15 210-degree cone
    _Weaponskill_Laparotomy = 26857, // BossP2->self, 5.0s cast, range 15 120-degree cone
    _Weaponskill_Amputation = 26858, // BossP2->self, 7.0s cast, range 20 120-degree cone
    _Weaponskill_Hypothermia = 26861, // BossP2->self, 5.0s cast, range 50 circle
    _Weaponskill_Cryonics = 26860, // Helper->player, 8.0s cast, range 6 circle
    _Weaponskill_Cryonics1 = 26859, // BossP2->self, 8.0s cast, single-target
    _Weaponskill_Craniotomy = 28386, // BossP2->self, 8.0s cast, range 40 circle
    _Weaponskill_RightLeftScalpel = 26862, // BossP2->self, 7.0s cast, range 15 210-degree cone
    _Weaponskill_RightLeftScalpel1 = 26863, // BossP2->self, 3.0s cast, range 15 210-degree cone
    _Weaponskill_LeftRightScalpel = 26864, // BossP2->self, 7.0s cast, range 15 210-degree cone
    _Weaponskill_LeftRightScalpel1 = 26865, // BossP2->self, 3.0s cast, range 15 210-degree cone
    _Weaponskill_Frigotherapy = 26866, // BossP2->self, 5.0s cast, single-target
    _Weaponskill_Frigotherapy1 = 26867, // Helper->players/Mahaud/Loifa, 7.0s cast, range 5 circle
}

public enum IconID : uint
{
    Tankbuster = 230, // Lalah
    Noulith = 244, // player/Loifa
}

public enum TetherID : uint
{
    Noulith = 17, // _Gen_StrengthenedNoulith->Lalah/player/Loifa
    Craniotomy = 174, // _Gen_EnhancedNoulith->Lalah/Loifa/player/Mahaud/Ancel
}

public enum SID : uint
{
    Craniotomy = 2968, // none->player/Lalah/Mahaud/Ancel/Loifa, extra=0x0
    DownForTheCount = 1953, // none->player/Lalah/Mahaud/Ancel/Loifa, extra=0xEC7

}
