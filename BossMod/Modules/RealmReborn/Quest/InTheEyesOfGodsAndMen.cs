namespace BossMod.RealmReborn.Quest.InTheEyesOfGodsAndMen;

public enum OID : uint
{
    Boss = 0x6C7,
    Helper = 0x233C,

    IshgardianArcher = 0x6C5, // R0.450, x1
    TempleChirurgeon = 0x6C4, // R0.450, x1 (spawn during fight)
    TempleBanneret = 0x6C8, // R0.550, x0 (spawn during fight)
    IshgardianLightInfantry = 0x6C6, // R0.500, x0 (spawn during fight)
    IshgardianHeavyInfantry = 0x6C3, // R0.500, x0 (spawn during fight)
    Wyvern = 0x6C9, // R2.880, x0 (spawn during fight)
}

class AlderiqueTheUnyieldingStates : StateMachineBuilder
{
    public AlderiqueTheUnyieldingStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 66448, NameID = 1454)]
public class AlderiqueTheUnyielding(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override void UpdateModule()
    {
        Arena.Center = WorldState.Party.Player()!.Position;
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            switch ((OID)h.Actor.OID)
            {
                case OID.Wyvern:
                    // wyvern is invincible for the whole encounter
                    h.Priority = -1;
                    break;
                case OID.TempleChirurgeon:
                    h.Priority = 2;
                    break;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var h in WorldState.Actors.Where(x => !x.IsAlly && !x.IsDead))
        {
            if ((OID)h.OID == OID.Wyvern)
                Arena.ActorOutsideBounds(h.Position, h.Rotation, ArenaColor.Enemy);
            else
                Arena.Actor(h.Position, h.Rotation, ArenaColor.Enemy);
        }
    }
}

