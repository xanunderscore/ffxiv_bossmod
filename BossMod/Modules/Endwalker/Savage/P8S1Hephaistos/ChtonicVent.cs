﻿namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class CthonicVent(BossModule module) : Components.GenericAOEs(module)
{
    public int NumTotalCasts { get; private set; }
    private readonly List<WPos> _centers = [];
    private static readonly AOEShapeCircle _shape = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _centers)
            yield return new(_shape, c);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // note: we can determine position ~0.1s earlier by using eobjanim
        if ((AID)spell.Action.ID == AID.CthonicVentAOE1)
            _centers.Add(caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CthonicVentMoveNear:
                _centers.Add(caster.Position + caster.Rotation.ToDirection() * 30);
                break;
            case AID.CthonicVentMoveDiag:
                _centers.Add(caster.Position + caster.Rotation.ToDirection() * 42.426407f);
                break;
            case AID.CthonicVentAOE1:
            case AID.CthonicVentAOE2:
            case AID.CthonicVentAOE3:
                ++NumTotalCasts;
                _centers.RemoveAll(c => c.AlmostEqual(caster.Position, 2));
                break;
        }
    }
}
