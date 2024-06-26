﻿using ImGuiNET;

namespace BossMod.ReplayVisualization;

public class ColumnPlayerDetails : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBraches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _enc;
    private readonly Replay.Participant _player;
    private readonly Class _playerClass;
    private readonly ModuleRegistry.Info? _moduleInfo;

    private readonly ColumnPlayerActions _actions;
    private readonly ColumnActorStatuses _statuses;

    private readonly ColumnActorHP _hp;
    private readonly ColumnPlayerGauge? _gauge;
    private readonly ColumnSeparator _resourceSep;

    private readonly CooldownPlanningConfigNode? _planConfig;
    private int _selectedPlan = -1;
    private CooldownPlannerColumns? _planner;

    public bool PlanModified { get; private set; }

    public ColumnPlayerDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
        : base(timeline)
    {
        _tree = tree;
        _phaseBraches = phaseBranches;
        _replay = replay;
        _enc = enc;
        _player = player;
        _playerClass = playerClass;
        _moduleInfo = ModuleRegistry.FindByOID(enc.OID);

        _actions = Add(new ColumnPlayerActions(timeline, tree, phaseBranches, replay, enc, player, playerClass));
        _actions.Name = player.NameHistory.FirstOrDefault().Value.name;

        _statuses = Add(new ColumnActorStatuses(timeline, tree, phaseBranches, replay, enc, player));

        _hp = Add(new ColumnActorHP(timeline, tree, phaseBranches, replay, enc, player));
        _gauge = ColumnPlayerGauge.Create(timeline, tree, phaseBranches, replay, enc, player, playerClass);
        if (_gauge != null)
            Add(_gauge);
        _resourceSep = Add(new ColumnSeparator(timeline));

        var info = ModuleRegistry.FindByOID(enc.OID);
        if (info?.CooldownPlanningSupported ?? false)
        {
            _planConfig = Service.Config.Get<CooldownPlanningConfigNode>(info.ConfigType!);
            var plans = _planConfig?.CooldownPlans.GetValueOrDefault(playerClass);
            if (plans != null)
                UpdateSelectedPlan(plans, plans.SelectedIndex);
        }
    }

    public void DrawConfig(UITree tree)
    {
        DrawConfigPlanner(tree);
        foreach (var _1 in tree.Node("Actions"))
            _actions.DrawConfig(tree);
        foreach (var _1 in tree.Node("Statuses"))
            _statuses.DrawConfig(tree);

        foreach (var _1 in tree.Node("Resources"))
        {
            DrawResourceColumnToggle(_hp, "HP");
            if (_gauge != null)
                DrawResourceColumnToggle(_gauge, "Gauge");
        }
    }

    public void SaveChanges()
    {
        if (_planner != null && PlanModified)
        {
            _planner.UpdateEditedPlan();
            _planConfig?.NotifyModified();
            PlanModified = false;
        }
    }

    private void DrawConfigPlanner(UITree tree)
    {
        if (_planConfig == null)
        {
            tree.LeafNode("Planner: not supported for this encounter");
            return;
        }

        var plans = _planConfig.CooldownPlans.GetValueOrDefault(_playerClass);
        if (plans == null)
        {
            tree.LeafNode("Planner: not supported for this class");
            return;
        }

        foreach (var _1 in tree.Node("Planner"))
        {
            UpdateSelectedPlan(plans, DrawPlanSelector(plans, _selectedPlan));
            _planner?.DrawConfig();
        }
    }

    private int DrawPlanSelector(CooldownPlanningConfigNode.PlanList list, int selection)
    {
        selection = CooldownPlanningConfigNode.DrawPlanCombo(list, selection, "###planner");
        ImGui.SameLine();

        bool isDefault = selection == list.SelectedIndex;
        if (ImGui.Checkbox("Default", ref isDefault))
        {
            list.SelectedIndex = isDefault ? selection : -1;
            _planConfig?.NotifyModified();
        }
        ImGui.SameLine();

        if (ImGui.Button(selection >= 0 ? "Create copy" : "Create new"))
        {
            CooldownPlan plan;
            if (selection >= 0)
            {
                plan = list.Available[selection].Clone();
                plan.Name += " Copy";
            }
            else
            {
                plan = new(_playerClass, _planConfig?.SyncLevel ?? 0, $"New {list.Available.Count}");
            }
            selection = list.Available.Count;
            list.Available.Add(plan);
            _planConfig?.NotifyModified();
        }

        if (_planner != null && PlanModified)
        {
            ImGui.SameLine();
            if (ImGui.Button("Save modifications"))
            {
                SaveChanges();
            }
        }

        return selection;
    }

    private void UpdateSelectedPlan(CooldownPlanningConfigNode.PlanList list, int newSelection)
    {
        if (_selectedPlan == newSelection)
            return;

        if (_planner != null)
        {
            Columns.Remove(_planner);
            _planner = null;
        }
        _selectedPlan = newSelection;
        PlanModified = false;
        if (_selectedPlan >= 0)
        {
            _planner = AddBefore(new CooldownPlannerColumns(list.Available[newSelection], () => PlanModified = true, Timeline, _tree, _phaseBraches, _moduleInfo, false), _actions);

            // TODO: this should be reworked...
            var minTime = _enc.Time.Start.AddSeconds(Timeline.MinTime);
            foreach (var a in _replay.Actions.SkipWhile(a => a.Timestamp < minTime).TakeWhile(a => a.Timestamp <= _enc.Time.End).Where(a => a.Source == _player))
            {
                _planner.TrackForAction(a.ID)?.AddHistoryEntryDot(_enc.Time.Start, a.Timestamp, $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}", 0xffffffff).AddActionTooltip(a);
            }
        }
    }

    private void DrawResourceColumnToggle(IToggleableColumn col, string name)
    {
        bool visible = col.Visible;
        if (ImGui.Checkbox(name, ref visible))
        {
            col.Visible = visible;
            _resourceSep.Width = _hp.Visible || (_gauge?.Visible ?? false) ? 1 : 0;
        }
    }
}
