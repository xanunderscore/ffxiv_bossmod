namespace BossMod.Dawntrail.Quest.TheProtectorAndTheDestroyer;
public enum OID : uint
{
    Boss = 0x4342,
    Helper = 0x233C,
    BossP2 = 0x4349,
    _Gen_EverkeepAerostat = 0x4344, // R2.300, x4
    _Gen_EverkeepSentryG10 = 0x4343, // R0.900, x2
    _Gen_EverkeepAerostat2 = 0x4345, // R2.300, x0 (spawn during fight)
    _Gen_EverkeepTurret = 0x4346, // R0.600, x0 (spawn during fight)
    _Gen_EverkeepSentryR10 = 0x4347, // R1.999, x0 (spawn during fight)
    _Gen_Gwyddrud = 0x3A5E, // R1.000, x24
    _Gen_ = 0x40B5, // R1.000, x1
    _Gen_BallOfLevin = 0x434A, // R1.500, x0 (spawn during fight)
    _Gen_SuperchargedLevin = 0x39C4, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player/4144, no cast, single-target
    _Ability_ = 38193, // Boss->location, no cast, single-target
    _Ability_FormationAlpha = 38194, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ThrownFlames = 38205, // 4345->self, 6.0s cast, range 8 circle
    _Weaponskill_BastionBreaker = 38198, // Helper->players/4144/4339, 6.0s cast, range 6 circle
    _Weaponskill_SearingSlash = 38197, // Boss->self, 6.0s cast, range 8 circle
    _Weaponskill_StormlitShockwave = 38202, // Boss->self, 5.0s cast, range 40 circle
    _Ability_FormationBeta = 38195, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Electrobeam = 38207, // 4346->self, 6.0s cast, range 40 width 4 rect
    _Weaponskill_HolyBlade = 38199, // Helper->4339, 6.0s cast, range 6 circle
    _Weaponskill_SteadfastWill = 38201, // Boss->4144, 5.0s cast, single-target
    _Ability_FormationGamma = 38196, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Rush = 38209, // 4347->location, 5.0s cast, width 5 rect charge
    _Weaponskill_ValorousAscension = 38203, // Boss->self, 8.0s cast, range 40 circle
    _Weaponskill_RendPower = 38200, // Helper->self, 4.5s cast, range 40 30-degree cone

    _Weaponskill_CracklingHowl = 38211, // BossP2->self, 4.3+0.7s cast, single-target
    _Weaponskill_CracklingHowl1 = 38212, // Helper->self, 5.0s cast, range 40 circle
    _Weaponskill_ = 38220, // Helper->self, 2.5s cast, range 20 180-degree cone
    VioletVoltage3 = 38214, // BossP2->self, 8.3+0.7s cast, single-target
    _Weaponskill_VioletVoltage1 = 38216, // BossP2->self, no cast, single-target
    _Weaponskill_VioletVoltage2 = 38221, // Helper->self, no cast, range 20 ?-degree cone
    _Weaponskill_VioletVoltage3 = 38217, // BossP2->self, no cast, single-target
    _Weaponskill_Gnaw = 38222, // BossP2->4146, 5.0s cast, single-target
    _Weaponskill_RollingThunder = 38223, // BossP2->self, 4.3+0.7s cast, single-target
    _Weaponskill_RollingThunder1 = 38224, // Helper->self, 5.0s cast, range 20 ?-degree cone
    VioletVoltage4 = 38215, // BossP2->self, 10.3+0.7s cast, single-target
    _Weaponskill_VioletVoltage5 = 38218, // BossP2->self, no cast, single-target
    _Weaponskill_VioletVoltage = 38219, // BossP2->self, no cast, single-target
    _Weaponskill_GatheringStorm = 38225, // BossP2->self, no cast, single-target
    _Weaponskill_GatheringSurge = 38243, // BossP2->self, no cast, single-target
    _Weaponskill_UntamedCurrent = 38232, // BossP2->self, 3.3+0.7s cast, range 40 circle
    _Weaponskill_UntamedCurrent1 = 38233, // 3A5E->location, 3.1s cast, range 5 circle
    _Weaponskill_UntamedCurrent2 = 19718, // 3A5E->location, 3.2s cast, range 5 circle
    _Weaponskill_UntamedCurrent3 = 19719, // 3A5E->location, 3.3s cast, range 5 circle
    _Weaponskill_UntamedCurrent4 = 19999, // 3A5E->location, 3.0s cast, range 5 circle
    _Weaponskill_UntamedCurrent5 = 38234, // 3A5E->location, 3.1s cast, range 5 circle
    _Weaponskill_1 = 19179, // 3A5E->location, 3.1s cast, range 5 circle
    _Weaponskill_UntamedCurrent6 = 19720, // 3A5E->location, 3.2s cast, range 5 circle
    _Weaponskill_UntamedCurrent7 = 19721, // 3A5E->location, 3.3s cast, range 5 circle
    _Weaponskill_2 = 19728, // 3A5E->location, 3.3s cast, range 5 circle
    _Weaponskill_UntamedCurrent8 = 19181, // Helper->players/4146/4339, 5.0s cast, range 5 circle
}

