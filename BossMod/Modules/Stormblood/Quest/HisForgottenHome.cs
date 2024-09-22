namespace BossMod.Stormblood.Quest.HisForgottenHome;
public enum OID : uint
{
    Boss = 0x213A,
    Helper = 0x233C,
    _Gen_SoftshellOfTheRed = 0x213B, // R1.600, x4 (spawn during fight)
    _Gen_SoftshellOfTheRed1 = 0x213C, // R1.600, x0 (spawn during fight)
    _Gen_SoftshellOfTheRed2 = 0x213D, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Water = 971, // Boss/_Gen_SoftshellOfTheRed2->2135/2138/2137, 1.0s cast, single-target
    _AutoAttack_Attack = 870, // _Gen_SoftshellOfTheRed/_Gen_SoftshellOfTheRed1->player/2136/2137/2138/2135, no cast, single-target
    _Weaponskill_Kasaya = 8585, // _Gen_SoftshellOfTheRed->self, 2.5s cast, range 6+R 120-degree cone
    _Weaponskill_ShellShock = 8584, // _Gen_SoftshellOfTheRed->2136/2137/2135/2138, no cast, single-target
    _Spell_WaterIII = 5831, // Boss->location, 3.0s cast, range 8 circle
    _Spell_WaterIII1 = 10573, // Boss->location, 5.0s cast, range 6 circle
    _Ability_ = 3269, // Boss->self, no cast, single-target
    _Spell_BlizzardIII = 10874, // Boss->location, 3.0s cast, range 5 circle
}

class Kasaya(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Kasaya), new AOEShapeCone(7.6f, 60.Degrees()));
class WaterIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_WaterIII), 8);

class BlizzardIIIIcon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), 26, centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_BlizzardIII)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count > 0 && WorldState.Actors.FirstOrDefault(a => a.OID == 0x2138) is Actor isse)
            hints.AddForbiddenZone(new AOEShapeCircle(8), isse.Position, activation: CurrentBaits[0].Activation);
    }
}
class BlizzardIIICast(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID._Spell_BlizzardIII), m => m.Enemies(0x1E8D9C).Where(x => x.EventState != 7), 0);

class SlickshellCaptainStates : StateMachineBuilder
{
    public SlickshellCaptainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Kasaya>()
            .ActivateOnEnter<BlizzardIIIIcon>()
            .ActivateOnEnter<BlizzardIIICast>()
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68563, NameID = 6891)]
public class SlickshellCaptain(WorldState ws, Actor primary) : BossModule(ws, primary, BoundsCenter, CustomBounds)
{
    public static readonly WPos BoundsCenter = new(468.92f, 301.30f);

    private static readonly List<WPos> vertices = [
        new(464.25f, 320.19f), new(455.65f, 313.35f), new(457.72f, 308.20f), new(445.00f, 292.92f), new(468.13f, 283.56f), new(495.55f, 299.63f), new(487.19f, 313.73f)
    ];

    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices.Select(v => v - BoundsCenter)));

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // attack anyone targeting isse
        foreach (var h in hints.PotentialTargets)
            h.Priority = WorldState.Actors.Find(h.Actor.TargetID)?.OID == 0x2138 ? 1 : 0;
    }
}

