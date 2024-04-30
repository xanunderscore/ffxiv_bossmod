﻿namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to 'single' and 'multi' fireplumes (normal or parts of gloryplume)
class Fireplume(BossModule module) : BossComponent(module)
{
    private WPos? _singlePos;
    private Angle _multiStartingDirection;
    private int _multiStartedCasts;
    private int _multiFinishedCasts;

    private const float _singleRadius = 15;
    private const float _multiRadius = 10;
    private const float _multiPairOffset = 15;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_singlePos != null && actor.Position.InCircle(_singlePos.Value, _singleRadius))
        {
            hints.Add("GTFO from plume!");
        }

        if (_multiStartedCasts > _multiFinishedCasts)
        {
            if (_multiFinishedCasts > 0 && actor.Position.InCircle(Module.Center, _multiRadius) ||
                _multiFinishedCasts < 8 && InPair(_multiStartingDirection + 45.Degrees(), actor) ||
                _multiFinishedCasts < 6 && InPair(_multiStartingDirection - 90.Degrees(), actor) ||
                _multiFinishedCasts < 4 && InPair(_multiStartingDirection - 45.Degrees(), actor) ||
                _multiFinishedCasts < 2 && InPair(_multiStartingDirection, actor))
            {
                hints.Add("GTFO from plume!");
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_singlePos != null)
        {
            Arena.ZoneCircle(_singlePos.Value, _singleRadius, ArenaColor.AOE);
        }

        if (_multiStartedCasts > _multiFinishedCasts)
        {
            if (_multiFinishedCasts > 0) // don't draw center aoe before first explosion, it's confusing - but start drawing it immediately after first explosion, to simplify positioning
                Arena.ZoneCircle(Module.Center, _multiRadius, _multiFinishedCasts >= 6 ? ArenaColor.Danger : ArenaColor.AOE);

            // don't draw more than two next pairs
            if (_multiFinishedCasts < 8)
                DrawPair(_multiStartingDirection + 45.Degrees(), _multiStartedCasts > 6 && _multiFinishedCasts >= 4);
            if (_multiFinishedCasts < 6)
                DrawPair(_multiStartingDirection - 90.Degrees(), _multiStartedCasts > 4 && _multiFinishedCasts >= 2);
            if (_multiFinishedCasts < 4)
                DrawPair(_multiStartingDirection - 45.Degrees(), _multiStartedCasts > 2);
            if (_multiFinishedCasts < 2)
                DrawPair(_multiStartingDirection, true);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalFireplumeSingleAOE:
            case AID.ExperimentalGloryplumeSingleAOE:
                _singlePos = caster.Position;
                break;
            case AID.ExperimentalFireplumeMultiAOE:
            case AID.ExperimentalGloryplumeMultiAOE:
                if (_multiStartedCasts++ == 0)
                    _multiStartingDirection = Angle.FromDirection(caster.Position - Module.Center);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalFireplumeSingleAOE:
            case AID.ExperimentalGloryplumeSingleAOE:
                _singlePos = null;
                break;
            case AID.ExperimentalFireplumeMultiAOE:
            case AID.ExperimentalGloryplumeMultiAOE:
                ++_multiFinishedCasts;
                break;
        }
    }

    private bool InPair(Angle direction, Actor actor)
    {
        var offset = _multiPairOffset * direction.ToDirection();
        return actor.Position.InCircle(Module.Center - offset, _multiRadius)
            || actor.Position.InCircle(Module.Center + offset, _multiRadius);
    }

    private void DrawPair(Angle direction, bool imminent)
    {
        var offset = _multiPairOffset * direction.ToDirection();
        Arena.ZoneCircle(Module.Center + offset, _multiRadius, imminent ? ArenaColor.Danger : ArenaColor.AOE);
        Arena.ZoneCircle(Module.Center - offset, _multiRadius, imminent ? ArenaColor.Danger : ArenaColor.AOE);
    }
}
