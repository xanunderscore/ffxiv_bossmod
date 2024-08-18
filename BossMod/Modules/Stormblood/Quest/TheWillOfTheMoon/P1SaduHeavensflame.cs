namespace BossMod.Modules.Stormblood.Quest.TheWillOfTheMoonP1;

public enum OID : uint
{
    Boss = 0x24A0,
    Helper = 0x233C,
    KhunShavar = 0x252F, // R1.820, x0 (spawn during fight)
}

public enum AID : uint
{
    DispellingWind = 13223, // Boss->self, 3.0s cast, range 40+R width 8 rect
    Epigraph = 13225, // 252D->self, 3.0s cast, range 45+R width 8 rect
    WhisperOfLivesPast = 13226, // 252E->self, 3.5s cast, range -12 donut
    AncientBlizzard = 13227, // 252F->self, 3.0s cast, range 40+R 45-degree cone
    Tornado = 13228, // 252F->location, 5.0s cast, range 6 circle
    Epigraph2 = 13222, // 2530->self, 3.0s cast, range 45+R width 8 rect
}

public enum SID : uint
{
    Invincibility = 775, // none->Boss, extra=0x0
}

class DispellingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DispellingWind), new AOEShapeRect(40, 4));
class Epigraph(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Epigraph), new AOEShapeRect(45, 4));
class Whisper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhisperOfLivesPast), new AOEShapeDonut(6, 12));
class Blizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(40, 22.5f.Degrees()));
class Tornado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Tornado), 6);
class Epigraph1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Epigraph2), new AOEShapeRect(45, 4));

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
            if (e.Actor.FindStatus(SID.Invincibility) != null)
                e.Priority = -1;

            // they do very little damage and sadu will raise them after a short delay, no point in attacking
            if ((OID)e.Actor.OID == OID.KhunShavar)
                e.Priority = -1;
        }
    }
}

