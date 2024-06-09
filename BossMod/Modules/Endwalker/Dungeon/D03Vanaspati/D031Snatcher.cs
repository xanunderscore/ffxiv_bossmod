﻿namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D031Snatcher;

public enum OID : uint
{
    Boss = 0x33E8, // R=3.99
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    LastGasp = 25141, // Boss->player, 5.0s cast, single-target
    LostHope = 25143, // Boss->self, 4.0s cast, range 20 circle, applies temporary misdirection
    MouthOff = 25137, // Boss->self, 3.0s cast, single-target
    NoteOfDespair = 25144, // Boss->self, 5.0s cast, range 40 circle
    Vitriol = 25138, // Helper->self, 9.0s cast, range 13 circle
    Wallow = 25142, // Helper->player, 5.0s cast, range 6 circle
    WhatIsLeft = 25140, // Boss->self, 8.0s cast, range 20 180-degree cone
    WhatIsRight = 25139, // Boss->self, 8.0s cast, range 20 180-degree cone
}

public enum SID : uint
{
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon304 = 304, // player
}

class WhatIsLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhatIsLeft), new AOEShapeCone(40, 90.Degrees()));
class WhatIsRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhatIsRight), new AOEShapeCone(40, 90.Degrees()));
class LostHope(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.LostHope), "Applies temporay misdirection");
class Vitriol(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Vitriol), new AOEShapeCircle(13));
class NoteOfDespair(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.NoteOfDespair));
class Wallow(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Wallow), 6);
class LastGasp(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.LastGasp));

class D031SnatcherStates : StateMachineBuilder
{
    public D031SnatcherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhatIsLeft>()
            .ActivateOnEnter<WhatIsRight>()
            .ActivateOnEnter<LostHope>()
            .ActivateOnEnter<Vitriol>()
            .ActivateOnEnter<NoteOfDespair>()
            .ActivateOnEnter<Wallow>()
            .ActivateOnEnter<LastGasp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10717)]
public class D031Snatcher(WorldState ws, Actor primary) : BossModule(ws, primary, new(-375, 85), new ArenaBoundsCircle(20));
