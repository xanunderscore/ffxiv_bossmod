namespace BossMod.Modules.Stormblood.Quest.TheWillOfTheMoonP1;

public enum OID : uint
{
    Boss = 0x24A0,
    Helper = 0x233C,
    _Gen_DotharliChuluu = 0x252D, // R1.800, x0 (spawn during fight)
    _Gen_TheShatteredStar = 0x252E, // R1.000, x0 (spawn during fight)
    _Gen_KhunShavar = 0x252F, // R1.820, x0 (spawn during fight)
    _Gen_StellarChuluu = 0x2530, // R1.800, x0 (spawn during fight)
    _Gen_StarfallCircle = 0x2531, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_DispellingWind = 13223, // Boss->self, 3.0s cast, range 40+R width 8 rect
    _Weaponskill_Epigraph = 13225, // 252D->self, 3.0s cast, range 45+R width 8 rect
    _Weaponskill_WhisperOfLivesPast = 13226, // 252E->self, 3.5s cast, range -12 donut
    _Ability_TheStoneSpeaks = 13247, // Boss->self, 3.0s cast, single-target
    _Ability_CircleOfLife = 13246, // Boss->252F, 4.0s cast, single-target
    _Spell_AncientBlizzard = 13227, // 252F->self, 3.0s cast, range 40+R 45-degree cone
    _Spell_Tornado = 13228, // 252F->location, 5.0s cast, range 6 circle
    _AutoAttack_Attack = 872, // 252F->player, no cast, single-target
    _Weaponskill_Epigraph1 = 13222, // 2530->self, 3.0s cast, range 45+R width 8 rect
    _Ability_ImmortalMettle = 13245, // Boss->self, no cast, single-target
    _Weaponskill_FallingDusk = 13224, // Boss->location, 60.0s cast, range 25 circle
}

public enum SID : uint
{
    _Gen_BurningSoul = 1613, // none->Boss, extra=0x3/0x2/0x1/0xA
    _Gen_Invincibility = 775, // none->Boss, extra=0x0
}

class DispellingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_DispellingWind), new AOEShapeRect(40, 4));
class Epigraph(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Epigraph), new AOEShapeRect(45, 4));
class Whisper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_WhisperOfLivesPast), new AOEShapeDonut(6, 12));
class Blizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_AncientBlizzard), new AOEShapeCone(40, 22.5f.Degrees()));
class Tornado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Tornado), 6);
class Epigraph1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Epigraph1), new AOEShapeRect(45, 4));

class SaduHeavensflameStates : StateMachineBuilder
{
    public SaduHeavensflameStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph>()
            .ActivateOnEnter<Whisper>()
            .ActivateOnEnter<Blizzard>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Epigraph1>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 609, NameID = 6152)]
public class SaduHeavensflame(WorldState ws, Actor primary) : BossModule(ws, primary, new(-223, 519), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.FindStatus(SID._Gen_Invincibility) != null)
                e.Priority = -1;

            // she will raise them, no point in killing
            if ((OID)e.Actor.OID == OID._Gen_KhunShavar)
                e.Priority = -1;
        }
    }
}

