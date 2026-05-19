using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Data.Weapons;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// One equippable item. Lives in a single slot, contributes stat
    /// buffs to its wearer (multiplied by rarity), and optionally carries
    /// an element affinity and weapon class.
    /// 3D mesh swap is deferred to M10 polish; M6 ships the stat model.
    /// </summary>
    [CreateAssetMenu(fileName = "Equipment", menuName = "CapsuleWars/Equipment/Equipment", order = 91)]
    public class Equipment_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and Database lookup.")]
        [SerializeField] private string equipmentId;

        [Tooltip("I2 term key for display name (e.g. Equipment.IronSword.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("I2 term key for description.")]
        [SerializeField] private string descTermKey;

        [Tooltip("UI icon for the item card.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Slot this item occupies on a unit.")]
        [SerializeField] private EquipmentSlot slot;

        [Tooltip("Rarity tier. Multiplies the item's stat buffs (and tints UI).")]
        [SerializeField] private Rarity_SO rarity;

        [Header("Stats")]
        [Tooltip("Stat modifiers granted by this item. Multiplied by rarity.StatMultiplier before application.")]
        [SerializeField] private List<StatBuff> statBuffs = new();

        [Header("Optional")]
        [Tooltip("Optional element affinity. Element-based effects can read this (e.g. fire enchantment).")]
        [SerializeField] private ElementType_SO elementAffinity;

        [Tooltip("Hand-slot items: weapon class. Drives Animator sub-SM + attack range/cooldown when equipped in RightHand.")]
        [SerializeField] private WeaponClass_SO weaponClass;

        [Tooltip("Optional 3D mesh swap. Wired in M10 polish.")]
        [SerializeField] private Mesh visualMesh;

        [Tooltip("Optional materials to pair with visualMesh.")]
        [SerializeField] private Material[] visualMaterials;

        public string EquipmentId => equipmentId;
        public string NameTermKey => nameTermKey;
        public string DescTermKey => descTermKey;
        public Sprite Icon => icon;
        public EquipmentSlot Slot => slot;
        public Rarity_SO Rarity => rarity;
        public IReadOnlyList<StatBuff> StatBuffs => statBuffs;
        public ElementType_SO ElementAffinity => elementAffinity;
        public WeaponClass_SO WeaponClass => weaponClass;
        public Mesh VisualMesh => visualMesh;
        public IReadOnlyList<Material> VisualMaterials => visualMaterials;

        public float RarityMultiplier => rarity != null ? rarity.StatMultiplier : 1f;
    }
}
