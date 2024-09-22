namespace BossMod.Stormblood.Quest.TheBattleOnBekko;

public enum OID : uint
{
    Boss = 0x1BF8,
    Helper = 0x233C,
    _Gen_UgetsuSlayerOfAThousandSouls = 0x1BF9, // R0.500, x20, Helper type
    _Gen_ = 0x1E8EA9, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 1BFA/Boss->player/1BFB/1BFC, no cast, single-target
    _Weaponskill_Hakaze = 8721, // 1BFA->1BFC/1BFB, no cast, single-target
    _Weaponskill_Yukikaze = 8724, // Boss/1BFA->player/1BFC/1BFB, no cast, single-target
    _Weaponskill_Kasha = 8726, // Boss->player, no cast, single-target
    _Ability_HissatsuKyuten = 8433, // Boss->self, 3.0s cast, range 5+R circle
    _Weaponskill_Jinpu = 8722, // 1BFA->1BFC, no cast, single-target
    _Weaponskill_TenkaGoken = 9145, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    _Weaponskill_Gekko = 8725, // 1BFA->1BFC, no cast, single-target
    _Ability_ShinGetsubaku = 8437, // 1BF9->location, 3.0s cast, range 6 circle
    _Ability_ShinGetsubaku1 = 8436, // Boss->self, 3.0s cast, single-target
    _Ability_HissatsuGyoten = 8432, // Boss->player, no cast, single-target
    _Ability_MijinGiri = 8435, // 1BF9->self, 2.5s cast, range 80+R width 10 rect
    _Ability_MijinGiri1 = 8434, // Boss->location, 2.5s cast, single-target
    _Weaponskill_ = 9590, // 1F48->self, no cast, range 40+R width 4 rect
    _Ability_Ugetsuzan = 8439, // 1BF9->self, 2.5s cast, range -7 donut
    _Ability_Ugetsuzan1 = 8438, // Boss->self, 2.5s cast, single-target
    _Ability_Ugetsuzan2 = 8440, // 1BF9->self, 2.5s cast, range -12 donut
    _Ability_Ugetsuzan3 = 8441, // 1BF9->self, 2.5s cast, range -17 donut
    _Ability_KuruiYukikaze = 8446, // _Gen_UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 44+R width 4 rect
    _Ability_KuruiYukikaze1 = 8443, // Boss->self, 2.5s cast, single-target
    _Ability_KuruiGekko = 8444, // Boss->self, no cast, single-target
    _Ability_KuruiGekko1 = 8447, // _Gen_UgetsuSlayerOfAThousandSouls->self, 2.0s cast, range 30 circle
    _Ability_KuruiKasha = 8445, // Boss->self, no cast, single-target
    _Ability_KuruiKasha1 = 8448, // _Gen_UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 8+R ?-degree cone
    _Ability_Ugetsuzan4 = 8442, // _Gen_UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range -22 donut
}

class KuruiGekko(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Ability_KuruiGekko1));
class KuruiKasha(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_KuruiKasha1), new AOEShapeDonutSector(4.5f, 8.5f, 45.Degrees()));
class KuruiYukikaze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_KuruiYukikaze), new AOEShapeRect(44, 2), 8);
class HissatsuKyuten(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_HissatsuKyuten), new AOEShapeCircle(5.5f));
class TenkaGoken(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TenkaGoken), new AOEShapeCone(8.5f, 60.Degrees()));
class ShinGetsubaku(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_ShinGetsubaku), 6);
class ShinGetsubakuVoidzone(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID._Gen_).Where(e => e.EventState != 7));
class MijinGiri(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_MijinGiri), new AOEShapeRect(80, 5, 2));
class Ugetsuzan(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonutSector(2, 7, 90.Degrees()), new AOEShapeDonutSector(7, 12, 90.Degrees()), new AOEShapeDonutSector(12, 17, 90.Degrees())/*, new AOEShapeDonutSector(17, 22, 90.Degrees())*/])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Ability_Ugetsuzan)
            AddSequence(caster.Position - caster.Rotation.ToDirection() * 4, Module.CastFinishAt(spell), caster.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var idx = (AID)spell.Action.ID switch
        {
            AID._Ability_Ugetsuzan => 0,
            AID._Ability_Ugetsuzan2 => 1,
            AID._Ability_Ugetsuzan3 => 2,
            AID._Ability_Ugetsuzan4 => 3,
            _ => -1
        };
        AdvanceSequence(idx, caster.Position - caster.Rotation.ToDirection() * 4, WorldState.FutureTime(2.5f), caster.Rotation);
    }
}

class UgetsuSlayerOfAThousandSoulsStates : StateMachineBuilder
{
    public UgetsuSlayerOfAThousandSoulsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<TenkaGoken>()
            .ActivateOnEnter<ShinGetsubaku>()
            .ActivateOnEnter<ShinGetsubakuVoidzone>()
            .ActivateOnEnter<MijinGiri>()
            .ActivateOnEnter<Ugetsuzan>()
            .ActivateOnEnter<KuruiYukikaze>()
            .ActivateOnEnter<KuruiGekko>()
            .ActivateOnEnter<KuruiKasha>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68106, NameID = 6096)]
public class UgetsuSlayerOfAThousandSouls(WorldState ws, Actor primary) : BossModule(ws, primary, new(808.8f, 69.5f), new ArenaBoundsSquare(14));

