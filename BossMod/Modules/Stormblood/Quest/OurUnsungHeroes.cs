namespace BossMod.Stormblood.Quest.OurUnsungHeroes;

public enum OID : uint
{
    Boss = 0x1CAF, // R2.700, x1
    _Gen_FallenKuribu = 0x18D6, // R0.500, x5
    _Gen_ShadowSprite = 0x1CB4, // R0.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_Reverse = 5592, // Boss->self, no cast, single-target
    _AutoAttack_Attack = 870, // Boss->1CAD, no cast, single-target
    _Weaponskill_Glory = 5604, // Boss->self, 3.0s cast, range 40+R 90-degree cone
    _Spell_CureIV = 8635, // Boss->self, 5.0s cast, range 40 circle
    _Spell_CureIII = 8633, // Boss->self, 5.0s cast, single-target
    _Spell_CureIII1 = 8636, // _Gen_FallenKuribu->players/1CAD/1CAE, no cast, range 10 circle
    _Spell_CureV = 8638, // Boss->self, 5.0s cast, single-target
    _Spell_CureV1 = 8637, // _Gen_FallenKuribu->players, no cast, range 6 circle
    _Weaponskill_Decoy = 5602, // Boss->self, 2.0s cast, single-target
    _Spell_Ruin = 1874, // _Gen_ShadowSprite->1CAE/1CAD, 1.0s cast, single-target
    _Weaponskill_Transference = 5598, // Boss->location, no cast, single-target
    _Spell_DarkII = 4366, // _Gen_ShadowSprite->self, 2.5s cast, range 50+R 60-degree cone
}

public enum IconID : uint
{
    CureIII = 71, // player/1CAD/1CAE
    Stack = 62, // player
}

public enum SID : uint
{
    _Gen_Invincibility = 325, // Boss->Boss, extra=0x0
}

class CureIV(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_CureIV), new AOEShapeCircle(12));
class Glory(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Glory), new AOEShapeCone(42.7f, 45.Degrees()));
class CureIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CureIII, ActionID.MakeSpell(AID._Spell_CureIII1), 10, 5.15f);
class CureV(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, ActionID.MakeSpell(AID._Spell_CureV1), 6, 5.15f);
class DarkII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_DarkII), new AOEShapeCone(50.8f, 30.Degrees()));

class FallenKuribuStates : StateMachineBuilder
{
    public FallenKuribuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Glory>()
            .ActivateOnEnter<CureIII>()
            .ActivateOnEnter<CureV>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<CureIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 265, NameID = 6345)]
public class FallenKuribu(WorldState ws, Actor primary) : BossModule(ws, primary, new(232.3f, 407.7f), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID._Gen_Invincibility) == null ? 1 : 0;
    }
}
