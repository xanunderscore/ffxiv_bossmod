namespace BossMod;

[ConfigDisplay(Name = "Solo duty settings")]
public sealed class QuestBattleConfig : ConfigNode
{
    public bool UseDash = true;
#if DEBUG
    public bool Speedhack = false;
#endif
}
