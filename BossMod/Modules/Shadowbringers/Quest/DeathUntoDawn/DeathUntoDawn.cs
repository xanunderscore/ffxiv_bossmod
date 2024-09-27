namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;

public enum OID : uint
{
    TelotekGamma = 0x3376,
    LunarFetters = 0x3218,
    LunarOdin = 0x3200,
    LunarRavana = 0x3201,
    MoonGana = 0x3219,
    SpiritGana = 0x321A,
    RavanasWill = 0x321B,
    LunarIfrit = 0x3202,
    InfernalNail = 0x3205,
}

public enum AID : uint
{
    _Weaponskill_BallisticMissile = 24842, // TelotekGamma->self, 4.0s cast, single-target
    _Weaponskill_AntiPersonnelMissile = 24845, // 233C->player/321D, 5.0s cast, range 6 circle
    _Weaponskill_MRVMissile = 24843, // 233C->location, 8.0s cast, range 12 circle

    _Spell_Einherjar = 24036, // LunarOdin->self, 5.0s cast, range 40 circle
    _Weaponskill_LeadWeight = 24024, // LunarOdin->self, 4.0s cast, single-target
    _Weaponskill_LunarGungnir = 24025, // LunarOdin->31EC, 12.0s cast, range 6 circle
    _Weaponskill_Gungnir = 24027, // LunarOdin->self, 4.0s cast, single-target
    _Weaponskill_Gungnir1 = 24028, // 321C->self, 9.3s cast, single-target
    _Weaponskill_Gungnir2 = 24698, // 233C->self, 10.0s cast, range 10 circle
    _Weaponskill_Gagnrath = 24030, // 321C->self, 3.0s cast, range 50 width 4 rect
    _Weaponskill_Gungnir3 = 24029, // 321C->self, no cast, range 10 circle

    _Weaponskill_LeftZantetsuken = 24033, // LunarOdin->self, no cast, single-target
    _Weaponskill_LeftZantetsuken1 = 24034, // LunarOdin->self, 4.0s cast, range 70 width 39 rect

    _Weaponskill_RightZantetsuken = 24031, // LunarOdin->self, no cast, single-target
    _Weaponskill_RightZantetsuken1 = 24032, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
    _Weaponskill_LunarGungnir1 = 24026, // LunarOdin->2E2E, 25.0s cast, range 6 circle

    _Ability_Explosion = 24046, // 3204->self, 5.0s cast, range 80 width 10 cross

    _Weaponskill_VulcanBurst = 24064, // LunarIfrit->self, no cast, range 20 circle
    _Weaponskill_RadiantPlume = 24056, // LunarIfrit->self, 2.0s cast, single-target
    _Weaponskill_RadiantPlume1 = 24057, // 233C->self, 7.0s cast, range 8 circle
    _Weaponskill_ = 24053, // LunarIfrit->self, no cast, single-target
    _Weaponskill_Hellfire = 24058, // LunarIfrit->self, 36.0s cast, range 40 circle
    _Weaponskill_CrimsonCyclone = 24054, // 3203->self, 4.5s cast, range 49 width 18 rect
}

public enum SID : uint
{
    Invincibility = 325, // Lunar Ravana
}

class DUDStates : StateMachineBuilder
{
    public DUDStates(BossModule module) : base(module)
    {
        bool DutyEnd() => Module.WorldState.CurrentCFCID != 780;

        ushort GetRPParam() => (ushort)((Module.Raid.Player()?.FindStatus(Roleplay.SID.RolePlaying)?.Extra ?? 0) & 0xFF);

        // bool P1End() => GetRPParam() == UriangerAI.StatusParam || P2End();
        // bool P2End() => GetRPParam() == GrahaAI.StatusParam || P3End();
        bool P3End() => Module.Enemies(OID.LunarIfrit).Any(x => x.IsTargetable) || P4End();
        bool P4End() => DutyEnd();

        TrivialPhase(4)
            .ActivateOnEnter<IfritHints>()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<CrimsonCyclone>()
            // .ActivateOnEnter<Explosion>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(0, 0);
            })
            .Raw.Update = P4End;
    }
}

/*
[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 780, PrimaryActorOID = 0x321D)]
public class DUD(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -180), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
*/
