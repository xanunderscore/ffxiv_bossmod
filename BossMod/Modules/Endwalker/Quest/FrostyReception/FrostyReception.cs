namespace BossMod.Endwalker.Quest.FrostyReception;

public enum OID : uint
{
    VergiliaVanCorculum = 0x3646
}

public enum AID : uint
{
    _Weaponskill_AirborneExplosion = 28364, // 233C->363F, 5.0s cast, range 6 circle
    _Weaponskill_GigaTempest = 27440, // 3646->self, 5.0s cast, range 20 circle
    _Weaponskill_SpiralScourge = 27441, // 3646->364F, 4.0s cast, single-target
    _Weaponskill_Ruination = 27443, // 3646->self, 5.0s cast, range 40 width 8 cross
    _Weaponskill_Ruination1 = 27444, // 233C->self, 5.0s cast, range 30 width 8 rect

    _Weaponskill_Gunberd = 27447, // VergiliaVanCorculum->self, 5.0s cast, single-target
    _Weaponskill_Gunberd1 = 27448, // VergiliaVanCorculum->3641, no cast, range 40 width 6 rect
    _Weaponskill_ResinBomb = 27449, // 233C->363F/364F/363D/3641/3640/363E, 5.0s cast, range 5 circle
    _Ability_BombsAway = 27460, // VergiliaVanCorculum->self, 3.0s cast, single-target
    _Weaponskill_LockOn = 27461, // 3648->self, 1.0s cast, range 6 circle
    _Ability_Reinforcements = 27456, // VergiliaVanCorculum->self, 3.0s cast, single-target
    _Weaponskill_Bombardment = 27458, // 363A->self, no cast, single-target
    _Weaponskill_MagitekCannon = 27457, // 363B->player/363E/364F/3640/363D/363F/3641, 5.0s cast, range 6 circle
    _Weaponskill_Bombardment1 = 27459, // 233C->location, 4.0s cast, range 6 circle

}


public class FrostyReceptionStates : StateMachineBuilder
{
    public FrostyReceptionStates(BossModule module) : base(module)
    {
        bool DutyEnd() => Module.WorldState.CurrentCFCID != 812;

        TrivialPhase()
            .ActivateOnEnter<MetalGearThancred>()
            .ActivateOnEnter<MetalGearCheckpoints>()
            .ActivateOnEnter<MetalGearBounds>()
            .Raw.Update = () => DutyEnd() || Module.Raid.Player()?.FindStatus(Roleplay.SID.RolePlaying) == null;
        TrivialPhase(1)
            .ActivateOnEnter<CarriageBounds>()
            .OnEnter(() =>
            {
                Module.Arena.Bounds = new ArenaBoundsCircle(20);
            })
            .Raw.Update = () => DutyEnd() || Module.Raid.Player()?.Position.Z < 0;
        TrivialPhase(2)
            .ActivateOnEnter<PostCarriage>()
            .ActivateOnEnter<GigaTempest>()
            .ActivateOnEnter<Ruination>()
            .ActivateOnEnter<Ruination2>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<ResinBomb>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<Bombardment>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(0, -80);
            })
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 812, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class FrostyReception(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(30))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }
}
