using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Data.Weapons;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// The equipment <b>Definition</b>: the fixed IDENTITY of a piece — id, name/desc keys, icon,
    /// slot, weapon class, element, and visual (mesh/prefab + attach socket). As of ADR-019, stats
    /// are NOT the runtime source of truth here: an <see cref="EquipmentInstance"/> carries the stat
    /// modifiers assigned/rolled at runtime. The same definition can back many instances.
    ///
    /// <see cref="statBuffs"/> + <see cref="rarity"/> remain as LEGACY default stats (migration
    /// source) so existing/authored items don't silently lose stats — see <see cref="BuildDefaultModifiers"/>.
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

        [Tooltip("LEGACY: rarity tier whose multiplier scaled the baked stat buffs (migration source). " +
                 "Runtime stats now live on EquipmentInstance. Still tints UI.")]
        [SerializeField] private Rarity_SO rarity;

        [Header("Legacy default stats (migration source — runtime stats live on EquipmentInstance)")]
        [Tooltip("LEGACY baked stats. No longer the runtime source of truth (see EquipmentInstance); kept so " +
                 "existing items/saves migrate into a default instance instead of losing stats. New items roll modifiers.")]
        [SerializeField] private List<StatBuff> statBuffs = new();

        [Header("Optional")]
        [Tooltip("Optional element affinity. Element-based effects can read this (e.g. fire enchantment).")]
        [SerializeField] private ElementType_SO elementAffinity;

        [Tooltip("Hand-slot items: weapon class. Drives Animator sub-SM + attack range/cooldown when equipped in RightHand.")]
        [SerializeField] private WeaponClass_SO weaponClass;

        [Header("Visual attachment")]
        [Tooltip("Name of the unit socket to attach this item's visual to (must match a socket on the unit's UnitEquipmentVisuals).")]
        [SerializeField] private string attachSocketName;

        [Tooltip("Prefab instantiated at the attach socket when equipped (preferred). Falls back to visualMesh if empty.")]
        [SerializeField] private GameObject visualPrefab;

        [Tooltip("Optional 3D mesh; instantiated as a MeshFilter+MeshRenderer at the socket when no visualPrefab is set.")]
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
        public string AttachSocketName => attachSocketName;
        public GameObject VisualPrefab => visualPrefab;
        public Mesh VisualMesh => visualMesh;
        public IReadOnlyList<Material> VisualMaterials => visualMaterials;

        public float RarityMultiplier => rarity != null ? rarity.StatMultiplier : 1f;

        /// <summary>
        /// LEGACY default stats for migration/compat: the authored <see cref="statBuffs"/> with the
        /// rarity multiplier folded into each amount. Used to build a default
        /// <see cref="EquipmentInstance"/> when an item/save has no rolled modifiers, so baked stats
        /// aren't lost. New items get their stats from a rolled instance instead.
        /// </summary>
        public List<StatBuff> BuildDefaultModifiers()
        {
            float mult = RarityMultiplier;
            var result = new List<StatBuff>(statBuffs.Count);
            for (int i = 0; i < statBuffs.Count; i++)
            {
                var b = statBuffs[i];
                result.Add(new StatBuff { stat = b.stat, modType = b.modType, amount = b.amount * mult });
            }
            return result;
        }
    }
}
