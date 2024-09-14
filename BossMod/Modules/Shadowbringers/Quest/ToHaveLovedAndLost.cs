namespace BossMod.Shadowbringers.Quest.ToHaveLovedAndLost;

public enum OID : uint
{
    Boss = 0x2927,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player/2928, no cast, single-target
    _Weaponskill_FastBlade = 16176, // Boss->player, no cast, single-target
    _Weaponskill_SavageBlade = 16177, // Boss->player, no cast, single-target
    _Weaponskill_Swashbuckler = 16178, // Boss->player, no cast, single-target
    _Weaponskill_Bloodstain = 4747, // Boss->self, 2.5s cast, range 5 circle
    _Ability_Quickstep = 4743, // Boss->location, no cast, ???
    _Weaponskill_BrandOfSin = 16132, // Boss->self, 3.0s cast, range 80 circle
    _Ability_Backstep = 16817, // Boss->self, no cast, single-target
    _Ability_BurningBrand = 16135, // Boss->self, 1.5s cast, single-target
    _Weaponskill_AetherialPull = 16166, // 2929->player, no cast, single-target
    _Weaponskill_Fetters = 16167, // 2929->player, no cast, single-target
    _Weaponskill_BladeOfJustice = 16134, // Boss->players, 8.0s cast, range 6 circle
    _Ability_SanctifiedHolyII = 17427, // Boss->self, 3.0s cast, range 5 circle
    _Ability_SanctifiedHolyIII = 17430, // 2AB3/2AB2->location, 3.0s cast, range 6 circle
    _Ability_Backstep1 = 1291, // Boss->self, no cast, single-target
    _Ability_Brightsphere = 17428, // Boss->self, 3.0s cast, single-target
    _Weaponskill_HereticsFork = 17552, // 2779->self, 4.0s cast, range 40 width 6 cross
    _Weaponskill_SpiritsWithout = 4746, // Boss->self, 2.5s cast, range 3+R width 3 rect
    _Ability_SeraphBlade = 16131, // Boss->self, 5.0s cast, range 40+R ?-degree cone
    _Ability_ = 15548, // Boss->self, no cast, single-target
    _Ability_Fracture = 15576, // 2612->location, 5.0s cast, range 3 circle
    _Ability_Fracture1 = 13208, // 2612->location, 5.0s cast, range 3 circle
    _Ability_FearsomeFracture = 16613, // 2669/2655/2642->self, 2.0s cast, range 80 circle
    _Ability_Fracture2 = 13207, // 2612->location, 5.0s cast, range 3 circle
    _Ability_Fracture3 = 15374, // 2612->location, 5.0s cast, range 3 circle
    _Ability_Fracture4 = 16612, // 2612->location, 5.0s cast, range 3 circle
    _Ability_Fracture5 = 13209, // 2612->location, 5.0s cast, range 3 circle
    _Weaponskill_HereticsQuoit = 17470, // 2968->self, 5.0s cast, range -15 donut
    _Spell_MightMakesRight = 16133, // Boss->location, 20.0s cast, range 80 circle
    _Ability_SanctifiedHoly = 17429, // Boss->self, 6.0s cast, single-target
    _Ability_SanctifiedHoly1 = 17431, // 2AB3/2AB2->players/2928, 5.0s cast, range 6 circle
}

class HereticsFork(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HereticsFork), new AOEShapeCross(40, 3));
class SpiritsWithout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SpiritsWithout), new AOEShapeRect(3.5f, 1.5f));
class SeraphBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_SeraphBlade), new AOEShapeCone(40, 90.Degrees()));
class HereticsQuoit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HereticsQuoit), new AOEShapeDonut(5, 15));
class SanctifiedHoly(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Ability_SanctifiedHoly1), 6);

class Fracture(BossModule module) : Components.GenericTowers(module)
{
    private readonly AID[] TowerCasts = [AID._Ability_Fracture, AID._Ability_Fracture1, AID._Ability_Fracture2, AID._Ability_Fracture3, AID._Ability_Fracture4, AID._Ability_Fracture5];

    private bool IsTower(ActionID act) => TowerCasts.Contains((AID)act.ID);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (IsTower(spell.Action))
            Towers.Add(new(spell.LocXZ, 3, activation: Module.CastFinishAt(spell), includeNPCs: true));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (IsTower(spell.Action))
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.LocXZ, 1));
    }
}
class Bloodstain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bloodstain), new AOEShapeCircle(5));
class BrandOfSin(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_BrandOfSin), 10);
class BladeOfJustice(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_BladeOfJustice), 6, minStackSize: 1);
class SanctifiedHolyII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_SanctifiedHolyII), new AOEShapeCircle(5));
class SanctifiedHolyIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_SanctifiedHolyIII), 6);

class DikaiosyneStates : StateMachineBuilder
{
    public DikaiosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<BrandOfSin>()
            .ActivateOnEnter<BladeOfJustice>()
            .ActivateOnEnter<SanctifiedHoly>()
            .ActivateOnEnter<SanctifiedHolyII>()
            .ActivateOnEnter<SanctifiedHolyIII>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<HereticsQuoit>()
            .ActivateOnEnter<SpiritsWithout>()
            .ActivateOnEnter<SeraphBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68784, NameID = 8922)]
public class Dikaiosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(-798.6f, -40.49f), new ArenaBoundsCircle(20))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
}

