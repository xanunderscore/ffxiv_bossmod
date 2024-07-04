using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan.AI;

public abstract class TankAI(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected static RotationModuleDefinition DefineAI(string klass, params Class[] classes)
     => new($"{klass} AI", $"Utilities for {klass} AI", "xan", RotationModuleQuality.WIP, BitMask.Build(classes), 100);

    internal void AutoStance<AID, SID>(AID stanceAid, SID stanceSid)
        where AID : Enum
        where SID : Enum
    {
        var statusId = (uint)(object)stanceSid;
        var stanceAction = ActionID.MakeSpell(stanceAid);

        if (ActionUnlocked(stanceAction) && !Player.Statuses.Any(x => x.ID == statusId))
            Hints.ActionsToExecute.Push(stanceAction, Player, ActionQueue.Priority.Minimal);
    }

    internal void AutoRanged<AID>(AID rangedAid, Actor? primaryTarget) where AID : Enum
    {
        var rangedAction = ActionID.MakeSpell(rangedAid);
        if (ActionUnlocked(rangedAction) && Player.DistanceTo(primaryTarget) > 5)
            Hints.ActionsToExecute.Push(rangedAction, primaryTarget, ActionQueue.Priority.Low);
    }

    internal void AutoCommon()
    {
        var interject = ActionID.MakeSpell(ClassShared.AID.Interject);
        var interCd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.Interject)!.MainCooldownGroup].Remaining;

        if (ActionUnlocked(interject) && interCd == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(interject, interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // todo rampart
    }

    private static bool ShouldInterrupt(Actor act)
    {
        var ci = act.CastInfo;
        if (ci == null)
            return false;

        return ci.Interruptible && ci.TotalTime > 1.5;
    }
}

public sealed class PLD(RotationModuleManager manager, Actor player) : TankAI(manager, player)
{
    public static RotationModuleDefinition Definition() => DefineAI("PLD", Class.PLD, Class.GLA);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        AutoCommon();
        AutoStance(BossMod.PLD.AID.IronWill, BossMod.PLD.SID.IronWill);
        AutoRanged(BossMod.PLD.AID.ShieldLob, primaryTarget);

        if (ActionUnlocked(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron)) && Player.InCombat && Service.JobGauges.Get<PLDGauge>().OathGauge >= 95)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron), Player, ActionQueue.Priority.Minimal);
    }
}

public sealed class WAR(RotationModuleManager manager, Actor player) : TankAI(manager, player)
{
    public static RotationModuleDefinition Definition() => DefineAI("WAR", Class.WAR, Class.MRD);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        AutoCommon();
        AutoStance(BossMod.WAR.AID.Defiance, BossMod.WAR.SID.Defiance);
        AutoRanged(BossMod.WAR.AID.Tomahawk, primaryTarget);
    }
}

enum GenericAID : uint
{
    Unmend = 3624,
    Grit = 3629,
    RoyalGuard = 16142,
    LightningShot = 16143
}

enum GenericSID : uint
{
    Grit = 743,
    RoyalGuard = 1833
}

public sealed class DRK(RotationModuleManager manager, Actor player) : TankAI(manager, player)
{
    public static RotationModuleDefinition Definition() => DefineAI("DRK", Class.DRK);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        AutoCommon();
        AutoStance(GenericAID.Grit, GenericSID.Grit);
        AutoRanged(GenericAID.Unmend, primaryTarget);
    }
}

public sealed class GNB(RotationModuleManager manager, Actor player) : TankAI(manager, player)
{
    public static RotationModuleDefinition Definition() => DefineAI("GNB", Class.GNB);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        AutoCommon();
        AutoStance(GenericAID.RoyalGuard, GenericSID.RoyalGuard);
        AutoRanged(GenericAID.LightningShot, primaryTarget);
    }
}
