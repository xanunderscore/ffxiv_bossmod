using RPID = BossMod.Roleplay.AID;

namespace BossMod.Modules.Stormblood.Quest.TheWillOfTheMoonP2;

public enum OID : uint
{
    Boss = 0x24A1,
    Helper = 0x233C,
    Hien = 0x24A3,
    Daidukul = 0x24A2, // R0.500, x1
    TheScaleOfTheFather = 0x2532, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    FlatlandFury = 13244, // 2532->self, 17.0s cast, range 10 circle
    FlatlandFuryEnrage = 13329, // 249F->self, 25.0s cast, range 10 circle
    ViolentEarth = 13236, // 233C->location, 3.0s cast, range 6 circle
    WindChisel = 13518, // 233C->self, 2.0s cast, range 34+R 20-degree cone
    TranquilAnnihilation = 13233, // _Gen_DaidukulTheMirthful->24A3, 15.0s cast, single-target
}

public class FlatlandFury(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlatlandFury), new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // if all 9 adds are alive, instead of drawing forbidden zones (which would fill the whole arena), force AI to target nearest one to kill it
        if (ActiveCasters.Count() == 9)
            hints.ForcedTarget = ActiveCasters.MinBy(actor.DistanceToHitbox);
        else
        {
            if (ActiveCasters.Any())
                hints.ForcedTarget = Module.PrimaryActor;

            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

public class FlatlandFuryEnrage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlatlandFuryEnrage), new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveCasters.Count() < 9)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class ViolentEarth(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ViolentEarth), 6);
public class WindChisel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindChisel), new AOEShapeCone(34, 10.Degrees()));

public class Scales(BossModule module) : Components.Adds(module, (uint)OID.TheScaleOfTheFather);

class MagnaiTheOlderStates : StateMachineBuilder
{
    public MagnaiTheOlderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<YshtolaAI>()
            .ActivateOnEnter<Scales>()
            .ActivateOnEnter<FlatlandFury>()
            .ActivateOnEnter<FlatlandFuryEnrage>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<WindChisel>()
            ;
    }
}

class YshtolaAI(BossModule module) : Components.RoleplayModule(module)
{
    private Actor Magnai => Module.PrimaryActor;
    private Actor Hien => Module.WorldState.Actors.First(x => (OID)x.OID == OID.Hien);
    private Actor Daidukul => Module.WorldState.Actors.First(x => (OID)x.OID == OID.Daidukul);

    private WPos? _safeZone;

    public override void Execute(Actor? primaryTarget)
    {
        var hienMinHP = Daidukul.CastInfo?.Action.ID == (uint)AID.TranquilAnnihilation
            ? 28000
            : 10000;

        if (PredictedHP(Hien) < hienMinHP)
        {
            if (Player.DistanceToHitbox(Hien) > 25)
                Hints.ForcedMovement = Player.DirectionTo(Hien).ToVec3();

            UseGCD(RPID.CureIISeventhDawn, Hien);
        }

        if (_safeZone != null && (_safeZone.Value - Player.Position).Length() > 2)
            Hints.ForcedMovement = (_safeZone.Value - Player.Position).Normalized().ToVec3();

        var aero = StatusDetails(Magnai, WHM.SID.Aero2, Player.InstanceID);
        if (aero.Left < 4.6f)
            UseGCD(RPID.AeroIISeventhDawn, Magnai);

        UseGCD(RPID.StoneIVSeventhDawn, primaryTarget);

        if (Player.HPMP.CurMP < 5000)
            UseOGCD(RPID.Aetherwell, Player);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1)
        {
            if (state == 0x10002)
                _safeZone = actor.Position;
            else if (state == 0x40008)
                _safeZone = null;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 609, NameID = 6153)]
public class MagnaiTheOlder(WorldState ws, Actor primary) : BossModule(ws, primary, new(-186.5f, 550.5f), new ArenaBoundsCircle(20));
