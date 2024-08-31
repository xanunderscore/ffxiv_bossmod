using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public record struct PartyMemberState
    {
        public int Slot;
        public int PredictedHP;
        public int PredictedHPMissing;
        public float AttackerStrength;
        // predicted ratio including pending HP loss and current attacker strength
        public float PredictedHPRatio;
        // *actual* ratio including pending HP loss, used mainly just for essential dignity
        public float PendingHPRatio;
        // remaining time on cleansable status, to avoid casting it on a target that will lose the status by the time we finish
        public float EsunableStatusRemaining;
        // tank invulns go here, but also statuses like Excog that give burst heal below a certain HP threshold
        // no point in spam healing a tank in an area with high mob density (like Sirensong Sea pull after second boss) until their excog falls off
        public float NoHealStatusRemaining;
    }

    private readonly PartyMemberState[] PartyMemberStates = new PartyMemberState[PartyState.MaxPartySize];

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

    private (Actor Target, PartyMemberState State) BestSTHealTarget
    {
        get
        {
            var best = PartyMemberStates.Where(x => x.NoHealStatusRemaining < 1.5f).MinBy(x => x.PredictedHPRatio);
            return (World.Party[best.Slot]!, best);
        }
    }

    private static readonly uint[] NoHealStatuses = [
        82, // Hallowed Ground
        409, // Holmgang
        810, // Living Dead
        811, // Walking Dead
        1220, // Excogitation
        1836, // Superbolide
        2685, // Catharsis of Corundum
        (uint)WAR.SID.BloodwhettingDefenseLong
    ];

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        // copied from veyn's HealerActions in EW bossmod - i am a thief
        BitMask esunas = new();
        foreach (var caster in World.Party.WithoutSlot(partyOnly: true).Where(a => a.CastInfo?.IsSpell(BossMod.WHM.AID.Esuna) ?? false))
            esunas.Set(World.Party.FindSlot(caster.CastInfo!.TargetID));

        for (var i = 0; i < PartyMemberStates.Length; i++)
        {
            var actor = World.Party[i];
            ref var state = ref PartyMemberStates[i];
            state.Slot = i;
            state.EsunableStatusRemaining = 0;
            state.NoHealStatusRemaining = 0;
            if (actor == null || actor.IsDead || actor.HPMP.MaxHP == 0)
            {
                state.PredictedHP = state.PredictedHPMissing = 0;
                state.PredictedHPRatio = state.PendingHPRatio = 1;
            }
            else
            {
                state.PredictedHP = (int)actor.HPMP.CurHP + World.PendingEffects.PendingHPDifference(actor.InstanceID);
                state.PredictedHPMissing = (int)actor.HPMP.MaxHP - state.PredictedHP;
                state.PredictedHPRatio = state.PendingHPRatio = (float)state.PredictedHP / actor.HPMP.MaxHP;
                var canEsuna = actor.IsTargetable && !esunas[i];
                foreach (var s in actor.Statuses)
                {
                    if (canEsuna && Utils.StatusIsRemovable(s.ID))
                        state.EsunableStatusRemaining = Math.Max(StatusDuration(s.ExpireAt), state.EsunableStatusRemaining);

                    if (NoHealStatuses.Contains(s.ID))
                        state.NoHealStatusRemaining = StatusDuration(s.ExpireAt);
                }
            }
        }
        foreach (var enemy in Hints.PotentialTargets)
        {
            var targetSlot = World.Party.FindSlot(enemy.Actor.TargetID);
            if (targetSlot >= 0 && targetSlot < PartyMemberStates.Length)
            {
                ref var state = ref PartyMemberStates[targetSlot];
                state.AttackerStrength += enemy.AttackStrength;
                if (state.PredictedHPRatio < 0.99f)
                    state.PredictedHPRatio -= enemy.AttackStrength;
            }
        }

        AutoRaise(strategy);

        if (strategy.Enabled(Track.Esuna))
        {
            foreach (var st in PartyMemberStates)
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

    private void AutoWHM(StrategyValues strategy)
    {
        var gauge = GetGauge<WhiteMageGauge>();

        var (bestSTHealTarget, state) = BestSTHealTarget;
        if (state.PredictedHPRatio < 0.25)
        {
            if (gauge.Lily > 0)
                UseGCD(BossMod.WHM.AID.AfflatusSolace, bestSTHealTarget);

            UseOGCD(BossMod.WHM.AID.Tetragrammaton, bestSTHealTarget);
        }
    }

    private static readonly (AstrologianCard, BossMod.AST.AID)[] SupportCards = [
        (AstrologianCard.Arrow, BossMod.AST.AID.TheArrow),
        (AstrologianCard.Spire, BossMod.AST.AID.TheSpire),
        (AstrologianCard.Bole, BossMod.AST.AID.TheBole),
        (AstrologianCard.Ewer, BossMod.AST.AID.TheEwer)
    ];

    private void AutoAST(StrategyValues strategy)
    {
        var gauge = GetGauge<AstrologianGauge>();

        var (bestSTHealTarget, state) = BestSTHealTarget;
        if (state.PendingHPRatio < 0.3)
            UseOGCD(BossMod.AST.AID.EssentialDignity, bestSTHealTarget);

        if (state.PredictedHPRatio < 0.3)
        {
            UseOGCD(BossMod.AST.AID.CelestialIntersection, bestSTHealTarget);

            if (gauge.CurrentArcana == AstrologianCard.Lady)
                UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player);

            foreach (var (card, action) in SupportCards)
                if (gauge.CurrentCards.Contains(card))
                    UseOGCD(action, bestSTHealTarget);
        }

        if (Player.InCombat)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.EarthlyStar), Player, ActionQueue.Priority.Medium, targetPos: Player.PosRot.XYZ());
    }

    private void AutoSCH(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = GetGauge<ScholarGauge>();

        if (World.Party.WithoutSlot().Count() == 1 && gauge.Aetherflow > 0 && primaryTarget != null)
            UseOGCD(BossMod.SCH.AID.EnergyDrain, primaryTarget);

        var (bestSTHealTarget, state) = BestSTHealTarget;
        if (state.PredictedHPRatio < 0.3)
        {
            var canLustrate = gauge.Aetherflow > 0 && Unlocked(BossMod.SCH.AID.Lustrate);
            if (canLustrate)
            {
                UseOGCD(BossMod.SCH.AID.Excogitation, bestSTHealTarget);
                UseOGCD(BossMod.SCH.AID.Lustrate, bestSTHealTarget);
            }
            else
            {
                UseGCD(BossMod.SCH.AID.Adloquium, bestSTHealTarget);
                UseGCD(BossMod.SCH.AID.Physick, bestSTHealTarget);
            }
        }
    }
}
