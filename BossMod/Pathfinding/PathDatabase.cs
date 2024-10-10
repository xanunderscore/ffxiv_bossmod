using System.IO;
using System.Text.Json;

namespace BossMod.Pathfinding;
public sealed class PathDatabase
{
    public sealed record class Entry(Vector3 Destination, List<Vector3> Waypoints)
    {
        public Vector3 Destination = Destination;
        public List<Vector3> Waypoints = Waypoints;
    }

    public readonly Dictionary<uint, List<Entry>> Entries = [];

    public void Load(string listPath)
    {
        Entries.Clear();
        try
        {
            using var json = Serialization.ReadJson(listPath);
            foreach (var jentries in json.RootElement.EnumerateObject())
            {
                var sep = jentries.Name.IndexOf('.', StringComparison.Ordinal);
                var zone = sep >= 0 ? uint.Parse(jentries.Name.AsSpan()[..sep]) : uint.Parse(jentries.Name);
                var cfc = sep >= 0 ? uint.Parse(jentries.Name.AsSpan()[(sep + 1)..]) : 0;
                var entries = Entries[(zone << 16) | cfc] = [];
                foreach (var jentry in jentries.Value.EnumerateArray())
                {
                    var entry = new Entry(ReadVec3(jentry, nameof(Entry.Destination)), []);
                    foreach (var p in jentry.GetProperty("Waypoints").EnumerateArray())
                        entry.Waypoints.Add(ReadVec3(p, ""));
                    entries.Add(entry);
                }
            }
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to load path database '{listPath}': {ex}");
        }
    }

    public void Save(string listPath)
    {
        try
        {
            using var fstream = new FileStream(listPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = Serialization.WriteJson(fstream);
            jwriter.WriteStartObject();
            foreach (var (key, entries) in Entries)
            {
                if (entries.Count == 0)
                    continue; // no entries, skip

                var zone = key >> 16;
                var cfc = key & 0xFFFF;
                jwriter.WriteStartArray(cfc != 0 ? $"{zone}.{cfc}" : $"{zone}");
                foreach (var e in entries)
                {
                    jwriter.WriteStartObject();
                    WriteVec3(jwriter, nameof(Entry.Destination), e.Destination);
                    jwriter.WriteStartArray(nameof(Entry.Waypoints));
                    foreach (var wp in e.Waypoints)
                    {
                        jwriter.WriteStartObject();
                        WriteVec3(jwriter, "", wp);
                        jwriter.WriteEndObject();
                    }
                    jwriter.WriteEndArray();
                    jwriter.WriteEndObject();
                }
                jwriter.WriteEndArray();
            }
            jwriter.WriteEndObject();
            Service.Log($"Path database successfully to '{listPath}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to save path database to '{listPath}': {ex}");
        }
    }

    private Vector3 ReadVec3(JsonElement obj, string tag) => new(obj.GetProperty(tag + "X").GetSingle(), obj.GetProperty(tag + "Y").GetSingle(), obj.GetProperty(tag + "Z").GetSingle());

    private void WriteVec3(Utf8JsonWriter w, string tag, Vector3 v)
    {
        w.WriteNumber(tag + "X", v.X);
        w.WriteNumber(tag + "Y", v.Y);
        w.WriteNumber(tag + "Z", v.Z);
    }
}
