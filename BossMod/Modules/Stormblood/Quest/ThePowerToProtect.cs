namespace BossMod.Stormblood.Quest.ThePowerToProtect;

public enum OID : uint
{
    Boss = 0x1BCB, // R5.400, x1
    _Gen_CorpseBrigadeKnuckledancer = 0x1C0C, // R0.500, x2 (spawn during fight)
    _Gen_CorpseBrigadeBowdancer = 0x1C0D, // R0.500, x2 (spawn during fight)
    _Gen_HeweraldIronaxe = 0x1C01, // R0.500, x1
    _Gen_CorpseBrigadeFiredancer = 0x1C00, // R0.500, x0 (spawn during fight)
    _Gen_CorpseBrigadeBowdancer1 = 0x1BFF, // R0.500, x0 (spawn during fight)
    _Gen_CorpseBrigadeKnuckledancer1 = 0x1BFE, // R0.500, x0 (spawn during fight)
    _Gen_CorpseBrigadeBarber = 0x1BFD, // R0.500, x0 (spawn during fight)
    _Gen_SalvagedSlasher = 0x1C1F, // R1.050, x0 (spawn during fight)
    _Gen_CorpseBrigadeVanguard = 0x1C02, // R2.000, x0 (spawn during fight)
    FireII = 0x1EA4C6,
}

public enum AID : uint
{
    _AutoAttack_ = 7351, // Boss->1C03, no cast, single-target
    _Weaponskill_MagitekCannon = 8347, // Boss->1C03, no cast, single-target
    _Weaponskill_IronTempest = 1003, // _Gen_HeweraldIronaxe->self, 3.5s cast, range 5+R circle
    _Ability_Foresight = 32, // _Gen_HeweraldIronaxe->self, no cast, single-target
    _Ability_Infusion = 8305, // _Gen_HeweraldIronaxe->player, no cast, single-target
    _Ability_BarbaricSurge = 963, // _Gen_HeweraldIronaxe->self, no cast, single-target
    _Spell_Thunder = 968, // _Gen_CorpseBrigadeFiredancer->player, 1.0s cast, single-target
    _Spell_Stone = 119, // _Gen_CorpseBrigadeBarber->player, 1.5s cast, single-target
    _Spell_Cure = 120, // _Gen_CorpseBrigadeBarber->_Gen_CorpseBrigadeBowdancer, 1.5s cast, single-target
    _Spell_FireII = 2175, // _Gen_CorpseBrigadeFiredancer->location, 2.5s cast, range 5 circle
    _Ability_Bloodbath = 34, // _Gen_HeweraldIronaxe->self, no cast, single-target
    _Weaponskill_DynamicSensoryJammer = 8342, // Boss->player, 3.0s cast, single-target
    _Weaponskill_Overpower = 720, // _Gen_HeweraldIronaxe->self, 2.5s cast, range 6+R 90-degree cone
    _Spell_Paralyze = 308, // _Gen_CorpseBrigadeFiredancer->player, 4.0s cast, single-target
    _Weaponskill_Rive = 1135, // _Gen_HeweraldIronaxe->self, 2.5s cast, range 30+R width 2 rect
    _Weaponskill_PhotonStream = 4710, // _Gen_CorpseBrigadeVanguard->self, no cast, range 10+R width 2 rect
    _Weaponskill_DiffractiveLaser = 8348, // Boss->location, 4.0s cast, range 5 circle
}

public enum SID : uint
{
    _Gen_ExtremeCaution = 1269, // Boss->player, extra=0x0

}

class ExtremeCaution(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}
class IronTempest(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_IronTempest), new AOEShapeCircle(5.5f));
class FireII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID._Spell_FireII), m => m.Enemies(OID.FireII).Where(x => x.EventState != 7), 0);
class Overpower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Overpower), new AOEShapeCone(6.5f, 45.Degrees()));
class Rive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Rive), new AOEShapeRect(30.5f, 1));
class DiffractiveLaser(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DiffractiveLaser), 5);

class IoStates : StateMachineBuilder
{
    public IoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<ExtremeCaution>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67966, NameID = 5667)]
public class Io(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, B)
{
    public static readonly WPos ArenaCenter = new(76.28f, -659.47f);
    public static readonly WPos[] Corners = [new(101.93f, -666.63f), new(94.49f, -639.63f), new(50.64f, -652.38f), new(57.58f, -679.32f)];

    public static readonly ArenaBoundsCustom B = new(25, new(Corners.Select(c => c - ArenaCenter)));

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

