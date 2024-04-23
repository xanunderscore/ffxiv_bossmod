﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class TurretsTour : Components.GenericAOEs
{
    private readonly List<(Actor turret, AOEShapeRect shape)> _turrets = [];
    private readonly List<(Actor caster, AOEShapeRect shape)> _casters = [];
    private DateTime _activation;

    public TurretsTour(BossModule module) : base(module)
    {
        var turrets = module.Enemies(OID.AutomaticTurret);
        foreach (var t in turrets)
        {
            var shape = new AOEShapeRect(50, 3);
            var target = turrets.Exclude(t).InShape(shape, t).Closest(t.Position);
            if (target != null)
                shape.LengthFront = (target.Position - t.Position).Length();
            _turrets.Add((t, shape));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _turrets)
            yield return new(t.shape, t.turret.Position, t.turret.Rotation, _activation);
        foreach (var c in _casters)
            yield return new(c.shape, c.caster.Position, c.caster.CastInfo!.Rotation, c.caster.CastInfo.NPCFinishAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TurretsTourAOE1)
        {
            var shape = new AOEShapeRect(0, 3);
            shape.SetEndPoint(spell.LocXZ, caster.Position, spell.Rotation);
            _casters.Add((caster, shape));
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TurretsTourAOE1)
            _casters.RemoveAll(c => c.caster == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TurretsTourAOE2 or AID.TurretsTourAOE3)
        {
            _turrets.RemoveAll(t => t.turret == caster);
            ++NumCasts;
        }
    }
}
