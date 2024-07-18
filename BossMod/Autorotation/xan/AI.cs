using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Lumina.Excel.GeneratedSheets;

namespace BossMod.Autorotation.xan;

public abstract class AIBase(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float Cooldown<AID>(AID aid) where AID : Enum => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    internal bool ShouldInterrupt(Actor act) => IsCastReactable(act) && act.CastInfo!.Interruptible;
    internal bool ShouldStun(Actor act) => IsCastReactable(act) && !act.CastInfo!.Interruptible && !IsBossFromIcon(act.OID);

    private static bool IsBossFromIcon(uint oid) => Service.LuminaRow<BNpcBase>(oid)?.Rank is 1 or 2 or 6;

    internal bool IsCastReactable(Actor act)
    {
        var castInfo = act.CastInfo;
        if (castInfo == null || castInfo.TotalTime <= 1.5 || castInfo.EventHappened)
            return false;

        return castInfo.NPCTotalTime - castInfo.NPCRemainingTime > 1;
    }

    internal bool IsAutoingMe(Actor act) => act.CastInfo == null && act.TargetID == Player.InstanceID && Player.DistanceToHitbox(act) <= 6;
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

public enum AbilityUse
{
    Enabled,
    Disabled
}

internal static class AIExt
{
    public static RotationModuleDefinition.ConfigRef<AbilityUse> AbilityTrack<Track>(this RotationModuleDefinition def, Track track, string name) where Track : Enum
    {
        return def.Define(track).As<AbilityUse>(name).AddOption(AbilityUse.Enabled, "Enabled").AddOption(AbilityUse.Disabled, "Disabled");
    }

    public static bool Enabled<Track>(this StrategyValues strategy, Track track) where Track : Enum
        => strategy.Option(track).As<AbilityUse>() == AbilityUse.Enabled;
}

public class TankAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Stance, Ranged, Interject, Stun }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Tank AI", "Utilities for tank AI - stance, provoke, interrupt, ranged attack", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.GLA, Class.WAR, Class.MRD, Class.DRK, Class.GNB), 100);

        def.AbilityTrack(Track.Stance, "Stance");
        def.AbilityTrack(Track.Ranged, "Ranged GCD");
        def.AbilityTrack(Track.Interject, "Interject").AddAssociatedActions(ClassShared.AID.Interject);
        def.AbilityTrack(Track.Stun, "Low Blow").AddAssociatedActions(ClassShared.AID.LowBlow);

        return def;
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

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimationLockDelay)
    {
        // ranged
        if (strategy.Enabled(Track.Ranged) && ActionUnlocked(RangedAction) && Player.DistanceToHitbox(primaryTarget) is > 5 and <= 20 && primaryTarget!.Type is ActorType.Enemy && !primaryTarget.IsAlly)
            Hints.ActionsToExecute.Push(RangedAction, primaryTarget, ActionQueue.Priority.Low);

        // stance
        var (stanceAction, stanceStatus) = Stance;
        if (strategy.Enabled(Track.Stance) && ActionUnlocked(stanceAction) && !Player.Statuses.Any(x => x.ID == stanceStatus))
            Hints.ActionsToExecute.Push(stanceAction, Player, ActionQueue.Priority.Minimal);

        // interrupt
        if (strategy.Enabled(Track.Interject) && Unlocked(ClassShared.AID.Interject) && Cooldown(ClassShared.AID.Interject) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // low blow
        if (strategy.Enabled(Track.Stun) && Unlocked(ClassShared.AID.LowBlow) && Cooldown(ClassShared.AID.LowBlow) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldStun(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LowBlow), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (Player.Class is Class.PLD or Class.GLA)
            ExecutePLD(strategy, primaryTarget);

        // more job-specific stuff below here
    }

    private void ExecutePLD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (ActionUnlocked(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron)) && Player.InCombat && GetGauge<PaladinGauge>().OathGauge >= 95)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron), Player, ActionQueue.Priority.Minimal);
    }
}

public class RangedAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    private DateTime _pelotonLockout = DateTime.MinValue;

    public enum Track { Peloton, Interrupt, SecondWind }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phys Ranged AI", "Utilities for physical ranged dps - peloton, interrupt, defensive abilities", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.ARC, Class.BRD, Class.MCH, Class.DNC), 100);

        def.AbilityTrack(Track.Peloton, "Peloton").AddAssociatedActions(ClassShared.AID.Peloton);
        def.AbilityTrack(Track.Interrupt, "Head Graze").AddAssociatedActions(ClassShared.AID.HeadGraze);
        def.AbilityTrack(Track.SecondWind, "Second Wind").AddAssociatedActions(ClassShared.AID.SecondWind);

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimationLockDelay)
    {
        if (Player.InCombat)
            _pelotonLockout = World.CurrentTime;

        // interrupt
        if (strategy.Enabled(Track.Interrupt) && Unlocked(ClassShared.AID.HeadGraze) && Cooldown(ClassShared.AID.HeadGraze) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 25);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeadGraze), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // peloton
        if (strategy.Enabled(Track.Peloton)
            && Unlocked(ClassShared.AID.Peloton)
            && Cooldown(ClassShared.AID.Peloton) == 0
            && (World.CurrentTime - _pelotonLockout).TotalSeconds > 3
            && Manager.ActionManager.InputOverride.IsMoving()
            && !Player.InCombat
            // if player is targeting npc (fate npc, vendor, etc) we assume they want to interact with target;
            // peloton animationlock will be annoying and unhelpful here
            // we use TargetManager because most friendly NPCs aren't Actors (or something)
            && Service.TargetManager.Target == null
            && PelotonWillExpire(Player))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), Player, ActionQueue.Priority.Minimal);

        // second wind
        if (strategy.Enabled(Track.SecondWind) && Unlocked(ClassShared.AID.SecondWind) && Cooldown(ClassShared.AID.SecondWind) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP / 2)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);
    }

    private bool PelotonWillExpire(Actor actor)
    {
        var pending = World.PendingEffects.PendingStatus(actor.InstanceID, (uint)GenericSID.Peloton);
        if (pending != null)
            // just applied, should have >30s remaining duration, assume that's fine
            return false;

        var status = actor.FindStatus((uint)GenericSID.Peloton);
        if (status == null)
            return true;

        var duration = Math.Max((float)(status.Value.ExpireAt - World.CurrentTime).TotalSeconds, 0.0f);
        return duration < 5;
    }
}

public class MeleeAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { SecondWind, Bloodbath, Stun }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Melee DPS AI", "Utilities for melee - bloodbath, second wind, stun", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PGL, Class.MNK, Class.LNC, Class.DRG, Class.ROG, Class.NIN, Class.SAM, Class.RPR, Class.VPR), 100);

        def.AbilityTrack(Track.SecondWind, "Second Wind");
        def.AbilityTrack(Track.Bloodbath, "Bloodbath");
        def.AbilityTrack(Track.Stun, "Stun");

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimationLockDelay)
    {
        // second wind
        if (strategy.Enabled(Track.SecondWind) && Unlocked(ClassShared.AID.SecondWind) && Cooldown(ClassShared.AID.SecondWind) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP / 2)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);

        // bloodbath
        if (strategy.Enabled(Track.Bloodbath) && Unlocked(ClassShared.AID.Bloodbath) && Cooldown(ClassShared.AID.Bloodbath) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP * 0.75)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Bloodbath), Player, ActionQueue.Priority.Medium);

        // low blow
        if (strategy.Enabled(Track.Stun) && Unlocked(ClassShared.AID.LegSweep) && Cooldown(ClassShared.AID.LegSweep) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldStun(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LegSweep), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (Player.Class == Class.SAM)
            AISAM();
    }

    private void AISAM()
    {
        // if nearby enemies are auto-attacking us, use guard skill
        if (Cooldown(BossMod.SAM.AID.ThirdEye) == 0
            && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8
            && Hints.PriorityTargets.Any(x => IsAutoingMe(x.Actor)))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SAM.AID.ThirdEye), Player, ActionQueue.Priority.Low);
    }
}
