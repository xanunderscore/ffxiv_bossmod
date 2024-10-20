using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Autorotation.xan;
using static BossMod.PartyRolesConfig;

namespace BossMod.Endwalker.Ultimate.TOP.AI;
internal class StandardTank(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("TOP Mitty - Tank", "TOP Mitty - Tank", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.WAR, Class.GNB, Class.DRK), 90, 90, typeof(TOP));
    }

    private readonly PartyRolesConfig _config = Service.Config.Get<PartyRolesConfig>();
    private Assignment PlayerAssignment => _config[World.Party.Members[0].ContentId];
    private Actor? Cotank
    {
        get
        {
            switch (PlayerAssignment)
            {
                case Assignment.OT:
                    foreach (var (i, actor) in World.Party.WithSlot())
                        if (_config[World.Party.Members[i].ContentId] == Assignment.MT)
                            return actor;
                    return null;
                case Assignment.MT:
                    foreach (var (i, actor) in World.Party.WithSlot())
                        if (_config[World.Party.Members[i].ContentId] == Assignment.OT)
                            return actor;
                    return null;
                default:
                    return null;
            }
        }
    }

    private TankAI.TankActions TankActions => TankAI.ActionsForJob(Player.Class);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is not TOP module)
            return;

        if (module.StateMachine.ActiveState is not StateMachine.State state)
            return;

        if (state.ID == 0x10001)
        {
            // first stack happens 12.1 seconds after transition, last stack happens 18 seconds later; both tanks should use party mit to cover entire mech
        }
    }
}
