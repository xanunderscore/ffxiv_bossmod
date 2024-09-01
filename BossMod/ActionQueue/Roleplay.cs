namespace BossMod.Roleplay;

public enum AID : uint
{
    // magitek reaper in Fly Free, My Pretty
    MagitekCannon = 7619, // range 30 radius 6 ground targeted aoe
    PhotonStream = 7620, // range 10 width 4 rect aoe
    DiffractiveMagitekCannon = 7621, // range 30 radius 10 ground targeted aoe
    HighPoweredMagitekCannon = 7622, // range 42 width 8 rect aoe

    // Alphinaud - I forgot the name of this quest and it needs a module
    RuinIII = 11191,
    Physick = 11192,
    TriShackle = 11482,

    // Y'shtola - Will of the Moon (StB)
    StoneIVSeventhDawn = 13423,
    AeroIISeventhDawn = 13424,
    CureIISeventhDawn = 13425,
    Aetherwell = 13426,

    // Hien - Requiem for Heroes (StB)
    Kyokufu = 14840,
    Gofu = 19046,
    Yagetsu = 19047,
    Ajisai = 14841,
    HissatsuGyoten = 14842,
    SecondWind = 15375,

    // Nyelbert - Nyelbert's Lament (ShB)
    RonkanFire3 = 16574,
    RonkanBlizzard3 = 16575,
    RonkanThunder3 = 16576,
    RonkanFlare = 16577,
    FallingStar = 16578,

    // Thancred - Coming Clean (ShB)
    KeenEdge = 16434,
    BrutalShell = 16418,
    SolidBarrel = 16435,
    RoughDivide = 16804,
    Nebula = 17839,
    SoothingPotion = 16436,
    Smackdown = 17901,
    ShiningBlade = 16437,
    PerfectDeception = 16438,
    SouldeepInvisibility = 17291,
    LeapOfFaith = 16439,

    // Estinien - VOVDOC (ShB)
    DoomSpike = 18772,
    SonicThrust = 18773,
    CoerthanTorment = 18774,
    SkydragonDive = 18775,
    AquaVitae = 19218,
    // quest part 2
    AlaMorn = 18776,
    Drachenlance = 18777,
    HorridRoar = 18778,
    Stardiver = 18780,
    DragonshadowDive = 18781,

    // Alisaie - DUD
    Verfire = 20529,
    Veraero = 20530,
    Verstone = 20531,
    Verflare = 20532,
    CorpsACorps = 24917,
    EnchantedRiposte = 24918,
    EnchantedZwerchhau = 24919,
    EnchantedRedoublement = 24920,
    Displacement = 21496,
    Verholy = 21923,
    Scorch = 24831,
    CrimsonSavior = 20533,
    Fleche = 21494,
    ContreSixte = 21495,
    Vercure = 21497,

    // Urianger - DUD
    MaleficIII = 21498,
    Benefic = 21608,
    AspectedHelios = 21609,
    DestinyDrawn = 21499,
    LordOfCrowns = 21607,
    DestinysSleeve = 24066,
    TheScroll = 21610,
    FixedSign = 21611,

    // Graha - DUD
    FireIV = 21612,
    FireIV2 = 22502,
    FireIV3 = 22817,
    Foul = 21613,
    AllaganBlizzardIV = 21852,
    ThunderIV = 21884,
    CureII = 21886,
    MedicaII = 21888,
    Break = 21921
}

public enum TraitID : uint { }

public enum SID : uint
{
    RolePlaying = 1534,

    // Hien
    Ajisai = 1779,

    // Nyelbert
    Electrocution = 271,

    // thancred
    PerfectDeception = 1906,
    SouldeepInvisibility = 1956,

    // estinien VOVDOC phase 2
    StabWound = 1466,

    // urianger
    DestinyDrawn = 2571,

    // graha
    ThunderIV = 1210,
    Break = 2573,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.MagitekCannon);
        d.RegisterSpell(AID.PhotonStream);
        d.RegisterSpell(AID.DiffractiveMagitekCannon);
        d.RegisterSpell(AID.HighPoweredMagitekCannon);

        d.RegisterSpell(AID.RuinIII);
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.TriShackle);

        d.RegisterSpell(AID.StoneIVSeventhDawn);
        d.RegisterSpell(AID.AeroIISeventhDawn);
        d.RegisterSpell(AID.CureIISeventhDawn);
        d.RegisterSpell(AID.Aetherwell);

        d.RegisterSpell(AID.Kyokufu);
        d.RegisterSpell(AID.Gofu);
        d.RegisterSpell(AID.Yagetsu);
        d.RegisterSpell(AID.Ajisai);
        d.RegisterSpell(AID.HissatsuGyoten);
        d.RegisterSpell(AID.SecondWind);

        d.RegisterSpell(AID.RonkanFire3);
        d.RegisterSpell(AID.RonkanBlizzard3);
        d.RegisterSpell(AID.RonkanThunder3);
        d.RegisterSpell(AID.RonkanFlare);
        d.RegisterSpell(AID.FallingStar);

        d.RegisterSpell(AID.KeenEdge);
        d.RegisterSpell(AID.BrutalShell);
        d.RegisterSpell(AID.SolidBarrel);
        d.RegisterSpell(AID.RoughDivide);
        d.RegisterSpell(AID.Nebula);
        d.RegisterSpell(AID.SoothingPotion);
        d.RegisterSpell(AID.Smackdown);
        d.RegisterSpell(AID.PerfectDeception);
        d.RegisterSpell(AID.ShiningBlade);

        d.RegisterSpell(AID.DoomSpike);
        d.RegisterSpell(AID.SonicThrust);
        d.RegisterSpell(AID.CoerthanTorment);
        d.RegisterSpell(AID.SkydragonDive, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.AquaVitae, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.AlaMorn);
        d.RegisterSpell(AID.Drachenlance);
        d.RegisterSpell(AID.HorridRoar);
        d.RegisterSpell(AID.Stardiver, instantAnimLock: 1.5f);
        d.RegisterSpell(AID.DragonshadowDive);

        d.RegisterSpell(AID.Verfire);
        d.RegisterSpell(AID.Veraero);
        d.RegisterSpell(AID.Verstone);
        d.RegisterSpell(AID.Verflare);
        d.RegisterSpell(AID.CorpsACorps);
        d.RegisterSpell(AID.EnchantedRiposte);
        d.RegisterSpell(AID.EnchantedZwerchhau);
        d.RegisterSpell(AID.EnchantedRedoublement);
        d.RegisterSpell(AID.Displacement);
        d.RegisterSpell(AID.Verholy);
        d.RegisterSpell(AID.Scorch);
        d.RegisterSpell(AID.CrimsonSavior);
        d.RegisterSpell(AID.Fleche);
        d.RegisterSpell(AID.ContreSixte);
        d.RegisterSpell(AID.Vercure);

        d.RegisterSpell(AID.MaleficIII);
        d.RegisterSpell(AID.Benefic);
        d.RegisterSpell(AID.AspectedHelios);
        d.RegisterSpell(AID.DestinyDrawn);
        d.RegisterSpell(AID.LordOfCrowns);
        d.RegisterSpell(AID.DestinysSleeve);
        d.RegisterSpell(AID.TheScroll);
        d.RegisterSpell(AID.FixedSign);

        d.RegisterSpell(AID.FireIV);
        d.RegisterSpell(AID.FireIV2);
        d.RegisterSpell(AID.FireIV3);
        d.RegisterSpell(AID.Foul);
        d.RegisterSpell(AID.AllaganBlizzardIV);
        d.RegisterSpell(AID.ThunderIV);
        d.RegisterSpell(AID.CureII);
        d.RegisterSpell(AID.MedicaII);
        d.RegisterSpell(AID.Break);
    }

    public void Dispose() { }
}
