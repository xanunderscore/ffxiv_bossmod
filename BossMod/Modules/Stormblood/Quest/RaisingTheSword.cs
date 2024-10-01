namespace BossMod.Stormblood.Quest.RaisingTheSword;

public enum OID : uint
{
    Boss = 0x1B51,
    Helper = 0x233C,
    _Gen_AldisSwordOfNald = 0x18D6, // R0.500, x10
    _Gen_TaintedWindSprite = 0x1B52, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_FastBlade = 9, // Boss->player, no cast, single-target
    _Weaponskill_SavageBlade = 11, // Boss->player, no cast, single-target
    _Weaponskill_RageOfHalone = 21, // Boss->player, no cast, single-target
    _Weaponskill_TheFourWinds = 8138, // Boss->self, 3.0s cast, range 60+R circle
    _Weaponskill_TheFourWinds1 = 8139, // 1B52->self, no cast, range 6 circle
    _Ability_ShudderingSwipe = 8136, // Boss->player, 3.0s cast, single-target
    _Weaponskill_ShudderingSwipe = 8137, // 18D6->self, 3.0s cast, range 60+R 30-degree cone
    _Ability_ShieldBlast = 8135, // Boss->player, 3.0s cast, single-target
    _Ability_NaldsWhisper = 8140, // Boss->self, 9.0s cast, single-target
    _Weaponskill_NaldsWhisper = 8141, // 18D6->self, 9.0s cast, range 4 circle
    _Ability_LungeCut = 8133, // Boss->player, no cast, single-target
    _Ability_VictorySlash = 8134, // Boss->self, 3.0s cast, range 6+R 120-degree cone
}

class VictorySlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_VictorySlash), new AOEShapeCone(6.5f, 60.Degrees()));
class ShudderingSwipeCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ShudderingSwipe), new AOEShapeCone(60, 15.Degrees()));
class ShudderingSwipeKB(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID._Ability_ShudderingSwipe), stopAtWall: true)
{
    private TheFourWinds? winds;
    private readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, 10, Module.CastFinishAt(c.CastInfo), null, c.AngleTo(actor), Kind.DirForward));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_ShudderingSwipe)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_ShudderingSwipe)
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        winds ??= Module.FindComponent<TheFourWinds>();

        var aoes = (winds?.Sources(Module) ?? []).Select(a => ShapeDistance.Circle(a.Position, 6)).ToList();
        if (aoes.Count == 0)
            return;

        var windzone = ShapeDistance.Union(aoes);
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(p =>
            {
                var dir = c.DirectionTo(p);
                var projected = p + dir * 10;
                return windzone(projected);
            }, Module.CastFinishAt(c.CastInfo));
    }
}
class NaldsWhisper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NaldsWhisper), new AOEShapeCircle(20));
class TheFourWinds(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID._Gen_TaintedWindSprite).Where(x => x.EventState != 7));

class AldisSwordOfNaldStates : StateMachineBuilder
{
    public AldisSwordOfNaldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFourWinds>()
            .ActivateOnEnter<NaldsWhisper>()
            .ActivateOnEnter<VictorySlash>()
            .ActivateOnEnter<ShudderingSwipeKB>()
            .ActivateOnEnter<ShudderingSwipeCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 270, NameID = 6311)]
public class AldisSwordOfNald(WorldState ws, Actor primary) : BossModule(ws, primary, new(-89.3f, 0), new ArenaBoundsCircle(20.5f));
