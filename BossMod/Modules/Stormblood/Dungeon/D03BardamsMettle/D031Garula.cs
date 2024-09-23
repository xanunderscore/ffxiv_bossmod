namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D031Garula;

public enum OID : uint
{
    Boss = 0x1A9E, // R4.000, x1
    Helper = 0x233C,
    _Gen_WallOfBardam = 0x1B5B, // R1.800, x1
    _Gen_SteppeYamaa1 = 0x1AA1, // R1.920, x4
    _Gen_SteppeYamaa = 0x1AA0, // R1.920, x2
    _Gen_SteppeSheep = 0x1A9F, // R0.700, x6
    _Gen_Garula = 0x19A, // R0.500, x4, Helper type
    _Gen_SteppeCoeurl = 0x1AA2, // R3.150, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_Heave = 7927, // Boss->self, 2.5s cast, range 9+R 120-degree cone
    _Weaponskill_CrumblingCrust = 7928, // Boss->self, 4.0s cast, single-target
    _Weaponskill_CrumblingCrust1 = 7955, // _Gen_Garula->location, 1.5s cast, range 3 circle
    _Weaponskill_Rush = 7929, // Boss->player, 10.0s cast, width 8 rect charge
    _Weaponskill_WarCry = 7930, // Boss->self, no cast, range 15+R circle
    _Weaponskill_Earthquake = 7931, // Boss->self, no cast, range 50+R circle
    _Weaponskill_Lullaby = 9394, // _Gen_SteppeSheep->self, 3.0s cast, range 3+R circle
}

class Earthquake(BossModule module) : Components.RaidwideInstant(module, ActionID.MakeSpell(AID._Weaponskill_Earthquake), 2.6f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID._Weaponskill_WarCry)
            Activation = WorldState.FutureTime(Delay);
    }
}
class Heave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Heave), new AOEShapeCone(13, 60.Degrees()));
class CrumblingCrust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CrumblingCrust1), 3);
class Lullaby(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Lullaby), new AOEShapeCircle(3.7f));
class Rush(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID._Weaponskill_Rush), 4)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (CurrentBaits.Any(b => b.Target == actor && (actor.Position - b.Source.Position).Length() < 13))
            hints.Add("Stretch tether!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(b => b.Target == actor))
            hints.AddForbiddenZone(new AOEShapeCircle(13), Module.PrimaryActor.Position, default, CurrentBaits[0].Activation);
    }
}

class GarulaStates : StateMachineBuilder
{
    public GarulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Heave>()
            .ActivateOnEnter<CrumblingCrust>()
            .ActivateOnEnter<Lullaby>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Earthquake>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6173)]
public class Garula(WorldState ws, Actor primary) : BossModule(ws, primary, new(4, 248.5f), new ArenaBoundsCircle(25));
