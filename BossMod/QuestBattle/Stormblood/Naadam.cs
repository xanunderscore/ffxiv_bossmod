﻿namespace BossMod.QuestBattle.Stormblood.Naadam;

[Quest(BossModuleInfo.Maturity.WIP, 246)]
public sealed class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        var go =
            new QuestObjective(ws)
                .WithConnection(new Vector3(307.38f, 0.89f, 23.38f))
                .WithConnection(new Vector3(352.01f, -1.45f, 288.59f))
                .NavStrategy(NavigationStrategy.Continue);

        go.OnNavigationComplete += () => go.Completed = true;

        return [go];
    }
}
