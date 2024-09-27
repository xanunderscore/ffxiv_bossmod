using BossMod.Autorotation;

namespace BossMod.QuestBattle.Endwalker.MSQ;

class AutoThancred(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget is not { IsAlly: false })
            return;

        if (Player.FindStatus(2957) != null)
        {
            UseAction(Roleplay.AID.SilentTakedown, primaryTarget);
            return;
        }

        switch (ComboAction)
        {
            case Roleplay.AID.KeenEdgeFR:
                UseAction(Roleplay.AID.BrutalShellFR, primaryTarget);
                break;
            case Roleplay.AID.BrutalShellFR:
                UseAction(Roleplay.AID.SolidBarrelFR, primaryTarget);
                break;
            default:
                UseAction(Roleplay.AID.KeenEdgeFR, primaryTarget);
                break;
        }
    }
}

[Quest(BossModuleInfo.Maturity.Contributed, 812)]
internal class AFrostyReception(WorldState ws) : QuestBattle(ws)
{
    private AutoThancred _ai = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) => _ai.Execute(player, hints, maxCastTime);

    private QuestObjective Takedown(WorldState ws, Vector3 destination, uint enemyOID)
        => new QuestObjective(ws)
            .WithConnection(destination)
            .Hints((player, hints) =>
            {
                var tar = hints.PotentialTargets.FirstOrDefault(x => x.Actor.Position.AlmostEqual(new WPos(destination.XZ()), 3) && x.Actor.OID == enemyOID);
                if (tar is AIHints.Enemy t)
                    t.Priority = 1;
            })
            .With(obj =>
            {
                obj.OnModelStateChanged += (act) => obj.CompleteIf(act.OID == enemyOID && act.ModelState.ModelState == 83);
            });

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Commence")
            .WithInteract(0x384C)
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x80000000 && diru.Param1 == 0x883020 && diru.Param2 == 0);
            }),

        Takedown(ws, new(55, 0, -375), 0x3627).Named("Guard 1"),

        QuestObjective.StandardInteract(ws, 0x1EB310).Named("Gate 1"),
        QuestObjective.StandardInteract(ws, 0x1EB30F, new Vector3(61.49f, 0.36f, -383.41f)).Named("Sabotage 1.1"),
        QuestObjective.StandardInteract(ws, 0x1EB467).Named("Sabotage 1.2"),
        QuestObjective.StandardInteract(ws, 0x1EB466).Named("Sabotage 1.3"),

        new QuestObjective(ws)
            .Named("Guard 2")
            .Hints((player, hints) => {
                hints.GoalZones.Add(hints.GoalSingleTarget(new WPos(45.5f, -425), 3));
                hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.SwiftDeception), player, ActionQueue.Priority.High);
            })
    ];
}

