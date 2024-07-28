namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D012Drowsie;

public enum OID : uint
{
    Boss = 0x4195, // R5.000, x1
    Helper = 0x233C, // R0.500, x10, 523 type
}

public enum AID : uint
{
    Uppercut = 39132, // Boss->none, 5.0s cast, single-target
    Wallop = 36479, // _Gen_IhuykatumuIvy->self, 7.0s cast, range 40 width 10 rect
    BigWallop = 36482, // _Gen_IhuykatumuIvy->self, 7.0s cast, range 40 width 16 rect
    Sneeze = 36475, // Boss->self, 5.0s cast, range 60 150-degree cone
    FlagrantSpread = 36522, // _Gen_Mimiclot5/_Gen_Mimiclot7->none, 5.0s cast, range 6 circle
    FlagrantSpread2 = 36485, // _Gen_Mimiclot8->self, 5.0s cast, range 6 circle
    Arise = 36478, // 419C->self, 3.0s cast, range 8 circle

}


class Wallop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(40, 5));
class BigWallop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BigWallop), new AOEShapeRect(40, 8));
class Sneeze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sneeze), new AOEShapeCone(60, 75.Degrees()));
class Uppercut(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Uppercut));
class Clots(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => x.NameID == 12720), ArenaColor.Enemy);
    }
}
class Spread1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FlagrantSpread), 6);
class Spread2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlagrantSpread2), new AOEShapeCircle(6));
class Arise(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Arise), new AOEShapeCircle(8));

class D012DrowsieStates : StateMachineBuilder
{
    public D012DrowsieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<BigWallop>()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<Sneeze>()
            .ActivateOnEnter<Clots>()
            .ActivateOnEnter<Spread1>()
            .ActivateOnEnter<Spread2>()
            .ActivateOnEnter<Arise>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12716)]
public class D012Drowsie(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, 53), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if ((OID)e.Actor.OID == OID.Boss)
                e.Priority = 1;

            if (e.Actor.NameID == 12720)
                e.Priority = 2;
        }
    }
}

