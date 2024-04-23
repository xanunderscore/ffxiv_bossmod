﻿namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state relared to darkened fire add placement mechanic
// adds should be neither too close (or they insta explode and wipe raid) nor too far (or during brightened fire someone wouldn't be able to hit two adds)
class DarkenedFire(BossModule module) : BossComponent(module)
{
    private const float _minRange = 11; // note: on one of our pulls adds at (94.14, 105.55) and (94.21, 94.69) (distance=10.860) linked and wiped us
    private const float _maxRange = 13; // brigthened fire aoe radius is 7, so this is x2 minus some room for positioning

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool haveTooClose = false;
        int numInRange = 0;
        foreach (var player in Raid.WithoutSlot().Where(player => CanBothBeTargets(player, actor)))
        {
            haveTooClose |= player.Position.InCircle(actor.Position, _minRange);
            if (player.Position.InCircle(actor.Position, _maxRange))
                ++numInRange;
        }

        if (haveTooClose)
        {
            hints.Add("Too close to other players!");
        }
        else if (numInRange < 2)
        {
            hints.Add("Too far from other players!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw other potential targets, to simplify positioning
        bool healerOrTank = pc.Role is Role.Tank or Role.Healer;
        foreach (var player in Raid.WithoutSlot().Where(player => CanBothBeTargets(player, pc)))
        {
            bool tooClose = player.Position.InCircle(pc.Position, _minRange);
            bool inRange = player.Position.InCircle(pc.Position, _maxRange);
            Arena.Actor(player, tooClose ? ArenaColor.Danger : (inRange ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric));
        }

        // draw circles around pc
        Arena.AddCircle(pc.Position, _minRange, ArenaColor.Danger);
        Arena.AddCircle(pc.Position, _maxRange, ArenaColor.Safe);
    }

    private bool CanBothBeTargets(Actor one, Actor two)
    {
        // i'm quite sure it selects either 4 dds or 2 tanks + 2 healers
        return one != two && (one.Role == Role.Tank || one.Role == Role.Healer) == (two.Role == Role.Tank || two.Role == Role.Healer);
    }
}
