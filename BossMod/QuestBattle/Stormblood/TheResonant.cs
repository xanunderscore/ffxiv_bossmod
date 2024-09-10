namespace BossMod.QuestBattle.Stormblood.TheResonant;

[Quest(BossModuleInfo.Maturity.WIP, 269)]
public class Quest(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(676.12f, 70.00f, 512.76f))
            .With(obj => {
                var waitUntil = DateTime.MaxValue;

                obj.OnNavigationComplete += () => {
                    waitUntil = World.FutureTime(10);
                };
                obj.Update += () => obj.CompleteIf(World.CurrentTime > waitUntil);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(593.39f, 70.00f, 540.88f))
            .With(obj => {
                obj.OnActorKilled += (act) => obj.CompleteIf(act.OID == 0x1EB6);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(592.57f, 70.00f, 535.17f))
            .WithConnection(new Vector3(581.57f, 70.00f, 534.72f))
            .WithConnection(new Vector3(511.07f, 70.00f, 583.65f))
            .WithInteract(0x1EA74B)
            .With(obj => {
                obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x1EA74B && act.EventState == 7);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(581.73f, 70.00f, 533.20f))
            .WithConnection(new Vector3(655.03f, 70.00f, 527.29f))
            .WithConnection(new Vector3(748.96f, 70.00f, 510.77f))
            .WithConnection(new Vector3(754.20f, 70.00f, 429.63f))
    ];
}
