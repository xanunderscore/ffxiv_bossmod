using BossMod.Autorotation;

namespace BossMod.QuestBattle.Endwalker.MSQ;

class ImperialAI(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.75f && World.Client.DutyActions[0].CurCharges > 0)
            UseAction(Roleplay.AID.MedicalKit, Player, -50);

        if (primaryTarget is not { IsAlly: false })
            return;

        if (Player.InCombat)
            UseAction(Roleplay.AID.RampartIFTC, Player, -50);

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

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 20)))
            if (!h.Actor.InCombat && !h.Actor.Position.AlmostEqual(new(111, -317), 10))
                hints.AddForbiddenZone(ShapeDistance.Cone(h.Actor.Position, 8.5f + h.Actor.HitboxRadius, h.Actor.Rotation, 45.Degrees()));

        _ai.Execute(player, hints);
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => hints.ForcedMovement = new(0, 0, 1))
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z > -320);
            }),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x3506))
            .WithInteract(0x3506)
            .With(obj => obj.OnDutyActionsChange += (op) => obj.CompleteIf(op.Slot0.Action.ID == 27315)),

        new QuestObjective(ws)
            .WithInteract(0x3506)
            .With(obj => obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1 && p1 == 4 && p2 == 8)),

        new QuestObjective(ws)
            .WithInteract(0x1EB456)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76DF))
    ];
}

