using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;

public abstract class AIBase(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float Cooldown<AID>(AID aid) where AID : Enum => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    internal bool ShouldInterrupt(Actor act) => act.CastInfo != null && act.CastInfo.Interruptible && act.CastInfo.TotalTime > 1.5;
}

public class TankAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new("Tank AI", "Utilities for tank AI - stance, provoke, interrupt, ranged attack", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.GLA, Class.WAR, Class.MRD, Class.DRK, Class.GNB), 100);
    }

    private ActionID RangedAction => Player.Class switch
    {
        Class.GLA or Class.PLD => ActionID.MakeSpell(BossMod.PLD.AID.ShieldLob),
        Class.MRD or Class.WAR => ActionID.MakeSpell(WAR.AID.Tomahawk),
        Class.DRK => ActionID.MakeSpell(GenericAID.Unmend),
        Class.GNB => ActionID.MakeSpell(GenericAID.LightningShot),
        _ => default
    };

    private (ActionID, uint) Stance => Player.Class switch
    {
        Class.GLA or Class.PLD => (ActionID.MakeSpell(BossMod.PLD.AID.IronWill), (uint)BossMod.PLD.SID.IronWill),
        Class.MRD or Class.WAR => (ActionID.MakeSpell(WAR.AID.Defiance), (uint)WAR.SID.Defiance),
        Class.DRK => (ActionID.MakeSpell(GenericAID.Grit), (uint)GenericSID.Grit),
        Class.GNB => (ActionID.MakeSpell(GenericAID.RoyalGuard), (uint)GenericSID.RoyalGuard),
        _ => default
    };

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        // ranged
        if (ActionUnlocked(RangedAction) && Player.DistanceTo(primaryTarget) > 5)
            Hints.ActionsToExecute.Push(RangedAction, primaryTarget, ActionQueue.Priority.Low);

        // stance
        var (stanceAction, stanceStatus) = Stance;
        if (ActionUnlocked(stanceAction) && !Player.Statuses.Any(x => x.ID == stanceStatus))
            Hints.ActionsToExecute.Push(stanceAction, Player, ActionQueue.Priority.Minimal);

        // interrupt
        if (Unlocked(ClassShared.AID.Interject) && Cooldown(ClassShared.AID.Interject) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && Player.DistanceTo(e.Actor) <= 3);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (Player.Class is Class.PLD or Class.GLA)
            ExecutePLD(strategy, primaryTarget);

        // more job-specific stuff below here
    }

    private void ExecutePLD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (ActionUnlocked(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron)) && Player.InCombat && Service.JobGauges.Get<PLDGauge>().OathGauge >= 95)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron), Player, ActionQueue.Priority.Minimal);
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
    RoyalGuard = 1833,
    Peloton = 1199
}

public class RangedAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new("Phys Ranged AI", "Utilities for physical ranged dps - peloton, interrupt, defensive abilities", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.ARC, Class.BRD, Class.MCH, Class.DNC), 100);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        // interrupt
        if (Unlocked(ClassShared.AID.HeadGraze) && Cooldown(ClassShared.AID.HeadGraze) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && Player.DistanceTo(e.Actor) <= 25);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeadGraze), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // peloton
        if (Unlocked(ClassShared.AID.Peloton) && World.Party.WithoutSlot().Any(x => Player.DistanceTo(x) <= 30 && !x.InCombat && x.FindStatus((uint)GenericSID.Peloton) == null))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), Player, ActionQueue.Priority.Minimal);

        // second wind
        if (Unlocked(ClassShared.AID.SecondWind) && Cooldown(ClassShared.AID.SecondWind) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP / 2)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);
    }
}
