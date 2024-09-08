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
    BossP2 = 0x154E,
    Tizona = 0x1552
}

public enum AID : uint
{
    _Weaponskill_HeavyShot = 97, // 1539/153D->1542/1544, no cast, single-target
    _AutoAttack_Attack = 873, // 153D/1539->1544/1542, no cast, single-target
    _Weaponskill_FastBlade = 9, // 153A/154F->154C/1549, no cast, single-target
    _AutoAttack_Attack1 = 870, // 1550/1538/153A/154F/Boss->player/154B/1541/154C/1549, no cast, single-target
    _AutoAttack_Attack2 = 871, // 1551/153C->154A/1543, no cast, single-target
    _Spell_Fire = 966, // 153B->1544, 1.0s cast, single-target
    _Weaponskill_TrueThrust = 722, // 153C->1543, no cast, single-target
    _Weaponskill_TrueThrust1 = 75, // 1551->154A, no cast, single-target
    _Weaponskill_HeavySwing = 31, // 1538/1550->1541/154B, no cast, single-target
    _Weaponskill_FastBlade1 = 5896, // Boss->player, no cast, single-target
    _Weaponskill_VorpalThrust = 78, // 1551->154A, no cast, single-target
    _Weaponskill_SavageBlade = 11, // 154F->1549, no cast, single-target
    _Weaponskill_SkullSunder = 35, // 1550->154B, no cast, single-target
    _Weaponskill_FlamingTizona = 5762, // Boss->self, 2.0s cast, single-target
    _Weaponskill_FlamingTizona1 = 5763, // D25->location, 3.0s cast, range 6 circle
    _Weaponskill_FullThrust = 84, // 1551->154A, no cast, single-target
    _Weaponskill_RageOfHalone = 21, // 154F->1549, no cast, single-target
    _Weaponskill_ButchersBlock = 47, // 1550->154B, no cast, single-target
    _Weaponskill_ = 5760, // Boss->self, no cast, single-target
    _Weaponskill_Demoralize = 5761, // D25->location, no cast, range 5 circle
    _Weaponskill_TheCurse = 5765, // D25->self, 3.0s cast, range 7+R ?-degree cone
    _Weaponskill_TheCurse1 = 5764, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TheBullOfAlaMhigo = 5759, // Boss->player, 2.0s cast, single-target
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

class FlamingTizona(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FlamingTizona1), 6);
class TheCurse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TheCurse), new AOEShapeDonutSector(2, 7, 90.Degrees()));

class Demoralize(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(0x1E9FA8).Where(e => e.EventState != 7));
class Tizona(BossModule module) : Components.Adds(module, (uint)OID.Tizona)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            if (e.Actor.OID == (uint)OID.Tizona)
                e.Priority = 5;
    }
}

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlagTarget>()
            .Raw.Update = () => module.Enemies(OID.BossP2).Any(x => x.IsTargetable);
        TrivialPhase(1)
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<TheCurse>()
            .ActivateOnEnter<Demoralize>()
            .ActivateOnEnter<Tizona>()
            .OnEnter(() =>
            {
                module.Arena.Center = new(-35.75f, -205.5f);
                module.Arena.Bounds = new ArenaBoundsCircle(15);
            })
            .Raw.Update = () => module.Enemies(OID.BossP2).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 167, NameID = 4739)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-38, -209), new ArenaBoundsRect(25, 25, 45.Degrees()))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = Math.Max(0, h.Priority);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}

