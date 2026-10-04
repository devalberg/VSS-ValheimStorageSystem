using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VSS;

// A row must never collapse equipment with different durability, variants or mod data.
internal readonly struct VssItemKey
{
    public readonly string Id;
    public readonly string PrefabName;
    public readonly string DisplayName;
    public VssItemKey(ItemDrop.ItemData item)
    {
        PrefabName = Prefab(item);
        DisplayName = Localization.instance != null ? Localization.instance.Localize(item.m_shared.m_name) : item.m_shared.m_name;
        Id = Identity(item);
    }
    public bool Matches(ItemDrop.ItemData item) => item != null && Id == Identity(item);
    public static string Prefab(ItemDrop.ItemData item) => item.m_dropPrefab != null
        ? item.m_dropPrefab.name.Replace("(Clone)", "").Trim() : item.m_shared.m_name;
    private static string Identity(ItemDrop.ItemData item)
    {
        var id = new StringBuilder();
        Add(id, Prefab(item)); Add(id, item.m_shared.m_name);
        Add(id, item.m_quality.ToString(CultureInfo.InvariantCulture));
        Add(id, item.m_variant.ToString(CultureInfo.InvariantCulture));
        Add(id, item.m_worldLevel.ToString(CultureInfo.InvariantCulture));
        Add(id, item.m_durability.ToString("R", CultureInfo.InvariantCulture));
        Add(id, item.m_crafterID.ToString(CultureInfo.InvariantCulture)); Add(id, item.m_crafterName ?? "");
        Add(id, item.m_cheated ? "1" : "0");
        if (item.m_customData != null)
            foreach (var pair in item.m_customData.OrderBy(p => p.Key, StringComparer.Ordinal))
            { Add(id, pair.Key); Add(id, pair.Value ?? ""); }
        return id.ToString();
    }
    private static void Add(StringBuilder id, string value) => id.Append(value.Length).Append(':').Append(value);
}

internal enum VssCategory { Any, Wood, RawFood, Food, Mead, Weapons, Armor, Ammo, Materials, Trophies, Tools, Misc }
internal sealed class VssContainerRef
{
    public Container Container;
    public float Distance;
    public Inventory Inventory => Container != null ? Container.GetInventory() : null;
}
internal sealed class VssItemRow
{
    public VssItemKey Key;
    public ItemDrop.ItemData Sample;
    public int Total;
    public int StackCount;
    public int MaxStack;
    public Sprite Icon => Sample.GetIcon();
    public string Tooltip => $"{Key.DisplayName}\n{Total:N0} stored in {StackCount} stack(s)\nQuality {Sample.m_quality}" +
        (MaxStack == 1 ? $"  •  Durability {Sample.m_durability:0}" : "");
}
internal sealed class VssSnapshot
{
    public readonly List<VssContainerRef> Containers = new();
    public readonly List<VssItemRow> Rows = new();
    public int TotalSlots;
    public int UsedSlots;
    public int BusyContainers;
    public int EmptySlots => Math.Max(0, TotalSlots - UsedSlots);
}
