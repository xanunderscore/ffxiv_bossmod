namespace BossMod.Stormblood.Quest.ReturnOfTheBull;
public enum OID : uint
{
    Boss = 0x1FD2,
    Helper = 0x233C,
    _Gen_Lakshmi = 0x18D6, // R0.500, x12, Helper type
    _Gen_DreamingKshatriya = 0x1FDD, // R1.000, x0 (spawn during fight)
    _Gen_DreamingFighter = 0x1FDB, // R0.500, x0 (spawn during fight)
    _Gen_Aether = 0x1FD3, // R1.000, x0 (spawn during fight)
    FordolaShield = 0x1EA080,
}

public enum AID : uint
{
    _AutoAttack_ = 9989, // Boss->1FDC, no cast, single-target
    _AutoAttack_Attack = 870, // _Gen_DreamingKshatriya->1FD6/1FD4/1FDE/1FD9, no cast, single-target
    _AutoAttack_Attack1 = 871, // _Gen_DreamingFighter->1FD5/201E/201D, no cast, single-target
    __AetherSphere = 9870, // Boss->self, 3.0s cast, single-target
    __HandOfGrace = 9871, // Boss->self, 3.0s cast, single-target
    __BlissfulSpear = 9872, // _Gen_Lakshmi->self, 11.0s cast, range 40 width 8 cross
    __HandOfBeauty = 9873, // Boss->self, 3.0s cast, single-target
    __BlissfulHammer = 9874, // _Gen_Lakshmi->self, no cast, range 7 circle
    __ThePallOfLight = 9877, // Boss->players/1FD8, 5.0s cast, range 6 circle
    __TightEmbrace = 10007, // Boss->self, no cast, range 40 circle
    __AetherDrain = 9889, // _Gen_Lakshmi->1FD9, no cast, single-target
    __DivineDenial = 9990, // Boss->self, 7.0s cast, range 40 circle
    __AlluringArm = 9878, // Boss->self, 3.0s cast, single-target
    __ThePathOfLight = 9875, // Boss->self, 5.0s cast, range 40+R 120-degree cone
    __Chanchala = 10058, // Boss->self, 3.0s cast, single-target
}

class PathOfLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.__ThePathOfLight), new AOEShapeCone(43.5f, 60.Degrees()));
class BlissfulSpear(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.__BlissfulSpear), new AOEShapeCross(40, 4));
class ThePallOfLight(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.__ThePallOfLight), 6, 1);
class BlissfulHammer(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(7), 109, ActionID.MakeSpell(AID.__BlissfulHammer), 12.15f, true);
class FordolaShield(BossModule module) : BossComponent(module)
{
    public Actor? Shield => WorldState.Actors.FirstOrDefault(a => (OID)a.OID == OID.FordolaShield);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield != null)
            Arena.AddCircleFilled(Shield.Position, 4, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield != null)
            hints.AddForbiddenZone(new AOEShapeDonut(4, 100), Shield.Position);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield != null && !actor.Position.InCircle(Shield.Position, 4))
            hints.Add("Go to safe zone!");
    }
}

class Deflect(BossModule module) : BossComponent(module)
{
    public IEnumerable<Actor> Spheres => Module.Enemies(OID._Gen_Aether).Where(x => !x.IsDeadOrDestroyed);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Spheres, 0xFFFFA080);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var deflectRadius = WorldState.Client.DutyActions[0].ID == (uint)ClassShared.AID.DeflectSmall ? 4 : 20;

        var closestSphere = Spheres.MaxBy(x => x.Position.Z);
        if (closestSphere != null)
        {
            var optimalDeflectPosition = closestSphere.Position with { Z = closestSphere.Position.Z + 1 };

            if (actor.DistanceToHitbox(optimalDeflectPosition, 0) > deflectRadius)
                hints.ForcedMovement = actor.DirectionTo(optimalDeflectPosition).ToVec3();

            if (actor.DistanceToHitbox(closestSphere) <= deflectRadius)
                hints.ActionsToExecute.Push(WorldState.Client.DutyActions[0], actor, ActionQueue.Priority.VeryHigh);
        }
    }
}

class LakshmiStates : StateMachineBuilder
{
    public LakshmiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Deflect>()
            .ActivateOnEnter<BlissfulSpear>()
            .ActivateOnEnter<ThePallOfLight>()
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<BlissfulHammer>()
            .ActivateOnEnter<FordolaShield>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 282, NameID = 6385)]
public class Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -353), new ArenaBoundsSquare(23))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID._Gen_Aether => -1,
                _ => 0
            };
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => x.OID is 0x1FDC or 0x1FD6 or 0x1FD8 or 0x1FD9), ArenaColor.PlayerGeneric);
    }
}

