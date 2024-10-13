﻿using BossMod.Autorotation.xan.AI;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    private readonly TrackPartyHealth Health = new(manager.WorldState);

    public enum Track { Raise, RaiseTarget, Heal, Esuna }
    public enum RaiseStrategy
    {
        None,
        Swiftcast,
        Slowcast,
        Hardcast,
    }
    public enum RaiseTarget
    {
        Party,
        Alliance,
        Everyone
    }

    public ActionID RaiseAction => Player.Class switch
    {
        Class.CNJ or Class.WHM => ActionID.MakeSpell(BossMod.WHM.AID.Raise),
        Class.ACN or Class.SCH => ActionID.MakeSpell(BossMod.SCH.AID.Resurrection),
        Class.AST => ActionID.MakeSpell(BossMod.AST.AID.Ascend),
        Class.SGE => ActionID.MakeSpell(BossMod.SGE.AID.Egeiro),
        _ => default
    };

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Healer AI", "Auto-healer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.CNJ, Class.WHM, Class.ACN, Class.SCH, Class.SGE, Class.AST), 100);

        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Don't automatically raise")
            .AddOption(RaiseStrategy.Swiftcast, "Raise using Swiftcast only")
            .AddOption(RaiseStrategy.Slowcast, "Raise without requiring Swiftcast to be available")
            .AddOption(RaiseStrategy.Hardcast, "Never use Swiftcast to raise");

        def.Define(Track.RaiseTarget).As<RaiseTarget>("RaiseTargets")
            .AddOption(RaiseTarget.Party, "Party members")
            .AddOption(RaiseTarget.Alliance, "Alliance raid members")
            .AddOption(RaiseTarget.Everyone, "Any dead player");

        def.AbilityTrack(Track.Heal, "Heal");
        def.AbilityTrack(Track.Esuna, "Esuna");

        return def;
    }

    private void HealSingle(Action<Actor, TrackPartyHealth.PartyMemberState> healFun)
    {
        if (Health.BestSTHealTarget is (var a, var b))
            healFun(a, b);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.MountId > 0)
            return;

        Health.Update(Hints);

        AutoRaise(strategy);

        if (strategy.Enabled(Track.Esuna))
        {
            foreach (var st in Health.PartyMemberStates)
            {
                if (st.EsunableStatusRemaining > GCD + 2f)
                {
                    UseGCD(BossMod.WHM.AID.Esuna, World.Party[st.Slot]);
                    break;
                }
            }
        }

        if (strategy.Enabled(Track.Heal))
            switch (Player.Class)
            {
                case Class.WHM:
                    AutoWHM(strategy);
                    break;
                case Class.AST:
                    AutoAST(strategy);
                    break;
                case Class.SCH:
                    AutoSCH(strategy, primaryTarget);
                    break;
                case Class.SGE:
                    AutoSGE(strategy, primaryTarget);
                    break;
            }
    }

    private void UseGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseGCD(ActionID action, Actor? target, int extraPriority = 0)
        => Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + 500 + extraPriority);

    private void UseOGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseOGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseOGCD(ActionID action, Actor? target, int extraPriority = 0)
        => Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Medium + extraPriority);

    private void AutoRaise(StrategyValues strategy)
    {
        var swiftcast = StatusDetails(Player, (uint)BossMod.WHM.SID.Swiftcast, Player.InstanceID, 15).Left;
        var thinair = StatusDetails(Player, (uint)BossMod.WHM.SID.ThinAir, Player.InstanceID, 12).Left;
        var swiftcastCD = NextChargeIn(BossMod.WHM.AID.Swiftcast);
        var raise = strategy.Option(Track.Raise).As<RaiseStrategy>();

        void UseThinAir()
        {
            if (thinair == 0 && Player.Class == Class.WHM)
                UseGCD(BossMod.WHM.AID.ThinAir, Player, extraPriority: 3);
        }

        switch (raise)
        {
            case RaiseStrategy.None:
                break;
            case RaiseStrategy.Hardcast:
                if (swiftcast == 0 && GetRaiseTarget(strategy) is Actor tar)
                {
                    UseThinAir();
                    UseGCD(RaiseAction, tar);
                }
                break;
            case RaiseStrategy.Swiftcast:
                if (GetRaiseTarget(strategy) is Actor tar2)
                {
                    if (swiftcast > GCD)
                    {
                        UseThinAir();
                        UseGCD(RaiseAction, tar2);
                    }
                    else
                        UseGCD(BossMod.WHM.AID.Swiftcast, Player);
                }
                break;
            case RaiseStrategy.Slowcast:
                if (GetRaiseTarget(strategy) is Actor tar3)
                {
                    UseThinAir();
                    UseGCD(BossMod.WHM.AID.Swiftcast, Player, extraPriority: 2);
                    if (swiftcastCD > 8)
                        UseGCD(RaiseAction, tar3, extraPriority: 1);
                }
                break;
        }
    }

    private Actor? GetRaiseTarget(StrategyValues strategy)
    {
        var candidates = strategy.Option(Track.RaiseTarget).As<RaiseTarget>() switch
        {
            RaiseTarget.Everyone => World.Actors.Where(x => x.Type is ActorType.Player or ActorType.DutySupport && x.IsAlly),
            RaiseTarget.Alliance => World.Party.WithoutSlot(true, false),
            _ => World.Party.WithoutSlot(true, true)
        };

        return candidates.Where(x => x.IsDead && Player.DistanceToHitbox(x) <= 30 && !BeingRaised(x)).MaxBy(actor => actor.Class.GetRole() switch
        {
            Role.Healer => 5,
            Role.Tank => 4,
            _ => actor.Class is Class.RDM or Class.SMN or Class.ACN ? 3 : 2
        });
    }

    private static bool BeingRaised(Actor actor) => actor.Statuses.Any(s => s.ID is 148 or 1140 or 2648);

    private bool ShouldHealInArea(WPos pos, float radius, float ratio) => Health.ShouldHealInArea(pos, radius, ratio);

    private void AutoWHM(StrategyValues strategy)
    {
        var gauge = World.Client.GetGauge<WhiteMageGauge>();

        HealSingle((target, state) =>
        {
            if (state.PredictedHPRatio < 0.5 && gauge.Lily > 0)
                UseGCD(BossMod.WHM.AID.AfflatusSolace, target);

            if (state.PredictedHPRatio < 0.25)
                UseOGCD(BossMod.WHM.AID.Tetragrammaton, target);
        });

        if (ShouldHealInArea(Player.Position, 15, 0.75f) && gauge.Lily > 0)
            UseGCD(BossMod.WHM.AID.AfflatusRapture, Player);

        if (ShouldHealInArea(Player.Position, 10, 0.5f))
            UseGCD(BossMod.WHM.AID.Cure3, Player);
    }

    private static readonly (AstrologianCard, BossMod.AST.AID)[] SupportCards = [
        (AstrologianCard.Arrow, BossMod.AST.AID.TheArrow),
        (AstrologianCard.Spire, BossMod.AST.AID.TheSpire),
        (AstrologianCard.Bole, BossMod.AST.AID.TheBole),
        (AstrologianCard.Ewer, BossMod.AST.AID.TheEwer)
    ];

    private void AutoAST(StrategyValues strategy)
    {
        var gauge = World.Client.GetGauge<AstrologianGauge>();

        HealSingle((target, state) =>
        {
            if (state.PendingHPRatio < 0.3)
                UseOGCD(BossMod.AST.AID.EssentialDignity, target);

            if (state.PredictedHPRatio < 0.3)
            {
                UseOGCD(BossMod.AST.AID.CelestialIntersection, target);

                if (gauge.CurrentArcana == AstrologianCard.Lady)
                    UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player);

                foreach (var (card, action) in SupportCards)
                    if (gauge.CurrentCards.Contains(card))
                        UseOGCD(action, target);
            }

            if (state.PredictedHPRatio < 0.5 && !Unlocked(BossMod.AST.AID.CelestialIntersection) && NextChargeIn(BossMod.AST.AID.EssentialDignity) > 2.5f)
                UseGCD(BossMod.AST.AID.Benefic, target);
        });

        if (ShouldHealInArea(Player.Position, 15, 0.7f))
            UseGCD(BossMod.AST.AID.AspectedHelios, Player);

        if (Player.InCombat)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.EarthlyStar), Player, ActionQueue.Priority.Medium, targetPos: Player.PosRot.XYZ());
    }

    private void AutoSCH(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = World.Client.GetGauge<ScholarGauge>();

        var pet = World.Client.ActivePet.InstanceID == 0xE0000000 ? null : World.Actors.Find(World.Client.ActivePet.InstanceID);
        var haveSeraph = gauge.SeraphTimer > 0;
        var haveEos = !haveSeraph;

        var aetherflow = gauge.Aetherflow > 0;

        if (aetherflow && ShouldHealInArea(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SCH.AID.Indomitability, Player);

        if (pet != null)
        {
            if (haveEos && ShouldHealInArea(pet.Position, 20, 0.5f))
                UseOGCD(BossMod.SCH.AID.FeyBlessing, Player);

            if (ShouldHealInArea(pet.Position, 15, 0.8f))
                UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player);
        }

        HealSingle((target, state) =>
        {
            if (state.PredictedHPRatio < 0.3)
            {
                var canLustrate = gauge.Aetherflow > 0 && Unlocked(BossMod.SCH.AID.Lustrate);
                if (canLustrate)
                {
                    UseOGCD(BossMod.SCH.AID.Excogitation, target);
                    UseOGCD(BossMod.SCH.AID.Lustrate, target);
                }
                else
                    UseGCD(BossMod.SCH.AID.Physick, target);
            }
        });
    }

    private void AutoSGE(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = World.Client.GetGauge<SageGauge>();

        var haveBalls = gauge.Addersgall > 0;

        if (haveBalls && ShouldHealInArea(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SGE.AID.Ixochole, Player);

        if (ShouldHealInArea(Player.Position, 30, 0.8f))
        {
            UseOGCD(Unlocked(BossMod.SGE.AID.PhysisII) ? BossMod.SGE.AID.PhysisII : BossMod.SGE.AID.Physis, Player);
        }

        HealSingle((target, state) =>
        {
            if (haveBalls && state.PredictedHPRatio < 0.5)
            {
                UseOGCD(BossMod.SGE.AID.Taurochole, target);
                UseOGCD(BossMod.SGE.AID.Druochole, target);
            }

            if (state.PredictedHPRatio < 0.3)
            {
                UseOGCD(BossMod.SGE.AID.Haima, target);
            }
        });

        foreach (var rw in Raidwides)
        {
            if ((rw - World.CurrentTime).TotalSeconds < 15 && haveBalls)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player);
        }
    }
}
