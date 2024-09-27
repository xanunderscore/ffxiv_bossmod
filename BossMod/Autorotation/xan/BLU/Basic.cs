using BossMod.BLU;

namespace BossMod.Autorotation.xan;

public sealed class BLU(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BLU", "Blue Mage", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BLU), 80);

        def.DefineShared().AddAssociatedActions(AID.Nightbloom, AID.BeingMortal, AID.BothEnds, AID.Apokalypsis, AID.MatraMagic);

        return def;
    }

    public enum Mimicry
    {
        None,
        Tank,
        DPS,
        Healer
    }

    public enum GCDPriority : int
    {
        None = 0,
        FillerST = 100,
        FillerAOE = 150,
        GCDWithCooldown = 200,
        Poop = 300,
        Scoop = 301,
        BuffRefresh = 600,
        SurpanakhaRepeat = 900,
    }

    private Mimicry Mimic;

    protected override bool CanCast(AID aid) => HaveSpell(aid) && base.CanCast(aid);

    private bool HaveSpell(AID aid) => World.Client.BlueMageSpells.Contains((uint)aid);

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        Mimic = CurrentMimic();

        if (World.CurrentCFCID > 0 && World.Party.WithoutSlot().Count(p => p.Type == ActorType.Player) == 1)
        {
            if (HaveSpell(AID.BasicInstinct) && Player.FindStatus(SID.MightyGuard) == null)
                PushGCD(AID.MightyGuard, Player);

            if (Player.FindStatus(SID.BasicInstinct) == null)
                PushGCD(AID.BasicInstinct, Player);
        }

        if (Mimic == Mimicry.Tank)
        {
            TankSpecific(primaryTarget);
            // Chelonian Gate
            if (Player.FindStatus(2496) != null)
                return;
        }

        // mortal flame
        if (primaryTarget is Actor p && StatusDetails(p, 3643, Player.InstanceID).Left == 0 && Hints.PriorityTargets.Count() == 1)
            PushGCD(AID.MortalFlame, p, GCDPriority.GCDWithCooldown);

        // if channeling surpanakha, don't use anything else
        var numSurpTargets = AdjustNumTargets(strategy, Hints.NumPriorityTargetsInAOECone(Player.Position, 16, Player.Rotation.ToDirection(), 60.Degrees()));
        var surp = StatusLeft(SID.SurpanakhasFury);
        if (numSurpTargets > 0 && (MaxChargesIn(AID.Surpanakha) == 0 || surp > 0 && ReadyIn(AID.Surpanakha) <= 1))
        {
            PushGCD(AID.Surpanakha, Player, GCDPriority.SurpanakhaRepeat);
            return;
        }

        if (ReadyIn(AID.TheRoseOfDestruction) <= GCD)
            PushGCD(AID.TheRoseOfDestruction, primaryTarget, GCDPriority.GCDWithCooldown);

        // standard filler spells
        if (HaveSpell(AID.GoblinPunch))
        {
            if (primaryTarget is Actor t)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(t, 3));
            Hints.RecommendedPositional = (primaryTarget, Positional.Front, false, true);
            PushGCD(AID.GoblinPunch, primaryTarget, GCDPriority.FillerST);
        }
        PushGCD(AID.SonicBoom, primaryTarget, GCDPriority.FillerST);

        if (HaveSpell(AID.PeatPelt) && HaveSpell(AID.DeepClean))
        {
            var (poopTarget, poopNum) = SelectTarget(strategy, primaryTarget, 25, (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 6));
            if (poopTarget != null && poopNum > 2)
            {
                var scoopNum = Hints.NumPriorityTargetsInAOE(act => StatusDetails(act.Actor, 3636, Player.InstanceID).Left > SpellGCDLength && Hints.TargetInAOECircle(act.Actor, poopTarget.Position, 6));
                if (scoopNum > 2)
                    PushGCD(AID.DeepClean, poopTarget, GCDPriority.Scoop);
                PushGCD(AID.PeatPelt, poopTarget, GCDPriority.Poop);
            }
        }

        if (NumNearbyTargets(strategy, 10) > 0)
        {
            PushOGCD(AID.BeingMortal, Player);
            PushOGCD(AID.Nightbloom, Player);
        }

        PushOGCD(AID.FeatherRain, primaryTarget);
        PushOGCD(AID.ShockStrike, primaryTarget);
        PushOGCD(AID.JKick, primaryTarget);
    }

    private void TankSpecific(Actor? primaryTarget)
    {
        if (HaveSpell(AID.Devour) && !CanFitGCD(StatusLeft(SID.HPBoost), 1))
        {
            if (primaryTarget is Actor t)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(t, 3));
            PushGCD(AID.Devour, primaryTarget, GCDPriority.BuffRefresh);
        }

        var d = Hints.PredictedDamage.Count(p => p.players[0] && p.activation >= World.FutureTime(GCD + GetCastTime(AID.ChelonianGate)));
        if (d > 0)
            PushGCD(AID.ChelonianGate, Player, GCDPriority.BuffRefresh);

        if (Player.FindStatus(2497) != null)
            PushGCD(AID.DivineCataract, Player, GCDPriority.BuffRefresh);
    }

    public Mimicry CurrentMimic()
    {
        foreach (var st in Player.Statuses)
        {
            switch ((SID)st.ID)
            {
                case SID.AethericMimicryTank:
                    return Mimicry.Tank;
                case SID.AethericMimicryDPS:
                    return Mimicry.DPS;
                case SID.AethericMimicryHealer:
                    return Mimicry.Healer;
            }
        }

        return Mimicry.None;
    }
}
