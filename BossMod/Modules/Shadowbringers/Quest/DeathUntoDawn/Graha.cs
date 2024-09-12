using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;
class GrahaAI(BossModule module) : Components.DeprecatedRoleplayModule(module)
{
    public const ushort StatusParam = 159;

    private Actor Confluence => Module.Enemies(0x2E2E)[0];
    private IEnumerable<Actor> Adds => WorldState.Actors.Where(x => (OID)x.OID is OID.MoonGana or OID.SpiritGana or OID.RavanasWill && x.IsTargetable && !x.IsDead);

    // Ravana's Wills just move to boss, whereas butterflies are only a threat once they start casting
    private bool ShouldBreak(Actor a) => StatusDetails(a, Roleplay.SID.Break, Player.InstanceID).Left == 0 && ((OID)a.OID == OID.RavanasWill || a.CastInfo != null);

    public override void Execute(Actor? primaryTarget)
    {
        if (Adds.Any())
        {
            if ((Player.Position - Confluence.Position).Length() > 1)
                Hints.ForcedMovement = Player.DirectionTo(Confluence).ToVec3();
        }

        if (Adds.Any(ShouldBreak))
        {
            UseAction(RID.Break, Player);
            return;
        }

        if (MP >= 1000 && Player.HPMP.CurHP * 3 < Player.HPMP.MaxHP)
            UseAction(RID.CureII, Player);

        if (MP < 800)
            UseAction(RID.AllaganBlizzardIV, primaryTarget);

        if (primaryTarget?.OID == 0x3201)
        {
            var thunder = StatusDetails(primaryTarget, Roleplay.SID.ThunderIV, Player.InstanceID);
            if (thunder.Left < 3)
                UseAction(RID.ThunderIV, primaryTarget);
        }

        switch (ComboAction)
        {
            case RID.FireIV:
                UseAction(RID.FireIV2, primaryTarget);
                break;
            case RID.FireIV2:
                UseAction(RID.FireIV3, primaryTarget);
                break;
            case RID.FireIV3:
                UseAction(RID.Foul, primaryTarget);
                break;
            default:
                UseAction(RID.FireIV, primaryTarget);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}

class DirectionalParry(BossModule module) : Components.DirectionalParry(module, 0x3201);
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Explosion), new AOEShapeCross(80, 5), maxCasts: 2);
