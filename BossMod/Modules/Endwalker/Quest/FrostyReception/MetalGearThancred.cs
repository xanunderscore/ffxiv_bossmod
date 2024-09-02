using RID = BossMod.Roleplay.AID;

namespace BossMod.Endwalker.Quest.FrostyReception;

class MetalGearThancred(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget?.InCombat ?? false)
        {
            Hints.RecommendedRangeToTarget = 3;
            switch (ComboAction)
            {
                case RID.BrutalShellFR:
                    UseAction(RID.SolidBarrelFR, primaryTarget);
                    break;
                case RID.KeenEdgeFR:
                    UseAction(RID.BrutalShellFR, primaryTarget);
                    break;
                default:
                    UseAction(RID.KeenEdgeFR, primaryTarget);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (actor.FindStatus(Roleplay.SID.SwiftDeception) != null)
            return;

        foreach (var e in WorldState.Actors.Where(a => !a.IsAlly && a.IsTargetable && !a.IsDead && actor.DistanceToHitbox(a) < 25 && !a.InCombat))
            hints.AddForbiddenZone(new AOEShapeCone(8 + e.HitboxRadius, 45.Degrees()), e.Position, e.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in WorldState.Actors.Where(a => !a.IsAlly && a.IsTargetable && !a.IsDead && pc.DistanceToHitbox(a) < 25))
        {
            if (e.InCombat)
            {
                Arena.Actor(e, ArenaColor.Enemy);
            }
            else
            {
                Arena.Actor(e, ArenaColor.Danger);
                Arena.ZoneCone(e.Position, 0, 8 + e.HitboxRadius, e.Rotation, 45.Degrees(), ArenaColor.AOE);
            }
        }
    }
}

class MetalGearCheckpoints(BossModule module) : BossComponent(module)
{
    public bool MaximaTalk1;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x10000002)
            MaximaTalk1 = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!MaximaTalk1)
            hints.InteractWithTarget = Module.Enemies(0x384C).FirstOrDefault(x => x.IsTargetable);
    }
}

class MetalGearBounds(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        Service.Log($"[MGT] DIRU {updateID:X8} {param1:X2} {param2:X2} {param3:X2} {param4:X2}");
    }
}
