using System;
using UnityEngine;

namespace CapsuleWars.Data.Arena
{
    /// <summary>
    /// Block-set-agnostic mapping {floor-A / floor-B / obstacle / hazard-marker} → a prefab + material + height.
    /// The runtime <c>ArenaBuilder</c> reads this to construct a themed board. When a role has no prefab, the
    /// builder falls back to a scaled primitive cube — so the whole system works with NO authored assets, and a
    /// real kit (Kubikos, Meshy blocks) drops in later by assigning prefabs here, with no code change.
    /// </summary>
    [CreateAssetMenu(menuName = "CapsuleWars/Arena/Theme Block Set", fileName = "ThemeBlockSet")]
    public class ThemeBlockSet : ScriptableObject
    {
        /// <summary>One block role's appearance: an optional prefab, an optional material, and a world height.</summary>
        [Serializable]
        public class BlockDef
        {
            [Tooltip("Prefab for this block. If null, the builder generates a primitive cube scaled to the cell.")]
            public GameObject prefab;
            [Tooltip("Material applied to the block (tints the primitive fallback or overrides the prefab). Optional.")]
            public Material material;
            [Tooltip("Block height in world units. Floor tiles are thin slabs; obstacles are raised.")]
            [Min(0.01f)] public float height = 0.2f;
        }

        [Header("Floor (checkerboard)")]
        [SerializeField] private BlockDef floorA = new BlockDef { height = 0.2f };
        [SerializeField] private BlockDef floorB = new BlockDef { height = 0.2f };

        [Header("Terrain")]
        [SerializeField] private BlockDef obstacle = new BlockDef { height = 2f };
        [SerializeField] private BlockDef hazardMarker = new BlockDef { height = 0.25f };

        /// <summary>The block definition for a role (never null for a valid role).</summary>
        public BlockDef Resolve(ArenaBlock role) => role switch
        {
            ArenaBlock.FloorA => floorA,
            ArenaBlock.FloorB => floorB,
            ArenaBlock.Obstacle => obstacle,
            ArenaBlock.HazardMarker => hazardMarker,
            _ => floorA,
        };

        /// <summary>True when a role has no authored prefab → the builder generates a primitive cube.</summary>
        public static bool UsesPrimitive(BlockDef def) => def == null || def.prefab == null;
    }
}
