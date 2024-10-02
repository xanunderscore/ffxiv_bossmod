using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.QuestBattle.Shadowbringers.SideQuests;

[Quest(BossModuleInfo.Maturity.Contributed, 670)]
internal class GambolingForGil(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                var g = ws.Client.GetGauge<DancerGauge>();

                if (g.DanceSteps[0] == 0)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.StandardStep), player, ActionQueue.Priority.High);

                if (g.StepIndex == 2)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.DoubleStandardFinish), player, ActionQueue.Priority.High);
            })
    ];
}

