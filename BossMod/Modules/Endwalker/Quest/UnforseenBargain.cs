using RID = BossMod.Roleplay.AID;

namespace BossMod.Endwalker.Quest.UnforseenBargain;

public enum OID : uint
{
    _Gen_VisitantBlackguard = 0x3EA2, // R1.700, x4
    _Gen_VisitantTaurus = 0x3EA7, // R1.680, x4 (spawn during fight)
    _Gen_VisitantArchDemon = 0x3EA3, // R1.000, x1 (spawn during fight)
    _Gen_VisitantPersona = 0x3D74, // R1.600, x1
    _Gen_VisitantDahak = 0x3D75, // R2.750, x3 (spawn during fight)
    _Gen_VisitantVoidskipper = 0x3D72, // R1.080, x0 (spawn during fight)
    _Gen_Furcas = 0x233C, // R0.500, x0 (spawn during fight), Helper type
    _Gen_Furcas1 = 0x3D71, // R6.000, x0 (spawn during fight)
    _Gen_Hellsfire = 0x3ED4, // R0.600, x0 (spawn during fight)
    AlphiShield = 0x1EB87A
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 3EA2/3EA7->player, no cast, single-target
    _Weaponskill_VoidSlash = 33027, // 3EA2->self, 4.0s cast, range 8+R 90-degree cone
    _Weaponskill_Voidblood = 33026, // 3EA7->self, 4.0s cast, range 6 circle
    _AutoAttack_Attack1 = 6499, // 3D75->player, no cast, single-target
    _Weaponskill_RottenBreath = 31795, // 3D75->self, no cast, range 6+R ?-degree cone
    _AutoAttack_Attack2 = 872, // 3EA3->player, no cast, single-target
    _Weaponskill_InnerDemons = 33042, // 3D74->self, 6.0s cast, range 40 circle
    _Weaponskill_StraightSpindle = 31796, // 3D72->self, 4.0s cast, range 50+R width 5 rect
    _AutoAttack_ = 33023, // 3D71->player, no cast, single-target
    _Ability_SinisterSphere = 33003, // 3D71->self, 4.0s cast, single-target
    _Ability_Explosion = 33004, // 233C->self, 10.0s cast, range 5 circle
    _Ability_UnmitigatedExplosion = 33005, // 233C->self, no cast, range 60 circle
    _Weaponskill_JongleursX = 31802, // 3D71->player, 4.0s cast, single-target
    _Ability_VoidTorch = 33006, // 3D71->self, 3.0s cast, single-target
    _Ability_VoidTorch1 = 33007, // 233C->location, 3.0s cast, range 6 circle
    _Weaponskill_HellishScythe = 31800, // 3D71->self, 5.0s cast, range 10 circle
    _Ability_FlameBlast = 33008, // 3ED4->self, 4.0s cast, range 80+R width 4 rect
    _Weaponskill_Blackout = 31798, // 3D72->self, 13.0s cast, range 60 circle
    _Weaponskill_AbyssalShot = 33028, // _Gen_VisitantArchDemon->self, 4.0s cast, range 40+R width 4 rect
    _Weaponskill_JestersReward = 33031, // _Gen_Furcas1->self, 6.0s cast, range 28 180-degree cone
    _Spell_Blackout = 31801, // _Gen_Furcas1->self, 4.0s cast, range 60 circle
    _Ability_UnmitigatedExplosion1 = 33039, // _Gen_Furcas->self, no cast, range 60 circle

    _Spell_Cackle = 31820, // 3D76->player/3D80, 4.0s cast, single-target
    _Weaponskill_ChainOfCommands = 31813, // 3D76->self, 9.0s cast, single-target
    _Weaponskill_StraightSpindle1 = 31808, // 3D78->self, 5.0s cast, range 50+R width 5 rect
    _Weaponskill_Dark = 31815, // _Gen_Furcas->location, 5.0s cast, range 10 circle
    _Weaponskill_StraightSpindle2 = 31809, // 3D78->self, 9.0s cast, range 50+R width 5 rect
    _Spell_EvilMist = 31825, // 3D76->self, 5.0s cast, range 60 circle
    _Weaponskill_Decay = 32857, // 3D78->self, 13.0s cast, range 60 circle
    _Ability_SinisterSphere1 = 33009, // 3D76->self, 4.0s cast, single-target
    _Ability_Explosion1 = 33010, // _Gen_Furcas->self, 10.0s cast, range 5 circle
    _Ability_UnmitigatedExplosion2 = 33011, // _Gen_Furcas->self, no cast, range 60 circle
    _Weaponskill_Hellsnap = 31816, // 3D76->3D80, 5.0s cast, range 6 circle
    _Weaponskill_VoidEvocation = 31821, // 3D76->self, no cast, single-target
    _Weaponskill_VoidEvocation1 = 31822, // 3D76->self, no cast, single-target
    _Spell_VoidEvocation = 31823, // _Gen_Furcas->self, 1.5s cast, range 60 circle

}

class VoidSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VoidSlash), new AOEShapeCone(9.7f, 45.Degrees()));
class Voidblood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Voidblood), new AOEShapeCircle(6));
class InnerDemons(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_InnerDemons));
class StraightSpindle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StraightSpindle), new AOEShapeRect(51.08f, 2.5f));
class Explosion(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Ability_Explosion), 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var t in Towers)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(5, 100), t.Position);
            break;
        }
    }
}
class VoidTorch(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_VoidTorch1), 6);
class HellishScythe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HellishScythe), new AOEShapeCircle(10));
class FlameBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_FlameBlast), new AOEShapeRect(80.6f, 2));
class AbyssalShot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AbyssalShot), new AOEShapeRect(41, 2));
class JestersReward(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_JestersReward), new AOEShapeCone(28, 90.Degrees()));
class Blackout(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_Blackout));
class JongleursX(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_JongleursX), "Zerobuster");

public class ZeroAI(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        Hints.RecommendedRangeToTarget = 3;

        UseAction(RID.Communio, primaryTarget);

        var numAOETargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

        if (numAOETargets > 2)
        {
            if (ComboAction == RID.SpinningScythe)
                UseAction(RID.NightmareScythe, Player);

            UseAction(RID.SpinningScythe, Player);
        }
        else
        {
            switch (ComboAction)
            {
                case RID.WaxingSlice:
                    UseAction(RID.InfernalSlice, primaryTarget);
                    break;
                case RID.Slice:
                    UseAction(RID.WaxingSlice, primaryTarget);
                    break;
                default:
                    UseAction(RID.Slice, primaryTarget);
                    break;
            }
        }

        UseAction(RID.Engravement, primaryTarget, -100);
        UseAction(RID.Bloodbath, Player, -100);
    }
}

class Dark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Dark), 10);
class StraightSpindleFast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StraightSpindle1), new AOEShapeRect(50, 2.5f));
class StraightSpindleSlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_StraightSpindle2), new AOEShapeRect(50, 2.5f))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<StraightSpindleFast>()?.Casters.Count > 0)
            yield break;
        else
            foreach (var e in base.ActiveAOEs(slot, actor))
                yield return e;
    }
}

class Hellsnap(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Hellsnap), 6, 1);

class Explosion1(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Ability_Explosion1), 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var t in Towers)
        {
            if (!WorldState.Actors.Exclude(actor).Any(x => x.IsAlly && x.IsTargetable && x.Position.InCircle(t.Position, 5)))
            {
                hints.AddForbiddenZone(new AOEShapeDonut(5, 100), t.Position);
                break;
            }
        }
    }
}

class AlphiShield(BossModule module) : BossComponent(module)
{
    private Actor? Shield => Module.Enemies(OID.AlphiShield).FirstOrDefault();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield is Actor s && !actor.Position.InCircle(s.Position, 5))
            hints.Add("Take cover!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield is Actor s)
            hints.AddForbiddenZone(new AOEShapeDonut(5, 100), s.Position);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield is Actor s)
            Arena.AddCircleFilled(s.Position, 5, ArenaColor.SafeFromAOE);
    }
}

public class ZeroStates : StateMachineBuilder
{
    public ZeroStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 910;
        ushort GetRPParam() => (ushort)((Module.Raid.Player()?.FindStatus(Roleplay.SID.RolePlaying)?.Extra ?? 0) & 0xFF);
        bool P1End() => GetRPParam() == 0 || DutyEnd();

        TrivialPhase()
            .ActivateOnEnter<ZeroAI>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<Voidblood>()
            .ActivateOnEnter<InnerDemons>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<VoidTorch>()
            .ActivateOnEnter<HellishScythe>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<AbyssalShot>()
            .ActivateOnEnter<JestersReward>()
            .ActivateOnEnter<Blackout>()
            .ActivateOnEnter<JongleursX>()
            .Raw.Update = P1End;
        TrivialPhase(1)
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<StraightSpindleFast>()
            .ActivateOnEnter<StraightSpindleSlow>()
            .ActivateOnEnter<Hellsnap>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<AlphiShield>()
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 910, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class Zero(WorldState ws, Actor primary) : BossModule(ws, primary, new(97.85f, 286), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }
}

