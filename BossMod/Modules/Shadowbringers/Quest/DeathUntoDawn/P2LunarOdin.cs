using BossMod.Autorotation;
using BossMod.QuestBattle;
using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn.P2;

public enum OID : uint
{
    Boss = 0x3200,
    Fetters = 0x3218
}

public enum AID : uint
{
    _Weaponskill_LunarGungnir = 24025, // LunarOdin->31EC, 12.0s cast, range 6 circle
    _Weaponskill_LunarGungnir1 = 24026, // LunarOdin->2E2E, 25.0s cast, range 6 circle
    _Weaponskill_Gungnir2 = 24698, // 233C->self, 10.0s cast, range 10 circle
    _Weaponskill_Gagnrath = 24030, // 321C->self, 3.0s cast, range 50 width 4 rect
    _Weaponskill_Gungnir3 = 24029, // 321C->self, no cast, range 10 circle
    _Weaponskill_LeftZantetsuken1 = 24034, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
    _Weaponskill_RightZantetsuken1 = 24032, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
    _Spell_Einherjar = 24036, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_LeadWeight = 24024, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Gungnir = 24027, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Gungnir1 = 24028, // 321C->self, 9.3s cast, single-target
    _Weaponskill_LeftZantetsuken = 24033, // Boss->self, no cast, single-target
}

class UriangerAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    public const ushort StatusParam = 158;

    private float HeliosLeft(Actor p) => p.IsTargetable ? StatusDetails(p, 836, Player.InstanceID).Left : float.MaxValue;

    protected override void Exec(Actor? primaryTarget)
    {
        var partyPositions = World.Party.WithoutSlot().Select(p => p.Position).ToList();

        Hints.GoalZones.Add(pos => partyPositions.Count(p => p.InCircle(pos, 16)));

        if (World.Party.WithoutSlot().All(p => HeliosLeft(p) < 1 && p.Position.InCircle(Player.Position, 15.5f + p.HitboxRadius)))
            UseAction(RID.AspectedHelios, Player);

        if (World.Party.WithoutSlot().FirstOrDefault(p => p.HPMP.CurHP < p.HPMP.MaxHP * 0.4f) is Actor low)
            UseAction(RID.Benefic, low);

        UseAction(RID.MaleficIII, primaryTarget);

        if (Player.FindStatus(Roleplay.SID.DestinyDrawn) != null)
        {
            if (ComboAction == RID.DestinyDrawn)
                UseAction(RID.LordOfCrowns, primaryTarget, -100);

            if (ComboAction == RID.DestinysSleeve)
                UseAction(RID.TheScroll, Player, -100);
        }
        else
        {
            UseAction(RID.DestinyDrawn, Player, -100);
            UseAction(RID.DestinysSleeve, Player, -100);
        }

        UseAction(RID.FixedSign, Player, -150);
    }
}

class Fetters(BossModule module) : Components.Adds(module, (uint)OID.Fetters);
class AutoUri(BossModule module) : Components.RotationModule<UriangerAI>(module);
class GunmetalSoul(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(0x1EB1D5).Where(e => e.EventState != 7).Select(e => new AOEInstance(new AOEShapeDonut(4, 100), e.Position));
}
class LunarGungnir(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_LunarGungnir), 6);
class LunarGungnir2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_LunarGungnir1), 6);
class Gungnir(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Gungnir2), new AOEShapeCircle(10));
class Gagnrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Gagnrath), new AOEShapeRect(50, 2));
class GungnirSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), 189, ActionID.MakeSpell(AID._Weaponskill_Gungnir3), 5.3f, centerAtTarget: true);

class Zantetsuken(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(70, 19.5f), actor.CastInfo!.LocXZ, actor.CastInfo!.Rotation, Module.CastFinishAt(actor.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_RightZantetsuken1 or AID._Weaponskill_LeftZantetsuken1)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_RightZantetsuken1 or AID._Weaponskill_LeftZantetsuken1)
            Casters.Remove(caster);
    }
}

public class LunarOdinStates : StateMachineBuilder
{
    public LunarOdinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoUri>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<LunarGungnir>()
            .ActivateOnEnter<Gungnir>()
            .ActivateOnEnter<Gagnrath>()
            .ActivateOnEnter<GungnirSpread>()
            .ActivateOnEnter<GunmetalSoul>()
            .ActivateOnEnter<LunarGungnir2>()
            .ActivateOnEnter<Zantetsuken>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10034)]
public class LunarOdin(WorldState ws, Actor primary) : InstapullModule(ws, primary, new(146.5f, 84.5f), new ArenaBoundsCircle(20));
