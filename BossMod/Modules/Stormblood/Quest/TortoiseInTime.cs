namespace BossMod.Stormblood.Quest.TortoiseInTime;

public enum OID : uint
{
    Boss = 0x2339,
    Helper = 0x233C,
    _Gen_Soroban = 0x2351, // R0.500, x8
    _Gen_MonkeyMagick = 0x23C2, // R1.000, x0 (spawn during fight)
    _Gen_Font = 0x233B, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Water = 11560, // Boss->player, 1.0s cast, single-target
    _Weaponskill_Eddy = 11510, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Eddy1 = 11511, // 2351->location, 3.0s cast, range 6 circle
    _Weaponskill_GreatFlood = 11512, // Boss->self, 20.0s cast, single-target
    _Weaponskill_GreatFlood1 = 11513, // 2351->self, no cast, range 60 circle
    _Ability_BlessedBubbles = 11710, // Boss->self, 5.0s cast, single-target
    _Weaponskill_SpiritBurst = 11706, // 23C2->self, 1.0s cast, range 6 circle
    _Spell_WaterDrop = 11301, // 2351->234F, 8.0s cast, range 6 circle
    _Weaponskill_Whitewater = 11520, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Whitewater1 = 11521, // 2351->self, 3.0s cast, range 40+R width 7 rect
    _Ability_Whirlwind = 11514, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Upwell = 11515, // 233B->self, 3.0s cast, range 37+R ?-degree cone
}

class Whitewater(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Whitewater1), new AOEShapeRect(40.5f, 3.5f));
class Upwell(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Upwell), new AOEShapeCone(41, 15.Degrees()));
class SpiritBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SpiritBurst), new AOEShapeCircle(6));
class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_WaterDrop), 6);

class ExplosiveTataru(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> Balls = [];
    private Actor? Tataru = null;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 3)
        {
            Balls.Add(source);
            Tataru ??= WorldState.Actors.Find(tether.Target);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_SpiritBurst)
        {
            Balls.Remove(caster);
            if (Balls.Count == 0)
                Tataru = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Tataru != null)
            Arena.AddCircle(Tataru.Position, 6, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Tataru != null)
            hints.AddForbiddenZone(ShapeDistance.Circle(Tataru.Position, 6));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tataru != null && actor.Position.InCircle(Tataru.Position, 6))
            hints.Add("GTFO from Tataru!");
    }
}

class Eddy(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Eddy1), 6);

class ShieldHint(BossModule module) : BossComponent(module)
{
    private const float Radius = 7;
    private Actor? Shield;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == 0x1EA9C7 && state == 2)
            Shield = actor;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield is Actor s)
            Arena.ZoneCircle(s.Position, Radius, ArenaColor.SafeFromAOE);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_GreatFlood1)
            Shield = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield is Actor s)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(s.Position, Radius), Module.CastFinishAt(Module.PrimaryActor.CastInfo));
    }
}

class SorobanStates : StateMachineBuilder
{
    public SorobanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Eddy>()
            .ActivateOnEnter<ShieldHint>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<ExplosiveTataru>()
            .ActivateOnEnter<SpiritBurst>()
            .ActivateOnEnter<Whitewater>()
            .ActivateOnEnter<Upwell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 551, NameID = 7240)]
public class Soroban(WorldState ws, Actor primary) : BossModule(ws, primary, new(62, -372), new ArenaBoundsSquare(19));

