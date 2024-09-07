using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod.QuestBattle;
public class QuestBattleWindow(QuestBattleDirector director) : UIWindow(_windowID, false, new(300, 200), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse)
{
    private readonly QuestBattleDirector _director = director;
    private const string _windowID = "vbm Quest###Quest module";
    private WorldState World => _director.World;

    private delegate void AbandonDuty(bool a1);
    private readonly AbandonDuty abandonDuty = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B2 01 EB 39"));

    public override void PreOpenCheck()
    {
        IsOpen = true; //  _director.CurrentModule != null;
    }

    public override void Draw()
    {
        if (ImGui.Button("Leave duty"))
            abandonDuty.Invoke(false);
        ImGui.SameLine();
        UIMisc.HelpMarker("Attempt to leave duty by directly sending the \"abandon duty\" packet. May bypass the out-of-combat restriction. Only works in some duties.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (_director.CurrentModule is QuestBattle qb)
            ImGui.Text($"Module: {qb.GetType().Name}");

        if (_director.CurrentNavigation is QuestNavigation obj)
        {
            ImGui.Text($"Objective: {obj.Name}");
            ImGui.Text($"Destination: {Utils.Vec3String(obj.Connections.Last()!)}");

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
                var x = player.PosRot.X; var y = player.PosRot.Y; var z = player.PosRot.Z;
                ImGui.SetClipboardText($"new Vector3({x:F2}f, {y:F2}f, {z:F2}f)");
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy moveto"))
            {
                var x = player.PosRot.X; var y = player.PosRot.Y; var z = player.PosRot.Z;
                ImGui.SetClipboardText($"/vnav moveto {x:F2} {y:F2} {z:F2}");
            }
            if (World.Actors.Find(player.TargetID) is Actor tar)
            {
                ImGui.TextUnformatted($"Target: {tar.Name} ({tar.Type}; {tar.OID:X}) (hb={tar.HitboxRadius})");
                ImGui.TextUnformatted($"Distance: {player.DistanceToHitbox(tar)}");
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}
