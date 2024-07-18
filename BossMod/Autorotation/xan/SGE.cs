using BossMod.SGE;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class SGE(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Kardia, Druo }
    public enum KardiaStrategy { Auto, Manual }
    public enum DruoStrategy { Auto, Manual }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("SGE", "Sage", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.SGE), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);

        def.Define(Track.Kardia).As<KardiaStrategy>("Kardia")
            .AddOption(KardiaStrategy.Auto, "Auto", "Automatically choose Kardia target")
            .AddOption(KardiaStrategy.Manual, "Manual", "Don't automatically choose Kardia target");
        def.Define(Track.Druo).As<DruoStrategy>("Druochole")
            .AddOption(DruoStrategy.Auto, "Auto", "Prevent Addersgall overcap by using Druochole on lowest-HP ally")
            .AddOption(DruoStrategy.Manual, "Manual", "Do not automatically use Druochole");

        return def;
    }

    public int Gall;
    public float NextGall;
    public int Sting;
    public bool Eukrasia;
    public float ZoeLeft;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumPhlegmaTargets;
    public int NumPneumaTargets;

    public int NumNearbyDotTargets;

    private Actor? BestPhlegmaTarget; // 6y/5y
    private Actor? BestRangedAOETarget; // 25y/5y toxikon, psyche
    private Actor? BestPneumaTarget; // 25y/4y rect

    private Actor? BestDotTarget;

    protected override float GetCastTime(AID aid) => Eukrasia ? 0 : base.GetCastTime(aid);

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (strategy.Option(Track.Kardia).As<KardiaStrategy>() == KardiaStrategy.Auto
            && Unlocked(AID.Kardia)
            && Player.FindStatus((uint)SID.Kardia) == null
            && FindKardiaTarget() is Actor kardiaTarget
            && !World.Party.Members[World.Party.FindSlot(kardiaTarget.InstanceID)].InCutscene)
            PushGCD(AID.Kardia, kardiaTarget);

        if (!Player.InCombat && Unlocked(AID.Eukrasia) && !Eukrasia)
            PushGCD(AID.Eukrasia, Player);

        if (Unlocked(AID.Eukrasia))
        {
            if (NumNearbyDotTargets > 1 && Unlocked(AID.EukrasianDyskrasia))
            {
                if (!Eukrasia)
                    PushGCD(AID.Eukrasia, Player);

                PushGCD(AID.Dyskrasia, Player);
            }
            else if (BestDotTarget != null)
            {
                if (!Eukrasia)
                    PushGCD(AID.Eukrasia, Player);

                PushGCD(AID.Dosis, BestDotTarget);
            }
        }

        if (Unlocked(AID.Pneuma) && _state.CD(AID.Pneuma) < _state.GCD && NumPneumaTargets > 1)
            PushGCD(AID.Pneuma, BestPneumaTarget);

        if (Unlocked(AID.Phlegma)
            && (NumPhlegmaTargets > 2 && _state.CD(AID.Phlegma) - 40 < _state.GCD
                || _state.CD(AID.Phlegma) < _state.GCD))
            PushGCD(AID.Phlegma, BestPhlegmaTarget);

        if (NumAOETargets > 1)
        {
            if (Sting > 0 && NumPhlegmaTargets > 1)
                PushGCD(AID.Toxikon, BestPhlegmaTarget);

            if (Unlocked(AID.Dyskrasia))
                PushGCD(AID.Dyskrasia, Player);
        }

        PushGCD(AID.Dosis, primaryTarget);

        if (Unlocked(AID.Phlegma) && _state.CD(AID.Phlegma) - 40 < _state.GCD && NumPhlegmaTargets > 0)
            PushGCD(AID.Phlegma, BestPhlegmaTarget);

        if (Unlocked(AID.Toxikon) && NumRangedAOETargets > 0 && Sting > 0)
            PushGCD(AID.Toxikon, BestRangedAOETarget);

        if (NumAOETargets > 0 && Unlocked(AID.Dyskrasia))
            PushGCD(AID.Dyskrasia, Player);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (!Player.InCombat)
            return;

        if (Unlocked(AID.Rhizomata) && Gall < 2 && NextGall > 10 && _state.CanWeave(AID.Rhizomata, 0.6f, deadline))
            PushOGCD(AID.Rhizomata, Player);

        if (Unlocked(AID.Druochole) && (Gall == 3 || Gall == 2 && NextGall < 2.5f) && Player.HPMP.CurMP <= 9000 && strategy.Option(Track.Druo).As<DruoStrategy>() == DruoStrategy.Auto)
        {
            var healTarget = World.Party.WithoutSlot().MinBy(x => x.HPMP.CurHP / x.HPMP.MaxHP);
            PushOGCD(AID.Druochole, healTarget);
        }

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);

        if (Unlocked(AID.Psyche) && _state.CanWeave(AID.Psyche, 0.6f, deadline) && NumRangedAOETargets > 0)
            PushOGCD(AID.Psyche, BestRangedAOETarget);
    }

    static readonly SID[] DotStatus = [SID.EukrasianDosis, SID.EukrasianDosisII, SID.EukrasianDosisIII, SID.EukrasianDyskrasia];

    private bool HaveDot(Actor x)
    {
        foreach (var stat in DotStatus)
            if (_state.StatusDetails(x, (uint)stat, Player.InstanceID).Left > _state.SpellGCDTime)
                return true;

        return false;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<SageGauge>();

        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.EukrasiaActive;

        (BestPhlegmaTarget, NumPhlegmaTargets) = SelectTarget(targeting, primaryTarget, 6, IsSplashTarget);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(targeting, primaryTarget, 25, IsSplashTarget);
        (BestPneumaTarget, NumPneumaTargets) = SelectTarget(targeting, primaryTarget, 25, Is25yRectTarget);

        var aoeStrat = strategy.Option(Track.AOE).As<AOEStrategy>();
        if (aoeStrat == AOEStrategy.AOE)
        {
            var meleeAOETargets = Hints.PriorityTargets.Where(x => x.Actor.DistanceToHitbox(Player) <= 5);
            NumAOETargets = 0;
            NumNearbyDotTargets = 0;
            foreach (var target in meleeAOETargets)
            {
                NumAOETargets++;
                if (!HaveDot(target.Actor))
                    NumNearbyDotTargets++;
            }
        }
        else
        {
            // allow Dyskrasia (instant cast) for dps uptime during movement
            NumAOETargets = _state.RangeToTarget <= 5 ? 1 : 0;
            NumNearbyDotTargets = 0;
        }

        if (targeting == Targeting.Auto)
        {
            var allPossibleDotTargets = Hints.PriorityTargets.Where(x => x.Actor.DistanceToHitbox(Player) <= 25);
            if (allPossibleDotTargets.Count() > 2)
                BestDotTarget = null;
            else
                BestDotTarget = allPossibleDotTargets.FirstOrDefault(x => !HaveDot(x.Actor))?.Actor;
        }
        else
        {
            BestDotTarget = primaryTarget == null || HaveDot(primaryTarget) ? null : primaryTarget;
        }

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private Actor? FindKardiaTarget()
    {
        var party = World.Party.WithoutSlot();
        var total = 0;
        var tanks = 0;
        Actor? tank = null;
        foreach (var actor in party)
        {
            total++;
            if (actor.Class.GetRole() == Role.Tank)
            {
                tanks++;
                tank ??= actor;
            }
        }
        if (total == 1)
            return Player;

        if (tanks == 1)
            return tank;

        return null;
    }
}
