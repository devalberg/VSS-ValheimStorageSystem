using System;
using System.Collections.Generic;
using UnityEngine;

namespace VSS;
internal static class VssPreferences
{
    internal const string Key = "vss_preference";
    private static readonly HashSet<string> Wood = new(StringComparer.OrdinalIgnoreCase)
        { "Wood", "RoundLog", "FineWood", "ElderBark", "YggdrasilWood", "Blackwood" };
    private static readonly HashSet<string> Raw = new(StringComparer.OrdinalIgnoreCase)
    {
        "RawMeat", "NeckTail", "DeerMeat", "WolfMeat", "LoxMeat", "SerpentMeat", "HareMeat", "ChickenMeat", "BugMeat", "AsksvinMeat", "FishRaw",
        "Raspberry", "Blueberries", "Cloudberry", "Honey", "Carrot", "Turnip", "Onion", "Mushroom", "MushroomYellow", "MushroomBlue",
        "MushroomJotunPuffs", "MushroomMagecap", "Barley", "BarleyFlour", "Thistle", "Dandelion", "Egg", "Fiddlehead", "Vineberry", "Sap"
    };
    public static VssCategory Get(Container container)
    {
        var view = container != null ? VssGame.View(container) : null;
        var value = view != null && view.IsValid() ? view.GetZDO().GetInt(Key, 0) : 0;
        return Enum.IsDefined(typeof(VssCategory), value) ? (VssCategory)value : VssCategory.Any;
    }
    public static bool Set(Container container, VssCategory category)
    {
        var view = container != null ? VssGame.View(container) : null;
        if (view == null || !view.IsValid() || !view.IsOwner() || Player.m_localPlayer == null ||
            !VssStorageScanner.CanAccess(container, Player.m_localPlayer)) return false;
        view.GetZDO().Set(Key, (int)category);
        return true;
    }
    public static string Label(VssCategory category) => category == VssCategory.RawFood ? "Raw food" : category.ToString();
    public static VssCategory Classify(ItemDrop.ItemData item)
    {
        var name = VssItemKey.Prefab(item);
        var type = item.m_shared.m_itemType;
        if (Wood.Contains(name)) return VssCategory.Wood;
        if (name.IndexOf("Mead", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Wine", StringComparison.OrdinalIgnoreCase) >= 0) return VssCategory.Mead;
        if (Raw.Contains(name) || type == ItemDrop.ItemData.ItemType.Fish) return VssCategory.RawFood;
        if (type == ItemDrop.ItemData.ItemType.Consumable) return VssCategory.Food;
        switch (type)
        {
            case ItemDrop.ItemData.ItemType.OneHandedWeapon: case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft: case ItemDrop.ItemData.ItemType.Bow:
            case ItemDrop.ItemData.ItemType.Attach_Atgeir: return VssCategory.Weapons;
            case ItemDrop.ItemData.ItemType.Helmet: case ItemDrop.ItemData.ItemType.Chest: case ItemDrop.ItemData.ItemType.Legs:
            case ItemDrop.ItemData.ItemType.Hands: case ItemDrop.ItemData.ItemType.Shoulder: case ItemDrop.ItemData.ItemType.Shield:
            case ItemDrop.ItemData.ItemType.Utility: case ItemDrop.ItemData.ItemType.Trinket: return VssCategory.Armor;
            case ItemDrop.ItemData.ItemType.Ammo: case ItemDrop.ItemData.ItemType.AmmoNonEquipable: return VssCategory.Ammo;
            case ItemDrop.ItemData.ItemType.Material: return VssCategory.Materials;
            case ItemDrop.ItemData.ItemType.Trophy: return VssCategory.Trophies;
            case ItemDrop.ItemData.ItemType.Tool: case ItemDrop.ItemData.ItemType.Torch: return VssCategory.Tools;
            default: return VssCategory.Misc;
        }
    }
    public static int Rank(Container chest, ItemDrop.ItemData item)
    {
        var preference = Get(chest);
        return preference == Classify(item) ? 0 : preference == VssCategory.Any ? 1 : 2;
    }
    public static Sprite Icon(VssCategory category)
    {
        var prefab = category switch
        {
            VssCategory.Wood => "Wood", VssCategory.RawFood => "RawMeat", VssCategory.Food => "CookedMeat",
            VssCategory.Mead => "MeadHealthMinor", VssCategory.Weapons => "SwordBronze", VssCategory.Armor => "HelmetLeather",
            VssCategory.Ammo => "ArrowWood", VssCategory.Materials => "Stone", VssCategory.Trophies => "TrophyDeer",
            VssCategory.Tools => "Hammer", VssCategory.Misc => "Coins", _ => "Chest"
        };
        var go = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefab) : null;
        return go != null && go.GetComponent<ItemDrop>() != null ? go.GetComponent<ItemDrop>().m_itemData.GetIcon() : VssSprites.StandIcon;
    }
}
