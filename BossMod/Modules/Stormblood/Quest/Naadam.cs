namespace BossMod.Stormblood.Quest.Naadam;

public enum OID : uint
{
    Boss = 0x1B31,
    Helper = 0x233C,
    _Gen_BuduganShaman = 0x1B33, // R0.500, x1
    _Gen_BuduganHunter = 0x1B32, // R0.500, x2 (spawn during fight)
    _Gen_BuduganWarrior = 0x1B30, // R0.500, x2 (spawn during fight)
    _Gen_OroniriBrother = 0x1B2F, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter = 0x1B35, // R0.500, x0 (spawn during fight)
    _Gen_MagnaiTheOlder = 0x1B38, // R0.500, x0 (spawn during fight)
    _Gen_DotharliSpiritcaller = 0x1B36, // R0.500, x0 (spawn during fight)
    _Gen_MagnaiTheOlder1 = 0x18D6, // R0.500, x0 (spawn during fight)
    _Gen_DotharliWarrior = 0x1B34, // R0.500, x0 (spawn during fight)
    _Gen_SaduHeavensflame = 0x1B39, // R0.500, x0 (spawn during fight)
    _Gen_OroniriWarrior = 0x1E2B, // R0.500, x0 (spawn during fight)
    _Gen_BuduganWarrior1 = 0x1E2C, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter1 = 0x1E2D, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter2 = 0x1B37, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter3 = 0x1DDA, // R0.500, x0 (spawn during fight)
    _Gen_OroniriHunter = 0x1DD9, // R0.500, x0 (spawn during fight)
    _Gen_StellarChuluu = 0x1B3F, // R1.800, x0 (spawn during fight)
    _Gen_DomanHoplomachus = 0x1B3C, // R0.500, x0 (spawn during fight)
    _Gen_DomanSignifer = 0x1B3E, // R0.500, x0 (spawn during fight)
    _Gen_DomanLaquierius = 0x1B3D, // R0.500, x0 (spawn during fight)
    _Gen_ArmoredWeapon = 0x1B3B, // R5.400, x0 (spawn during fight)
    _Gen_Grynewaht = 0x1B3A, // R0.500, x0 (spawn during fight)
    Ovoo = 0x1EA4E1
}

public enum AID : uint
{
    Holmgang = 8391
}

public enum SID : uint
{
    EarthenAccord = 778
}

class NaadamStates : StateMachineBuilder
{
    public NaadamStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "GoToOvoo")
            .Raw.Update = () => Module.FindComponent<Holmgang>()?.NumCasts > 0;
        SimplePhase(1, P2, "Defend");
    }

    private void P1(uint id)
    {
        SimpleState(id + 0xFF0000, 1800, "Duty timer")
            .ActivateOnEnter<P1Bounds>()
            .ActivateOnEnter<Enemies>()
            .ActivateOnEnter<DrawOvoo>()
            .ActivateOnEnter<Holmgang>()
            ;
    }

    private void P2(uint id)
    {
        SimpleState(id + 0xFF0000, 1800, "Duty timer")
            .ActivateOnEnter<OvooUser>()
            .OnEnter(() => Module.Arena.Center = new(354, 296.5f));
    }
}

class DrawOvoo : BossComponent
{
    private Actor? Ovoo => WorldState.Actors.FirstOrDefault(o => o.OID == 0x1EA4E1);
    public DrawOvoo(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(Ovoo, ArenaColor.Vulnerable, true);
    }
}

class OvooUser(BossModule module) : BossComponent(module)
{
    private Actor? Actor => WorldState.Actors.FirstOrDefault(a => a.FindStatus(SID.EarthenAccord) != null && !a.IsAlly);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Actor != null)
            Arena.AddCircle(Actor.Position, 1.5f, ArenaColor.Danger);
    }
}

class Holmgang(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Holmgang));

class Enemies : BossComponent
{
    public Enemies(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var act in WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable))
            Arena.Actor(act, act.OID == (uint)OID._Gen_StellarChuluu ? ArenaColor.Object : ArenaColor.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.FindStatus(SID.EarthenAccord) != null)
                e.Priority = 1;
            else if ((OID)e.Actor.OID == OID._Gen_StellarChuluu)
                e.Priority = 1;
            else
                e.Priority = 0;
        }
    }
}

class P1Bounds(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()!.Position;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 246, NameID = 6160)]
public class Naadam(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

