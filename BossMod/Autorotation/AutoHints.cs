﻿using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace BossMod;

// utility that determines ai hints automatically based on actor casts
// this is used e.g. in outdoor or on trash, where we have no active bossmodules
public sealed class AutoHints : IDisposable
{
    private const float RaidwideSize = 20;

    private readonly WorldState _ws;
    private readonly EventSubscriptions _subscriptions;
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = [];
    private ArenaBoundsCircle? _activeFateBounds;

    public AutoHints(WorldState ws)
    {
        _ws = ws;
        _subscriptions = new
        (
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Client.ActiveFateChanged.Subscribe(_ => _activeFateBounds = null)
        );
    }

    public void Dispose() => _subscriptions.Dispose();

    public unsafe void CalculateAIHints(AIHints hints, Actor player)
    {
        var currentFateId = _ws.Client.ActiveFate.ID;
        var withinFateLevel = false;
        if (currentFateId != 0 && player.Level <= Service.LuminaRow<Lumina.Excel.GeneratedSheets.Fate>(currentFateId)?.ClassJobLevelMax)
        {
            withinFateLevel = true;
            hints.Center = new(_ws.Client.ActiveFate.Center.XZ());
            hints.Bounds = (_activeFateBounds ??= new ArenaBoundsCircle(_ws.Client.ActiveFate.Radius));
        }
        else
        {
            hints.Center = player.Position;
            // keep default bounds
        }

        foreach (var aoe in _activeAOEs.Values)
        {
            var target = aoe.Target?.Position ?? aoe.Caster.CastInfo!.LocXZ;
            var rot = aoe.Caster.CastInfo!.Rotation;
            if (aoe.IsCharge)
            {
                hints.AddForbiddenZone(ShapeDistance.Rect(aoe.Caster.Position, target, ((AOEShapeRect)aoe.Shape).HalfWidth), aoe.Caster.CastInfo.NPCFinishAt);
            }
            else
            {
                hints.AddForbiddenZone(aoe.Shape, target, rot, aoe.Caster.CastInfo.NPCFinishAt);
            }
        }

        var epicEcho = _ws.Party[0]!.FindStatus(2734) is not null;

        foreach (var enemy in hints.PotentialTargets)
        {
            if (epicEcho)
            {
                enemy.ForbidDOTs = true;

                if (enemy.Actor.Position.InCircle(_ws.Party[0]!.Position, 25))
                    enemy.Priority = 0;
            }

            if (enemy.Actor.HPMP.CurHP == 1)
                enemy.Priority = -1;

            var obj = Utils.GameObjectInternal(Service.ObjectTable[enemy.Actor.SpawnIndex]);
            if (obj is null)
                continue;

            // enemy is part of fate we aren't in
            if (currentFateId == 0 && obj->FateId != 0)
                enemy.Priority = -1;

            if (currentFateId > 0 && obj->FateId == currentFateId)
                enemy.Priority = withinFateLevel ? 0 : -1;
        }
    }

    private void OnCastStarted(Actor actor)
    {
        if (actor.Type != ActorType.Enemy || actor.IsAlly)
            return;
        var data = actor.CastInfo!.IsSpell() ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(actor.CastInfo.Action.ID) : null;
        if (data == null || data.CastType == 1)
            return;
        if (data.CastType is 2 or 5 && data.EffectRange >= RaidwideSize)
            return;
        AOEShape? shape = data.CastType switch
        {
            2 => new AOEShapeCircle(data.EffectRange), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
            3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * 0.5f),
            4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius, data.XAxisModifier * 0.5f),
            5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius),
            //6 => ???
            //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
            //8 => charge rect
            //10 => new AOEShapeDonut(actor.HitboxRadius, data.EffectRange), // TODO: find a way to determine inner radius (omen examples: 28762 - 4/40 - gl_sircle_4004bp1)
            //11 => cross == 12 + another 12 rotated 90 degrees
            12 => new AOEShapeRect(data.EffectRange, data.XAxisModifier * 0.5f),
            13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f),
            _ => null
        };
        if (shape == null)
        {
            Service.Log($"[AutoHints] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
            return;
        }
        var target = _ws.Actors.Find(actor.CastInfo.TargetID);
        _activeAOEs[actor.InstanceID] = (actor, target, shape, data.CastType == 8);
    }

    private void OnCastFinished(Actor actor) => _activeAOEs.Remove(actor.InstanceID);

    private Angle DetermineConeAngle(Lumina.Excel.GeneratedSheets.Action data)
    {
        var omen = data.Omen.Value;
        if (omen == null)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        var path = omen.Path.ToString();
        var pos = path.IndexOf("fan", StringComparison.Ordinal);
        if (pos < 0 || pos + 6 > path.Length)
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }

        if (!int.TryParse(path.AsSpan(pos + 3, 3), out var angle))
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }

        return angle.Degrees();
    }
}
