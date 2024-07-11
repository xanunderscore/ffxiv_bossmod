namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D042Hermes;

public enum OID : uint
{
    _Gen_Venat = 0x33D5, // R0.500, x?, DutySupport type
    _Gen_EmetSelch = 0x33D8, // R0.500, x?, DutySupport type
    _Gen_KtiseosErinys = 0x3533, // R2.000, x?
    _Gen_Hythlodaeus = 0x33D9, // R0.500, x?, DutySupport type
    _Gen_KtiseosPanther = 0x3538, // R2.100, x?
    _Gen_KtiseosSaura = 0x353A, // R1.800, x?
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x?, EventObj type
    _Gen_FlyingLifeFormsAnIntroduction = 0x1EB245, // R0.500, x?, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x?, EventObj type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    _Gen_KtiseosAello = 0x3536, // R2.520, x?
    _Gen_ = 0xFE502, // R0.500-4.200, x?, EventNpc type
    _Gen_Meteion = 0xFE4CB, // R0.390-0.500, x?, EventNpc type
    _Gen_Karukeion = 0x348B, // R1.000, x?
    Boss = 0x348A, // R4.200, x?
    _Gen_Hermes = 0x233C, // R0.500, x?, 523 type
    _Gen_Meteor = 0x348C, // R2.400, x?
    _Gen_Hermes1 = 0xFE4CA, // R0.500, x?, EventNpc type
    _Gen_1 = 0xFE4DF, // R0.500, x?, EventNpc type
    _Gen_2 = 0xFE4E1, // R0.500, x?, EventNpc type
    _Gen_3 = 0xFE4E0, // R0.500, x?, EventNpc type
}

public enum AID : uint
{
    _Ability_BloodDraw = 25455, // _Gen_Hythlodaeus->Boss, no cast, single-target
    _Spell_TrueStone = 25484, // _Gen_Venat->Boss, 1.0s cast, single-target
    _Weaponskill_PolydegmonsPrice = 25440, // _Gen_EmetSelch->Boss, no cast, single-target
    _AutoAttack_ = 17058, // _Gen_EmetSelch->Boss, no cast, single-target
    _AutoAttack_1 = 25451, // _Gen_Hythlodaeus->Boss, no cast, single-target
    _Weaponskill_HailOfFire = 25454, // _Gen_Hythlodaeus->Boss, no cast, single-target
    _AutoAttack_Attack = 872, // Boss->none, no cast, single-target
    _Weaponskill_PolydegmonsPurgation = 25441, // _Gen_EmetSelch->Boss, no cast, single-target
    _Weaponskill_LuminousBolt = 25456, // _Gen_Hythlodaeus->Boss, no cast, single-target
    _Ability_TrueReprisal = 26990, // _Gen_EmetSelch->Boss, no cast, range 5 circle
    _Ability_Eubuleus = 25450, // _Gen_EmetSelch->self, no cast, range 15 circle
    _Weaponskill_AetherEater = 25442, // _Gen_EmetSelch->Boss, no cast, single-target
    _Spell_TrueWaterIV = 25485, // _Gen_Venat->Boss, 2.0s cast, range 8 circle
    _Spell_Trismegistos = 25886, // Boss->self, 5.0s cast, range 40 circle
    _Spell_TrueWater = 25482, // _Gen_Venat->Boss, 1.0s cast, single-target
    _Spell_TrueCureII = 25489, // _Gen_Venat->self/player, 1.5s cast, single-target
    _Spell_TrueStoneIV = 25487, // _Gen_Venat->Boss, 2.0s cast, single-target
    _Ability_HadessGates = 25445, // _Gen_EmetSelch->Boss, no cast, range 5 circle
    _AutoAttack_2 = 17235, // _Gen_Venat->Boss, no cast, single-target
    _Weaponskill_Hermetica = 25888, // Boss->self, 3.0s cast, single-target
    _Ability_HellbornYawp = 25446, // _Gen_EmetSelch->Boss, no cast, single-target
    _Ability_StormOfArrows = 25457, // _Gen_Hythlodaeus->Boss, no cast, range 8 circle
    _Ability_CthonicFlood = 25447, // _Gen_EmetSelch->Boss, no cast, range 10+R width 4 rect
    _Ability_VeiledArrow = 25458, // _Gen_Hythlodaeus->Boss, no cast, range 5 circle
    _Spell_TrueAeroIV = 25486, // _Gen_Venat->Boss, 2.0s cast, range 5 circle
    _Ability_CthonicEdge = 25448, // _Gen_EmetSelch->Boss, no cast, single-target
    _Weaponskill_TrueAeroIV = 25889, // _Gen_Karukeion->self, 4.0s cast, range 50 width 10 rect
    _Weaponskill_PeakShot = 25459, // _Gen_Hythlodaeus->self, no cast, range 25+R width 4 rect
    _Spell_DarkEruption = 25432, // _Gen_EmetSelch->Boss, no cast, range 6 circle
    _Weaponskill_LashingShot = 25460, // _Gen_Hythlodaeus->self, no cast, range 25+R width 4 rect
    _Spell_TrueAero = 25483, // _Gen_Venat->Boss, 1.0s cast, single-target
    _Ability_Ploutonos = 25449, // _Gen_EmetSelch->self, no cast, single-target
    _Spell_MegiddoFlame = 25461, // _Gen_Hythlodaeus->self, no cast, range 15+R width 8 rect
    _Weaponskill_TrueTornado = 25902, // Boss->self, 5.0s cast, single-target
    _Weaponskill_TrueTornado1 = 25903, // Boss->self, no cast, single-target
    _Weaponskill_TrueTornado2 = 25905, // _Gen_Hermes->none, no cast, range 4 circle
    _Weaponskill_Meteor = 25890, // Boss->self, 3.0s cast, single-target
    _Weaponskill_CosmicKiss = 25891, // _Gen_Meteor->self, 5.0s cast, range 40 circle
    _Weaponskill_Double = 25892, // Boss->self, 3.0s cast, single-target
    _Spell_TrueMedicaII = 25495, // _Gen_Venat->self, 2.0s cast, range 20 circle
    _Weaponskill_Hermetica1 = 25893, // Boss->self, 6.0s cast, single-target
    _Weaponskill_TrueAeroIV1 = 27836, // _Gen_Karukeion->self, 4.0s cast, range 50 width 10 rect
    _Spell_TrueStoneskinII = 25496, // _Gen_Venat->self, 2.5s cast, range 15 circle
    _Spell_AfflatusAzem = 25488, // _Gen_Venat->Boss, no cast, range 5 circle
    _Ability_ = 25887, // _Gen_Hermes->player, no cast, single-target
    _Weaponskill_TrueAero = 25899, // Boss->self, 5.0s cast, single-target
    _Weaponskill_TrueAero1 = 25900, // _Gen_Hermes->player, no cast, range 40 width 6 rect
    _Weaponskill_TrueAero2 = 25901, // _Gen_Hermes->self, 2.5s cast, range 40 width 6 rect
    _Ability_Interject = 25084, // _Gen_EmetSelch->Boss, no cast, single-target
    _Spell_TrueMedica = 25494, // _Gen_Venat->self, 2.0s cast, range 20 circle
    _Weaponskill_Quadruple = 25894, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Hermetica2 = 25895, // Boss->self, 12.0s cast, single-target
    _Weaponskill_TrueAeroIV2 = 27837, // _Gen_Karukeion->self, 10.0s cast, range 50 width 10 rect
    _Weaponskill_TrueAeroII = 25896, // Boss->self, 5.0s cast, single-target
    _Weaponskill_TrueAeroII1 = 25897, // _Gen_Hermes->player, 5.0s cast, range 6 circle
    _Weaponskill_TrueAeroII2 = 25898, // _Gen_Hermes->location, 3.5s cast, range 6 circle
    _Weaponskill_TrueTornado3 = 25904, // Boss->self, no cast, single-target
    _Weaponskill_TrueTornado4 = 25906, // _Gen_Hermes->location, 2.5s cast, range 4 circle
    _Ability_1 = 3269, // _Gen_Hythlodaeus->self, no cast, single-target
    _Weaponskill_TrueBravery = 25907, // Boss->self, 5.0s cast, single-target
}

class Trismegistos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_Trismegistos));
class TrueTornado(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_TrueTornado));
class TrueTornado2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TrueTornado4), 4);
class CosmicKiss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CosmicKiss), new AOEShapeCircle(10));
class Meteor(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID._Gen_Meteor), ArenaColor.Object, true);
    }
}

class WindBlocks(BossModule module) : BossComponent(module)
{
    private byte Index = 0;
    private uint State = 0;

    public override void OnEventEnvControl(byte index, uint state)
    {
        Index = index;
        State = state;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Index > 0)
            hints.Add($"Index: {Index:D2}", false);
        if (State > 0)
            hints.Add($"State: {State:X8}", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var wind in Module.Enemies(OID._Gen_Karukeion))
        {
            Arena.Actor(wind, wind.CastInfo == null ? ArenaColor.PlayerGeneric : ArenaColor.Object, true);
        }
    }
}

class TrueAeroIV(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TrueAeroIV), new AOEShapeRect(50, 5));
class TrueAeroIV2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TrueAeroIV1), new AOEShapeRect(50, 5), maxCasts: 4);
class TrueAeroIV3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TrueAeroIV2), new AOEShapeRect(50, 5), maxCasts: 4);
class WindSafe(BossModule module) : Components.GenericAOEs(module)
{
    private IEnumerable<Actor> Meteors => Module.Enemies(OID._Gen_Meteor);
    private readonly List<(Actor source, AOEInstance aoe)> SafeZones = [];

    // TODO this is wrong
    private readonly float SafeZoneWidth = 5;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => SafeZones.Select(x => x.aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_TrueAeroIV1)
        {
            NumCasts++;
            var meteorBlocking = Meteors.FirstOrDefault(x => x.Position.InRect(caster.Position, spell.Rotation, 50, 0, 5) && (NumCasts <= 4 || x.ModelState.AnimState2 != 1));
            if (meteorBlocking != null)
                SafeZones.Add((caster, new AOEInstance(new AOEShapeRect(50, SafeZoneWidth), meteorBlocking.Position, spell.Rotation, spell.NPCFinishAt, ArenaColor.SafeFromAOE, false)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_TrueAeroIV1)
            SafeZones.RemoveAll(x => x.source == caster);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        // reset cast counter for next iteration of mechanic
        if (!Meteors.Any())
            NumCasts = 0;
    }
}

class TrueAero(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID._Weaponskill_TrueAero))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            foreach (var player in WorldState.Party.WithoutSlot())
                CurrentBaits.Add(new(caster, player, new AOEShapeRect(40, 3), spell.NPCFinishAt));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

class TrueAero2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TrueAero2), new AOEShapeRect(40, 3));
class TrueBravery(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID._Weaponskill_TrueBravery));
class TrueAeroII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_TrueAeroII1), 6);

class HermesStates : StateMachineBuilder
{
    public HermesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Trismegistos>()
            .ActivateOnEnter<TrueTornado>()
            .ActivateOnEnter<TrueTornado2>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<WindBlocks>()
            .ActivateOnEnter<TrueAeroIV>()
            .ActivateOnEnter<TrueAeroIV3>()
            .ActivateOnEnter<WindSafe>()
            .ActivateOnEnter<TrueAero>()
            .ActivateOnEnter<TrueAero2>()
            .ActivateOnEnter<TrueBravery>()
            .ActivateOnEnter<TrueAeroII>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10399)]
public class Hermes(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(20));
