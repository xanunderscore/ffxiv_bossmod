﻿using BossMod.Pathfinding;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using ImGuiNET;

namespace BossMod.AI;

// constantly follow master
sealed class AIBehaviour(AIController ctrl, Autorotation autorot) : IDisposable
{
    private readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private NavigationDecision _naviDecision;
    private bool _forbidMovement;
    private bool _forbidActions;
    private bool _afkMode;
    private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
    private float _maxCastTime;
    private WPos _masterPrevPos;
    private WPos _masterMovementStart;
    private DateTime _masterLastMoved;

    public void Dispose()
    {
    }

    public unsafe void Execute(Actor player, Actor master)
    {
        if (player.IsDead || ctrl.InCutscene)
            return;

        // keep master in focus
        if (_config.FocusTargetLeader)
            FocusMaster(master);

        _afkMode = !master.InCombat && (autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 10 && FateManager.Instance()->CurrentFate is null;
        bool forbidActions = _forbidActions || ctrl.IsMounted || _afkMode || autorot.ClassActions == null || autorot.ClassActions.AutoAction >= CommonActions.AutoActionFirstCustom;

        CommonActions.Targeting target = new();
        if (!forbidActions)
        {
            target = SelectPrimaryTarget(player, master);
            if (target.Target != null || TargetIsForbidden(autorot.PrimaryTarget))
                ctrl.SetPrimaryTarget(target.Target?.Actor);
            autorot.PrimaryTarget = target.Target?.Actor;
            AdjustTargetPositional(player, ref target);
        }

        _followMaster = master != player && autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null && (!master.InCombat || (_masterPrevPos - _masterMovementStart).LengthSq() > 100);
        _naviDecision = BuildNavigationDecision(player, master, ref target);

        bool masterIsMoving = TrackMasterMovement(master);
        bool moveWithMaster = masterIsMoving && (master == player || _followMaster);
        _maxCastTime = moveWithMaster || ctrl.ForceFacing ? 0 : _naviDecision.LeewaySeconds;

        // note: that there is a 1-frame delay if target and/or strategy changes - we don't really care?..
        if (!forbidActions)
        {
            int actionStrategy = target.Target != null ? CommonActions.AutoActionAIFight : CommonActions.AutoActionAIIdle;
            autorot.ClassActions?.UpdateAutoAction(actionStrategy, _maxCastTime, false);
        }

        UpdateMovement(player, master, target, !forbidActions);
    }

    // returns null if we're to be idle, otherwise target to attack
    private unsafe CommonActions.Targeting SelectPrimaryTarget(Actor player, Actor master)
    {
        if (!autorot.Hints.PriorityTargets.Any() || (!master.InCombat && FateManager.Instance()->CurrentFate is null) || autorot.ClassActions == null)
            return new(); // there are no valid targets to attack, or we're not fighting - remain idle

        // we prefer not to switch targets unnecessarily, so start with current target - it could've been selected manually or by AI on previous frames
        // if current target is not among valid targets, clear it - this opens way for future target selection heuristics
        var target = autorot.Hints.PriorityTargets.FirstOrDefault(e => e.Actor == autorot.PrimaryTarget);

        // if we don't have a valid target yet, use some heuristics to select some 'ok' target to attack
        // try assisting master, otherwise (if player is own master, or if master has no valid target) just select closest valid target
        target ??= master != player ? autorot.Hints.PriorityTargets.FirstOrDefault(t => master.TargetID == t.Actor.InstanceID) : null;
        target ??= autorot.Hints.PriorityTargets.MinBy(e => (e.Actor.Position - player.Position).LengthSq());

        // now give class module a chance to improve targeting
        // typically it would switch targets for multidotting, or to hit more targets with AOE
        // in case of ties, it should prefer to return original target - this would prevent useless switches
        return autorot.ClassActions.SelectBetterTarget(target!);
    }

    private void AdjustTargetPositional(Actor player, ref CommonActions.Targeting targeting)
    {
        if (targeting.Target == null || targeting.PreferredPosition == Positional.Any)
            return; // nothing to adjust

        // if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting (TODO: reconsider?)
        if (targeting.Target.Actor.TargetID == player.InstanceID && targeting.Target.Actor.CastInfo == null)
            targeting.PreferredPosition = Positional.Any;
    }

    private NavigationDecision BuildNavigationDecision(Actor player, Actor master, ref CommonActions.Targeting targeting)
    {
        if (_forbidMovement)
            return new() { LeewaySeconds = float.MaxValue };
        if (_followMaster)
            return NavigationDecision.Build(autorot.WorldState, autorot.Hints, player, master.Position, 1, new(), Positional.Any);
        if (targeting.Target == null)
            return NavigationDecision.Build(autorot.WorldState, autorot.Hints, player, null, 0, new(), Positional.Any);

        var adjRange = targeting.PreferredRange + player.HitboxRadius + targeting.Target.Actor.HitboxRadius;
        if (targeting.PreferTanking)
        {
            // see whether we need to move target
            // TODO: think more about keeping uptime while tanking, this is tricky...
            var desiredToTarget = targeting.Target.Actor.Position - targeting.Target.DesiredPosition;
            if (desiredToTarget.LengthSq() > 4 /*&& (_autorot.ClassActions?.GetState().GCD ?? 0) > 0.5f*/)
            {
                var dest = autorot.Hints.ClampToBounds(targeting.Target.DesiredPosition - adjRange * desiredToTarget.Normalized());
                return NavigationDecision.Build(autorot.WorldState, autorot.Hints, player, dest, 0.5f, new(), Positional.Any);
            }
        }

        var adjRotation = targeting.PreferTanking ? targeting.Target.DesiredRotation : targeting.Target.Actor.Rotation;
        return NavigationDecision.Build(autorot.WorldState, autorot.Hints, player, targeting.Target.Actor.Position, adjRange, adjRotation, targeting.PreferredPosition);
    }

    private void FocusMaster(Actor master)
    {
        bool masterChanged = Service.TargetManager.FocusTarget?.ObjectId != master.InstanceID;
        if (masterChanged)
        {
            ctrl.SetFocusTarget(master);
            _masterPrevPos = _masterMovementStart = master.Position;
            _masterLastMoved = autorot.WorldState.CurrentTime.AddSeconds(-1);
        }
    }

    private bool TrackMasterMovement(Actor master)
    {
        // keep track of master movement
        // idea is that if master is moving forward (e.g. running in outdoor or pulling trashpacks in dungeon), we want to closely follow and not stop to cast
        bool masterIsMoving = true;
        if (master.Position != _masterPrevPos)
        {
            _masterLastMoved = autorot.WorldState.CurrentTime;
            _masterPrevPos = master.Position;
        }
        else if ((autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 0.5f)
        {
            // master has stopped, consider previous movement finished
            _masterMovementStart = _masterPrevPos;
            masterIsMoving = false;
        }
        // else: don't consider master to have stopped moving unless he's standing still for some small time

        return masterIsMoving;
    }

    private void UpdateMovement(Actor player, Actor master, CommonActions.Targeting target, bool allowSprint)
    {
        var destRot = AvoidGaze.Update(player, target.Target?.Actor.Position, autorot.Hints, autorot.WorldState.CurrentTime.AddSeconds(0.5));
        if (destRot != null)
        {
            // rotation check imminent, drop any movement - we should have moved to safe zone already...
            ctrl.NaviTargetPos = null;
            ctrl.NaviTargetRot = destRot;
            ctrl.NaviTargetVertical = null;
            ctrl.ForceCancelCast = true;
            ctrl.ForceFacing = true;
        }
        else
        {
            var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : new();
            var distSq = toDest.LengthSq();
            ctrl.NaviTargetPos = _naviDecision.Destination;
            //_ctrl.NaviTargetRot = distSq >= 0.01f ? toDest.Normalized() : null;
            ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
            ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= (player.CastInfo.FinishAt - autorot.WorldState.CurrentTime).TotalSeconds - 0.5;
            ctrl.ForceCancelCast = TargetIsForbidden(autorot.WorldState.Actors.Find(player.CastInfo?.TargetID ?? 0));
            ctrl.ForceFacing = false;
            ctrl.WantJump = distSq >= 0.01f && autorot.Bossmods.ActiveModule?.StateMachine.ActiveState != null && autorot.Bossmods.ActiveModule.NeedToJump(player.Position, toDest.Normalized());

            //var cameraFacing = _ctrl.CameraFacing;
            //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
            //if (dot < -0.707107f)
            //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
            //else if (dot < 0.707107f)
            //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();

            // sprint, if not in combat and far enough away from destination
            if (allowSprint && (player.InCombat ? _naviDecision.LeewaySeconds <= 0 && distSq > 25 : player != master && distSq > 400))
            {
                autorot.ClassActions?.HandleUserActionRequest(CommonDefinitions.IDSprint, player);
            }
        }
    }

    public void DrawDebug()
    {
        ImGui.Checkbox("Forbid actions", ref _forbidActions);
        ImGui.SameLine();
        ImGui.Checkbox("Forbid movement", ref _forbidMovement);
        var player = autorot.WorldState.Party.Player();
        var dist = _naviDecision.Destination != null && player != null ? (_naviDecision.Destination.Value - player.Position).Length() : 0;
        ImGui.TextUnformatted($"Max-cast={MathF.Min(_maxCastTime, 1000):f3}, afk={_afkMode}, follow={_followMaster}, algo={_naviDecision.DecisionType} {_naviDecision.Destination} (d={dist:f3}), master standing for {Math.Clamp((autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds, 0, 1000):f1}");
    }

    private bool TargetIsForbidden(Actor? actor) => actor != null && autorot.Hints.ForbiddenTargets.Any(e => e.Actor == actor);
}
