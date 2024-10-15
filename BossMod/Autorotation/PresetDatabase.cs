﻿using System.IO;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: presets in the database are immutable (otherwise eg. manager won't see the changes in active preset)
public sealed class PresetDatabase
{
    private readonly AutorotationConfig _cfg = Service.Config.Get<AutorotationConfig>();

    public readonly List<Preset> DefaultPresets; // default presets, distributed as part of the plugin
    public readonly List<Preset> UserPresets; // user-defined presets, stored in user's preset db
    public Event<Preset?, Preset?> PresetModified = new(); // (old, new); old == null if preset is added, new == null if preset is removed

    private readonly FileInfo _dbPath;

    public IEnumerable<Preset> VisiblePresets => _cfg.HideDefaultPreset ? UserPresets : DefaultPresets.Concat(UserPresets);

    public PresetDatabase(string rootPath, FileInfo defaultPresets)
    {
        _dbPath = new(rootPath + ".db.json");
        DefaultPresets = LoadPresetsFromFile(defaultPresets);
        UserPresets = LoadPresetsFromFile(_dbPath);
    }

    private List<Preset> LoadPresetsFromFile(FileInfo file)
    {
        try
        {
            using var json = Serialization.ReadJson(file.FullName);
            var version = json.RootElement.GetProperty("version").GetInt32();
            var payload = json.RootElement.GetProperty("payload");
            return payload.Deserialize<List<Preset>>(Serialization.BuildSerializationOptions()) ?? [];
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to parse preset database '{file.FullName}': {ex}");
            return [];
        }
    }

    // if index >= 0: replace or delete
    // if index == -1: add (if replacement is non-null) or notify about reordering (otherwise)
    public void Modify(int index, Preset? replacement)
    {
        var previous = index >= 0 ? UserPresets[index] : null;

        if (index < 0 && replacement != null)
            UserPresets.Add(replacement);
        else if (index >= 0 && replacement == null)
            UserPresets.RemoveAt(index);
        else if (index >= 0 && replacement != null)
            UserPresets[index] = replacement;

        if (previous != null || replacement != null)
            PresetModified.Fire(previous, replacement);

        Save();
    }

    public void Save()
    {
        try
        {
            using var fstream = new FileStream(_dbPath.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = Serialization.WriteJson(fstream);
            jwriter.WriteStartObject();
            jwriter.WriteNumber("version", 0);
            jwriter.WritePropertyName("payload");
            JsonSerializer.Serialize(jwriter, UserPresets, Serialization.BuildSerializationOptions());
            jwriter.WriteEndObject();
            Service.Log($"Database saved successfully to '{_dbPath.FullName}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to write database to '{_dbPath.FullName}': {ex}");
        }
    }

    public IEnumerable<Preset> PresetsForClass(Class c) => VisiblePresets.Where(p => p.Modules.Any(m => RotationModuleRegistry.Modules[m.Key].Definition.Classes[(int)c]));

    public Preset? FindPresetByName(ReadOnlySpan<char> name, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        foreach (var p in VisiblePresets)
            if (name.Equals(p.Name, cmp))
                return p;
        return null;
    }
}
