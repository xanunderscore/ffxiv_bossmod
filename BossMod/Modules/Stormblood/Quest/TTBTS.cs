﻿namespace BossMod.Stormblood.Quest.TheTimeBetweenTheSeconds;

public enum OID : uint
{
    Boss = 0x1A36,
    Helper = 0x233C,
    _Gen_ZenosYaeGalvus = 0x1CEE, // R0.500, x9
    _Gen_DomanSignifer = 0x1A3A, // R0.500, x3
    _Gen_DomanHoplomachus = 0x1A39, // R0.500, x2
    _Gen_ZenosYaeGalvus1 = 0x1EBC, // R0.920, x1
    _Gen_DarkReflection = 0x1A37, // R0.920, x2
    _Gen_LightlessFlame = 0x1CED, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 1A39/Boss->player/1A38, no cast, single-target
    _Spell_Fire = 966, // 1A3A->1A38, 1.0s cast, single-target
    _Weaponskill_FastBlade = 717, // 1A39->1A38, no cast, single-target
    _Ability_ = 8690, // 1A36->location, no cast, ???
    _Weaponskill_VeinSplitter = 8987, // 1A36->self, 3.5s cast, range 10 circle
    _Ability_1 = 3269, // 1A36->self, no cast, single-target
    _Weaponskill_Concentrativity = 8986, // 1A36->self, 3.0s cast, range 80 circle
    _Ability_2 = 4777, // 1A36->self, no cast, single-target
    _Weaponskill_LightlessFlame = 8988, // 1CED->self, 1.0s cast, range 10+R circle
    _Weaponskill_LightlessSpark = 8985, // 1A36->self, 3.0s cast, range 40+R 90-degree cone
    _Weaponskill_Unsheathe = 8997, // 1A36->self, 3.0s cast, single-target
    _Weaponskill_Unsheathe1 = 9016, // 1CEE->self, no cast, range 80+R circle
    _Weaponskill_ArtOfTheSword = 8992, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ArtOfTheSword1 = 8993, // 1CEE->self, 3.0s cast, range 40+R width 6 rect
}

class ArtOfTheSword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArtOfTheSword1), new AOEShapeRect(41, 3));
class VeinSplitter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VeinSplitter), new AOEShapeCircle(10));
class Concentrativity(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Concentrativity));
class LightlessFlame(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LightlessFlame))
{
    private readonly Dictionary<ulong, (WPos position, DateTime activation)> Flames = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Flames.Values.Select(p => new AOEInstance(new AOEShapeCircle(11), p.position, Activation: p.activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_LightlessFlame)
            Flames.Add(actor.InstanceID, (actor.Position, WorldState.CurrentTime.AddSeconds(7)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_LightlessFlame)
            Flames[caster.InstanceID] = (caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_LightlessFlame)
            Flames.Remove(caster.InstanceID);
    }
}
class LightlessSpark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LightlessSpark), new AOEShapeCone(40.92f, 45.Degrees()));
class P2Boss(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID._Gen_ZenosYaeGalvus1), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_DarkReflection), ArenaColor.Enemy);
    }
}

class ZenosYaeGalvusStates : StateMachineBuilder
{
    public ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        SimplePhase(0, id => BuildState(id, "P1 enrage", 1800), "P1")
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, id => BuildState(id, "P2 enrage", 1800).ActivateOnEnter<ArtOfTheSword>().ActivateOnEnter<P2Boss>(), "P2")
            .Raw.Update = () => !Module.Enemies(OID._Gen_ZenosYaeGalvus1).Any();
    }

    private State BuildState(uint id, string name, float duration = 10000)
    {
        return SimpleState(id, duration, name)
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<Concentrativity>()
            .ActivateOnEnter<LightlessFlame>()
            .ActivateOnEnter<LightlessSpark>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 465, NameID = 5954)]
public class ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 546.5f), CustomBounds)
{
    // Centroid of the polygon is at: (-246.96f, 546.41f)
    private static readonly List<WDir> vertices = [
        new(-226.91f, 523.65f), new(-254.46f, 524.46f), new(-254.66f, 541.06f), new(-269.99f, 544.12f), new(-269.58f, 565.97f), new(-254.58f, 565.89f), new(-249.05f, 554.06f), new(-229.18f, 562.35f)
];

    public static readonly ArenaBoundsCustom CustomBounds = new(25, new(vertices.Select(v => v - new WDir(-247, 546.5f))));
}

