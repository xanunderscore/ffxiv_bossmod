namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class SGEConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal damage rotation on Dosis (ST) or Dyskrasia (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Use mouseover targeting for friendly spells")]
        public bool MouseoverFriendly = true;

        // icarus can target enemies
        [PropertyDisplay("Use mouseover targeting for Icarus")]
        public bool MouseoverIcarus = true;

        [PropertyDisplay("Automatic Esuna")]
        public bool AutoEsuna = true;

        [PropertyDisplay("Automatic raise")]
        public bool AutoRaise = false;

        [PropertyDisplay("Automatically choose Kardia target")]
        public bool AutoKardia = true;

        [PropertyDisplay("Automatically use Druochole (ST heal) to prevent Addersgall overcap")]
        public bool PreventGallOvercap = true;
    }
}
