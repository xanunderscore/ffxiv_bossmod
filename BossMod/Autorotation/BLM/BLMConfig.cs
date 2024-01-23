namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class BLMConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Fire/Fire IV (ST) or Fire II (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Use mouseover targeting for friendly spells")]
        public bool MouseoverFriendly = true;
    }
}
