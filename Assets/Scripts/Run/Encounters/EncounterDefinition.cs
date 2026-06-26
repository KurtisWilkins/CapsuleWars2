using UnityEngine;

namespace CapsuleWars.Run.Encounters
{
    /// <summary>
    /// Tunable parameters for generating a combat encounter (Slice C). Iteration 1 (C1) uses only the obstacle/
    /// hazard fields — the generator turns these + the run seed into a per-node terrain layout. The enemy roster
    /// spec + placement strategy land here in C2/C3. Authorable; later one per biome/floor.
    /// </summary>
    [CreateAssetMenu(menuName = "CapsuleWars/Encounters/Encounter Definition", fileName = "EncounterDefinition")]
    public class EncounterDefinition : ScriptableObject
    {
        [Header("Obstacles (Impassable)")]
        [Tooltip("Min/max Impassable obstacle cells per encounter (before per-floor scaling).")]
        [Min(0)] public int minObstacles = 2;
        [Min(0)] public int maxObstacles = 5;
        [Tooltip("Extra obstacles added per floor of depth (floored). Deeper floors get busier boards.")]
        [Min(0f)] public float obstaclesPerFloor = 0.5f;

        [Header("Hazards (placeable but harmful)")]
        [Tooltip("Min/max Hazard cells per encounter.")]
        [Min(0)] public int minHazards = 0;
        [Min(0)] public int maxHazards = 2;

        [Header("Placement constraints")]
        [Tooltip("Never place terrain on the player's deploy-zone rows (keeps deployment unblocked).")]
        public bool keepPlayerZoneClear = true;
        [Tooltip("Allow terrain on the far enemy-zone rows. If off, terrain is confined to the neutral middle rows.")]
        public bool allowEnemyZone = true;

        // --- Roster spec (Slice C2) lands here later: enemy count by NodeType/floor, archetype pool, etc. ---
    }
}
