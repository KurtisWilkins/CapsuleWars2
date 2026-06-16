using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Authored master list of all <see cref="Equipment_SO"/> in the game.
    /// Implements <see cref="IEquipmentDatabase"/> directly (like
    /// <c>PartCatalog_SO</c>) so it can be handed to <c>UnitFactory</c> to
    /// resolve equipment ids when spawning units from DTOs.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "CapsuleWars/Equipment/Equipment Catalog", order = 92)]
    public class EquipmentCatalog_SO : ScriptableObject, IEquipmentDatabase
    {
        [SerializeField] private List<Equipment_SO> items = new List<Equipment_SO>();

        public IReadOnlyList<Equipment_SO> Items => items;

        public Equipment_SO GetEquipment(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId)) return null;
            for (int i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].EquipmentId == equipmentId) return items[i];
            return null;
        }

        /// <summary>Build a standalone in-memory database from this catalog's items.</summary>
        public EquipmentDatabase BuildDatabase() => new EquipmentDatabase(items);
    }
}
