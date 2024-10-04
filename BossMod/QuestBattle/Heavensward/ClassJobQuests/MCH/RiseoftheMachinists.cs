﻿namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[Quest(BossModuleInfo.Maturity.Contributed, 426)]
internal class RiseOfTheMachinists(WorldState ws) : QuestBattle(ws)
{
    private static readonly Vector3 Center = new(-650.33f, 97.38f, -452.71f);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                Actor? tedal = null;
                obj.OnActorCreated += (act) => {
                    if (act.OID == 0x1158)
                        tedal ??= act;
                };

                obj.AddAIHints += (player, hints, _) => {
                    foreach(var h in hints.PotentialTargets)
                        if (h.Actor.TargetID == tedal?.InstanceID)
                            h.Priority = 5;
                };
            })
        ];

    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        hints.Center = new(Center.X, Center.Z);
        hints.Bounds = new ArenaBoundsCircle(120, 2);
    }
}

