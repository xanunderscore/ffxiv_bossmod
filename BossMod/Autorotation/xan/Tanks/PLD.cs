using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class PLD(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("PLD", "Paladin", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.GLA), 100);

        def.DefineShared().AddAssociatedActions(AID.FightOrFlight);

        return def;
    }

    public float FightOrFlight; // max 20
    public float GoringBladeReady; // max 30
    public float DivineMight; // max 30
    public float AtonementReady; // max 30
    public float SupplicationReady; // max 30
    public float SepulchreReady; // max 30
    public float BladeOfHonorReady; // max 30
    public (float Left, int Stacks) Requiescat; // max 30/4 stacks
    public float CCTimer;
    public ushort CCStep;

    public int OathGauge; // 0-100

    public AID ConfiteorCombo;

    public int NumAOETargets;

    private Actor? BestRangedTarget;

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.HolyCircle or AID.HolySpirit => DivineMight > _state.GCD || Requiescat.Stacks > 0 ? 0 : base.GetCastTime(aid),
        _ => 0
    };

    private void CalcNextBestGCD(Actor? primaryTarget)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 2 && Unlocked(AID.HolySpirit))
                PushGCD(AID.HolySpirit, BestRangedTarget);

            return;
        }

        if (ConfiteorCombo != AID.None && _state.CurMP >= 1000)
            PushGCD(ConfiteorCombo, BestRangedTarget);

        // use goring blade even in AOE
        if (GoringBladeReady > _state.GCD)
            PushGCD(AID.GoringBlade, primaryTarget, 50);

        if (NumAOETargets >= 3 && Unlocked(AID.TotalEclipse))
        {
            if (Unlocked(AID.HolyCircle) &&
                (Requiescat.Left > _state.GCD || DivineMight > _state.GCD && FightOrFlight > _state.GCD) &&
                    _state.CurMP >= 1000)
                PushGCD(AID.HolyCircle, Player);

            if (Unlocked(AID.Prominence) && ComboLastMove == AID.TotalEclipse)
            {
                if (DivineMight > _state.GCD && Unlocked(AID.HolyCircle) && _state.CurMP >= 1000)
                    PushGCD(AID.HolyCircle, Player);

                PushGCD(AID.Prominence, Player);
            }

            PushGCD(AID.TotalEclipse, Player);
        }
        else
        {
            // fallback - cast holy spirit if we don't have a melee
            if (DivineMight > _state.GCD && _state.CurMP >= 1000)
                PushGCD(AID.HolySpirit, primaryTarget, -50);

            if (Requiescat.Left > _state.GCD || DivineMight > _state.GCD && FightOrFlight > _state.GCD)
                PushGCD(AID.HolySpirit, primaryTarget ?? BestRangedTarget);

            if (AtonementReady > _state.GCD && FightOrFlight > _state.GCD)
                PushGCD(AID.Atonement, primaryTarget);

            if (SepulchreReady > _state.GCD)
                PushGCD(AID.Sepulchre, primaryTarget);

            if (SupplicationReady > _state.GCD)
                PushGCD(AID.Supplication, primaryTarget);

            if (Unlocked(AID.RageOfHalone) && ComboLastMove == AID.RiotBlade)
            {
                if (DivineMight > _state.GCD && _state.CurMP >= 1000)
                    PushGCD(AID.HolySpirit, primaryTarget ?? BestRangedTarget);

                if (AtonementReady > _state.GCD)
                    PushGCD(AID.Atonement, primaryTarget);

                PushGCD(AID.RageOfHalone, primaryTarget);
            }

            if (Unlocked(AID.RiotBlade) && ComboLastMove == AID.FastBlade)
                PushGCD(AID.RiotBlade, primaryTarget);

            PushGCD(AID.FastBlade, primaryTarget);
        }
    }

    private void CalcNextBestOGCD(float deadline, Actor? primaryTarget)
    {
        if ((AtonementReady > 0 || Requiescat.Left > 0 || DivineMight > 0) && _state.CanWeave(AID.FightOrFlight, 0.6f, deadline))
            PushOGCD(AID.FightOrFlight, Player);

        if (BladeOfHonorReady > deadline && _state.CanWeave(AID.BladeOfHonor, 0.6f, deadline))
            PushOGCD(AID.BladeOfHonor, BestRangedTarget);

        if (FightOrFlight > 0 && Unlocked(AID.Requiescat) && _state.CanWeave(AID.Requiescat, 0.6f, deadline))
        {
            if (Unlocked(AID.Imperator))
                PushOGCD(AID.Imperator, BestRangedTarget);
            else
                PushOGCD(AID.Requiescat, primaryTarget);
        }

        if (FightOrFlight > 0 || _state.CD(AID.FightOrFlight) > 15)
        {
            if (Unlocked(AID.SpiritsWithin) && _state.CanWeave(AID.SpiritsWithin, 0.6f, deadline))
                PushOGCD(AID.SpiritsWithin, primaryTarget);

            if (Unlocked(AID.CircleOfScorn) && _state.CanWeave(AID.CircleOfScorn, 0.6f, deadline) && NumAOETargets > 0)
                PushOGCD(AID.CircleOfScorn, Player);
        }

        if (FightOrFlight > 0 && Unlocked(AID.Intervene) && _state.CanWeave(_state.CD(AID.Intervene) - 30, 0.6f, deadline))
            PushOGCD(AID.Intervene, primaryTarget);
    }

    public override unsafe void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<PaladinGauge>();
        OathGauge = gauge.OathGauge;

        FightOrFlight = StatusLeft(SID.FightOrFlight);
        GoringBladeReady = StatusLeft(SID.GoringBladeReady);
        DivineMight = StatusLeft(SID.DivineMight);
        AtonementReady = StatusLeft(SID.AtonementReady);
        SupplicationReady = StatusLeft(SID.SupplicationReady);
        SepulchreReady = StatusLeft(SID.SepulchreReady);
        BladeOfHonorReady = StatusLeft(SID.BladeOfHonorReady);
        Requiescat = Status(SID.Requiescat);
        ConfiteorCombo = gauge.ConfiteorComboStep switch
        {
            0 => StatusLeft(SID.ConfiteorReady) > _state.GCD ? AID.Confiteor : AID.None,
            1 => AID.BladeOfFaith,
            2 => AID.BladeOfTruth,
            3 => AID.BladeOfValor,
            _ => AID.None
        };

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget).Best;

        NumAOETargets = NumMeleeAOETargets(strategy);

        CalcNextBestGCD(primaryTarget);

        QueueOGCD(deadline => CalcNextBestOGCD(deadline, primaryTarget));
    }
}
