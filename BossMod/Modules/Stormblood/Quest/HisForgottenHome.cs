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

class BlizzardVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(0x1E8D9C).Where(x => x.EventState != 7));
class Kasaya(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Kasaya), new AOEShapeCone(7.6f, 60.Degrees()));
class WaterIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_WaterIII), 8);

class BlizzardIII(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), 26, ActionID.MakeSpell(AID._Spell_BlizzardIII), 3.1f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var isse = WorldState.Actors.FirstOrDefault(a => a.OID == 0x2138);
        if (isse != null)
            hints.AddForbiddenZone(new AOEShapeCircle(5), isse.Position);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x213B, 0x213C, 0x213D])
{
    private Actor? Isse => WorldState.Actors.FirstOrDefault(a => a.OID == 0x2138);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = e.Actor.TargetID == Isse?.InstanceID ? 1 : 0;
        }
    }
}

class Allies(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var actor in WorldState.Actors.Where(a => a.IsAlly))
            Arena.Actor(actor, ArenaColor.PlayerGeneric);
    }
}

class SlickshellCaptainStates : StateMachineBuilder
{
    public SlickshellCaptainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Allies>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Kasaya>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<BlizzardVoidzone>()
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68563, NameID = 6891)]
public class SlickshellCaptain(WorldState ws, Actor primary) : BossModule(ws, primary, new(468.92f, 301.30f), CustomBounds)
{
    private static readonly List<WDir> vertices = [
  new WDir(-1.25f, -20.91f),
  new WDir(0.55f, -19.91f),
  new WDir(2.37f, -18.90f),
  new WDir(4.21f, -17.87f),
  new WDir(6.04f, -16.85f),
  new WDir(7.86f, -15.84f),
  new WDir(9.72f, -14.80f),
  new WDir(11.56f, -13.78f),
  new WDir(13.41f, -12.75f),
  new WDir(15.28f, -11.71f),
  new WDir(17.10f, -10.69f),
  new WDir(18.94f, -9.67f),
  new WDir(20.76f, -8.66f),
  new WDir(22.57f, -7.65f),
  new WDir(24.38f, -6.64f),
  new WDir(26.26f, -5.61f),
  new WDir(28.08f, -4.60f),
  new WDir(29.29f, -2.52f),
  new WDir(29.00f, -0.51f),
  new WDir(28.55f, 1.55f),
  new WDir(28.24f, 3.67f),
  new WDir(28.48f, 5.72f),
  new WDir(27.85f, 7.73f),
  new WDir(25.74f, 7.84f),
  new WDir(24.09f, 9.47f),
  new WDir(22.79f, 11.11f),
  new WDir(21.25f, 12.54f),
  new WDir(19.78f, 13.97f),
  new WDir(17.49f, 15.03f),
  new WDir(15.50f, 15.64f),
  new WDir(13.45f, 16.26f),
  new WDir(11.44f, 16.88f),
  new WDir(9.47f, 17.48f),
  new WDir(7.46f, 18.09f),
  new WDir(5.41f, 18.72f),
  new WDir(3.35f, 19.35f),
  new WDir(1.25f, 19.99f),
  new WDir(-0.76f, 20.61f),
  new WDir(-2.85f, 21.11f),
  new WDir(-4.82f, 21.85f),
  new WDir(-7.07f, 21.25f),
  new WDir(-8.65f, 20.00f),
  new WDir(-10.27f, 18.72f),
  new WDir(-11.96f, 17.40f),
  new WDir(-13.64f, 16.08f),
  new WDir(-15.31f, 14.77f),
  new WDir(-16.96f, 13.47f),
  new WDir(-16.79f, 11.38f),
  new WDir(-15.11f, 10.02f),
  new WDir(-13.79f, 8.49f),
  new WDir(-15.14f, 6.92f),
  new WDir(-17.17f, 6.28f),
  new WDir(-18.40f, 4.62f),
  new WDir(-19.64f, 2.94f),
  new WDir(-20.39f, 0.63f),
  new WDir(-20.98f, -1.29f),
  new WDir(-22.47f, -2.67f),
  new WDir(-23.98f, -4.16f),
  new WDir(-25.46f, -5.62f),
  new WDir(-26.95f, -7.09f),
  new WDir(-28.42f, -8.54f),
  new WDir(-28.42f, -10.55f),
  new WDir(-26.45f, -11.34f),
  new WDir(-24.48f, -12.13f),
  new WDir(-22.48f, -12.63f),
  new WDir(-20.63f, -13.67f),
  new WDir(-18.68f, -14.45f),
  new WDir(-16.72f, -15.24f),
  new WDir(-14.77f, -16.02f),
  new WDir(-12.87f, -16.78f),
  new WDir(-10.95f, -17.54f),
  new WDir(-9.00f, -18.32f),
  new WDir(-7.05f, -19.10f),
  new WDir(-5.09f, -19.89f),
  new WDir(-3.13f, -20.67f),
];
    // Centroid of the polygon is at: (468.92f, 301.30f)

    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices));
}

