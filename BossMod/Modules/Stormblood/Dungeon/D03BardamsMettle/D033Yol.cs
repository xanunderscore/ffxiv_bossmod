namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D033Yol;

public enum OID : uint
{
    Boss = 0x1AA6,
    Helper = 0x233C,
    _Gen_Yol = 0x19A, // R0.500, x16, Helper type
    _Gen_YolFeather = 0x1AA8, // R0.500, x0 (spawn during fight)
    _Gen_CorpsecleanerEagle = 0x1AA7, // R2.520, x0 (spawn during fight)
    _Gen_RightWing = 0x1C0A, // R0.800, x0 (spawn during fight), Part type
    _Gen_LeftWing = 0x1C0B, // R0.800, x0 (spawn during fight), Part type
}

public enum AID : uint
{
    _Weaponskill_Feathercut = 7945, // Boss->player, no cast, single-target
    _Weaponskill_WindUnbound = 7946, // Boss->self, 3.0s cast, range 40+R circle
    _Weaponskill_Flutterfall = 7947, // Boss->self, 3.5s cast, single-target
    _Weaponskill_Pinion = 7953, // _Gen_YolFeather->self, 2.5s cast, range 40+R width 2 rect
    _Weaponskill_Flutterfall1 = 7948, // _Gen_Yol->players, no cast, range 6 circle
    _Ability_EyeOfTheFierce = 7949, // Boss->self, 4.5s cast, range 40+R circle
    _Weaponskill_FeatherSquall = 7950, // Boss->self, no cast, range 40+R width 6 rect
    _Weaponskill_Flutterfall2 = 7952, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Flutterfall3 = 7954, // _Gen_Yol->location, 2.5s cast, range 6 circle
    _Weaponskill_Wingbeat = 7951, // Boss->self, no cast, range 40+R ?-degree cone
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID._Gen_CorpsecleanerEagle, (uint)OID._Gen_RightWing, (uint)OID._Gen_LeftWing]);
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Ability_EyeOfTheFierce));
class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_WindUnbound));
class Pinion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Pinion), new AOEShapeRect(40.5f, 1));
class FlutterfallSpread(BossModule module) : Components.SpreadFromIcon(module, 23, ActionID.MakeSpell(AID._Weaponskill_Flutterfall1), 6, 5.5f);
class FlutterfallAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Flutterfall3), 6);

class YolStates : StateMachineBuilder
{
    public YolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<FlutterfallSpread>()
            .ActivateOnEnter<FlutterfallAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6155)]
public class Yol(WorldState ws, Actor primary) : BossModule(ws, primary, new(24, -475.5f), new ArenaBoundsCircle(20));

