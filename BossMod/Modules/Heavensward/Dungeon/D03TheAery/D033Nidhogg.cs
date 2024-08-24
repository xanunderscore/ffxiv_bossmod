namespace BossMod.Heavensward.Dungeon.D03TheAery.D033Nidhogg;

public enum OID : uint
{
    _Gen_Nidhogg = 0x233C, // R0.500, x?, Helper type
    Boss = 0x39CA, // R12.000, x?
    _Gen_TheSablePrice = 0x39CB, // R1.000, x?
    _Gen_Liegedrake = 0x39CD, // R3.600, x?
    _Gen_Ahleh = 0x39CC, // R5.000, x?
}

public enum AID : uint
{
    _Ability_HotWing = 30194, // 39CA->self, 3.0s cast, single-target
    _Weaponskill_HotWing = 30195, // 233C->self, 3.5s cast, range 30 width 68 rect
    _Ability_ = 30201, // 39CA->self, no cast, single-target
    _AutoAttack_Attack = 870, // 39CA/39CD/39CC->player, no cast, single-target
    _Weaponskill_HorridRoar = 30200, // 233C->location, 4.0s cast, range 6 circle
    _Weaponskill_HorridRoar1 = 30202, // 233C->player, 5.0s cast, range 6 circle
    _Weaponskill_Cauterize = 30198, // 39CA->self, 5.0s cast, range 80 width 22 rect
    _Weaponskill_Touchdown = 30199, // 39CA->self, no cast, range 80 circle
    _Ability_TheSablePrice = 30203, // 39CA->self, 3.0s cast, single-target
    _Weaponskill_TheScarletPrice = 30205, // 39CA->player, 5.0s cast, single-target
    _Weaponskill_Roast = 30209, // 39CC->self, 4.0s cast, range 30 width 8 rect
    _Weaponskill_Massacre = 30207, // 39CA->location, 6.0s cast, range 80 circle
    _Weaponskill_HorridBlaze = 30224, // 39CA->players, 7.0s cast, range 6 circle
    _Ability_HotTail = 30196, // 39CA->self, 3.0s cast, single-target
    _Weaponskill_HotTail = 30197, // 233C->self, 3.5s cast, range 68 width 16 rect
    _Weaponskill_DeafeningBellow = 30206, // 39CA->self, 5.0s cast, range 80 circle
    _Weaponskill_SableWeave = 30204, // 39CB->player, 15.0s cast, single-target
}

public enum SID : uint
{
    Fetters = 3324
}

class HorridRoar2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_HorridRoar1), 6);
class HorridRoar1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HorridRoar), 6);
class HorridBlaze(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_HorridBlaze), 6, minStackSize: 4);

class HotWing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HotWing), new AOEShapeRect(30, 34, -4));
class HotTail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HotTail), new AOEShapeRect(34, 8, 34));
class Cauterize(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Cauterize), new AOEShapeRect(80, 11));
class SablePrice(BossModule module) : Components.Adds(module, 0x39CB)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.OID == 0x39CB)
                h.Priority = 5;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveActors.Any() && actor.FindStatus(SID.Fetters) == null)
            hints.Add("Attack add!", false);
    }
}
class ScarletPrice(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_TheScarletPrice));
class Minions(BossModule module) : Components.AddsMulti(module, [0x39CD, 0x39CC]);
class Massacre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Massacre));
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DeafeningBellow));

class NidhoggStates : StateMachineBuilder
{
    public NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotWing>()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<SablePrice>()
            .ActivateOnEnter<ScarletPrice>()
            .ActivateOnEnter<Minions>()
            .ActivateOnEnter<Massacre>()
            .ActivateOnEnter<HorridRoar1>()
            .ActivateOnEnter<HorridBlaze>()
            .ActivateOnEnter<HorridRoar2>()
            .ActivateOnEnter<DeafeningBellow>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3458)]
public class Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, -267), new ArenaBoundsCircle(20));

