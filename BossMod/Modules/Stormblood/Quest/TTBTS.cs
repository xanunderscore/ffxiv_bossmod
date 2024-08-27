namespace BossMod.Stormblood.Quest.TheTimeBetweenTheSeconds;

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
  new WDir(10.05f, -23.83f),
  new WDir(12.22f, -23.90f),
  new WDir(14.30f, -23.90f),
  new WDir(16.43f, -23.90f),
  new WDir(18.54f, -23.90f),
  new WDir(20.71f, -23.90f),
  new WDir(20.88f, -21.84f),
  new WDir(20.88f, -19.65f),
  new WDir(20.88f, -17.51f),
  new WDir(20.88f, -15.38f),
  new WDir(20.88f, -13.14f),
  new WDir(20.88f, -11.00f),
  new WDir(20.88f, -8.84f),
  new WDir(20.88f, -6.72f),
  new WDir(20.88f, -4.53f),
  new WDir(20.88f, -2.35f),
  new WDir(20.88f, -0.25f),
  new WDir(20.88f, 1.89f),
  new WDir(20.87f, 4.04f),
  new WDir(20.87f, 6.19f),
  new WDir(20.87f, 8.35f),
  new WDir(20.87f, 10.44f),
  new WDir(20.87f, 12.55f),
  new WDir(20.87f, 14.64f),
  new WDir(20.87f, 16.77f),
  new WDir(20.87f, 18.90f),
  new WDir(20.87f, 21.02f),
  new WDir(18.75f, 20.69f),
  new WDir(16.65f, 19.93f),
  new WDir(14.67f, 19.15f),
  new WDir(12.72f, 18.23f),
  new WDir(10.70f, 17.27f),
  new WDir(8.93f, 16.05f),
  new WDir(6.82f, 15.41f),
  new WDir(4.95f, 14.53f),
  new WDir(3.05f, 13.63f),
  new WDir(1.11f, 12.72f),
  new WDir(-0.64f, 11.54f),
  new WDir(-2.21f, 13.23f),
  new WDir(-2.76f, 15.28f),
  new WDir(-3.72f, 17.20f),
  new WDir(-4.62f, 19.10f),
  new WDir(-5.52f, 21.02f),
  new WDir(-7.64f, 21.09f),
  new WDir(-9.81f, 21.09f),
  new WDir(-11.86f, 21.09f),
  new WDir(-13.98f, 21.09f),
  new WDir(-16.09f, 21.09f),
  new WDir(-18.21f, 21.09f),
  new WDir(-20.28f, 21.09f),
  new WDir(-22.39f, 21.09f),
  new WDir(-24.12f, 19.46f),
  new WDir(-24.12f, 17.24f),
  new WDir(-24.12f, 15.14f),
  new WDir(-24.12f, 13.06f),
  new WDir(-24.12f, 10.97f),
  new WDir(-24.12f, 8.76f),
  new WDir(-24.12f, 6.53f),
  new WDir(-24.12f, 4.37f),
  new WDir(-24.12f, 2.24f),
  new WDir(-24.11f, -0.00f),
  new WDir(-24.11f, -2.18f),
  new WDir(-24.11f, -4.39f),
  new WDir(-23.66f, -6.46f),
  new WDir(-21.42f, -6.50f),
  new WDir(-19.24f, -6.52f),
  new WDir(-18.74f, -8.58f),
  new WDir(-16.64f, -8.88f),
  new WDir(-14.51f, -8.88f),
  new WDir(-12.42f, -8.88f),
  new WDir(-10.25f, -8.88f),
  new WDir(-9.03f, -10.86f),
  new WDir(-9.27f, -12.99f),
  new WDir(-9.27f, -15.21f),
  new WDir(-10.02f, -17.16f),
  new WDir(-10.84f, -19.49f),
  new WDir(-10.84f, -21.60f),
  new WDir(-9.34f, -23.09f),
  new WDir(-6.99f, -23.90f),
  new WDir(-4.71f, -23.90f),
  new WDir(-2.61f, -23.90f),
  new WDir(-0.47f, -23.90f),
  new WDir(1.69f, -23.90f),
  new WDir(3.96f, -23.90f),
  new WDir(6.24f, -23.90f),
  new WDir(8.53f, -23.90f),
];

    public static readonly ArenaBoundsCustom CustomBounds = new(vertices.Select(v => v.Length()).Max(), new(vertices));
}

