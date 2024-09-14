namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Sophrosyne;

public enum OID : uint
{
    Boss = 0x29AA,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Spell_SanctifiedVivify = 16997, // Boss->29AB, 5.0s cast, single-target
    _Spell_SanctifiedAeroII = 16994, // Boss->player/29A9, no cast, single-target
    _AutoAttack_Attack = 870, // 29AB->29A9, no cast, single-target
    _Spell_SanctifiedStoneIII = 16993, // Boss->player/29A9, 3.0s cast, single-target
    _Weaponskill_Charge = 16999, // 29AB->29A9, 3.0s cast, width 4 rect charge
    _Spell_SanctifiedCureII = 16995, // Boss->self/29AB/Boss, 3.0s cast, single-target
    _Weaponskill_ScoldsBridle = 16998, // 29AB->self, 4.0s cast, range 40 circle
    _Spell_SanctifiedHoly = 16996, // Boss->self, 3.0s cast, range 8 circle
}

class Charge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Charge), 2);

class SaveTheDipshit(BossModule module) : BossComponent(module)
{

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
    }
}

class SophrosyneStates : StateMachineBuilder
{
    public SophrosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SaveTheDipshit>()
            .ActivateOnEnter<Charge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8395)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(632, 64.15f), new ArenaBoundsCircle(20))
{
    private Actor? Dwarf;

    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Dwarf ??= WorldState.Actors.FirstOrDefault(x => x.OID == 0x29A9);
        if (Dwarf?.IsTargetable ?? false)
            hints.Allies.Add(Dwarf);

        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
