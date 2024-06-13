namespace BossMod;

[ConfigDisplay(Parent = typeof(AutorotationConfig))]
class GNBConfig : ConfigNode
{
    [PropertyDisplay("Execute optimal rotations on Keen Edge (ST) or Demon Slice (AOE)")]
    public bool FullRotation = true;

    [PropertyDisplay("Execute preceeding actions for ST combos")]
    public bool STCombos = true;

    [PropertyDisplay("Execute preceeding actions for AOE combo")]
    public bool AOECombos = true;

    [PropertyDisplay("Smart targeting for Shirk and HoC (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)")]
    public bool SmartHeartofCorundumShirkTarget = true;

    [PropertyDisplay("Use provoke on mouseover, if available and hostile")]
    public bool ProvokeMouseover = true;

    [PropertyDisplay("Forbid 'Lightning Shot' too early in prepull")]
    public bool ForbidEarlyLightningShot = true;

    [PropertyDisplay("Use both Rough Divide charges in No Mercy")]
    public bool NoMercyRoughDivide = true;

    [PropertyDisplay("<= 2.47 sks rotation")]
    public bool Skscheck = true;

    [PropertyDisplay("Early No Mercy in Opener (NOTE: This will break Lv30-53 rotation)")]
    public bool EarlyNoMercy = true;

    [PropertyDisplay("Early Sonic Break in Opener (NOTE: This will break Lv30-53 rotation)")]
    public bool EarlySonicBreak = true;
}
