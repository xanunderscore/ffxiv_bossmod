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

    private Mimicry Mimic;

    private float DevourLeft;

    protected override bool CanCast(AID aid) => HaveSpell(aid) && base.CanCast(aid);

    private bool HaveSpell(AID aid) => World.Client.BlueMageSpells.Contains((uint)aid);

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        Mimic = CurrentMimic();

        DevourLeft = StatusLeft(SID.HPBoost);

        if (World.CurrentCFCID > 0 && World.Party.WithoutSlot().Count(p => p.Type == ActorType.Player) == 1)
        {
            if (HaveSpell(AID.BasicInstinct) && Player.FindStatus(SID.MightyGuard) == null)
                PushGCD(AID.MightyGuard, Player);

            if (Player.FindStatus(SID.BasicInstinct) == null)
                PushGCD(AID.BasicInstinct, Player);
        }

        var numNightbloomTargets = NumNearbyTargets(strategy, 10);
        var numSurpTargets = AdjustNumTargets(strategy, Hints.NumPriorityTargetsInAOECone(Player.Position, 16, Player.Rotation.ToDirection(), 60.Degrees()));

        var surp = StatusLeft(SID.SurpanakhasFury);

        if (HaveSpell(AID.GoblinPunch))
        {
            Hints.RecommendedRangeToTarget = 3;
            Hints.RecommendedPositional = (primaryTarget, Positional.Front, false, true);
        }

        if (Mimic == Mimicry.Tank && HaveSpell(AID.Devour) && !CanFitGCD(DevourLeft, 1))
        {
            Hints.RecommendedRangeToTarget = 3;
            PushGCD(AID.Devour, primaryTarget);
        }

        if (primaryTarget is Actor p && StatusDetails(p, 3643, Player.InstanceID).Left == 0)
            PushGCD(AID.MortalFlame, p);

        if (numSurpTargets > 0 && (MaxChargesIn(AID.Surpanakha) == 0 || surp > 0 && ReadyIn(AID.Surpanakha) <= 1))
        {
            PushGCD(AID.Surpanakha, Player);
            return;
        }

        if (ReadyIn(AID.TheRoseOfDestruction) <= GCD)
            PushGCD(AID.TheRoseOfDestruction, primaryTarget);

        PushGCD(AID.GoblinPunch, primaryTarget);
        PushGCD(AID.SonicBoom, primaryTarget);

        if (numNightbloomTargets > 0)
        {
            PushOGCD(AID.BeingMortal, Player);
            PushOGCD(AID.Nightbloom, Player);
        }

        PushOGCD(AID.FeatherRain, primaryTarget);
        PushOGCD(AID.ShockStrike, primaryTarget);
        PushOGCD(AID.JKick, primaryTarget);
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
