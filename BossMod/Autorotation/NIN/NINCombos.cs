using System;

namespace BossMod.NIN
{
    class Combos {
        static AID[] Sequences = [AID.Jin,AID.FumaShuriken,AID.FumaShuriken,AID.FumaShuriken,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Chi,AID.None,AID.Ten2,AID.Ten2,AID.None,AID.None,AID.Katon,AID.Katon,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Ten,AID.Chi2,AID.None,AID.Chi2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Raiton,AID.None,AID.Raiton,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Ten,AID.Jin2,AID.Jin2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Hyoton,AID.Hyoton,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Jin,AID.None,AID.Jin2,AID.Chi2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Ten2,AID.None,AID.None,AID.Ten2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Huton,AID.None,AID.None,AID.Huton,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Jin,AID.Jin2,AID.None,AID.Ten2,AID.None,AID.None,AID.None,AID.Chi2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Chi2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Doton,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Doton,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Chi,AID.Chi2,AID.Ten2,AID.None,AID.None,AID.None,AID.Jin2,AID.None,AID.None,AID.Jin2,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.None,AID.Suiton,AID.None,AID.None,AID.Suiton];

        static AID[] SequenceOrder = [AID.FumaShuriken, AID.Katon, AID.Raiton, AID.Hyoton, AID.Huton, AID.Doton, AID.Suiton];

        public static AID GetNextAction(AID ninjutsu, int comboState)
        {
            var ix = ninjutsu switch {
                AID.FumaShuriken => 0,
                AID.Katon or AID.GokaMekkyaku => 1,
                AID.Raiton => 2,
                AID.Hyoton or AID.HyoshoRanryu => 3,
                AID.Huton => 4,
                AID.Doton => 5,
                AID.Suiton => 6,
                _ => -1
            };
            if (ix == -1) {
                Service.Log($"Action {ninjutsu} is not a ninjutsu action");
                return AID.None;
            }
            return Sequences[ix * 58 + comboState];
        }
    }
}
