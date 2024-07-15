namespace BossMod.StrikingDummy;

public enum OID : uint
{
    Boss = 0x385,
}

class StrikingDummyStates : StateMachineBuilder
{
    public static float InitialDelay = 8f;
    public static int NumLoops = 5;

    public StrikingDummyStates(BossModule module) : base(module)
    {
        SimplePhase(1, DummyP0, "Striking dummy").Raw.Update = () => !Module.PrimaryActor.InCombat;
    }

    private void DummyP0(uint id)
    {
        Timeout(id, InitialDelay, "Simulated raid buff 1").SetHint(StateMachine.StateHint.VulnerableStart);
        for (uint i = 0; i < NumLoops; i++)
        {
            Timeout(id + (i + 1) * 50, 20, "Filler phase").SetHint(StateMachine.StateHint.VulnerableEnd);
            Timeout(id + (i + 1) * 50 + 1, 100, $"Simulated raid buff {2 + i}").SetHint(StateMachine.StateHint.VulnerableStart);
        }
    }
}

public class StrikingDummy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
