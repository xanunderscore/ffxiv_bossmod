namespace BossMod.Heavensward.Quest.ASpectacleForTheAges;

public enum OID : uint
{
    Boss = 0x154D,
    Helper = 0x233C,
    _Gen_SerpentVeteran = 0x1540, // R0.500, x1
    _Gen_ = 0x1547, // R0.500-1.500, x1
    _Gen_SerpentCommanderHeuloix = 0x1551, // R0.500, x1
    _Gen_SerpentVeteran1 = 0x153D, // R0.500, x1
    _Gen_SerpentVeteran2 = 0x153C, // R0.500, x1
    _Gen_StormVeteran = 0x153E, // R0.500, x1 (spawn during fight)
    _Gen_StormVeteran1 = 0x1539, // R0.500, x1
    _Gen_StormVeteran2 = 0x1538, // R0.500, x1
    _Gen_FlameVeteran = 0x153B, // R0.500, x1
    _Gen_FlameVeteran1 = 0x153F, // R0.500, x1
    _Gen_StormCommanderRhiki = 0x1550, // R0.500, x1
    _Gen_PipinOfTheSteelHeart = 0x154F, // R0.500, x1 (spawn during fight)
    _Gen_FlameVeteran2 = 0x153A, // R0.500, x1 (spawn during fight)
}

public enum SID : uint
{
    _Gen_VulnerabilityDown = 350, // none->1546/_Gen_FlameVeteran2/1544/_Gen_FlameVeteran1/1543, extra=0x0
    _Gen_HawksEye = 3861, // 1544->1544, extra=0x0
    _Gen_Stun = 2, // 154B->_Gen_StormCommanderRhiki, extra=0x0
}

class FlagTarget(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            if (e.Actor.FindStatus(SID._Gen_VulnerabilityDown) != null)
                e.Priority = 5;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in WorldState.Actors.Where(e => !e.IsAlly && e.IsTargetable))
            if (e.FindStatus(SID._Gen_VulnerabilityDown) != null)
                Arena.AddCircle(e.Position, 1.5f, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (WorldState.Actors.Any(e => !e.IsAlly && e.IsTargetable && e.FindStatus(SID._Gen_VulnerabilityDown) != null))
            hints.Add("Attack tethered enemy!", false);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x1538, 0x1539, 0x153A, 0x153B, 0x153C, 0x153D, 0x153E, 0x153F, 0x1540, 0x154E, 0x154F, 0x1550, 0x1551])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            if (!e.Actor.IsAlly && e.Actor.IsTargetable)
                e.Priority = Math.Max(0, e.Priority);
    }
}

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<Adds>().ActivateOnEnter<FlagTarget>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 167, NameID = 4739)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void UpdateModule() => Arena.Center = Raid.Player()!.Position;
    protected override bool CheckPull() => PrimaryActor.InCombat;
}

