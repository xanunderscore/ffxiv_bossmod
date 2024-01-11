using System;
using System.Net.NetworkInformation;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Lumina.Excel.GeneratedSheets;

namespace BossMod.NIN
{
    public static class Rotation
    {
        public class State : CommonRotation.PlayerState
        {
            public float KassatsuLeft;
            public float TCJLeft;
            public float HutonLeft;
            public float SuitonLeft;
            public float DotonLeft;
            public (float Left, int Combo) CurMudra;
            public float TargetMugLeft;
            public float TargetTrickLeft;
            public float MeisuiLeft;
            public float RaijuReadyLeft;
            public float HiddenLeft;
            public float BunshinLeft;
            public float KamaitachiLeft;

            public byte Ninki;

            // todo: tcj
            public AID CurTen => CurMudra.Combo > 0 ? AID.Ten2 : AID.Ten;
            public AID CurChi => CurMudra.Combo > 0 ? AID.Chi2 : AID.Chi;
            public AID CurJin => CurMudra.Combo > 0 ? AID.Jin2 : AID.Jin;

            public State(float[] cooldowns)
                : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public AID CurNinjutsu =>
                CurMudra.Combo switch
                {
                    0 => AID.Ninjutsu,
                    1 or 2 or 3 => AID.FumaShuriken,
                    6 or 7 => CurKaton,
                    9 or 11 => CurRaiton,
                    13 or 14 => AID.Hyoton,
                    27 or 30 => AID.Huton,
                    39 or 45 => AID.Doton,
                    54 or 57 => AID.Suiton,
                    _ => AID.RabbitMedium
                };

            public AID CurRaiton =>
                Unlocked(TraitID.EnhancedKassatsu) && KassatsuLeft > 0
                    ? AID.HyoshoRanryu
                    : AID.Raiton;

            public AID CurKaton =>
                Unlocked(TraitID.EnhancedKassatsu) && KassatsuLeft > 0
                    ? AID.GokaMekkyaku
                    : AID.Katon;
        }
    }
}
