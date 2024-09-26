namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    _Weaponskill_Hydrofall = 8871, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Hydrofall1 = 8893, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_LaughingLeap = 8852, // Boss->location, 4.0s cast, range 4 circle
    _Weaponskill_Landsblood = 7822, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_Landsblood1 = 7899, // Boss->self, no cast, range 40 circle
    _Weaponskill_Geyser = 8800, // Helper->self, no cast, range 6 circle
}

class CandyCane(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_CandyCane));
class Hydrofall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Hydrofall1), 6);
class LaughingLeap(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LaughingLeap), 4);
class Landsblood(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Landsblood));

class GeyserDisplay(BossModule module) : BossComponent(module)
{
    private List<(WPos, DateTime, uint)> Geysers = [];

    private List<WDir> Geysers1 = [new(0, -16), new(-9, 10)];
    private List<WDir> Geysers2 = [new(0, 5), new(-9, -15), new(7, -7)];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(0x1EAAA1), ArenaColor.Object, true);
        Arena.Actors(Module.Enemies(0x1EAAA2), ArenaColor.Object, true);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x100020)
        {
            var gidx = actor.OID switch
            {
                0x1EAAA1 => 0,
                0x1EAAA2 => 1
            };
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Geysers.RemoveAll(g => g.Item2.AddSeconds(2) < WorldState.CurrentTime);

        foreach (var g in Geysers)
            Arena.ZoneCircle(g.Item1, 6, 0xff8800ff);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Geyser)
            Geysers.Add((caster.Position, WorldState.CurrentTime, 0x808000ff));
    }
}

class AencThonLordOfTheLingeringGazeStates : StateMachineBuilder
{
    public AencThonLordOfTheLingeringGazeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandyCane>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<GeyserDisplay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class AencThonLordOfTheLingeringGaze(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 30), new ArenaBoundsCircle(19.5f));

