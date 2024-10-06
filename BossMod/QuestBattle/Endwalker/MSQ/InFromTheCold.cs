using BossMod.Autorotation;

namespace BossMod.QuestBattle.Endwalker.MSQ;

class ImperialAI(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget is not { IsAlly: false })
            return;

        switch (ComboAction)
        {
            case Roleplay.AID.RiotBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50);
                UseAction(Roleplay.AID.RageOfHaloneIFTC, primaryTarget);
                break;
            case Roleplay.AID.FastBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50);
                UseAction(Roleplay.AID.RiotBladeIFTC, primaryTarget);
                break;
        }

        UseAction(Roleplay.AID.FastBladeIFTC, primaryTarget);
    }
}

[Quest(BossModuleInfo.Maturity.Contributed, 793)]
internal class InFromTheCold(WorldState ws) : QuestBattle(ws)
{
    private readonly ImperialAI _ai = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 20)))
            if (!h.Actor.InCombat)
                hints.AddForbiddenZone(ShapeDistance.Cone(h.Actor.Position, 8.5f + h.Actor.HitboxRadius, h.Actor.Rotation, 45.Degrees()));

        _ai.Execute(player, hints, maxCastTime);
    }
}

