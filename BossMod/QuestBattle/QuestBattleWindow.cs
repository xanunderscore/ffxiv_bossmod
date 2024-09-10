using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod.QuestBattle;
public class QuestBattleWindow : UIWindow
{
    private readonly QuestBattleDirector _director;
    private readonly QuestBattleConfig _config = Service.Config.Get<QuestBattleConfig>();
    private const string _windowID = "vbm Quest###Quest module";
    private WorldState World => _director.World;

    private readonly List<WPos> Waymarks = [];

    private delegate void AbandonDuty(bool a1);
    private readonly AbandonDuty abandonDutyHook = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B2 01 EB 39"));

    public QuestBattleWindow(QuestBattleDirector director) : base(_windowID, false, new(300, 200), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse)
    {
        _director = director;

        _director.World.CurrentZoneChanged.Subscribe(OnZoneChange);
    }

    private void OnZoneChange(WorldState.OpZoneChange zc)
    {
        Waymarks.Clear();
    }

    public override void PreOpenCheck()
    {
        IsOpen = World.CurrentCFCID != 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _director.Dispose();
        base.Dispose(disposing);
    }

    public override void Draw()
    {
        if (ImGui.Button("Leave duty"))
            abandonDutyHook.Invoke(false);
        ImGui.SameLine();
        UIMisc.HelpMarker("Attempt to leave duty by directly sending the \"abandon duty\" packet, which may be able to bypass the out-of-combat restriction. Only works in some duties.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (_director.CurrentModule is QuestBattle qb)
        {
            ImGui.Text($"Module: {qb.GetType().Name}");
            DrawObjectives(qb);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }

        ImGui.TextUnformatted($"Zone: {World.CurrentZone} / CFC: {World.CurrentCFCID}");
        if (World.Party.Player() is Actor player)
        {
            ImGui.TextUnformatted($"Position: {Utils.Vec3String(player.PosRot.XYZ())}");
            ImGui.SameLine();
            if (ImGui.Button("Copy vec"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"new Vector3({x:F2}f, {y:F2}f, {z:F2}f)");
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy moveto"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"/vnav moveto {x:F2} {y:F2} {z:F2}");
            }
            if (World.Actors.Find(player.TargetID) is Actor tar)
            {
                ImGui.TextUnformatted($"Target: {tar.Name} ({tar.Type}; {tar.OID:X}) (hb={tar.HitboxRadius})");
                ImGui.TextUnformatted($"Distance: {player.DistanceToHitbox(tar)}");
                ImGui.TextUnformatted($"Angle: {player.AngleTo(tar)}");
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Record position"))
                Waymarks.Add(player.Position);

            if (ImGui.Button("Copy all"))
                ImGui.SetClipboardText(string.Join(", ", Waymarks.Select(w => $"new({w.X:F2}f, {w.Z:F2}f)")));

            foreach (var w in Waymarks)
                ImGui.TextUnformatted($"{w}");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Checkbox("Pause duty execution", ref _director.Paused);

        if (ImGui.Checkbox("Use dash abilities for movement", ref _config.UseDash))
            _config.Modified.Fire();
        if (ImGui.Checkbox("Use speedhack in duties", ref _config.Speedhack))
            _config.Modified.Fire();
    }

    private void DrawObjectives(QuestBattle sqb)
    {
        ImGui.TextUnformatted($"Waypoint progress: {_director.ObjectiveWaypointProgress}");
        for (var i = 0; i < sqb.Objectives.Count; i++)
        {
            var n = sqb.Objectives[i];
            var highlight = n == _director.CurrentObjective;
            using var c = ImRaii.PushColor(ImGuiCol.Text, highlight ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey);
            ImGui.TextUnformatted($"#{i} {n.Name}");
            if (highlight)
            {
                foreach (var vec in _director.CurrentWaypoints)
                {
                    if (vec.SpecifiedInPath)
                    {
                        using (var f = ImRaii.PushFont(UiBuilder.IconFont))
                        {
                            ImGui.Text(FontAwesomeIcon.Star.ToIconString());
                        }
                        ImGui.SameLine();
                    }
                    ImGui.TextUnformatted(Utils.Vec3String(vec.Position));
                }
            }
        }
        if (ImGui.Button("Skip step"))
            sqb.Advance();
    }
}
