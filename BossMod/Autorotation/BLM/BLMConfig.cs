namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class BLMConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Fire/Fire IV (ST), Fire II (AOE), or Blizzard/Blizzard IV (filler)")]
        public bool FullRotation = true;

        [PropertyDisplay("Use mouseover targeting for Aetherial Manipulation")]
        public bool SmartDash = true;
    }
}
