﻿using ImGuiNET;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.ReplayAnalysis;

class ArenaBounds
{
    private readonly List<(Replay, Replay.Participant, DateTime, Vector3, uint)> _points = [];
    private readonly UIPlot _plot = new();

    public ArenaBounds(List<Replay> replays, uint oid)
    {
        _plot.DataMin = new(float.MaxValue, float.MaxValue);
        _plot.DataMax = new(float.MinValue, float.MinValue);
        _plot.MainAreaSize = new(500, 500);
        _plot.TickAdvance = new(10, 10);

        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var ps in enc.ParticipantsByOID.Values)
                {
                    foreach (var p in ps)
                    {
                        int iStart = p.PosRotHistory.UpperBound(enc.Time.Start);
                        if (iStart > 0)
                            --iStart;
                        int iEnd = p.PosRotHistory.UpperBound(enc.Time.End);
                        int iNextDead = p.DeadHistory.UpperBound(enc.Time.Start);
                        for (int i = iStart; i < iEnd; ++i)
                        {
                            var t = p.PosRotHistory.Keys[i];
                            var pos = p.PosRotHistory.Values[i].XYZ();
                            if (iNextDead < p.DeadHistory.Count && p.DeadHistory.Keys[iNextDead] <= t)
                                ++iNextDead;
                            bool dead = iNextDead > 0 && p.DeadHistory.Values[iNextDead - 1];
                            uint color = dead ? 0xff404040 : p.Type is ActorType.Enemy ? 0xff00ffff : 0xff808080;
                            _points.Add((replay, p, t, pos, color));
                            _plot.DataMin.X = Math.Min(_plot.DataMin.X, pos.X);
                            _plot.DataMin.Y = Math.Min(_plot.DataMin.Y, pos.Z);
                            _plot.DataMax.X = Math.Max(_plot.DataMax.X, pos.X);
                            _plot.DataMax.Y = Math.Max(_plot.DataMax.Y, pos.Z);
                        }
                    }
                }
            }
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate complex arena bounds from player movement"))
        {
            Task.Run(() =>
            {
                // Get player points
                var playerPoints = _points.Where(p => p.Item2.OID == 0).Select(x => new WPos(x.Item4.XZ())).ToList();

                // Generate the concave hull
                var points = ConcaveHull.GenerateConcaveHull(playerPoints, 2, 0.5f);
                var center = ConcaveHull.CalculateCentroid(points);
                // Generate code for the polygon points
                var sb = new StringBuilder("private static readonly List<WDir> vertices = [");
                foreach (var p in points)
                {
                    sb.Append($"\n  new WDir({(p.X - center.X).ToString("F2", CultureInfo.InvariantCulture)}f, {(p.Z - center.Z).ToString("F2", CultureInfo.InvariantCulture)}f),");
                }
                sb.Append("\n];");

                // Calculate the centroid of the polygon

                sb.Append($"\n// Centroid of the polygon is at: ({center.X.ToString("F2", CultureInfo.InvariantCulture)}f, {center.Z.ToString("F2", CultureInfo.InvariantCulture)}f)");

                // Copy the generated text and centroid to clipboard
                ImGui.SetClipboardText(sb.ToString());
            });
        }
    }

    public void Draw(UITree tree)
    {
        _plot.Begin();
        foreach (var (replay, participant, time, pos, color) in _points)
            _plot.Point(new(pos.X, pos.Z), color, () => $"{ReplayUtils.ParticipantString(participant, time)} {replay.Path} @ {time:O}");
        _plot.End();
    }
}
