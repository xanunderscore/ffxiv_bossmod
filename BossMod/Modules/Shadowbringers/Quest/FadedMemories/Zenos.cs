﻿namespace BossMod.Shadowbringers.Quest.FadedMemories;

class Swords(BossModule module) : Components.AddsMulti(module, [0x2F2A, 0x2F2B, 0x2F2C]);

class EntropicFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_EntropicFlame), new AOEShapeRect(50, 4));
class VeinSplitter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_VeinSplitter), new AOEShapeCircle(10));

class ZenosYaeGalvusStates : StateMachineBuilder
{
    public ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swords>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<VeinSplitter>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 69311, NameID = 6039, PrimaryActorOID = (uint)OID.Zenos)]
public class ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-321.03f, 617.73f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}

