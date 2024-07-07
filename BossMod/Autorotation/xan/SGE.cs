using BossMod.SGE;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.xan;
public sealed class SGE(RotationModuleManager manager, Actor player) : xanmodule(manager, player)
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
    public float SwiftcastLeft;

    public bool ForcedMovement;

    public int NumAOETargets;
    public int NumPhlegmaTargets;
    public int NumToxikonTargets;
    public int NumPneumaTargets;

    public int NumNearbyDotTargets;

    private Actor? BestPhlegmaTarget; // 6y/5y
    private Actor? BestToxikonTarget; // 25y/5y
    private Actor? BestPneumaTarget; // 25y/4y rect

    private Actor? BestDotTarget;

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (strategy.Option(Track.Kardia).As<KardiaStrategy>() == KardiaStrategy.Auto
            && Unlocked(AID.Kardia)
            && Player.FindStatus((uint)SID.Kardia) == null
            && FindKardiaTarget() is Actor kardiaTarget)
            PushGCD(AID.Kardia, kardiaTarget);

        if (!Player.InCombat && Unlocked(AID.Eukrasia) && !Eukrasia)
            PushGCD(AID.Eukrasia, Player);

        if (Unlocked(AID.Eukrasia))
        {
            if (NumNearbyDotTargets > 1 && Unlocked(AID.Dyskrasia))
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

        if (Unlocked(AID.Pneuma) && _state.CD(AID.Pneuma) < _state.GCD && NumPneumaTargets > 1 && CanCast)
            PushGCD(AID.Pneuma, BestPneumaTarget);

        if (Unlocked(AID.Phlegma)
            && (NumPhlegmaTargets > 2 && _state.CD(AID.Phlegma) - 40 < _state.GCD
                || _state.CD(AID.Phlegma) < _state.GCD))
            PushGCD(AID.Phlegma, BestPhlegmaTarget);

        if (Unlocked(AID.Dyskrasia) && NumAOETargets > 1)
            PushGCD(AID.Dyskrasia, Player);

        if (CanCast)
            PushGCD(AID.Dosis, primaryTarget);

        if (Unlocked(AID.Phlegma) && _state.CD(AID.Phlegma) - 40 < _state.GCD && NumPhlegmaTargets > 0)
            PushGCD(AID.Phlegma, BestPhlegmaTarget);

        if (Unlocked(AID.Toxikon) && NumToxikonTargets > 0 && Sting > 0)
            PushGCD(AID.Toxikon, BestToxikonTarget);

        if (NumAOETargets > 0 && Unlocked(AID.Dyskrasia))
            PushGCD(AID.Dyskrasia, Player);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (Unlocked(AID.Rhizomata) && Gall < 2 && NextGall > 10 && _state.CanWeave(AID.Rhizomata, 0.6f, deadline))
            PushOGCD(AID.Rhizomata, Player);

        if (Unlocked(AID.Druochole) && (Gall == 3 || Gall == 2 && NextGall < 2.5f) && Player.HPMP.CurMP <= 9000 && strategy.Option(Track.Druo).As<DruoStrategy>() == DruoStrategy.Auto)
        {
            var healTarget = World.Party.WithoutSlot().MinBy(x => x.HPMP.CurHP / x.HPMP.MaxHP);
            PushOGCD(AID.Druochole, healTarget);
        }

        if (Player.HPMP.CurMP <= 7000 && Unlocked(AID.LucidDreaming) && _state.CanWeave(AID.LucidDreaming, 0.6f, deadline))
            PushOGCD(AID.LucidDreaming, Player);
    }

    static readonly SID[] DotStatus = [SID.EukrasianDosis, SID.EukrasianDosisII, SID.EukrasianDosisIII, SID.EukrasianDyskrasia];

    private bool HaveDot(Actor x)
    {
        foreach (var stat in DotStatus)
            if (_state.StatusDetails(x, (uint)stat, Player.InstanceID).Left > _state.SpellGCDTime)
                return true;

        return false;
    }

    private bool CanCast => SwiftcastLeft > _state.GCD || !ForcedMovement;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        var targeting = strategy.Option(Track.Targeting);
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget);

        _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);

        var gauge = Service.JobGauges.Get<SGEGauge>();

        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.Eukrasia;

        SwiftcastLeft = _state.StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;

        ForcedMovement = Manager.ActionManager.InputOverride.IsMoveRequested();

        (BestPhlegmaTarget, NumPhlegmaTargets) = SelectTarget(targeting, primaryTarget, 6, NumSplashTargets);
        (BestToxikonTarget, NumToxikonTargets) = SelectTarget(targeting, primaryTarget, 25, NumSplashTargets);
        (BestPneumaTarget, NumPneumaTargets) = SelectTarget(targeting, primaryTarget, 25, Num25yRectTargets);

        var aoeStrat = strategy.Option(Track.AOE).As<AOEStrategy>();
        if (aoeStrat == AOEStrategy.AOE)
        {
            var meleeAOETargets = Hints.PriorityTargets.Where(x => x.Actor.DistanceTo(Player) <= 5);
            NumAOETargets = meleeAOETargets.Count();
            NumNearbyDotTargets = meleeAOETargets.Count(x => !HaveDot(x.Actor));
        }
        else
        {
            // allow Dyskrasia (instant cast) for dps uptime during movement
            NumAOETargets = _state.RangeToTarget <= 5 ? 1 : 0;
            NumNearbyDotTargets = 0;
        }

        if (targeting.As<Targeting>() == Targeting.Manual)
        {
            BestDotTarget = primaryTarget == null || HaveDot(primaryTarget) ? null : primaryTarget;
        }
        else
        {
            var allPossibleDotTargets = Hints.PriorityTargets.Where(x => x.Actor.DistanceTo(Player) <= 25);
            if (allPossibleDotTargets.Count() > 2)
                BestDotTarget = null;
            else
                BestDotTarget = allPossibleDotTargets.FirstOrDefault(x => !HaveDot(x.Actor))?.Actor;
        }

        CalcNextBestGCD(strategy, primaryTarget);
        QueueOGCD((deadline, _) => CalcNextBestOGCD(strategy, primaryTarget, deadline));
    }

    private Actor? FindKardiaTarget()
    {
        var party = World.Party.WithoutSlot();
        if (party.Count(x => x.Type == ActorType.Player) == 1)
            return Player;

        var tanks = party.Where(x => x.Class.GetRole() == Role.Tank);
        if (tanks.Count() == 1)
            return tanks.First();

        return null;
    }
}
