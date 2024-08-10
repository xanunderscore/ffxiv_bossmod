namespace BossMod.RealmReborn.Quest.CapeWestwind;

public enum OID : uint
{
    Boss = 0x38F5, // R1.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    ImperialPilusPrior = 0x38F7, // R1.500, x0 (spawn during fight)
    ImperialCenturion = 0x38F6, // R1.500, x0 (spawn during fight)
}

public enum SID : uint
{
    DirectionalParry = 680
}

public enum AID : uint
{
    _AutoAttack_Attack = 6499, // 38F5->player, no cast, single-target
    _Weaponskill_TartareanShockwave = 28871, // 38F5->self, 3.0s cast, range 7 circle
    _Weaponskill_GalesOfTartarus = 28870, // 38F5->self, 3.0s cast, range 30 width 5 rect
    _Weaponskill_MagitekMissiles = 28864, // 38F5->self, 3.0s cast, single-target
    _Weaponskill_MagitekMissiles1 = 28865, // 233C->location, 4.0s cast, range 7 circle
    _Weaponskill_TartareanFore = 28866, // 38F5->self, 5.0s cast, single-target
    _Weaponskill_Counter = 28867, // 233C->player, no cast, single-target
    _Weaponskill_ = 28872, // 38F5->location, no cast, single-target
    _Weaponskill_TartareanTomb = 28868, // 38F5->self, 3.0s cast, single-target
    _Weaponskill_TartareanTomb1 = 28869, // 233C->self, 8.0s cast, range 11 circle
    _Weaponskill_TartareanImpact = 28873, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_FastBlade = 28884, // 38F7/38F6->player, no cast, single-target
    _Weaponskill_SavageBlade = 28885, // 38F7/38F6->player, no cast, single-target
    _Weaponskill_DullBlade = 28886, // 38F7->player, no cast, single-target
    _Weaponskill_DrillShot = 28874, // Boss->self, 3.0s cast, range 30 width 5 rect
    _Weaponskill_ViciousCharge = 28875, // Boss->player, 3.0s cast, single-target
    _Weaponskill_TartareanShockwave1 = 28877, // Boss->self, 6.0s cast, range 14 circle
    _Weaponskill_GalesOfTartarus1 = 28876, // Boss->self, 6.0s cast, range 30 width 30 rect
}

class Adds(BossModule module) : Components.Adds(module, (uint)OID.ImperialCenturion);
class Adds1(BossModule module) : Components.Adds(module, (uint)OID.ImperialPilusPrior);

class MagitekMissiles(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MagitekMissiles1), 7);
class DrillShot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DrillShot), new AOEShapeRect(30, 2.5f));
class TartareanShockwave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TartareanShockwave), new AOEShapeCircle(7));
class BigTartareanShockwave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TartareanShockwave1), new AOEShapeCircle(14));
class GalesOfTartarus(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GalesOfTartarus), new AOEShapeRect(30, 2.5f));
class BigGalesOfTartarus(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GalesOfTartarus1), new AOEShapeRect(30, 15));
class DirectionalParry(BossModule module) : Components.DirectionalParry(module, (uint)OID.Boss)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.FindStatus(SID.DirectionalParry) != null)
        {
            var dist = new AOEShapeCone(100, 90.Degrees()).Distance(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation);
            hints.AddForbiddenZone(dist);
            if (dist(actor.Position) < 0)
                foreach (var tar in hints.PotentialTargets)
                    tar.Priority = AIHints.Enemy.PriorityForbidFully;
        }
    }
}
class TartareanTomb(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TartareanTomb1), new AOEShapeCircle(11));

class RhitahtynSasArvinaStates : StateMachineBuilder
{
    public RhitahtynSasArvinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekMissiles>()
            .ActivateOnEnter<TartareanShockwave>()
            .ActivateOnEnter<DirectionalParry>()
            .ActivateOnEnter<TartareanTomb>()
            .ActivateOnEnter<GalesOfTartarus>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Adds1>()
            .ActivateOnEnter<DrillShot>()
            .ActivateOnEnter<BigTartareanShockwave>()
            .ActivateOnEnter<BigGalesOfTartarus>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 865, NameID = 2160)]
public class RhitahtynSasArvina(WorldState ws, Actor primary) : BossModule(ws, primary, new(-689, -815), new ArenaBoundsCircle(14.5f))
{
}

