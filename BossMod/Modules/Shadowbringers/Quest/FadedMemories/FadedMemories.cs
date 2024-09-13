namespace BossMod.Shadowbringers.Quest.FadedMemories;

public enum OID : uint
{
    KingThordan = 0x2F1D,
    FlameGeneralAldynn = 0x2F1E,
    Nidhogg = 0x2F21,
    Zenos = 0x2F28,
    Ardbert = 0x2F2E,
    Helper = 0x233C,
}

public enum AID : uint
{
    // raubahn
    FlamingTizona = 21094, // player->location, 4.0s cast, range 6 circle

    // thordan
    TheDragonsGaze = 21090, // 2F1D->self, 4.0s cast, range 80 circle

    // nidhogg
    HighJump = 21299, // player->self, 4.0s cast, range 8 circle
    Geirskogul = 21098, // 2F22/2F21->self, 4.0s cast, range 62 width 8 rect

    // zenos
    _Weaponskill_ = 21119, // Zenos->self, no cast, single-target
    _Weaponskill_EntropicFlame = 21116, // 2F29->self, 5.0s cast, single-target
    _Spell_EntropicFlame = 21117, // Helper->self, 5.0s cast, range 50 width 8 rect
    _Weaponskill_VeinSplitter = 21118, // 2F29->self, 5.0s cast, range 10 circle

    // ardbert
    Overcome = 21126, // Ardbert->self, 2.5s cast, range 8 120-degree cone
    Skydrive = 21127, // Ardbert->self, 2.5s cast, range 5 circle
    SkyHighDriveCCW = 21138, // Ardbert->self, 4.5s cast, single-target
    SkyHighDriveCW = 21139, // Ardbert->self, 4.5s cast, single-target
    _Weaponskill_SkyHighDrive1 = 21140, // 233C->self, 5.0s cast, range 40 width 8 rect
    _Weaponskill_SkyHighDrive2 = 21141, // 233C->self, no cast, range 40 width 8 rect
    _Weaponskill_AvalancheAxe1 = 21145, // 233C->self, 4.0s cast, range 10 circle
    _Weaponskill_AvalancheAxe2 = 21144, // 233C->self, 7.0s cast, range 10 circle
    _Weaponskill_AvalancheAxe3 = 21143, // 233C->self, 10.0s cast, range 10 circle
    _Weaponskill_OvercomeAllOdds = 21130, // 233C->self, 2.5s cast, range 60 30-degree cone
    _Weaponskill_Soulflash1 = 21136, // 233C->self, 4.0s cast, range 4 circle
    _Weaponskill_EtesianAxe1 = 21147, // 233C->self, 6.5s cast, range 80 circle
    _Weaponskill_Soulflash2 = 21137, // 233C->self, 4.0s cast, range 8 circle
    _Weaponskill_Groundbreaker1 = 21563, // 233C->self, 5.0s cast, range 6 circle
    _Weaponskill_Groundbreaker2 = 21151, // 233C->self, no cast, range 6 circle
    _Weaponskill_Groundbreaker4 = 21153, // 233C->self, 6.0s cast, range 40 90-degree cone
    _Weaponskill_Groundbreaker6 = 21157, // 233C->self, 6.0s cast, range 5-20 donut
    _Weaponskill_Groundbreaker8 = 21155, // 233C->self, 6.0s cast, range 15 circle
}

public enum SID : uint
{
    Invincibility = 671
}
