namespace BossMod.Modules.Stormblood.Quest.RequiemForHeroesP1;

public enum OID : uint
{
    Boss = 0x268A,
    Helper = 0x233C,
}

class ZenosYaeGalvusP1States : StateMachineBuilder
{
    public ZenosYaeGalvusP1States(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<Hien>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 648, NameID = 6039)]
public class ZenosYaeGalvusP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(20));

class Hien(BossModule module) : Components.RoleplayModule(module)
{
    public override void Execute(Actor? primaryTarget)
    {
        Hints.RecommendedRangeToTarget = 3;

        var ajisai = StatusDetails(primaryTarget, Roleplay.SID.Ajisai, Player.InstanceID);

        switch (ComboAction)
        {
            case Roleplay.AID.Gofu:
                UseGCD(Roleplay.AID.Yagetsu, primaryTarget);
                break;

            case Roleplay.AID.Kyokufu:
                UseGCD(Roleplay.AID.Gofu, primaryTarget);
                break;

            default:
                if (ajisai.Left < 5)
                    UseGCD(Roleplay.AID.Ajisai, primaryTarget);
                UseGCD(Roleplay.AID.Kyokufu, primaryTarget);
                break;
        }

        if (PredictedHP(Player) < 5000)
            UseOGCD(Roleplay.AID.SecondWind, Player);

        UseOGCD(Roleplay.AID.HissatsuGyoten, primaryTarget);
    }
}
