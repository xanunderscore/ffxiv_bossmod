using BossMod.Autorotation;
using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn.P2;

public enum OID : uint
{
    Boss = 0x3200,
    Fetters = 0x3218
}

class UriangerAI(WorldState ws) : StatelessRotation(ws, 25)
{
    public const ushort StatusParam = 158;

    private Actor? LunarOdin => Hints.PriorityTargets.FirstOrDefault(x => x.Actor.OID == (uint)OID.Boss)?.Actor;
    private Actor? Fetters => Hints.PriorityTargets.FirstOrDefault(x => x.Actor.OID == (uint)OID.Fetters)?.Actor;

    private float HeliosLeft(Actor p) => p.IsTargetable ? StatusDetails(p, 836, Player.InstanceID).Left : float.MaxValue;

    protected override void Exec(Actor? primaryTarget)
    {
        /*
        if (MP >= 700)
        {
            if (Math.Min(HeliosLeft(Thancred), HeliosLeft(Yshtola)) < 3)
                UseAction(RID.AspectedHelios, Player);
        }
        */

        foreach (var p in World.Party.WithoutSlot())
        {
            if (p.OID == 0x2E2E && p.HPMP.CurHP < 20000)
                UseAction(RID.Benefic, p);
        }

        UseAction(RID.MaleficIII, Fetters ?? LunarOdin);

        if (Player.FindStatus(Roleplay.SID.DestinyDrawn) != null)
        {
            if (ComboAction == RID.DestinyDrawn)
                UseAction(RID.LordOfCrowns, Fetters ?? LunarOdin, -100);

            if (ComboAction == RID.DestinysSleeve)
                UseAction(RID.TheScroll, Player, -100);
        }
        else
        {
            UseAction(RID.DestinyDrawn, Player, -100);
            UseAction(RID.DestinysSleeve, Player, -100);
        }

        UseAction(RID.FixedSign, Player, -150);
    }
}

class Fetters(BossModule module) : Components.Adds(module, (uint)OID.Fetters);
class AutoUri(BossModule module) : Components.RotationModule<UriangerAI>(module);
class GunmetalSoul(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(0x1EB1D5).Select(e => new AOEInstance(new AOEShapeDonut(4, 100), e.Position));
}
class LunarGungnir(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_LunarGungnir), 6);
class LunarGungnir2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_LunarGungnir1), 6);
class Gungnir(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Gungnir2), new AOEShapeCircle(10));
class Gagnrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Gagnrath), new AOEShapeRect(50, 2));
class GungnirSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), 189, ActionID.MakeSpell(AID._Weaponskill_Gungnir3), 5.3f, centerAtTarget: true);
class LeftZantetsuken(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LeftZantetsuken1), new AOEShapeRect(70, 19.5f))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.Position + c.Rotation.ToDirection().OrthoL() * 16, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Color, Risky));
}
class RightZantetsuken(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RightZantetsuken1), new AOEShapeRect(70, 19.5f))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.Position + c.Rotation.ToDirection().OrthoR() * 16, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Color, Risky));
}

public class LunarOdinStates : StateMachineBuilder
{
    public LunarOdinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoUri>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<LunarGungnir>()
            .ActivateOnEnter<Gungnir>()
            .ActivateOnEnter<Gagnrath>()
            .ActivateOnEnter<GungnirSpread>()
            .ActivateOnEnter<GunmetalSoul>()
            .ActivateOnEnter<LunarGungnir2>()
            .ActivateOnEnter<LeftZantetsuken>()
            .ActivateOnEnter<RightZantetsuken>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 780, NameID = 10034)]
public class LunarOdin(WorldState ws, Actor primary) : BossModule(ws, primary, new(146.5f, 84.5f), new ArenaBoundsCircle(20));
