
namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public enum OID : uint
{
    _Gen_CenterApa = 0x367E, // R2.400, x?
    _Gen_Berkanan = 0x344F, // R2.800, x?
    _Gen_EphemeralLushVegetationPatch = 0x8503, // R0.500, x?, GatheringPoint type
    _Gen_AethericGelatin = 0x37F8, // R3.000, x?
    _Gen_BlackCat = 0x233C, // R0.500, x?, 523 type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    _Gen_Actor0 = 0x0, // R340282346638528859811704183484516925440.000--340282346638528859811704183484516925440.000, x?, None type
    _Gen_Soulshade = 0x432C, // R5.610, x?
    Boss = 0x4329, // R3.993, x?
    _Gen_CopyCat = 0x432A, // R3.993, x?
    _Gen_ = 0x432B, // R1.000, x?
    _Gen_1 = 0x10075F, // R0.500, x?, EventNpc type
}

public enum AID : uint
{
    _AutoAttack_ = 39152, // 4329->player, no cast, single-target
    _Weaponskill_ = 37640, // 4329->location, no cast, single-target
    _Weaponskill_QuadrupleCrossing = 37948, // 4329->self, 5.0s cast, single-target
    _Weaponskill_QuadrupleCrossing1 = 37951, // 233C->self, no cast, range 100 ?-degree cone
    _Weaponskill_QuadrupleCrossing2 = 37949, // 4329->self, 1.0s cast, single-target
    _Weaponskill_QuadrupleCrossing3 = 37952, // 233C->self, 1.6s cast, range 100 45-degree cone
    _Weaponskill_QuadrupleCrossing4 = 37950, // 4329->self, 1.0s cast, single-target
    _Weaponskill_BiscuitMaker = 38037, // 4329->player, 5.0s cast, single-target
    _Weaponskill_BiscuitMaker1 = 38038, // 4329->player, no cast, single-target
    _Spell_NineLives = 37985, // 4329->self, 3.0s cast, single-target
    _Weaponskill_OneTwoPaw = 37945, // 4329->self, 5.0s cast, single-target
    _Weaponskill_OneTwoPaw1 = 37947, // 233C->self, 6.0s cast, range 100 ?-degree cone
    _Weaponskill_OneTwoPaw2 = 37946, // 233C->self, 9.0s cast, range 100 ?-degree cone
    _Spell_Soulshade = 37986, // 4329->self, 3.0s cast, single-target
    _Weaponskill_1 = 37987, // 432C->self, no cast, single-target
    _Weaponskill_OneTwoPaw3 = 37991, // 432C->self, 5.0s cast, single-target
    _Weaponskill_OneTwoPaw4 = 37993, // 233C->self, 6.0s cast, range 100 ?-degree cone
    _Weaponskill_OneTwoPaw5 = 37992, // 233C->self, 9.0s cast, range 100 ?-degree cone
    _Weaponskill_QuadrupleSwipe = 37981, // 4329->self, 4.0+1.0s cast, single-target
    _Weaponskill_QuadrupleSwipe1 = 37982, // 233C->players, 5.0s cast, range 4 circle
    _Weaponskill_LeapingQuadrupleCrossing = 38959, // 4329->self, 5.0s cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing1 = 37976, // 4329->self, no cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing2 = 37979, // 233C->self, no cast, range 100 ?-degree cone
    _Weaponskill_LeapingQuadrupleCrossing3 = 37977, // 4329->self, 1.0s cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing4 = 37980, // 233C->self, 1.6s cast, range 100 45-degree cone
    _Weaponskill_LeapingQuadrupleCrossing5 = 37978, // 4329->self, 1.0s cast, single-target
    _Weaponskill_QuadrupleSwipe2 = 38015, // 432C->self, 4.0+1.0s cast, single-target
    _Weaponskill_QuadrupleSwipe3 = 38016, // 233C->players, 5.0s cast, range 4 circle
    _Weaponskill_BloodyScratch = 38036, // 4329->self, 5.0s cast, range 100 circle
    _Weaponskill_2 = 37955, // 233C->self, 1.0s cast, range 10 width 10 rect
    _Weaponskill_3 = 39276, // 233C->self, 1.0s cast, range 10 width 10 rect
    _Weaponskill_Mouser = 37953, // 4329->self, 10.0s cast, single-target
    _Weaponskill_Mouser1 = 37956, // 233C->location, no cast, single-target
    _Weaponskill_Mouser2 = 38054, // 233C->self, no cast, range 10 width 10 rect
    _Weaponskill_4 = 37954, // 4329->self, no cast, single-target
    _Spell_Copycat = 37957, // 4329->self, 3.0s cast, single-target
    _Weaponskill_ElevateAndEviscerate = 37958, // 432A->self, 5.6s cast, single-target
    _Weaponskill_ElevateAndEviscerate1 = 37959, // 432A->player, no cast, single-target
    _Weaponskill_Shockwave = 37962, // 233C->self, no cast, range 60 width 10 cross
    _Weaponskill_Impact = 39251, // 233C->self, no cast, range 10 width 10 rect
    _Weaponskill_ElevateAndEviscerate2 = 37960, // 432A->self, 5.6s cast, single-target
    _Weaponskill_ElevateAndEviscerate3 = 37961, // 432A->player, no cast, single-target
    _Weaponskill_Impact1 = 39252, // 233C->self, no cast, range 10 width 10 rect
    _Weaponskill_GrimalkinGale = 39811, // 4329->self, no cast, single-target
    _Spell_Shockwave = 37963, // 4329->self, 6.0+1.0s cast, single-target
    _Spell_Shockwave1 = 37964, // 233C->self, 7.0s cast, range 30 circle
    _Spell_GrimalkinGale = 39812, // 233C->players, 5.0s cast, range 5 circle
    _Weaponskill_LeapingOneTwoPaw = 37966, // 4329->self, 5.0s cast, single-target
    _Weaponskill_LeapingOneTwoPaw1 = 37972, // 4329->self, no cast, single-target
    _Weaponskill_LeapingOneTwoPaw2 = 37974, // 233C->self, 0.8s cast, range 100 ?-degree cone
    _Weaponskill_LeapingOneTwoPaw3 = 37973, // 233C->self, 2.8s cast, range 100 ?-degree cone
    _Weaponskill_LeapingQuadrupleCrossing6 = 38995, // 432C->self, 5.0s cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing7 = 38010, // 432C->self, no cast, single-target
    _Spell_Nailchipper = 38021, // 4329->self, 7.0+1.0s cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing8 = 38013, // 233C->self, no cast, range 100 ?-degree cone
    _Spell_Nailchipper1 = 38022, // 233C->players, 8.0s cast, range 5 circle
    _Weaponskill_LeapingQuadrupleCrossing9 = 38011, // 432C->self, 1.0s cast, single-target
    _Weaponskill_LeapingQuadrupleCrossing10 = 38014, // 233C->self, 1.6s cast, range 100 45-degree cone
    _Weaponskill_LeapingQuadrupleCrossing11 = 38012, // 432C->self, 1.0s cast, single-target
    _Ability_ = 34722, // 233C->player, no cast, single-target
    _Weaponskill_LeapingOneTwoPaw4 = 38000, // 432C->self, 5.0s cast, single-target
    _Weaponskill_LeapingOneTwoPaw5 = 38006, // 432C->self, no cast, single-target
    _Spell_TempestuousTear = 38019, // 4329->self, 5.0+1.0s cast, single-target
    _Weaponskill_DoubleSwipe = 37983, // 4329->self, 4.0+1.0s cast, single-target
    _Weaponskill_DoubleSwipe1 = 37984, // 233C->players, 5.0s cast, range 5 circle
    _Weaponskill_LeapingQuadrupleCrossing12 = 37975, // 4329->self, 5.0s cast, single-target
    _Weaponskill_DoubleSwipe2 = 38017, // 432C->self, 4.0+1.0s cast, single-target
    _Weaponskill_DoubleSwipe3 = 38018, // 233C->players, 5.0s cast, range 5 circle
    _Weaponskill_LeapingOneTwoPaw6 = 37965, // 4329->self, 5.0s cast, single-target
    _Weaponskill_LeapingOneTwoPaw7 = 37969, // 4329->self, no cast, single-target
    _Weaponskill_LeapingOneTwoPaw8 = 37970, // 233C->self, 0.8s cast, range 100 ?-degree cone
    _Weaponskill_LeapingOneTwoPaw9 = 37971, // 233C->self, 2.8s cast, range 100 ?-degree cone
    _Weaponskill_LeapingQuadrupleCrossing13 = 38009, // 432C->self, 5.0s cast, single-target
    _Weaponskill_LeapingOneTwoPaw10 = 37999, // 432C->self, 5.0s cast, single-target
    _Weaponskill_LeapingOneTwoPaw11 = 38003, // 432C->self, no cast, single-target
    _Weaponskill_LeapingOneTwoPaw12 = 38004, // 233C->self, 0.8s cast, range 100 ?-degree cone
    _Spell_TempestuousTear1 = 38020, // 233C->players, no cast, range 100 width 6 rect
    _Weaponskill_LeapingOneTwoPaw13 = 38005, // 233C->self, 2.8s cast, range 100 ?-degree cone
    _Ability_1 = 26708, // 233C->player, no cast, single-target
    _Spell_Overshadow = 38039, // 4329->player, 5.0s cast, single-target
    _Spell_Overshadow1 = 38040, // 4329->players, no cast, range 100 width 5 rect
    _Weaponskill_SplinteringNails = 38041, // 4329->self, 5.0s cast, single-target
    _Weaponskill_SplinteringNails1 = 38042, // 233C->self, no cast, range 100 ?-degree cone
    _Weaponskill_RainingCats = 39611, // 4329->self, 6.0s cast, single-target
    _Weaponskill_RainingCats1 = 38045, // 233C->self, no cast, range 100 ?-degree cone
    _Spell_RainingCats = 38047, // 233C->players, no cast, range 4 circle
    _Weaponskill_RainingCats2 = 39612, // 4329->self, 5.0s cast, single-target
    _Weaponskill_RainingCats3 = 39613, // 4329->self, 5.0s cast, single-target
    _Weaponskill_5 = 38026, // 233C->location, 2.0s cast, width 6 rect charge
    _Weaponskill_6 = 38027, // 233C->self, 3.0s cast, range 11 circle
    _Weaponskill_7 = 38028, // 233C->location, 4.0s cast, width 6 rect charge
    _Weaponskill_8 = 38029, // 233C->self, 5.0s cast, range 11 circle
    _Weaponskill_9 = 38030, // 233C->location, 6.0s cast, width 6 rect charge
    _Weaponskill_10 = 38031, // 233C->self, 7.0s cast, range 11 circle
    _Weaponskill_11 = 38032, // 233C->location, 8.0s cast, width 6 rect charge
    _Weaponskill_12 = 38033, // 233C->self, 9.0s cast, range 11 circle
    _Weaponskill_13 = 38034, // 233C->location, 10.0s cast, width 6 rect charge
    _Weaponskill_14 = 38035, // 233C->self, 11.0s cast, range 11 circle
    _Weaponskill_15 = 39632, // 233C->location, 12.0s cast, width 6 rect charge
    _Weaponskill_PredaceousPounce = 39635, // 432A->location, 13.0s cast, single-target
    _Weaponskill_16 = 39633, // 233C->self, 13.0s cast, range 11 circle
    _Weaponskill_PredaceousPounce1 = 39704, // 233C->location, 13.5s cast, width 6 rect charge
    _Weaponskill_PredaceousPounce2 = 39709, // 233C->self, 14.0s cast, range 11 circle
    _Weaponskill_PredaceousPounce3 = 38024, // 432A->location, no cast, single-target
    _Weaponskill_PredaceousPounce4 = 39270, // 233C->location, 1.0s cast, width 6 rect charge
    _Weaponskill_PredaceousPounce5 = 38025, // 233C->self, 1.5s cast, range 11 circle
    _Weaponskill_OneTwoPaw6 = 37942, // 4329->self, 5.0s cast, single-target
    _Weaponskill_OneTwoPaw7 = 37943, // 233C->self, 6.0s cast, range 100 ?-degree cone
    _Weaponskill_OneTwoPaw8 = 37944, // 233C->self, 9.0s cast, range 100 ?-degree cone
    _Weaponskill_Mouser4 = 39822, // 4329->self, 8.0s cast, single-target
}

class BlackCatStates : StateMachineBuilder
{
    public BlackCatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        QuadrupleCrossing(id, 8.4f);
        BiscuitMaker(id + 0x10000, 4.5f);
        Cast(id + 0x10010, AID._Spell_NineLives, 13, 3);
        OneTwoPaw(id + 0x11000, 2);
        Cast(id + 0x11100, AID._Spell_Soulshade, 2, 3);
        LeapingQuadrupleCrossing(id + 0x12000, 2.6f);
        Cast(id + 0x12100, AID._Weaponskill_BloodyScratch, 5.6f, 5, "Raidwide");
        Mouser(id + 0x20000, 13.4f);

        Timeout(0x80000, 800, "Enrage");
    }

    private void QuadrupleCrossing(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_QuadrupleCrossing, delay, 5)
            .ActivateOnEnter<QuadrupleCrossing>().ActivateOnEnter<Soulshade>();
        ComponentCondition<QuadrupleCrossing>(id + 0x10, 0.8f, comp => comp.NumBaits == 4, "Baits 1");
        ComponentCondition<QuadrupleCrossing>(id + 0x20, 3, comp => comp.NumBaits == 8, "Baits 2");
        ComponentCondition<QuadrupleCrossing>(id + 0x30, 2.9f, comp => comp.NumAOEs == 4, "AOEs 1");
        ComponentCondition<QuadrupleCrossing>(id + 0x40, 3, comp => comp.NumAOEs == 8, "AOEs 2").DeactivateOnExit<QuadrupleCrossing>();
    }

    private void LeapingQuadrupleCrossing(uint id, float delay)
    {
        Condition(id, delay, () => Module.Enemies(OID._Gen_).Any());
        Timeout(id + 0x10, 0, "Tether appear").ActivateOnEnter<LeapingQuadrupleCrossing>();
        ComponentCondition<LeapingQuadrupleCrossing>(id + 0x20, 6.3f, comp => comp.NumBaits == 4, "Baits 1");
        ComponentCondition<LeapingQuadrupleCrossing>(id + 0x30, 3, comp => comp.NumBaits == 8, "Baits 2");
        ComponentCondition<LeapingQuadrupleCrossing>(id + 0x40, 2.9f, comp => comp.NumAOEs == 4, "AOEs 1");
        ComponentCondition<LeapingQuadrupleCrossing>(id + 0x50, 3, comp => comp.NumAOEs == 8, "AOEs 2").DeactivateOnExit<LeapingQuadrupleCrossing>();
        Swipes(id + 0x52, 0.8f);
    }

    private void BiscuitMaker(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_BiscuitMaker, delay, 5, "Tankbuster hit 1")
            .ActivateOnEnter<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<BiscuitMaker>(id + 2, 2f, comp => comp.NumCasts > 0, "Tankbuster hit 2")
            .DeactivateOnExit<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void OneTwoPaw(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_OneTwoPaw, delay, "Cleaves start")
            .ActivateOnEnter<OneTwoPaw>()
            .ActivateOnEnter<AddsOneTwoPaw>()
            .ActivateOnEnter<DoubleSwipe>()
            .ActivateOnEnter<QuadrupleSwipe>()
            .ActivateOnEnter<QuadrupleSwipe2>();
        ComponentCondition<OneTwoPaw>(id + 2, 6, comp => comp.Count > 0, "Cleave 1");
        ComponentCondition<OneTwoPaw>(id + 4, 3, comp => comp.Count > 1, "Cleave 2")
            .DeactivateOnExit<OneTwoPaw>();
        Cast(id + 0x10, AID._Spell_Soulshade, 1, 3, "Spawn adds");
        ComponentCondition<AddsOneTwoPaw>(id + 0x14, 17.5f, comp => comp.NumAOEs == 2, "Adds cleave 1");
        ComponentCondition<AddsOneTwoPaw>(id + 0x16, 3f, comp => comp.NumAOEs == 0, "Adds cleave 2");
        Swipes(id + 0x18, 3.2f);
    }

    private void Swipes(uint id, float delay)
    {
        Condition(id, delay, () => !(Module.FindComponent<QuadrupleSwipe>()?.Active ?? false) && !(Module.FindComponent<DoubleSwipe>()?.Active ?? false) && !(Module.FindComponent<QuadrupleSwipe2>()?.Active ?? false), "Stacks");
    }

    private void Mouser(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_Mouser, delay, "Mouser start")
            .ActivateOnEnter<Mouser>();
        ComponentCondition<Mouser>(id + 0x10, 10.1f, comp => comp.NumCasts >= 28, "Show tiles");
        ComponentCondition<Mouser>(id + 0x20, 10.1f, comp => comp.NumTiles == 4, "Destroy tiles").OnExit(() =>
        {
            var horiz = Module.FindComponent<Mouser>()!.IsHorizontal;
            Module.Arena.Bounds = horiz ? BlackCat.SmallBoundsEW : BlackCat.SmallBoundsNS;
        });
        Cast(id + 0x30, AID._Spell_Copycat, 7.8f, 3, "Clone");
    }

    //private void XXX(uint id, float delay)
}

class QuadrupleSwipe(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_QuadrupleSwipe1), 2, 2);
class QuadrupleSwipe2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_QuadrupleSwipe3), 2, 2);
class DoubleSwipe(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_DoubleSwipe1), 4, 4);

class BloodyScratch(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_BloodyScratch));

class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(DateTime, WPos)> _recorded = [];

    enum TileState
    {
        Normal = 0,
        Damaged = 1,
        Destroyed = 2
    }

    public bool IsHorizontal => TileStates[0] == TileState.Destroyed && TileStates[1] == TileState.Destroyed && TileStates[2] == TileState.Destroyed && TileStates[3] == TileState.Destroyed;

    public int NumTiles => TileStates.Count(x => x != TileState.Destroyed);

    private readonly TileState[] TileStates = new TileState[16];
    private static readonly List<Vector2> TileCenters = [
        new(85, 85),
        new(95, 85),
        new(105, 85),
        new(115, 85),
        new(85, 95),
        new(95, 95),
        new(105, 95),
        new(115, 95),
        new(85, 105),
        new(95, 105),
        new(105, 105),
        new(115, 105),
        new(85, 115),
        new(95, 115),
        new(105, 115),
        new(115, 115),
    ];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_2 or AID._Weaponskill_3)
        {
            NumCasts++;
            _recorded.Add((WorldState.CurrentTime.AddSeconds(10.45f), caster.Position));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _recorded.Where(x => x.Item1 > WorldState.CurrentTime).Zip(Enumerable.Range(1, int.MaxValue)).Select(aoe => new AOEInstance(new AOEShapeRect(5, 5, 5), aoe.First.Item2, default, aoe.First.Item1, aoe.Second < 4 ? ArenaColor.Danger : ArenaColor.AOE)).Take(6);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x20001)
            TileStates[index] = TileState.Damaged;
        if (state == 0x200010)
            TileStates[index] = TileState.Destroyed;
        if (state == 0x1000004)
            TileStates[index] = TileState.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        for (var i = 0; i < TileStates.Length; i++)
        {
            var tile = TileStates[i];
            switch (tile)
            {
                case TileState.Normal:
                    break;
                case TileState.Damaged:
                    new AOEShapeRect(5, 5, 5).Draw(Module.Arena, new WPos(TileCenters[i]), default, 0x20ffffff);
                    break;
                case TileState.Destroyed:
                    new AOEShapeRect(5, 5, 5).Draw(Module.Arena, new WPos(TileCenters[i]), default, 0x206666ff);
                    break;
            }
        }
    }
}

class LeapingQuadrupleCrossing(BossModule module) : Crossing(module, AID._Weaponskill_LeapingQuadrupleCrossing, () => module.Enemies(OID._Gen_).FirstOrDefault() ?? module.PrimaryActor, AID._Weaponskill_LeapingQuadrupleCrossing2, AID._Weaponskill_LeapingQuadrupleCrossing4);

abstract class Crossing(BossModule module, AID BossCast, Func<Actor?> Source, AID Protean1, AID Protean2) : Components.GenericBaitAway(module, ActionID.MakeSpell(BossCast))
{
    private bool BaitActive;
    private bool AOEsActive;
    private static readonly AOEShapeCone _shape = new(100, 22.5f.Degrees());

    private readonly List<Angle> _storedBaits = [];

    public int NumBaits;
    public int NumAOEs;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (BaitActive && Source() is Actor src)
            foreach (var target in Raid.WithoutSlot().SortedByRange(src.Position).Take(4))
                CurrentBaits.Add(new(src, target, _shape));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == BossCast)
            BaitActive = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == Protean1)
        {
            NumBaits++;
            _storedBaits.Add(caster.Rotation);
            if (NumBaits >= 8)
            {
                BaitActive = false;
                AOEsActive = true;
            }
        }

        if ((AID)spell.Action.ID == Protean2)
        {
            NumAOEs++;
            _storedBaits.RemoveAt(0);
            if (_storedBaits.Count == 0)
                AOEsActive = false;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (!AOEsActive || Source() is not Actor src)
            return;

        foreach (var angle in _storedBaits.Take(4))
            _shape.Draw(Module.Arena, src.Position, angle);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (!AOEsActive || Source() is not Actor src)
            return;

        foreach (var angle in _storedBaits.Take(4))
            if (_shape.Check(actor.Position, src.Position, angle))
                hints.Add("GTFO from aoe!");
    }
}

class QuadrupleCrossing(BossModule module) : Crossing(module, AID._Weaponskill_QuadrupleCrossing, () => module.PrimaryActor, AID._Weaponskill_QuadrupleCrossing1, AID._Weaponskill_QuadrupleCrossing3);

class BiscuitMaker(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_BiscuitMaker1))
{
    private ulong _firstTarget;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role != Role.Tank)
            return;

        if (Module.PrimaryActor.TargetID == _firstTarget)
            hints.Add(actor.InstanceID == _firstTarget ? "Past aggro to co-tank" : "Taunt");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_BiscuitMaker)
            _firstTarget = spell.TargetID;
    }
}

class OneTwoPaw(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OneTwoPaw2))
{
    public int Count;

    private AOEInstance? _paw1;
    private AOEInstance? _paw2;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Enumerable.Concat(Utils.ZeroOrOne(_paw1), Utils.ZeroOrOne(_paw2)).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_OneTwoPaw1)
            _paw1 = new AOEInstance(new AOEShapeCone(100, 90.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell));

        if ((AID)spell.Action.ID is AID._Weaponskill_OneTwoPaw2)
            _paw2 = new AOEInstance(new AOEShapeCone(100, 90.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_OneTwoPaw1)
        {
            _paw1 = null;
            Count++;
        }

        if ((AID)spell.Action.ID is AID._Weaponskill_OneTwoPaw2)
        {
            _paw2 = null;
            Count++;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

    }
}

class Soulshade(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID._Gen_Soulshade).Where(e => e.FindStatus(2193) != null), ArenaColor.Object, true);
    }
}

class AddsOneTwoPaw(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OneTwoPaw4))
{
    private static readonly AOEShapeCone _shape = new(100, 90.Degrees());

    private readonly List<AOEInstance> _aoes1 = [];
    private readonly List<AOEInstance> _aoes2 = [];

    public int NumAOEs => _aoes1.Count + _aoes2.Count;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Enumerable.Concat(_aoes1, _aoes2).Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_OneTwoPaw4)
            _aoes1.Add(new AOEInstance(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));

        if ((AID)spell.Action.ID == AID._Weaponskill_OneTwoPaw5)
            _aoes2.Add(new AOEInstance(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_OneTwoPaw4)
            _aoes1.Clear();

        if ((AID)spell.Action.ID == AID._Weaponskill_OneTwoPaw5)
            _aoes2.Clear();
    }
}

class AddsOneTwoPaw1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OneTwoPaw4), new AOEShapeCone(100, 90.Degrees()));
class AddsOneTwoPaw2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OneTwoPaw5), new AOEShapeCone(100, 90.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 986, NameID = 12686)]
public class BlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    public static readonly ArenaBoundsRect DefaultBounds = new(20, 20);
    public static readonly ArenaBoundsRect SmallBoundsNS = new(10, 20);
    public static readonly ArenaBoundsRect SmallBoundsEW = new(20, 10);
}
