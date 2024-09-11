namespace BossMod.Shadowbringers.Quest.AFeastOfLies;

public enum OID : uint
{
    Boss = 0x295A,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 6497, // Boss->player, no cast, single-target
    _Ability_ = 17115, // Boss->location, no cast, single-target
    _Weaponskill_UnceremoniousBeheading = 16274, // Boss->self, 4.0s cast, range 10 circle
    _Weaponskill_KatunCycle = 16275, // Boss->self, 4.0s cast, range 5-40 donut
    _Weaponskill_MercilessRight = 16278, // Boss->self, 4.0s cast, single-target
    _Weaponskill_MercilessRight1 = 16283, // 29FB->self, 3.8s cast, range 40 120-degree cone
    _Weaponskill_MercilessRight2 = 16284, // 29FE->self, 4.2s cast, range 40 120-degree cone
    _Weaponskill_Evisceration = 16277, // Boss->self, 4.5s cast, range 40 120-degree cone
    _Spell_HotPursuit = 16291, // Boss->self, 2.5s cast, single-target
    _Spell_HotPursuit1 = 16285, // 29E6->location, 3.0s cast, range 5 circle
    _Spell_NexusOfThunder = 16280, // Boss->self, 2.5s cast, single-target
    _Spell_NexusOfThunder1 = 16276, // 29E6->self, 4.3s cast, range 45 width 5 rect
    _Ability_LivingFlame = 16294, // Boss->self, 3.0s cast, single-target
    _Spell_Spiritcall = 16292, // Boss->self, 3.0s cast, range 40 circle
    _Spell_Burn = 16290, // 29C2->self, 4.5s cast, range 8 circle
    _Ability_RisingThunder = 16293, // Boss->self, 3.0s cast, single-target
    _Spell_Electrocution = 16286, // 295B->self, 10.0s cast, range 6 circle
    _Spell_ShatteredSky = 17191, // Boss->self, 4.0s cast, single-target
    _Spell_ShatteredSky1 = 16282, // 29E6->self, 0.5s cast, range 40 circle
    _Spell_NexusOfThunder2 = 16296, // 29E6->self, 6.3s cast, range 45 width 5 rect
    _Weaponskill_MercilessLeft = 16279, // Boss->self, 4.0s cast, single-target
    _Weaponskill_MercilessLeft1 = 16298, // 29FC->self, 3.8s cast, range 40 120-degree cone
    _Weaponskill_MercilessLeft2 = 16297, // 29FD->self, 4.2s cast, range 40 120-degree cone
}

class UnceremoniousBeheading(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UnceremoniousBeheading), new AOEShapeCircle(10));
class KatunCycle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_KatunCycle), new AOEShapeDonut(5, 40));
class MercilessRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessRight1), new AOEShapeCone(40, 60.Degrees()));
class MercilessRight1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessRight2), new AOEShapeCone(40, 60.Degrees()));
class MercilessLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessLeft1), new AOEShapeCone(40, 60.Degrees()));
class MercilessLeft1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MercilessLeft2), new AOEShapeCone(40, 60.Degrees()));
class Evisceration(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Evisceration), new AOEShapeCone(40, 60.Degrees()));
class HotPursuit(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_HotPursuit1), 5);
class NexusOfThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_NexusOfThunder1), new AOEShapeRect(45, 2.5f));
class NexusOfThunder1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_NexusOfThunder2), new AOEShapeRect(45, 2.5f));
class Burn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Burn), new AOEShapeCircle(8), maxCasts: 5);
class Spiritcall(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Spell_Spiritcall), 20, stopAtWall: true);

class Electrocution(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Electrocution), new AOEShapeCircle(6))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 12)
        {
            var enemy = hints.PotentialTargets.Where(x => x.Actor.OID == 0x295B).MinBy(e => actor.DistanceToHitbox(e.Actor));
            foreach (var e in hints.PotentialTargets)
                e.Priority = e == enemy ? 1 : 0;
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

class SerpentHead(BossModule module) : Components.Adds(module, 0x29E8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveActors.Any())
            foreach (var e in hints.PotentialTargets)
                e.Priority = e.Actor.OID == 0x29E8 ? 1 : 0;
    }
}

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UnceremoniousBeheading>()
            .ActivateOnEnter<KatunCycle>()
            .ActivateOnEnter<MercilessRight>()
            .ActivateOnEnter<MercilessRight1>()
            .ActivateOnEnter<MercilessLeft>()
            .ActivateOnEnter<MercilessLeft1>()
            .ActivateOnEnter<Evisceration>()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<NexusOfThunder>()
            .ActivateOnEnter<NexusOfThunder1>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<Spiritcall>()
            .ActivateOnEnter<SerpentHead>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69167, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 18), new ArenaBoundsCircle(15));

