using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;

class AlisaieAI(BossModule module) : Components.RoleplayModule(module)
{
    public const ushort StatusParam = 157;

    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget == null || primaryTarget.OID == 0x1EB183)
            return;

        switch (ComboAction)
        {
            case RID.Verfire:
                UseAction(RID.Veraero, primaryTarget);
                break;
            case RID.Veraero:
                UseAction(RID.Verstone, primaryTarget);
                break;
            case RID.Verstone:
                UseAction(RID.Verflare, primaryTarget);
                break;
            case RID.CorpsACorps:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EnchantedRiposte, primaryTarget);
                break;
            case RID.EnchantedRiposte:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EnchantedZwerchhau, primaryTarget);
                break;
            case RID.EnchantedZwerchhau:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.EnchantedRedoublement, primaryTarget);
                break;
            case RID.EnchantedRedoublement:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.Displacement, primaryTarget);
                break;
            case RID.Displacement:
                UseAction(RID.Verholy, primaryTarget);
                break;
            case RID.Verholy:
                UseAction(RID.Scorch, primaryTarget);
                break;
            default:
                UseAction(RID.CorpsACorps, primaryTarget);
                UseAction(RID.Verfire, primaryTarget);
                break;
        }

        UseAction(RID.Fleche, primaryTarget);
        UseAction(RID.ContreSixte, primaryTarget);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.InteractWithTarget = Module.Enemies(0x1EB183).FirstOrDefault(x => x.IsTargetable);
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;

        if (hints.InteractWithTarget != null)
            Arena.Center = actor.Position;
    }
}

class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_AntiPersonnelMissile), 6);
class MRVMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MRVMissile), 12, maxCasts: 6);
