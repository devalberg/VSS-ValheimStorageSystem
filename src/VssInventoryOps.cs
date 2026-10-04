using System;
using System.Linq;

namespace VSS;
internal static class VssInventoryOps
{
    public static int Capacity(Inventory inventory, ItemDrop.ItemData item, bool stacksOnly = false, int x = -1, int y = -1)
    {
        var max = Math.Max(1, item.m_shared.m_maxStackSize);
        var key = new VssItemKey(item);
        if (x >= 0 && y >= 0)
        {
            if (x >= inventory.GetWidth() || y >= inventory.GetHeight()) return 0;
            var target = inventory.GetItemAt(x, y);
            return target == null ? (stacksOnly ? 0 : max) :
                (key.Matches(target) && !target.m_equipped ? Math.Max(0, max - target.m_stack) : 0);
        }
        var free = inventory.GetAllItems().Where(i => !i.m_equipped && key.Matches(i)).Sum(i => Math.Max(0, max - i.m_stack));
        return free + (stacksOnly ? 0 : inventory.GetEmptySlots() * max);
    }
    // Explicit slots retain custom item data. Native MoveItemToThis removes the source if emptied.
    public static int Move(Inventory source, Inventory destination, ItemDrop.ItemData item, int requested,
        bool stacksOnly = false, int x = -1, int y = -1)
    {
        if (source == destination || item == null || item.m_equipped || !source.ContainsItem(item) || requested <= 0) return 0;
        var remaining = Math.Min(requested, item.m_stack);
        var moved = 0;
        var key = new VssItemKey(item);
        if (x >= 0 && y >= 0)
            return MoveSlot(source, destination, item, Math.Min(remaining, Capacity(destination, item, stacksOnly, x, y)), x, y);
        foreach (var target in destination.GetAllItems().ToList())
        {
            if (remaining <= 0) break;
            if (target.m_equipped || !key.Matches(target)) continue;
            var amount = Math.Min(remaining, Math.Max(0, item.m_shared.m_maxStackSize - target.m_stack));
            var actual = MoveSlot(source, destination, item, amount, target.m_gridPos.x, target.m_gridPos.y);
            moved += actual; remaining -= actual;
        }
        while (!stacksOnly && remaining > 0 && source.ContainsItem(item))
        {
            var slot = EmptySlot(destination);
            if (slot.x < 0 || slot.y < 0) break;
            var amount = Math.Min(remaining, Math.Max(1, item.m_shared.m_maxStackSize));
            var actual = MoveSlot(source, destination, item, amount, slot.x, slot.y);
            if (actual == 0) break;
            moved += actual; remaining -= actual;
        }
        return moved;
    }
    private static Vector2i EmptySlot(Inventory inventory)
    {
        for (var y = 0; y < inventory.GetHeight(); y++)
            for (var x = 0; x < inventory.GetWidth(); x++)
                if (inventory.GetItemAt(x, y) == null) return new Vector2i(x, y);
        return new Vector2i(-1, -1);
    }
    private static int MoveSlot(Inventory source, Inventory destination, ItemDrop.ItemData item, int amount, int x, int y)
    {
        if (amount <= 0) return 0;
        var before = item.m_stack;
        destination.MoveItemToThis(source, item, amount, x, y);
        return Math.Max(0, before - item.m_stack);
    }
}
