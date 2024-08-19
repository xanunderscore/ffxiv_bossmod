namespace BossMod.Shadowbringers.Quest.TheOracleOfLight;

public enum OID : uint
{
    Boss = 0x299D,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player/2999, no cast, single-target
    _AutoAttack_Attack1 = 870, // 299E->player/2999, no cast, single-target
    _Ability_ = 3269, // 299E->self, no cast, single-target
    _Ability_1 = 4777, // 299E->self, no cast, single-target
    _Ability_SilentLightning = 18094, // Boss->299C, 1.0s cast, single-target
    _Weaponskill_Wrath = 17626, // 299E->self, no cast, range 100 circle
    _Ability_SilentLightning1 = 17867, // Boss->299A/299B/2999, 1.0s cast, single-target
    _Weaponskill_HotPursuit = 17619, // Boss->self, 3.0s cast, single-target
    _Weaponskill_HotPursuit1 = 17622, // 2AF0->location, 3.0s cast, range 5 circle
    _Weaponskill_NexusOfThunder = 17617, // Boss->self, 3.0s cast, single-target
    _Weaponskill_NexusOfThunder1 = 17621, // 2AF0->self, 7.0s cast, range 60+R width 5 rect
    _Weaponskill_NexusOfThunder2 = 17823, // 2AF0->self, 8.5s cast, range 60+R width 5 rect
    _Ability_HiddenCurrent = 17620, // Boss->location, no cast, ???
    _Spell_ShatteredSky = 17618, // Boss->self, 5.0s cast, single-target
    _Spell_ShatteredSky1 = 17623, // 2AF0->self, 6.0s cast, range 45 circle
    _Ability_LivingFlame = 18038, // Boss->self, 3.0s cast, single-target
    _Spell_Burn = 18035, // 2BE6->self, 4.5s cast, range 8 circle
    _Weaponskill_UnbridledWrath = 18036, // 299E->self, 5.5s cast, range 90 width 90 rect
}

class HotPursuit(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HotPursuit1), 5);
class NexusOfThunder1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NexusOfThunder1), new AOEShapeRect(60, 2.5f));
class NexusOfThunder2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NexusOfThunder2), new AOEShapeRect(60, 2.5f));
class Burn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Burn), new AOEShapeCircle(8), maxCasts: 8);
class UnbridledWrath(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_UnbridledWrath), 20, kind: Kind.DirForward, stopAtWall: true);

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<NexusOfThunder1>()
            .ActivateOnEnter<NexusOfThunder2>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<UnbridledWrath>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 662, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, new(126.75f, -311.25f), new ArenaBoundsCircle(20));

