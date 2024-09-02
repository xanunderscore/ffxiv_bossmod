namespace BossMod.Endwalker.Quest.FrostyReception;

class PostCarriage(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_GigaTempest));
class Ruination(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination), new AOEShapeCross(40, 4));
class Ruination2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Ruination1), new AOEShapeRect(30, 4));
class LockOn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LockOn), new AOEShapeCircle(6));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_ResinBomb), 5);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_MagitekCannon), 6);
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bombardment1), 6);
