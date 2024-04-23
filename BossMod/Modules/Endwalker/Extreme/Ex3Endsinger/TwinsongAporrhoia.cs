﻿namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class TwinsongAporrhoia(BossModule module) : BossComponent(module)
{
    private enum HeadID { Center, Danger1, Danger2, Safe1, Safe2, Count }

    private int _castsDone;
    private bool _ringsAssigned;
    private Angle _centerStartingRotation;
    private readonly (Actor? Actor, int Rings)[] _heads = new (Actor?, int)[(int)HeadID.Count];

    private static readonly AOEShapeCone _aoeCenter = new(20, 90.Degrees());
    private static readonly AOEShapeCircle _aoeDanger = new(15);
    private static readonly AOEShapeDonut _aoeSafe = new(5, 15);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_castsDone >= 3 && !_ringsAssigned)
            return;

        bool inAOE = false;

        var center = _heads[(int)HeadID.Center];
        if (center.Actor != null)
        {
            Angle rot = _centerStartingRotation - (_castsDone - center.Rings) * 90.Degrees();
            inAOE = _aoeCenter.Check(actor.Position, center.Actor.Position, rot);
        }

        for (var i = HeadID.Danger1; i < HeadID.Count && !inAOE; ++i)
        {
            var head = _heads[(int)i];
            if (head.Actor != null)
            {
                int safeCounter = (i >= HeadID.Safe1 ? 1 : 0) + _castsDone - head.Rings;
                AOEShape aoe = (safeCounter & 1) != 0 ? _aoeSafe : _aoeDanger;
                inAOE |= aoe.Check(actor.Position, head.Actor);
            }
        }

        if (inAOE)
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_castsDone >= 3 && !_ringsAssigned)
            return;

        var center = _heads[(int)HeadID.Center];
        if (center.Actor != null)
        {
            Angle rot = _centerStartingRotation - (_castsDone - center.Rings) * 90.Degrees();
            _aoeCenter.Draw(Arena, center.Actor.Position, rot);
        }

        for (var i = HeadID.Danger1; i < HeadID.Count; ++i)
        {
            var head = _heads[(int)i];
            if (head.Actor != null)
            {
                int safeCounter = (i >= HeadID.Safe1 ? 1 : 0) + _castsDone - head.Rings;
                AOEShape aoe = (safeCounter & 1) != 0 ? _aoeSafe : _aoeDanger;
                aoe.Draw(Arena, head.Actor);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.RewindTwinsong)
        {
            int rings = status.Extra switch
            {
                0x178 => 1,
                0x179 => 2,
                0x17A => 3,
                _ => 0,
            };
            if (rings == 0)
            {
                ReportError($"Unexpected extra {status.Extra:X} for rewind status");
                return;
            }

            int slot = Array.FindIndex(_heads, ar => ar.Actor == actor);
            if (slot == -1)
            {
                ReportError($"Unexpected actor for rewind status");
                return;
            }

            _heads[slot].Rings = rings;
            _ringsAssigned = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DiairesisTwinsong:
                if (_heads[(int)HeadID.Center].Actor == null)
                {
                    _heads[(int)HeadID.Center] = (caster, 0);
                    _centerStartingRotation = caster.Rotation;
                }
                break;
            case AID.NecroticFluid:
                if (_heads[(int)HeadID.Danger1].Actor == null)
                    _heads[(int)HeadID.Danger1] = (caster, 0);
                else if (_heads[(int)HeadID.Danger2].Actor == null)
                    _heads[(int)HeadID.Danger2] = (caster, 0);
                break;
            case AID.WaveOfNausea:
                if (_heads[(int)HeadID.Safe1].Actor == null)
                    _heads[(int)HeadID.Safe1] = (caster, 0);
                else if (_heads[(int)HeadID.Safe2].Actor == null)
                    _heads[(int)HeadID.Safe2] = (caster, 0);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AporrhoiaUnforgotten:
                ++_castsDone;
                break;
            case AID.FatalismDiairesis:
                _ringsAssigned = false;
                break;
        }
    }
}
