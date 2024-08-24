
namespace BossMod.Heavensward.Quest.SpectacleP2;

public enum OID : uint
{
    Boss = 0x154E,
    Helper = 0x233C,
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

class FlameGeneralAldynn2States : StateMachineBuilder
{
    public FlameGeneralAldynn2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<TheCurse>()
            .ActivateOnEnter<Demoralize>()
            .ActivateOnEnter<Tizona>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 167, NameID = 4739)]
public class FlameGeneralAldynn2(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35.75f, -205.5f), new ArenaBoundsCircle(15));
