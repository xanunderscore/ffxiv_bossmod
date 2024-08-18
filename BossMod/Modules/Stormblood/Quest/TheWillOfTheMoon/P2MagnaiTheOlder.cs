using RPID = BossMod.Roleplay.AID;

namespace BossMod.Modules.Stormblood.Quest.TheWillOfTheMoonP2;

public enum OID : uint
{
    Boss = 0x24A1,
    Helper = 0x233C,
    _Gen_DaidukulTheMirthful = 0x24A2, // R0.500, x1
    Hien = 0x24A3,
    _Gen_TheScaleOfTheFather = 0x2532, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 24A1->Player, no cast, single-target
    _AutoAttack_Attack1 = 872, // 24A2->24A3, no cast, single-target
    _Weaponskill_PlacidPunch = 13231, // 24A2->24A3, no cast, single-target
    _Ability_TheScaleOfTheFather = 13347, // 24A1->self, 3.0s cast, single-target
    _Weaponskill_DispassionateKick = 13232, // 24A2->24A3, no cast, single-target
    _Ability_FlatlandFury = 13838, // 24A1->self, 14.0s cast, single-target
    _Weaponskill_FlatlandFury = 13244, // 2532->self, 17.0s cast, range 10 circle
    _Spell_ViolentEarth = 13236, // 233C->location, 3.0s cast, range 6 circle
    _Weaponskill_WindChisel = 13517, // 24A1->self, 2.0s cast, single-target
    _Weaponskill_WindChisel1 = 13518, // 233C->self, 2.0s cast, range 34+R 20-degree cone
    _Weaponskill_AzimZephyr = 13230, // Boss->Player, no cast, single-target
    _Weaponskill_ViolentEarth = 13235, // Boss->self, 1.5s cast, single-target
    _Weaponskill_ViolentEarth1 = 13237, // Boss->self, no cast, single-target
    _Weaponskill_Tomahawk = 13238, // Boss->24A3, 3.0s cast, single-target
    _Weaponskill_TranquilAnnihilation = 13233, // _Gen_DaidukulTheMirthful->24A3, 15.0s cast, single-target
    _Weaponskill_BrokenRidge = 13239, // Boss->self, 5.0s cast, range 50 circle
    _Ability_FlatlandFuryEnrage = 13833, // Boss->self, 23.0s cast, single-target
    _Ability_TheOneRuler = 13240, // Boss->self, no cast, single-target
    _Weaponskill_FlatlandFuryEnrage = 13329, // 249F->self, 25.0s cast, range 10 circle
    _Ability_ = 13252, // Helper->Player/Hien, no cast, single-target
    _Spell_SpiritualRay = 13348, // Helper->self, 17.0s cast, range 100 circle
    _Weaponskill_DawnJudgment = 13241, // Boss->self, 60.0s cast, range 10 circle
    _Weaponskill_FlatlandFuryEnrage1 = 13837, // 259D->self, 60.0s cast, range 10 circle
}

public class FlatlandFury(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FlatlandFury), new AOEShapeCircle(10))
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

public class FlatlandFuryEnrage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FlatlandFuryEnrage), new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveCasters.Count() < 9)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class ViolentEarth(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ViolentEarth), 6);
public class WindChisel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_WindChisel1), new AOEShapeCone(34, 10.Degrees()));

public class Scales(BossModule module) : Components.Adds(module, (uint)OID._Gen_TheScaleOfTheFather);

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
    private Actor Daidukul => Module.WorldState.Actors.First(x => (OID)x.OID == OID._Gen_DaidukulTheMirthful);

    private WPos? _safeZone;

    public override void Execute(Actor? primaryTarget)
    {
        var hienMinHP = Daidukul.CastInfo?.Action.ID == (uint)AID._Weaponskill_TranquilAnnihilation
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
