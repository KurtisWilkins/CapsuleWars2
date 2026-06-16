using System.Collections.Generic;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// In-memory <see cref="IEquipmentDatabase"/> built from a set of
    /// <see cref="Equipment_SO"/> (mirrors <c>UnitDefinitionDatabase</c>).
    /// Items with a null/empty id are ignored.
    /// </summary>
    public class EquipmentDatabase : IEquipmentDatabase
    {
        private readonly Dictionary<string, Equipment_SO> byId = new Dictionary<string, Equipment_SO>();

        public EquipmentDatabase() { }

        public EquipmentDatabase(IEnumerable<Equipment_SO> items)
        {
            if (items == null) return;
            foreach (var item in items) Register(item);
        }

        public void Register(Equipment_SO item)
        {
            if (item == null || string.IsNullOrEmpty(item.EquipmentId)) return;
            byId[item.EquipmentId] = item;
        }

        public Equipment_SO GetEquipment(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId)) return null;
            return byId.TryGetValue(equipmentId, out var e) ? e : null;
        }
    }
}
