namespace BossMod.Stormblood.Quest.AnArtForTheLiving;

public enum OID : uint
{
    Boss = 0x1CBA,
    Helper = 0x233C,
    _Gen_AetherochemicalExplosive = 0x1CD5, // R1.000, x1 (spawn during fight)
    _Gen_HighSummonerSari = 0x1CB6, // R0.500, x1
    _Gen_FX1975 = 0x1CD6, // R4.500, x1
    _Gen_ = 0x1CD7, // R0.500, x0 (spawn during fight)
    _Gen_FX1979 = 0x1CD8, // R5.000, x0 (spawn during fight)
    _Gen_SummonedMinotaur = 0x1CD1, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_PiercingLaser = 8683, // Boss->self, 3.0s cast, range 30+R width 6 rect
    _Spell_AllaganBioII = 8592, // 1CB6->player/1CB7, 3.0s cast, single-target
    _AutoAttack_Attack = 872, // 1CD6->1CB8, no cast, single-target
    _Weaponskill_ = 8639, // 1CD7->self, no cast, single-target
    _Spell_AllaganRuinIII = 8594, // 1CB6->1CB7, 2.0s cast, single-target
    _Weaponskill_AetherochemicalExplosion = 8640, // 1CD5->self, no cast, range 12 circle
    _Ability_ArtificialTrance = 8566, // 1CB6->self, 4.0s cast, single-target
    _Ability_Summon = 8632, // Boss->self, 5.0s cast, range 80 circle
    _AutoAttack_Attack1 = 3355, // 1CD8->1CB8, no cast, range 6+R ?-degree cone
    _Spell_HighVoltage = 8682, // Boss->self, 3.0s cast, range 30 circle
    _Weaponskill_NerveGas = 8707, // 1CD8->self, 3.0s cast, range 30+R 120-degree cone
    _Weaponskill_NerveGasLeft = 8708, // _Gen_FX1979->self, 3.0s cast, range 30+R 180-degree cone
    _Weaponskill_NerveGasRight = 8709, // 1CD8->self, 3.0s cast, range 30+R 180-degree cone
    _Weaponskill_ZoomIn = 8698, // 1CD1->players, no cast, width 8 rect charge
    _Weaponskill_111TonzeSwing = 8697, // 1CD1->self, 4.0s cast, range 8+R circle
    _Weaponskill_11TonzeSwipe = 8699, // 1CD1->self, 3.0s cast, range 5+R ?-degree cone
    _Spell_AllaganBlight = 8593, // _Gen_HighSummonerSari->player, 3.0s cast, single-target
    _Ability_ = 4731, // _Gen_AetherochemicalExplosive->self, no cast, single-target
}

public enum SID : uint
{
    Invincibility = 325
}

class OneOneOneTonzeSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_111TonzeSwing), new AOEShapeCircle(12));
class OneOneTonzeSwipe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_11TonzeSwipe), new AOEShapeCone(9, 45.Degrees())); // may be the wrong angle

class NerveGas1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NerveGas), new AOEShapeCone(35, 60.Degrees()));
class NerveGas2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NerveGasRight), new AOEShapeCone(35, 90.Degrees()));
class NerveGas3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NerveGasLeft), new AOEShapeCone(35, 90.Degrees()));

class PiercingLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_PiercingLaser), new AOEShapeRect(33.68f, 3));

class AetherochemicalExplosive(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, bool Primed, DateTime Activation)> Explosives = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Explosives.Where(e => !e.Actor.IsDead || !e.Primed).Select(e => new AOEInstance(new AOEShapeCircle(5), e.Actor.Position, Activation: e.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID._Gen_)
        {
            Explosives.Add((actor, false, WorldState.CurrentTime.AddSeconds(3)));
        }

        if ((OID)actor.OID is OID._Gen_AetherochemicalExplosive)
        {
            var slot = Explosives.FindIndex(e => e.Actor.Position.AlmostEqual(actor.Position, 1));
            if (slot >= 0)
                Explosives[slot] = (actor, true, Explosives[slot].Activation);
            else
                Module.ReportError(this, $"found explosive {actor} with no matching telegraph");
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_AetherochemicalExplosive)
            Explosives.RemoveAll(e => e.Actor.Position.AlmostEqual(actor.Position, 1));
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x1CB6, 0x1CD1, 0x1CD6, 0x1CD8])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}

class SummoningNodeStates : StateMachineBuilder
{
    public SummoningNodeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingLaser>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<NerveGas1>()
            .ActivateOnEnter<NerveGas2>()
            .ActivateOnEnter<NerveGas3>()
            .ActivateOnEnter<AetherochemicalExplosive>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneTonzeSwipe>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68165, NameID = 6695)]
public class SummoningNode(WorldState ws, Actor primary) : BossModule(ws, primary, new(-111, -295), ArenaBounds)
{
    private static readonly List<WDir> vertices = [
        new(-4.5f, 22.66f),
        new(4.5f, 22.66f),
        new(18f, 14.75f),
        new(22.2f, 7.4f),
        new(22.7f, 7.4f),
        new(22.7f, -7.4f),
        new(22.2f, -7.4f),
        new(18.15f, -15.77f),
        new(4.5f, -23.68f),
        new(-4.5f, -23.68f),
        new(-18.15f, -15.77f),
        new(-22.2f, -7.4f),
        new(-22.7f, -7.4f),
        new(-22.7f, 6.4f),
        new(-22.2f, 6.4f),
        new(-18f, 14.75f)
    ];

    public static readonly ArenaBoundsCustom ArenaBounds = new(30, new(vertices));
}
