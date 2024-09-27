﻿using BossMod.Autorotation;

namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Yxtlilton;

public enum OID : uint
{
    Boss = 0x29B0,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->29AF, no cast, single-target
    _Spell_TheCodexOfDarknessII = 17010, // Boss->self, 3.0s cast, range 100 circle
    _Spell_TheCodexOfThunderIII = 17012, // Boss->self, 6.0s cast, single-target
    _Spell_TheCodexOfThunderIII1 = 17013, // Helper->29AE, no cast, range 3 circle
    _Spell_TheCodexOfGravity = 17014, // Boss->player, 4.5s cast, range 6 circle
}

class CodexOfDarkness(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_TheCodexOfDarknessII));
class CodexOfGravity(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Spell_TheCodexOfGravity), 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Stacks.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(1.5f, 100), Arena.Center, default, Stacks[0].Activation);
    }
}

class LamittAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private float ActorHP(Actor a) => (float)(a.HPMP.CurHP + World.PendingEffects.PendingHPDifference(a.InstanceID)) / a.HPMP.MaxHP;

    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var partyHealth = World.Party.WithoutSlot().Where(x => x.Position.InCircle(Player.Position, 15)).Average(ActorHP);
        var lowest = World.Party.WithoutSlot().MinBy(ActorHP)!;
        var esunable = World.Party.WithoutSlot().FirstOrDefault(x => x.FindStatus(482) != null);
        var doomed = World.Party.WithoutSlot().FirstOrDefault(x => x.FindStatus(1769) != null);

        if (partyHealth < 0.6f)
            UseAction(Roleplay.AID.RonkanMedica, Player);

        if (lowest.HPMP.CurHP * 3 <= lowest.HPMP.MaxHP)
            UseAction(Roleplay.AID.RonkanCureII, lowest);

        if (esunable != null)
            UseAction(Roleplay.AID.RonkanEsuna, esunable);

        if (doomed != null)
        {
            UseAction(Roleplay.AID.RonkanRenew, doomed);
            UseAction(Roleplay.AID.RonkanCureII, doomed);
        }

        UseAction(Roleplay.AID.RonkanStoneII, primaryTarget);
    }
}

class AutoLamitt(BossModule module) : Components.RotationModule<LamittAI>(module);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}

class YxtliltonStates : StateMachineBuilder
{
    public YxtliltonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<AutoLamitt>()
            .ActivateOnEnter<CodexOfDarkness>()
            .ActivateOnEnter<CodexOfGravity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8393)]
public class Yxtlilton(WorldState ws, Actor primary) : BossModule(ws, primary, new(-120, -770), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
