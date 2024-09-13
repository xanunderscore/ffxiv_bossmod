namespace BossMod.Shadowbringers.Quest.FadedMemories;

class Overcome(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Overcome), new AOEShapeCone(8, 60.Degrees()), 2);
class Skydrive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Skydrive), new AOEShapeCircle(5));

class SkyHighDrive(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle angle;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SkyHighDriveCCW:
                angle = -20.Degrees();
                return;
            case AID.SkyHighDriveCW:
                angle = 20.Degrees();
                return;
            case AID._Weaponskill_SkyHighDrive1:
                if (angle != default)
                {
                    Sequences.Add(new(new AOEShapeRect(40, 4), caster.Position, spell.Rotation, angle, Module.CastFinishAt(spell, 0.5f), 0.6f, 10, 4));
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_SkyHighDrive1 or AID._Weaponskill_SkyHighDrive2)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
            if (Sequences.Count == 0)
                angle = default;
        }
    }
}

class AvalancheAxe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe1), new AOEShapeCircle(10));
class AvalancheAxe2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe2), new AOEShapeCircle(10));
class AvalancheAxe3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AvalancheAxe3), new AOEShapeCircle(10));
class OvercomeAllOdds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_OvercomeAllOdds), new AOEShapeCone(60, 15.Degrees()), 1)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts > 0)
            MaxCasts = 2;
    }
}
class Soulflash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Soulflash1), new AOEShapeCircle(4));
class EtesianAxe(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_EtesianAxe1), 15, kind: Kind.DirForward);
class Soulflash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Soulflash2), new AOEShapeCircle(8));

class GroundbreakerExaflares(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Groundbreaker1)
        {
            Lines.Add(new Line
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 6,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 8,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID._Weaponskill_Groundbreaker1 or (uint)AID._Weaponskill_Groundbreaker2)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class GroundbreakerCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker4), new AOEShapeCone(40, 45.Degrees()));
class GroundbreakerDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker6), new AOEShapeDonut(5, 20));
class GroundbreakerCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Groundbreaker8), new AOEShapeCircle(15));

class ArdbertStates : StateMachineBuilder
{
    public ArdbertStates(BossModule module) : base(module)
    {
        TrivialPhase(0)
            .ActivateOnEnter<SkyHighDrive>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<AvalancheAxe>()
            .ActivateOnEnter<AvalancheAxe2>()
            .ActivateOnEnter<AvalancheAxe3>()
            .ActivateOnEnter<OvercomeAllOdds>()
            .ActivateOnEnter<Soulflash>()
            .ActivateOnEnter<EtesianAxe>()
            .ActivateOnEnter<Soulflash2>()
            .ActivateOnEnter<GroundbreakerExaflares>()
            .ActivateOnEnter<GroundbreakerCone>()
            .ActivateOnEnter<GroundbreakerDonut>()
            .ActivateOnEnter<GroundbreakerCircle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 69311, NameID = 8258, PrimaryActorOID = (uint)OID.Ardbert)]
public class Ardbert(WorldState ws, Actor primary) : BossModule(ws, primary, new(-392, 780), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => hints.PrioritizeTargetsByOID(OID.Ardbert);
}
