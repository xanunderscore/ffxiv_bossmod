﻿namespace BossMod.QuestBattle.Stormblood.TheKeyToVictory;

enum OID : uint
{
    Wiscar = 0x1E82,
    Soblyn = 0x1E83,
    QueerDevice = 0x1EA757,
    TatteredDiary = 0x1EA752,
    Colossus = 0x1E7F
}

[Quest(BossModuleInfo.Maturity.WIP, 467)]
public sealed class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        return [
            new QuestObjective(ws)
                .WithConnection(new Vector3(-396.38f, 4.94f, 122.21f))
                .WithConnection(new Vector3(-285.98f, 11.18f, 223.66f))
                .Hints((player, hints) =>
                {
                    // eventobj doesn't spawn until all the npcs are out of combat - way faster to kill all the soblyns than to wait
                    hints.PrioritizeTargetsByOID(OID.Soblyn);
                })
                .WithInteract(OID.QueerDevice)
                .With(obj => {
                    obj.OnActorTargetableChanged += (act) => obj.CompleteIf(act.OID == (uint)OID.QueerDevice && !act.IsTargetable);
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(-278.78f, 11.18f, 158.27f))
                .NavStrategy(NavigationStrategy.Continue)
                .Hints((player, hints) => {
                    foreach(var h in hints.PotentialTargets)
                        h.Priority = 0;
                })
                .With(obj => {
                    obj.OnActorKilled += (act) => obj.CompleteIf(act.OID == (uint)OID.Colossus);
                }),

            new QuestObjective(ws)
                .WithInteract(OID.TatteredDiary)
                .With(obj => {
                    obj.OnActorDestroyed += (act) => obj.CompleteIf(act.OID == (uint)OID.TatteredDiary);
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(-100.29f, 3.63f, 527.66f))
                .WithConnection(new Vector3(43.84f, 37.55f, 699.77f))
                .Hints((player, hints) => {
                    hints.PrioritizeTargetsByOID(0x1EB3, 1);
                    hints.PrioritizeTargetsByOID(0x1EB0, 0);
                })
                .With(obj => {
                    obj.OnActorKilled += (act) => obj.CompleteIf(act.OID == 0x1EB0);
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(50.57f, 42.00f, 724.45f))
                .WithInteract(0x1EA771)
        ];
    }
}
