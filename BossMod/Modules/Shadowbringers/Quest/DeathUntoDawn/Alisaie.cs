using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;

class AlisaieAI(BossModule module) : Components.DeprecatedRoleplayModule(module)
{
    public const ushort StatusParam = 157;

    public override void Execute(Actor? primaryTarget)
    {
        if (primaryTarget == null || primaryTarget.OID == 0x1EB183)
            return;

        switch (ComboAction)
        {
            case RID.ShbVerfire:
                UseAction(RID.ShbVeraero, primaryTarget);
                break;
            case RID.ShbVeraero:
                UseAction(RID.ShbVerstone, primaryTarget);
                break;
            case RID.ShbVerstone:
                UseAction(RID.ShbVerflare, primaryTarget);
                break;
            case RID.ShbCorpsACorps:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.ShbEnchantedRiposte, primaryTarget);
                break;
            case RID.ShbEnchantedRiposte:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.ShbEnchantedZwerchhau, primaryTarget);
                break;
            case RID.ShbEnchantedZwerchhau:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.ShbEnchantedRedoublement, primaryTarget);
                break;
            case RID.ShbEnchantedRedoublement:
                Hints.RecommendedRangeToTarget = 3;
                UseAction(RID.ShbDisplacement, primaryTarget);
                break;
            case RID.ShbDisplacement:
                UseAction(RID.ShbVerholy, primaryTarget);
                break;
            case RID.ShbVerholy:
                UseAction(RID.ShbScorch, primaryTarget);
                break;
            default:
                UseAction(RID.ShbCorpsACorps, primaryTarget);
                UseAction(RID.ShbVerfire, primaryTarget);
                break;
        }

        UseAction(RID.ShbFleche, primaryTarget);
        UseAction(RID.ShbContreSixte, primaryTarget);
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
