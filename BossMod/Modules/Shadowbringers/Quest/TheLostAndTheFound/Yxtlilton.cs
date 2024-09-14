using BossMod.Autorotation;

namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Yxtlilton;

public enum OID : uint
{
    Boss = 0x29B0,
    Helper = 0x233C,
}

class LamittAI(WorldState ws) : StatelessRotation(ws, 25)
{
    protected override void Exec(Actor? primaryTarget)
    {

    }
}

class YxtliltonStates : StateMachineBuilder
{
    public YxtliltonStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 679, NameID = 8393)]
public class Yxtlilton(WorldState ws, Actor primary) : BossModule(ws, primary, new(-120, -770), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;
}
