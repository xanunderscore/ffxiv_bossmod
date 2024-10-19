using BossMod.QuestBattle;

namespace BossMod.Shadowbringers.Quest.FullSteamAhead;

public enum OID : uint
{
    Boss = 0x295D,

    LightningVoidzone = 0x1E9685
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _AutoAttack_Attack1 = 870, // 295E->player, no cast, single-target
    _Spell_ShatteredSky = 16405, // Boss->self, 5.0s cast, single-target
    _Spell_ShatteredSky1 = 16429, // 233C->self, 6.0s cast, range 45 circle
    _Weaponskill_HotPursuit = 16406, // Boss->self, 3.0s cast, single-target
    _Weaponskill_HotPursuit1 = 16430, // 233C->location, 3.0s cast, range 5 circle
    _Weaponskill_NexusOfThunder = 16404, // Boss->self, 3.0s cast, single-target
    _Weaponskill_NexusOfThunder1 = 16427, // 233C->self, 7.0s cast, range 60+R width 5 rect
    _Weaponskill_Wrath = 16425, // 295E->self, no cast, range 100 circle
    _Weaponskill_CoiledLevin = 16424, // 295E->self, 3.0s cast, single-target
    _Weaponskill_CoiledLevin1 = 16428, // 233C->self, 7.0s cast, range 6 circle
    _Weaponskill_UnbridledWrath = 16426, // 295E->self, no cast, range 100 circle
    _Ability_HiddenCurrent = 16403, // Boss->location, no cast, ???
    _Ability_VeilOfGukumatz = 16423, // 2998->self, no cast, single-target
    _Ability_VeilOfGukumatz1 = 16422, // 295D->self, no cast, single-target
    _Ability_VeilOfGukumatz2 = 16402, // Boss->self, no cast, single-target
    _Weaponskill_UnceremoniousBeheading = 16412, // 295D->self, 3.5s cast, range 10 circle
    _Ability_HiddenCurrent1 = 16411, // 295D->location, no cast, ???
    _Weaponskill_MercilessLeft = 16415, // 295D->self, 4.0s cast, single-target
    _Weaponskill_MercilessLeft1 = 33202, // 233C->self, 4.0s cast, range 40 120-degree cone
    _Weaponskill_MercilessRight = 16431, // 233C->self, 4.0s cast, range 40 120-degree cone
    _Weaponskill_KatunCycle = 16413, // 295D->self, 5.5s cast, range 5-40 donut
    _Weaponskill_HotPursuit2 = 16410, // 295D->self, 3.0s cast, single-target
    _Weaponskill_AgelessSerpent = 16417, // 295D->self, no cast, single-target
    _Weaponskill_SerpentRising = 16433, // 295F->self, no cast, single-target
    _Weaponskill_Evisceration = 16419, // 295D->self, 2.0s cast, range 40 120-degree cone
    _Weaponskill_Spiritcall = 16420, // 295D->self, no cast, range 100 circle
    _Weaponskill_SnakingFlame = 16432, // 295F->player, 40.0s cast, width 4 rect charge
}

public enum SID : uint
{
    _Gen_RolePlaying = 1534, // none->player, extra=0x159
    _Gen_DownForTheCount = 783, // 295E/295D->player, extra=0xEC7
    _Gen_Burns = 250, // 233C->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // 295D/233C->player, extra=0x1/0x2/0x3/0x4
    _Gen_FadedOut = 1907, // none->player, extra=0xEC7
    Smackdown = 2068,
}

class KatunCycle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_KatunCycle), new AOEShapeDonut(5, 40));
class MercilessLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessLeft1), new AOEShapeCone(40, 60.Degrees()));
class MercilessRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessRight), new AOEShapeCone(40, 60.Degrees()));
class UnceremoniousBeheading(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UnceremoniousBeheading), new AOEShapeCircle(10));
class Evisceration(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Evisceration), new AOEShapeCone(40, 60.Degrees()));

class HotPursuit(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HotPursuit1), 5);
class NexusOfThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NexusOfThunder1), new AOEShapeRect(60, 2.5f));
class CoiledLevin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CoiledLevin1), new AOEShapeCircle(6));
class LightningVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.LightningVoidzone).Where(x => x.EventState != 7));

class ThancredAI(BossModule module) : Components.RotationModule<AutoThancred>(module);

class AutoThancred(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 3));

        if (World.Client.DutyActions[0].CurCharges > 0)
        {
            UseAction(World.Client.DutyActions[0].Action, primaryTarget);
            return;
        }

        if (primaryTarget == null)
            return;

        var distance = Player.DistanceToHitbox(primaryTarget);

        if (distance <= 3)
        {
            UseAction(Roleplay.AID.Smackdown, Player, -100);

            if (Player.FindStatus(SID.Smackdown) != null)
                UseAction(Roleplay.AID.RoughDivide, primaryTarget, -100);
        }

        if (Player.HPMP.CurHP * 2 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.SoothingPotion, Player, -100);

        switch (ComboAction)
        {
            case Roleplay.AID.BrutalShell:
                UseAction(Roleplay.AID.SolidBarrel, primaryTarget);
                break;
            case Roleplay.AID.KeenEdge:
                UseAction(Roleplay.AID.BrutalShell, primaryTarget);
                break;
            default:
                UseAction(Roleplay.AID.KeenEdge, primaryTarget);
                break;
        }
    }
}

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<ThancredAI>()
            .ActivateOnEnter<NexusOfThunder>()
            .ActivateOnEnter<CoiledLevin>()
            .ActivateOnEnter<LightningVoidzone>()
            .ActivateOnEnter<KatunCycle>()
            .ActivateOnEnter<MercilessLeft>()
            .ActivateOnEnter<MercilessRight>()
            .ActivateOnEnter<UnceremoniousBeheading>()
            .ActivateOnEnter<Evisceration>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69155, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-203, 395), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(0x295C), ArenaColor.Enemy);
    }

    protected override bool CheckPull() => true;
}
