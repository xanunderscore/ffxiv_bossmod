﻿namespace BossMod.Autorotation.xan;
public class RangedAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    private DateTime _pelotonLockout = DateTime.MinValue;

    private readonly Random rand = new();
    public const float PelotonFast = 1f;
    public const float PelotonSlow = 3f;

    public enum Track { Peloton, Interrupt, SecondWind }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phys Ranged AI", "Utilities for physical ranged dps - peloton, interrupt, defensive abilities", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ARC, Class.BRD, Class.MCH, Class.DNC), 100);

        def.AbilityTrack(Track.Peloton, "Peloton").AddAssociatedActions(ClassShared.AID.Peloton);
        def.AbilityTrack(Track.Interrupt, "Head Graze").AddAssociatedActions(ClassShared.AID.HeadGraze);
        def.AbilityTrack(Track.SecondWind, "Second Wind").AddAssociatedActions(ClassShared.AID.SecondWind);

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        if (Player.InCombat || forceMovementIn > 1000)
            _pelotonLockout = World.CurrentTime.AddSeconds(rand.NextDouble() * (PelotonSlow - PelotonFast) + PelotonSlow);

        // interrupt
        if (strategy.Enabled(Track.Interrupt) && NextChargeIn(ClassShared.AID.HeadGraze) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.FirstOrDefault(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 25);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeadGraze), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // peloton
        if (strategy.Enabled(Track.Peloton)
            && World.CurrentTime > _pelotonLockout
            && forceMovementIn == 0
            && !Player.InCombat
            // if player is targeting npc (fate npc, vendor, etc) we assume they want to interact with target;
            // peloton animationlock will be annoying and unhelpful here
            // we use TargetManager because most friendly NPCs aren't Actors (or something)
            && Service.TargetManager.Target == null
            && PelotonWillExpire(Player))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), Player, ActionQueue.Priority.Minimal);

        // second wind
        if (strategy.Enabled(Track.SecondWind) && Player.InCombat && HPRatio() <= 0.5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);
    }

    private bool PelotonWillExpire(Actor actor)
    {
        var pending = World.PendingEffects.PendingStatus(actor.InstanceID, (uint)BossMod.BRD.SID.Peloton);
        if (pending != null)
            // just applied, should have >30s remaining duration, assume that's fine
            return false;

        var status = actor.FindStatus((uint)BossMod.BRD.SID.Peloton);
        if (status == null)
            return true;

        var duration = Math.Max((float)(status.Value.ExpireAt - World.CurrentTime).TotalSeconds, 0.0f);
        return duration < 5;
    }
}
