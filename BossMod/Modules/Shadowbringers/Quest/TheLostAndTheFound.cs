namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound;

public enum OID : uint
{
    Boss = 0x29AA,
    Helper = 0x233C,
}

class SaveTheDipshit(BossModule module) : BossComponent(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x29A9)
            WorldState.Execute(new PartyState.OpModify(1, new PartyState.Member(actor.InstanceID, actor.InstanceID, false, actor.Name)));
    }
}

class SophrosyneStates : StateMachineBuilder
{
    public SophrosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SaveTheDipshit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8395)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(632, 64.15f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;
}
