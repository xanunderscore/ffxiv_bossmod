using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;

class UriangerAI(BossModule module) : Components.RoleplayModule(module)
{
    public const ushort StatusParam = 158;

    private Actor Confluence => Module.Enemies(0x2E2E)[0];
    private Actor Thancred => Module.Enemies(0x31EB)[0];
    private Actor Yshtola => Module.Enemies(0x31EC)[0];
    private Actor? LunarOdin => Module.Enemies(0x3200).FirstOrDefault(x => x.IsTargetable);
    private Actor? Fetters => Module.Enemies(0x3218).FirstOrDefault(x => x.IsTargetable);

    private long ActorHP(Actor p) => Math.Max(0, p.HPMP.CurHP + WorldState.PendingEffects.PendingHPDifference(p.InstanceID));

    private float HeliosLeft(Actor p) => p.IsTargetable ? StatusDetails(p, 836, Player.InstanceID).Left : float.MaxValue;

    public override void Execute(Actor? primaryTarget)
    {
        Hints.ForcedTarget = (new Actor[] { Thancred, Yshtola }).Where(x => x.IsTargetable).MaxBy(Player.DistanceToHitbox);
        Hints.RecommendedRangeToTarget = 13;

        if (MP >= 700)
        {
            if (Math.Min(HeliosLeft(Thancred), HeliosLeft(Yshtola)) < 3)
                UseAction(RID.AspectedHelios, Player);
        }

        Service.Log($"{Fetters}, {LunarOdin}");

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
