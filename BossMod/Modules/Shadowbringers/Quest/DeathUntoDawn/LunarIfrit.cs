namespace BossMod.Shadowbringers.Quest.DeathUntoDawn;

public class IfritHints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID == OID.InfernalNail ? 1 : 0;
    }
}

class RadiantPlume(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RadiantPlume1), new AOEShapeCircle(8));
class CrimsonCyclone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_CrimsonCyclone), new AOEShapeRect(49, 9), maxCasts: 3);
