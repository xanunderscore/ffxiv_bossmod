namespace BossMod.Shadowbringers.Quest.GambolingForGil;

public enum OID : uint
{
    Boss = 0x29D2, // R0.500, x1
    _Gen_Whirlwind = 0x29D5, // R1.000, x0 (spawn during fight)
    _Gen_RanaaMihgo = 0x29D6, // R0.500, x4
    _Gen_RanaaMihgo1 = 0x2A0B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 223, // Boss->player, no cast, single-target
    _Weaponskill_WarDance = 17197, // Boss->self, 3.0s cast, range 5 circle
    _Spell_CharmingChasse = 17198, // Boss->self, 3.0s cast, range 40 circle
    _Spell_HannishFire = 17205, // Boss->self, 3.0s cast, single-target
    _Spell_HannishFire1 = 17204, // 29D6->location, 3.3s cast, range 6 circle
    _Spell_HannishWinds = 17208, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Gust = 17210, // 29D5->self, no cast, range 6 circle
    _Weaponskill_Foxshot = 17289, // Boss->player, 6.0s cast, width 4 rect charge
    _Ability_ = 4731, // 29D5->self, no cast, single-target
    _Ability_HannishWaters = 17288, // Boss->self, 3.0s cast, single-target
    _Spell_HannishWaters = 17214, // 2A0B->self, 5.0s cast, range 40+R 30-degree cone
    _Weaponskill_RanaasFinish = 15646, // Boss->self, 6.0s cast, range 15 circle
}

class Foxshot(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID._Weaponskill_Foxshot), 2);
class FoxshotKB(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private readonly List<Actor> Casters = [];
    private Whirlwind? ww;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, 25, Module.CastFinishAt(c.CastInfo)));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ww ??= Module.FindComponent<Whirlwind>();

        if (Casters.FirstOrDefault() is not Actor source)
            return;

        var sources = ww?.Sources(Module).Select(p => p.Position).ToList() ?? [];
        if (sources.Count == 0)
            return;

        hints.AddForbiddenZone(p =>
        {
            var dir = source.DirectionTo(p);
            foreach (var s in sources)
            {
                var winddir = s - source.Position;
                if (MathF.Abs(WDir.Cross(winddir, dir)) <= 6 && WDir.Dot(winddir, dir) > 0)
                    return -1;
            }
            return 0;
        }, Module.CastFinishAt(source.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Foxshot)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Foxshot)
            Casters.Remove(caster);
    }
}
class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID._Gen_Whirlwind).Where(x => !x.IsDead));
class WarDance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_WarDance), new AOEShapeCircle(5));
class CharmingChasse(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Spell_CharmingChasse));
class HannishFire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_HannishFire1), 6);
class HannishWaters(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_HannishWaters), new AOEShapeCone(40, 15.Degrees()));
class RanaasFinish(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RanaasFinish), new AOEShapeCircle(15));

class RanaaMihgoStates : StateMachineBuilder
{
    public RanaaMihgoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarDance>()
            .ActivateOnEnter<CharmingChasse>()
            .ActivateOnEnter<HannishFire>()
            .ActivateOnEnter<HannishWaters>()
            .ActivateOnEnter<RanaasFinish>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Foxshot>()
            .ActivateOnEnter<FoxshotKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 670, NameID = 8489)]
public class RanaaMihgo(WorldState ws, Actor primary) : BossModule(ws, primary, new(520.47f, 124.99f), WeirdBounds)
{
    public static readonly ArenaBoundsCustom WeirdBounds = new(17.5f, new(CurveApprox.Ellipse(17.5f, 16f, 0.01f)));
}

