namespace BossMod.Heavensward.Quest.FireAndBlood;

public enum OID : uint
{
    Boss = 0xF03,
    Helper = 0x233C,
    _Gen_TempleChirurgeon = 0x104A, // R0.500, x1
    _Gen_TempleKnightMagicker = 0x104B, // R0.500, x2
}

public enum AID : uint
{
    _Spell_Fire = 966, // F03->player, 1.0s cast, single-target
    _Ability_Manaward = 157, // F03->self, no cast, single-target
    _Spell_DarkFireIII = 3791, // F03->location, 2.5s cast, range 5 circle
    _Ability_Sentinel = 17, // 104D->self, no cast, single-target
    _AutoAttack_Attack = 870, // 104D->player/1049, no cast, single-target
    _Spell_Flash = 14, // 104D->self, no cast, range 5 circle
    _Weaponskill_Heartstopper = 866, // 104C->self, 2.5s cast, range 3+R width 3 rect
    _AutoAttack_Attack1 = 871, // 104C->player/1047/1049, no cast, single-target
    _Weaponskill_Phlebotomize = 91, // 104C->1049, no cast, single-target
    _Weaponskill_FastBlade = 9, // 104D->player/1049, no cast, single-target
    _Spell_Stone = 119, // 104A->1047, 1.5s cast, single-target
    _Weaponskill_SavageBlade = 11, // 104D->player/1049, no cast, single-target
    _Weaponskill_RageOfHalone = 21, // 104D->1049, no cast, single-target
    _Weaponskill_SpiritsWithout = 1098, // 104D->self, 2.5s cast, range 3+R width 3 rect
    _Spell_Aero = 121, // 104A->1047, no cast, single-target
    _Spell_Bravery = 1836, // F03->104A, 3.5s cast, single-target
    _Ability_LifeSurge = 83, // 104C->self, no cast, single-target
    _Spell_Cure = 5073, // 104A->self/104C/104B/104A, 4.0s cast, single-target
    _Ability_Convalescence = 12, // 104D->self, no cast, single-target
    _Spell_FireII = 147, // 104B->1049, 3.0s cast, range 5 circle
}

class Puddle(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(0x1E8D61).Where(x => x.EventState != 7));
class DarkFireIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_DarkFireIII), 5);
class Heartstopper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Heartstopper), new AOEShapeRect(3.5f, 1.5f));
class SpiritsWithout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SpiritsWithout), new AOEShapeRect(3.5f, 1.5f));
class FireII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_FireII), 5);

class SerCharibertTheSternStates : StateMachineBuilder
{
    public SerCharibertTheSternStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Puddle>()
            .ActivateOnEnter<DarkFireIII>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<SpiritsWithout>()
            .ActivateOnEnter<FireII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 398, NameID = 4142)]
public class SerCharibertTheStern(WorldState ws, Actor primary) : DutyModule(ws, primary, new(65, 0), new ArenaBoundsCircle(18))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.Position.InCircle(Arena.Center, 18))
                h.Priority = (OID)h.Actor.OID switch
                {
                    OID._Gen_TempleChirurgeon => 2,
                    OID.Boss => 1,
                    _ => 0
                };
    }
}

