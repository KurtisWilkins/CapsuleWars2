using UnityEngine;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Defines the deployment grid's dimensions, its cell↔world mapping, and the
    /// player's deployable zone. Plain serializable class so it can be tuned on a
    /// MonoBehaviour in the inspector and unit-tested without a scene.
    ///
    /// The grid lies on the world XZ plane: cell (0,0) is centered at
    /// <see cref="origin"/> and columns advance along +X, rows along +Z.
    /// </summary>
    [System.Serializable]
    public class DeploymentGridConfig
    {
        [Min(1)] public int columns = 9;
        [Min(1)] public int rows = 7;
        [Min(0.01f)] public float cellSize = 1.5f;

        [Tooltip("World position of the center of cell (0,0). Columns extend +X, rows extend +Z.")]
        public Vector3 origin = Vector3.zero;

        [Header("Player deploy zone (inclusive row range)")]
        [Tooltip("Lowest row index the player may place on.")]
        [Min(0)] public int playerRowMin = 0;
        [Tooltip("Highest row index the player may place on.")]
        [Min(0)] public int playerRowMax = 2;

        public bool InBounds(GridCoord c) => c.col >= 0 && c.col < columns && c.row >= 0 && c.row < rows;

        /// <summary>In bounds and within the inclusive player row range.</summary>
        public bool InPlayerZone(GridCoord c) => InBounds(c) && c.row >= playerRowMin && c.row <= playerRowMax;

        /// <summary>World-space center of a cell.</summary>
        public Vector3 CellToWorld(GridCoord c) =>
            origin + new Vector3(c.col * cellSize, 0f, c.row * cellSize);

        /// <summary>Nearest cell to a world position (may be out of bounds; check with InBounds).</summary>
        public GridCoord WorldToCell(Vector3 world)
        {
            Vector3 local = world - origin;
            int col = Mathf.RoundToInt(local.x / cellSize);
            int row = Mathf.RoundToInt(local.z / cellSize);
            return new GridCoord(col, row);
        }
    }
}
