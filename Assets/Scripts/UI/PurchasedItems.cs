using System.Collections.Generic;
using CapsuleWars.Data.Equipment;

namespace CapsuleWars.UI
{
    /// <summary>
    /// In-memory side-channel for shop purchases that need to apply across
    /// scene loads. ShopPanel writes here; a small applier in the Battle
    /// scene reads and equips the items on matching units when they spawn.
    /// M7-only; M9+ replaces this with the deployment UI's "equip" flow.
    /// </summary>
    public static class PurchasedItems
    {
        private static readonly Dictionary<string, List<Equipment_SO>> pending = new();

        public static void Add(string unitId, Equipment_SO item)
        {
            if (string.IsNullOrEmpty(unitId) || item == null) return;
            if (!pending.TryGetValue(unitId, out var list))
            {
                list = new List<Equipment_SO>();
                pending[unitId] = list;
            }
            list.Add(item);
        }

        public static IReadOnlyList<Equipment_SO> Drain(string unitId)
        {
            if (string.IsNullOrEmpty(unitId) || !pending.TryGetValue(unitId, out var list))
                return System.Array.Empty<Equipment_SO>();
            pending.Remove(unitId);
            return list;
        }

        public static void Clear() => pending.Clear();
    }
}
