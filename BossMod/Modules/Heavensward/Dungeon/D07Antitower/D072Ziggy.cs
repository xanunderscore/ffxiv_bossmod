namespace BossMod.Heavensward.Dungeon.D07Antitower.D072Ziggy;

public enum OID : uint
{
    Boss = 0x3D82, // R2.700
    Helper = 0x233C,
    Stardust = 0x3D83, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    GyratingGlare = 31835, // Boss->self, 5.0s cast, range 40 circle
    ShinySummoning = 31831, // Boss->self, no cast, single-target
    MysticLight = 31838, // Stardust->self, 6.0s cast, range 12 circle
    JitteringGlare = 31832, // Boss->self, 3.0s cast, range 40 30-degree cone
    JitteringJounceCast = 31833, // Boss->self, 6.0s cast, single-target
    JitteringJounceCharge = 31840, // Boss->player/Stardust, no cast, width 6 rect charge
    DeepFracture = 31839, // Stardust->self, 4.0s cast, range 11 circle
    JitteringJab = 31837, // Boss->player, 5.0s cast, single-target
}

public enum IconID : uint
{
    JitteringJounce = 2, // player
}

public enum TetherID : uint
{
    JitteringJounce = 2, // Boss->player/Stardust
}

class JitteringGlare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JitteringGlare), new AOEShapeCone(40, 15.Degrees()));
class JitteringJab(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.JitteringJab));

class JitteringJounce(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, int, Actor)> Tethers = [];
    private IEnumerable<Actor> Meteors => Module.Enemies(OID.Stardust).Where(x => !x.IsDead);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.JitteringJounce && WorldState.Actors.Find(tether.Target) is Actor tar)
            Tethers.Add((source, Raid.FindSlot(tether.Target), tar));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.JitteringJounce)
            Tethers.RemoveAll(t => t.Item3.InstanceID == tether.Target);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, slot, target) in Tethers)
        {
            if (slot == pcSlot)
            {
                Arena.AddRect(src.Position, src.DirectionTo(target), (target.Position - src.Position).Length(), 0, 3, ArenaColor.Danger);
                foreach (var m in Meteors)
                    Arena.ZoneCircle(m.Position, 2, ArenaColor.SafeFromAOE);
            }
            else
                Arena.ZoneRect(src.Position, target.Position, 3, ArenaColor.AOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (src, tslot, tar) in Tethers)
        {
            if (tslot == slot)
                hints.Add("Hide behind meteor!");
            else if (actor.Position.InRect(src.Position, tar.Position - src.Position, 3))
                hints.Add("GTFO from aoe!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (src, tslot, tar) in Tethers)
        {
            if (tslot == slot)
                hints.AddForbiddenZone(ShapeDistance.Intersection(Meteors.Select(st => ShapeDistance.InvertedCircle(st.Position, st.HitboxRadius)).ToList()), Module.CastFinishAt(src.CastInfo));
            else
                hints.AddForbiddenZone(new AOEShapeRect((tar.Position - src.Position).Length(), 3), src.Position, src.AngleTo(tar), Module.CastFinishAt(src.CastInfo));
        }
    }
}
class Stardust(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Module.Enemies(OID.Stardust).Where(x => !x.IsDead), ArenaColor.Object, true);
}
class DeepFracture(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeepFracture), new AOEShapeCircle(11));
class GyratingGlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GyratingGlare));
class MysticLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MysticLight), new AOEShapeCircle(12));

class ZiggyStates : StateMachineBuilder
{
    public ZiggyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stardust>()
            .ActivateOnEnter<GyratingGlare>()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<DeepFracture>()
            .ActivateOnEnter<JitteringJounce>()
            .ActivateOnEnter<JitteringJab>()
            .ActivateOnEnter<JitteringGlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4808)]
public class Ziggy(WorldState ws, Actor primary) : BossModule(ws, primary, new(185.78f, 137.5f), new ArenaBoundsCircle(20));

