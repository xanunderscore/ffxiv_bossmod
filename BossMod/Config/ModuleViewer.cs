﻿using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System.Data;
using System.Globalization;
using System.Text;

namespace BossMod;

public class ModuleViewer : IDisposable
{
    private record struct ModuleInfo(ModuleRegistry.Info Info, string Name, int SortOrder);
    private record struct ModuleGroupInfo(string Name, uint Id, uint SortOrder, IDalamudTextureWrap? Icon = null);
    private record struct ModuleGroup(ModuleGroupInfo Info, List<ModuleInfo> Modules);

    private BitMask _filterExpansions;
    private BitMask _filterCategories;

    private (string name, IDalamudTextureWrap? icon)[] _expansions;
    private (string name, IDalamudTextureWrap? icon)[] _categories;
    private IDalamudTextureWrap? _iconFATE;
    private IDalamudTextureWrap? _iconHunt;
    private List<ModuleGroup>[,] _groups;
    private Vector2 _iconSize = new(30, 30);

    public ModuleViewer()
    {
        var defaultIcon = GetIcon(61762);
        _expansions = Enum.GetNames<BossModuleInfo.Expansion>().Take((int)BossModuleInfo.Expansion.Count).Select(n => (n, defaultIcon)).ToArray();
        _categories = Enum.GetNames<BossModuleInfo.Category>().Take((int)BossModuleInfo.Category.Count).Select(n => (n, defaultIcon)).ToArray();

        var exVersion = Service.LuminaGameData?.GetExcelSheet<ExVersion>();
        Customize(BossModuleInfo.Expansion.RealmReborn, 61875, exVersion?.GetRow(0)?.Name);
        Customize(BossModuleInfo.Expansion.Heavensward, 61876, exVersion?.GetRow(1)?.Name);
        Customize(BossModuleInfo.Expansion.Stormblood, 61877, exVersion?.GetRow(2)?.Name);
        Customize(BossModuleInfo.Expansion.Shadowbringers, 61878, exVersion?.GetRow(3)?.Name);
        Customize(BossModuleInfo.Expansion.Endwalker, 61879, exVersion?.GetRow(4)?.Name);

        var contentType = Service.LuminaGameData?.GetExcelSheet<ContentType>();
        Customize(BossModuleInfo.Category.Dungeon, contentType?.GetRow(2));
        Customize(BossModuleInfo.Category.Trial, contentType?.GetRow(4));
        Customize(BossModuleInfo.Category.Raid, contentType?.GetRow(5));
        Customize(BossModuleInfo.Category.PVP, contentType?.GetRow(6));
        Customize(BossModuleInfo.Category.Quest, contentType?.GetRow(7));
        Customize(BossModuleInfo.Category.FATE, contentType?.GetRow(8));
        Customize(BossModuleInfo.Category.TreasureHunt, contentType?.GetRow(9));
        Customize(BossModuleInfo.Category.GoldSaucer, contentType?.GetRow(19));
        Customize(BossModuleInfo.Category.DeepDungeon, contentType?.GetRow(21));
        Customize(BossModuleInfo.Category.Ultimate, contentType?.GetRow(28));
        Customize(BossModuleInfo.Category.Criterion, contentType?.GetRow(30));

        var playStyle = Service.LuminaGameData?.GetExcelSheet<CharaCardPlayStyle>();
        Customize(BossModuleInfo.Category.Foray, playStyle?.GetRow(6));
        Customize(BossModuleInfo.Category.MaskedCarnivale, playStyle?.GetRow(8));
        Customize(BossModuleInfo.Category.Hunt, playStyle?.GetRow(10));

        _categories[(int)BossModuleInfo.Category.Extreme].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Unreal].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Savage].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        _categories[(int)BossModuleInfo.Category.Alliance].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        //_categories[(int)BossModuleInfo.Category.Event].icon = GetIcon(61757);

        _iconFATE = GetIcon(contentType?.GetRow(8)?.Icon ?? 0);
        _iconHunt = GetIcon((uint)(playStyle?.GetRow(10)?.Icon ?? 0));

        _groups = new List<ModuleGroup>[(int)BossModuleInfo.Expansion.Count, (int)BossModuleInfo.Category.Count];
        for (int i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
            for (int j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
                _groups[i, j] = new();

        foreach (var info in ModuleRegistry.RegisteredModules.Values)
        {
            var groups = _groups[(int)info.Expansion, (int)info.Category];
            var (groupInfo, moduleInfo) = Classify(info);
            var groupIndex = groups.FindIndex(g => g.Info.Id == groupInfo.Id);
            if (groupIndex < 0)
            {
                groupIndex = groups.Count;
                groups.Add(new(groupInfo, new()));
            }
            else if (groups[groupIndex].Info != groupInfo)
            {
                Service.Log($"[ModuleViewer] Group properties mismatch between {groupInfo} and {groups[groupIndex].Info}");
            }
            groups[groupIndex].Modules.Add(moduleInfo);
        }

        foreach (var groups in _groups)
        {
            groups.SortBy(g => g.Info.SortOrder);
            foreach (var (g1, g2) in groups.Pairwise())
                if (g1.Info.SortOrder == g2.Info.SortOrder)
                    Service.Log($"[ModuleViewer] Same sort order between groups {g1.Info} and {g2.Info}");

            foreach (var g in groups)
            {
                g.Modules.SortBy(m => m.SortOrder);
                foreach (var (m1, m2) in g.Modules.Pairwise())
                    if (m1.SortOrder == m2.SortOrder)
                        Service.Log($"[ModuleViewer] Same sort order between modules {m1.Info.ModuleType.FullName} and {m2.Info.ModuleType.FullName}");
            }
        }
    }

    public void Dispose()
    {
        foreach (var e in _expansions)
            e.icon?.Dispose();
        foreach (var c in _categories)
            c.icon?.Dispose();
        _iconFATE?.Dispose();
        _iconHunt?.Dispose();
    }

    public void Draw(UITree _tree)
    {
        using (var group = ImRaii.Group())
            DrawFilters();
        ImGui.SameLine();
        using (var group = ImRaii.Group())
            DrawModules(_tree);
    }

    private void DrawFilters()
    {
        using var table = ImRaii.Table("Filters", 1, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedSame | ImGuiTableFlags.ScrollY);
        if (!table)
            return;

        ImGui.TableNextColumn();
        ImGui.TableHeader("Expansion");
        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        ImGui.TableNextColumn();
        DrawExpansionFilters();

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.TableHeader("Content");
        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        ImGui.TableNextColumn();
        DrawContentTypeFilters();
    }

    private void DrawExpansionFilters()
    {
        for (var e = BossModuleInfo.Expansion.RealmReborn; e < BossModuleInfo.Expansion.Count; ++e)
        {
            ref var expansion = ref _expansions[(int)e];
            UIMisc.ImageToggleButton(expansion.icon, _iconSize, !_filterExpansions[(int)e], expansion.name);
            if (ImGui.IsItemClicked())
            {
                _filterExpansions.Toggle((int)e);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterExpansions = ~_filterExpansions;
                _filterExpansions.Toggle((int)e);
            }
        }
    }

    private void DrawContentTypeFilters()
    {
        for (var c = BossModuleInfo.Category.Uncategorized; c < BossModuleInfo.Category.Count; ++c)
        {
            ref var category = ref _categories[(int)c];
            UIMisc.ImageToggleButton(category.icon, _iconSize, !_filterCategories[(int)c], category.name);
            if (ImGui.IsItemClicked())
            {
                _filterCategories.Toggle((int)c);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterCategories = ~_filterCategories;
                _filterCategories.Toggle((int)c);
            }
        }
    }

    private void DrawModules(UITree _tree)
    {
        using var table = ImRaii.Table("ModulesTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.NoHostExtendX);
        if (!table)
            return;

        for (int i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            if (_filterExpansions[i])
                continue;
            for (int j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                if (_filterCategories[j])
                    continue;

                foreach (var group in _groups[i, j])
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    UIMisc.Image(_expansions[i].icon, new(36));
                    ImGui.SameLine();
                    UIMisc.Image(group.Info.Icon ?? _categories[j].icon, new(36));
                    ImGui.TableNextColumn();

                    foreach (var _ in _tree.Node($"{group.Info.Name}###{i}/{j}/{group.Info.Id}"))
                    {
                        foreach (var mod in group.Modules)
                        {
                            using (ImRaii.Disabled(mod.Info.ConfigType == null))
                                if (UIMisc.IconButton(FontAwesomeIcon.Cog, "cfg", $"###{mod.Info.ModuleType.FullName}"))
                                    new BossModuleConfigWindow(mod.Info, new(TimeSpan.TicksPerSecond, "fake"));
                            ImGui.SameLine();
                            UIMisc.HelpMarker(() => ModuleHelpText(mod));
                            ImGui.SameLine();
                            using var color = ImRaii.PushColor(ImGuiCol.Text, mod.Info.Maturity switch
                            {
                                BossModuleInfo.Maturity.WIP => 0xff0000ff,
                                BossModuleInfo.Maturity.Verified => 0xff00ff00,
                                _ => 0xffffffff
                            });
                            ImGui.TextUnformatted($"{mod.Name} [{mod.Info.ModuleType.Name}]");
                        }
                    }
                }
            }
        }
    }

    private void Customize((string name, IDalamudTextureWrap? icon)[] array, int element, uint iconId, SeString? name)
    {
        var icon = GetIcon(iconId);
        if (icon != null)
            array[element].icon = icon;
        if (name != null)
            array[element].name = name;
    }
    private void Customize(BossModuleInfo.Expansion expansion, uint iconId, SeString? name) => Customize(_expansions, (int)expansion, iconId, name);
    private void Customize(BossModuleInfo.Category category, uint iconId, SeString? name) => Customize(_categories, (int)category, iconId, name);
    private void Customize(BossModuleInfo.Category category, ContentType? ct) => Customize(category, ct?.Icon ?? 0, ct?.Name);
    private void Customize(BossModuleInfo.Category category, CharaCardPlayStyle? ps) => Customize(category, (uint)(ps?.Icon ?? 0), ps?.Name);

    private static IDalamudTextureWrap? GetIcon(uint iconId) => iconId != 0 ? Service.Texture?.GetIcon(iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes) : null;
    private static string FixCase(SeString? str) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str ?? "");
    private static string BNpcName(uint id) => FixCase(Service.LuminaRow<BNpcName>(id)?.Singular);

    private (ModuleGroupInfo, ModuleInfo) Classify(ModuleRegistry.Info module)
    {
        var groupId = (uint)module.GroupType << 24;
        switch (module.GroupType)
        {
            case BossModuleInfo.GroupType.CFC:
                groupId |= module.GroupID;
                var cfcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID);
                var cfcSort = cfcRow?.SortKey ?? 0u;
                var cfcName = FixCase(cfcRow?.Name);
                return (new(cfcName, groupId, cfcSort != 0 ? cfcSort : groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.MaskedCarnivale:
                groupId |= module.GroupID;
                var mcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID);
                var mcSort = uint.Parse((mcRow?.ShortCode ?? "").Substring(3)); // 'aozNNN'
                var mcName = $"Stage {mcSort}: {FixCase(mcRow?.Name)}";
                return (new(mcName, groupId, mcSort), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.RemovedUnreal:
                return (new("Removed Content", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.Quest:
                var questRow = Service.LuminaRow<Quest>(module.GroupID);
                groupId |= questRow?.JournalGenre.Row ?? 0;
                var questCategoryName = questRow?.JournalGenre.Value?.Name ?? "";
                return (new(questCategoryName, groupId, groupId), new(module, $"{questRow?.Name}: {BNpcName(module.NameID)}", module.SortOrder));
            case BossModuleInfo.GroupType.Fate:
                var fateRow = Service.LuminaRow<Fate>(module.GroupID);
                return (new($"{module.Expansion.ShortName()} FATE", groupId, groupId, _iconFATE), new(module, $"{fateRow?.Name}: {BNpcName(module.NameID)}", module.SortOrder));
            case BossModuleInfo.GroupType.Hunt:
                groupId |= module.GroupID;
                return (new($"{module.Expansion.ShortName()} Hunt {(BossModuleInfo.HuntRank)module.GroupID}", groupId, groupId, _iconHunt), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.BozjaCE:
                groupId |= module.GroupID;
                var ceName = $"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)?.Name)} CE";
                return (new(ceName, groupId, groupId), new(module, Service.LuminaRow<DynamicEvent>(module.NameID)?.Name ?? "", module.SortOrder));
            case BossModuleInfo.GroupType.BozjaDuel:
                groupId |= module.GroupID;
                var duelName = $"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)?.Name)} Duel";
                return (new(duelName, groupId, groupId), new(module, Service.LuminaRow<DynamicEvent>(module.NameID)?.Name ?? "", module.SortOrder));
            case BossModuleInfo.GroupType.GoldSaucer:
                return (new("Gold saucer", groupId, groupId), new(module, $"{Service.LuminaRow<GoldSaucerTextData>(module.GroupID)?.Text}: {BNpcName(module.NameID)}", module.SortOrder));
            default:
                return (new("Ungrouped", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
        }
    }

    private string ModuleHelpText(ModuleInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Cooldown planning: {(info.Info.CooldownPlanningSupported ? "supported!" : "not supported")}");
        if (info.Info.Contributors.Length > 0)
            sb.AppendLine($"Contributors: {info.Info.Contributors}");
        return sb.ToString();
    }
}
