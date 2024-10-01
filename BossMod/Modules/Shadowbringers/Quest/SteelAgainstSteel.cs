namespace BossMod.Shadowbringers.Quest.SteelAgainstSteel;

public enum OID : uint
{
    Boss = 0x2A45,
    Helper = 0x233C,
    Fustuarium = 0x2AD8, // R0.500, x1 (spawn during fight)
    _Gen_ = 0x2ADD, // R0.500, x16
    CullingBlade = 0x2AD3, // R0.500, x0 (spawn during fight)
    IndustrialForce = 0x2BCE, // R0.500, x0 (spawn during fight)
    TerminusEst = 0x2A46, // R1.000, x0 (spawn during fight)
    CaptiveBolt = 0x2AD7, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_Guilt = 17571, // Boss->self, no cast, range 6 ?-degree cone
    _Weaponskill_Bastardbluss = 17570, // Boss->self, no cast, range 8 ?-degree cone
    _Weaponskill_Quickstep = 17556, // Boss->location, no cast, single-target
    _Weaponskill_CullingBlade = 17551, // Boss->self, 4.0s cast, ???
    _Weaponskill_CullingBlade1 = 17553, // CullingBlade->self, 3.5s cast, range 60 30-degree cone
    _Weaponskill_CullingBlade2 = 17610, // Boss->self, 2.0s cast, ???
    _Weaponskill_ScaldingTank = 17557, // Boss->2A4A, 6.0s cast, range 2 circle
    _Weaponskill_IndustrialForce = 17988, // Boss->self, no cast, ???
    _Weaponskill_IndustrialForce1 = 17989, // IndustrialForce->self, no cast, range 50 circle
    _Weaponskill_Fustuarium = 17572, // Boss->self, 20.0s cast, ???
    _Weaponskill_Fustuarium1 = 17573, // Fustuarium->self, 20.0s cast, range 100+R width 40 rect
    _Weaponskill_TerminusEst = 17566, // Boss->self, no cast, ???
    _Ability_TheOrder = 17568, // Boss->self, 4.0s cast, single-target
    _Weaponskill_TerminusEst1 = 17567, // TerminusEst->self, no cast, range 40+R width 4 rect
    _Weaponskill_CaptiveBolt = 17560, // Boss->self, 7.0s cast, ???
    _Ability_CaptiveBolt = 17561, // CaptiveBolt->self, 7.0s cast, range 50+R width 10 rect
    _Weaponskill_AetherochemicalGrenado = 17575, // 2A47->location, 4.0s cast, range 8 circle
    _Weaponskill_Exsanguination = 17562, // Boss->self, 5.0s cast, ???
    _Ability_Exsanguination = 17565, // 2AD6->self, 5.0s cast, range -17 donut
    _Ability_Exsanguination1 = 17564, // 2AD5->self, 5.0s cast, range -12 donut
    _Ability_Exsanguination2 = 17563, // 2AD4->self, 5.0s cast, range -7 donut
    _Weaponskill_DiffractiveLaser = 17574, // 2A48->self, 3.0s cast, range 45+R width 4 rect
    _Weaponskill_SnakeShot = 17569, // Boss->self, 4.0s cast, range 20 240-degree cone
    _Weaponskill_ScaldingTank1 = 17558, // Fustuarium->2A4A, 6.0s cast, range 6 circle
    _Weaponskill_ToTheSlaughter = 17559, // Boss->self, 4.0s cast, range 40 180-degree cone
}

class ScaldingTank(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_ScaldingTank1), 6);
class ToTheSlaughter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ToTheSlaughter), new AOEShapeCone(40, 90.Degrees()));
class Exsanguination(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, float Inner)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeDonutSector(c.Inner, c.Inner + 5, 90.Degrees()), c.Actor.CastInfo!.LocXZ, c.Actor.Rotation, Module.CastFinishAt(c.Actor.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var radius = (AID)spell.Action.ID switch
        {
            AID._Ability_Exsanguination => 12,
            AID._Ability_Exsanguination1 => 7,
            AID._Ability_Exsanguination2 => 2,
            _ => 0
        };

        if (radius > 0)
            Casters.Add((caster, radius));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Exsanguination or AID._Ability_Exsanguination1 or AID._Ability_Exsanguination2)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}
class CaptiveBolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_CaptiveBolt), new AOEShapeRect(50, 5), maxCasts: 4);
class AetherochemicalGrenado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AetherochemicalGrenado), 8);
class DiffractiveLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DiffractiveLaser), new AOEShapeRect(45, 2));
class SnakeShot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SnakeShot), new AOEShapeCone(20, 120.Degrees()));
class CullingBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CullingBlade1), new AOEShapeCone(60, 15.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // zone rasterization can end up missing the arena center since it only contains the tips of a bunch of very pointy triangles
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(ShapeDistance.Circle(c.Position, 0.5f), Module.CastFinishAt(c.CastInfo));
    }
}
class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? Caster;
    private readonly List<Actor> Actors = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            Actors.Add(actor);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster is Actor c)
            foreach (var t in Actors)
                yield return new AOEInstance(new AOEShapeRect(40, 2), t.Position, t.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // check if we already have terminuses out, because he can use this spell for a diff mechanic
        if (spell.Action.ID == (uint)AID._Ability_TheOrder && Actors.Count > 0)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_TerminusEst1)
        {
            Actors.Remove(caster);
            // reset for next iteration
            if (Actors.Count == 0)
                Caster = null;
        }
    }
}

class VitusQuoMessallaStates : StateMachineBuilder
{
    public VitusQuoMessallaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<SnakeShot>()
            .ActivateOnEnter<Exsanguination>()
            .ActivateOnEnter<ToTheSlaughter>()
            .ActivateOnEnter<ScaldingTank>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68802, NameID = 8872)]
public class VitusQuoMessalla(WorldState ws, Actor primary) : BossModule(ws, primary, new(-266, -507), new ArenaBoundsCircle(19.5f));
