namespace BossMod.Stormblood.Quest.TheFaceOfTrueEvil;

public enum OID : uint
{
    Boss = 0x1BEE,
    Helper = 0x233C,
    _Gen_Musosai = 0x1BEF, // R0.500, x12, Helper type
    _Gen_Musosai1 = 0x1BF0, // R1.000, x0 (spawn during fight)
    _Gen_ViolentWind = 0x1BF1, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_Jinpu = 8722, // Boss->player, no cast, single-target
    _Weaponskill_Gekko = 8725, // Boss->player, no cast, single-target
    _Ability_HissatsuTo = 8414, // Boss->self, 2.0s cast, single-target
    _Ability_HissatsuTo1 = 8415, // 1BEF->self, 3.0s cast, range 44+R width 4 rect
    _Ability_HissatsuKyuten = 8412, // Boss->self, 3.0s cast, range 5+R circle
    _Ability_Shinkyugan = 8425, // Boss->self, no cast, single-target
    _Ability_Arashi = 8418, // Boss->self, 4.0s cast, single-target
    _Ability_Arashi1 = 8419, // 1BF0->self, no cast, range 4 circle
    _Ability_HissatsuKiku = 8416, // Boss->self, 3.0s cast, single-target
    _Ability_HissatsuKiku1 = 8417, // _Gen_Musosai->self, 4.0s cast, range 44+R width 4 rect
    _Ability_Maiogi = 8420, // Boss->self, 3.0s cast, single-target
    _Ability_Maiogi1 = 8421, // _Gen_Musosai->self, 4.0s cast, range 80+R ?-degree cone
    _Ability_Musojin = 8422, // Boss->self, 25.0s cast, single-target
    _Ability_Musojin1 = 8423, // _Gen_Musosai->self, 25.0s cast, range 40 circle
    _Ability_ = 8424, // _Gen_Musosai->self, no cast, range 40 circle
    _Ability_ArashiNoKiku = 8643, // Boss->self, 3.0s cast, single-target
    _Ability_ArashiNoMaiogi = 8642, // Boss->self, 3.0s cast, single-target
    _Ability_Musojin2 = 8998, // 1BF1->self, 25.0s cast, range 40 circle
}

class Musojin(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Ability_Musojin));
class HissatsuKiku(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_HissatsuKiku1), new AOEShapeRect(44.5f, 2));
class Maiogi(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Maiogi1), new AOEShapeCone(80, 25.Degrees()));
class HissatsuTo(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_HissatsuTo1), new AOEShapeRect(44.5f, 2));
class HissatsuKyuten(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_HissatsuKyuten), new AOEShapeCircle(5.5f));
class Arashi(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime? Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == null)
            yield break;

        foreach (var e in Module.Enemies(OID._Gen_Musosai1))
            yield return new AOEInstance(new AOEShapeCircle(4), e.Position, default, Activation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Arashi or AID._Ability_ArashiNoKiku or AID._Ability_ArashiNoMaiogi)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_Arashi1)
            Activation = null;
    }
}
class ViolentWind(BossModule module) : Components.Adds(module, (uint)OID._Gen_ViolentWind);

class MusosaiStates : StateMachineBuilder
{
    public MusosaiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuTo>()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<Arashi>()
            .ActivateOnEnter<HissatsuKiku>()
            .ActivateOnEnter<Maiogi>()
            .ActivateOnEnter<Musojin>()
            .ActivateOnEnter<ViolentWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 249, NameID = 6111)]
public class Musosai(WorldState ws, Actor primary) : BossModule(ws, primary, new(-217.27f, -158.31f), new ArenaBoundsSquare(15));

