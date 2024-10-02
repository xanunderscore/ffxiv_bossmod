namespace BossMod.Stormblood.Quest.TheOrphansAndTheBrokenBlade;

public enum OID : uint
{
    Boss = 0x1C5E,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Ability_Darkside = 9191, // Boss->self, no cast, single-target
    _AutoAttack_Attack = 870, // Boss->player/1C5C, no cast, single-target
    _Weaponskill_HardSlash = 9192, // Boss->player/1C5C, no cast, single-target
    _Weaponskill_SpinningSlash = 9193, // Boss->player/1C5C, no cast, single-target
    _Ability_Quickstep = 4743, // Boss->location, no cast, ???
    _Weaponskill_Insurgency = 8463, // Boss->self, 2.0s cast, single-target
    _Spell_Ruin = 1874, // 1C61->player, 1.0s cast, single-target
    _Weaponskill_PowerSlash = 9194, // Boss->player/1C5C, no cast, single-target
    _Spell_ShadowOfDeath = 8458, // Boss->self, no cast, single-target
    _Spell_ShadowOfDeath1 = 8459, // 1C5F->location, 3.0s cast, range 5 circle
    _Spell_DarkChain = 8460, // Boss->1C5D, 3.0s cast, single-target
    _Spell_Execution = 8461, // 1C60->1C5D, 15.0s cast, single-target
    _Ability_ForwardKick = 8456, // Boss->1C5C, no cast, single-target
    _Spell_HeadsmansDelight = 8457, // Boss->1C5C, 5.0s cast, range 5 circle
    _Spell_HeadmansDelight1 = 9299, // 1C5F->1C5C, no cast, single-target
    _Weaponskill_SpiralHell = 8453, // 1C5F->self, 3.0s cast, range 40+R width 4 rect
    _Weaponskill_SpiralHell1 = 8452, // Boss->self, 3.0s cast, single-target
    _Spell_HeadmansDelight = 9298, // 1C5F->player/1C5C, no cast, single-target
}

class SpiralHell(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SpiralHell), new AOEShapeRect(40, 2));
class HeadsmansDelight(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_HeadsmansDelight && WorldState.Actors.Find(spell.TargetID) is Actor tar)
            Stacks.Add(new(tar, 5, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_HeadmansDelight)
            Stacks.Clear();
    }
}
class ShadowOfDeath(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ShadowOfDeath1), 5);
class DarkChain(BossModule module) : Components.Adds(module, 0x1C60)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x1C60, 5);
    }
}

class OmpagneDeepblackStates : StateMachineBuilder
{
    public OmpagneDeepblackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowOfDeath>()
            .ActivateOnEnter<DarkChain>()
            .ActivateOnEnter<HeadsmansDelight>()
            .ActivateOnEnter<SpiralHell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68453, NameID = 6300)]
public class OmpagneDeepblack(WorldState ws, Actor primary) : BossModule(ws, primary, new(-166.8f, 290), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
