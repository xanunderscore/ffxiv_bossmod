namespace BossMod.Heavensward.Quest.Ateooh;

public class FindTheMillsStates : StateMachineBuilder
{
    public FindTheMillsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 416;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, PrimaryActorOID = 0x1E9ACF, NameID = 2005711)]
public class FindTheMills(WorldState ws, Actor primary) : DutyModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override void UpdateModule()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }
}
