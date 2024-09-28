namespace BossMod.Endwalker.Quest.AnUnforeseenBargain.P2Andromalius;

public enum OID : uint
{
    Boss = 0x3D76, // R6.000, x1
    Helper = 0x233C, // R0.500, x13, Helper type
    _Gen_Voidcrystal = 0x3D7C, // R0.350, x8
    _Gen_VisitantOgre = 0x3EA6, // R1.560, x2
    _Gen_VisitantBlackguard = 0x3EA5, // R1.700, x2
    _Gen_Voidcluster = 0x3D7D, // R0.600, x4
    _Gen_VisitantVoidskipper = 0x3D78, // R1.080, x0 (spawn during fight)
    AlphiShield = 0x1EB87A,
}

public enum AID : uint
{
    _AutoAttack_Attack = 6497, // 3EA5->3D7F/3D7E, no cast, single-target
    _AutoAttack_Attack1 = 6499, // 3EA6->player/3D80, no cast, single-target
    _AutoAttack_ = 19052, // Boss->player/3D80, no cast, single-target
    _Spell_Cackle = 31820, // Boss->player, 4.0s cast, single-target
    _Weaponskill_ChainOfCommands = 31813, // Boss->self, 9.0s cast, single-target
    _Weaponskill_StraightSpindle = 31808, // 3D78->self, 5.0s cast, range 50+R width 5 rect
    _Weaponskill_Dark = 31815, // Helper->location, 5.0s cast, range 10 circle
    _Weaponskill_Dark1 = 31814, // Boss->self, 5.0s cast, single-target
    _Weaponskill_StraightSpindle1 = 31809, // 3D78->self, 9.0s cast, range 50+R width 5 rect
    _Spell_EvilMist = 31825, // Boss->self, 5.0s cast, range 60 circle
    _Ability_SinisterSphere = 33009, // Boss->self, 4.0s cast, single-target
    _Ability_Explosion = 33010, // Helper->self, 10.0s cast, range 5 circle
    _Weaponskill_Hellsnap = 31816, // Boss->3D80, 5.0s cast, range 6 circle
    _Weaponskill_VoidEvocation = 31821, // Boss->self, no cast, single-target
    _Weaponskill_VoidEvocation1 = 31822, // Boss->self, no cast, single-target
    _Spell_VoidEvocation = 31823, // Helper->self, 1.5s cast, range 60 circle
    _Weaponskill_Decay = 32857, // _Gen_VisitantVoidskipper->self, 13.0s cast, range 60 circle
    _Weaponskill_StraightSpindle2 = 33174, // 3D77->self, 8.0s cast, range 50+R width 5 rect
    _Weaponskill_Voidblood = 33172, // 3EE4->location, 9.0s cast, range 6 circle
    _Weaponskill_VoidSlash = 33173, // 3EE5->self, 11.0s cast, range 8+R 90-degree cone
    _Spell_VoidEvocation1 = 31824, // Helper->self, 1.5s cast, range 60 circle
}

class StraightSpindleAdds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StraightSpindle2), new AOEShapeRect(50, 2.5f));
class Voidblood(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Voidblood), 6);
class VoidSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VoidSlash), new AOEShapeCone(9.7f, 45.Degrees()));
class EvilMist(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_EvilMist));
class Explosion(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Ability_Explosion), 5);
class Dark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Dark), 10);
class Hellsnap(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Hellsnap), 6);

class StraightSpindle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(50, 2.5f), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo))).OrderBy(x => x.Activation).Take(3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_StraightSpindle or AID._Weaponskill_StraightSpindle1)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_StraightSpindle or AID._Weaponskill_StraightSpindle1)
            Casters.Remove(caster);
    }
}
class Decay(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_Decay), "Kill wasp before enrage!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}
class ShieldHint(BossModule module) : BossComponent(module)
{
    private Actor? Shield;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AlphiShield)
            Shield = actor;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000C && param1 == 0x46)
            Shield = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield is Actor s)
            hints.GoalZones.Add(hints.GoalSingleTarget(s.Position, 5));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield is Actor s && !actor.Position.InCircle(s.Position, 5))
            hints.Add("Take cover!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield is Actor s)
            Arena.ZoneCircle(s.Position, 5, ArenaColor.SafeFromAOE);
    }
}

class ProtectZero(BossModule module) : BossComponent(module)
{
    private Actor? CastingZero => Raid.WithoutSlot().FirstOrDefault(x => x.OID == 0x3D80 && x.FindStatus(2056) != null);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CastingZero is Actor z)
        {
            foreach (var h in hints.PotentialTargets)
                if (h.Actor.TargetID == z.InstanceID)
                    h.Priority = 5;
        }
    }
}

class AndromaliusStates : StateMachineBuilder
{
    public AndromaliusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<EvilMist>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Hellsnap>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<ShieldHint>()
            .ActivateOnEnter<StraightSpindleAdds>()
            .ActivateOnEnter<Voidblood>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<ProtectZero>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209, NameID = 12071)]
public class Andromalius(WorldState ws, Actor primary) : BossModule(ws, primary, new(97.85f, 286), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
