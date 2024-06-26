﻿namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Hydrostasis(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];

    public bool Active => _sources.Count == 3 || NumCasts > 0;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Active ? _sources : Enumerable.Empty<Source>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HydrostasisAOE1 or AID.HydrostasisAOE2 or AID.HydrostasisAOE3)
            AddSource(caster.Position, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HydrostasisAOE1 or AID.HydrostasisAOE2 or AID.HydrostasisAOE3)
        {
            ++NumCasts;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }

    private void AddSource(WPos pos, DateTime activation)
    {
        _sources.Add(new(pos, 28, activation));
        _sources.SortBy(s => s.Activation);
    }
}
