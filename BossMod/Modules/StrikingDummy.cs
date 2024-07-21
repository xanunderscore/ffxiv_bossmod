#if DEBUG
using System.Diagnostics.CodeAnalysis;

namespace BossMod.StrikingDummy;

public enum OID : uint
{
    Boss = 0x2DE0,
}

// alternating gaze attack - 5s inactive, 1s active
// good for testing ai
class FakeGazeAttack : Components.GenericGaze
{
    private DateTime _start;
    private DateTime _end;

    public FakeGazeAttack(BossModule module) : base(module)
    {
        ResetTimer();
    }

    public override void Update()
    {
        if (Module.WorldState.CurrentTime >= _end)
            ResetTimer();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_start < WorldState.CurrentTime)
            hints.Add("Eye active now", false);
        else
            hints.Add($"Eye active in {(_start - WorldState.CurrentTime).TotalSeconds:f1}", false);
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        yield return new Eye(Module.PrimaryActor.Position, _start);
    }

    private void ResetTimer()
    {
        _start = Module.WorldState.FutureTime(5);
        _end = _start.AddSeconds(1);
    }
}

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "uncomment the component you want to test with")]
class StrikingDummyStates : StateMachineBuilder
{
    public static float InitialDelay = 6.5f;
    public static int NumLoops = 5;

    public StrikingDummyStates(BossModule module) : base(module)
    {
        var state = MockGaze; // MockRaidbuffs;
        SimplePhase(1, state, "Striking dummy").Raw.Update = () => !Module.PrimaryActor.InCombat;
    }

    private void MockGaze(uint id)
    {
        Timeout(id, 0, "Alternating gaze attack").ActivateOnEnter<FakeGazeAttack>().DeactivateOnExit<FakeGazeAttack>();
    }

    private void MockRaidbuffs(uint id)
    {
        Timeout(id, InitialDelay, "Simulated raid buff 1").SetHint(StateMachine.StateHint.VulnerableStart);
        for (uint i = 0; i < NumLoops; i++)
        {
            Timeout(id + (i + 1) * 50, 20, "Filler").SetHint(StateMachine.StateHint.VulnerableEnd);
            Timeout(id + (i + 1) * 50 + 1, 100, $"Simulated raid buff {2 + i}").SetHint(StateMachine.StateHint.VulnerableStart);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Boss, PlanLevel = 1)]
public class StrikingDummy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
#endif
