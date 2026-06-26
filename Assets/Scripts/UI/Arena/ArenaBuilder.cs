using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Data.Arena;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace CapsuleWars.UI.Arena
{
    /// <summary>
    /// Builds the physical battle board at runtime from themed blocks (Slice B of the themed-encounter
    /// system; consumes the Slice A terrain layer, ADR-024). One checkerboard floor tile per grid cell —
    /// aligned 1:1 with <see cref="DeploymentGridConfig"/> so every tile IS a deployment cell and the
    /// existing CellState highlight overlays the same grid — plus a raised obstacle block on each Impassable
    /// cell and a marker on each Hazard cell, all sized from cellSize and driven by the active
    /// <see cref="EncounterTheme"/>'s <see cref="ThemeBlockSet"/> (primitive-cube fallback when a role has no
    /// prefab, so it works with zero authored assets). After the geometry exists it re-bakes the
    /// NavMeshSurface so agents path around Impassable cells.
    ///
    /// Layout source is the hand-authored <c>DeploymentManager.Terrain</c> — no procedural generation
    /// (that's Slice C). Pathing stays a single 2D plane; obstacles are visually raised only.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArenaBuilder : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private DeploymentManager manager;
        [SerializeField] private EncounterTheme theme;
        [Tooltip("Scene NavMeshSurface re-baked after building (units path on it). Optional but recommended.")]
        [SerializeField] private NavMeshSurface navMeshSurface;
        [Tooltip("Renderer of the legacy ground Plane; hidden while the block floor is built (its collider " +
                 "stays as the placement-raycast + NavMesh ground).")]
        [SerializeField] private Renderer groundPlaneRenderer;

        [Header("Build")]
        [Tooltip("Build the arena automatically at Start (encounter entry).")]
        [SerializeField] private bool buildOnStart = true;
        [Tooltip("Re-bake the NavMeshSurface at runtime after building the blocks (before any unit moves).")]
        [SerializeField] private bool bakeNavMeshOnBuild = true;
        [Tooltip("Gap between adjacent tiles (0 = seamless; a small value makes the cell grid read more clearly).")]
        [SerializeField, Range(0f, 0.3f)] private float tileGap = 0.04f;
        [Tooltip("World Y of the floor's top surface. A small positive value lifts the checkerboard just proud of the " +
                 "legacy ground plane so it always reads (no z-fighting), while units (spawned at ~0) still sit on it.")]
        [SerializeField, Range(0f, 0.5f)] private float floorSurfaceY = 0.05f;

        private Transform arenaRoot;

        /// <summary>True while a built arena exists.</summary>
        public bool HasArena => arenaRoot != null;

        /// <summary>World Y of the floor's top surface — overlays (e.g. the deployment highlight) sit above this.</summary>
        public float FloorSurfaceY => floorSurfaceY;

        private void Start()
        {
            if (buildOnStart) Build();
        }

        /// <summary>(Re)build the whole board: blocks first, then bake the NavMesh (the order units depend on).</summary>
        public void Build()
        {
            BuildGeometry();
            if (bakeNavMeshOnBuild) Bake();
        }

        /// <summary>Build just the block geometry (no NavMesh bake) — used by the editor preview so it doesn't churn the baked NavMesh asset.</summary>
        public void BuildGeometry()
        {
            Teardown();

            if (manager == null) { Debug.LogWarning("[ArenaBuilder] No DeploymentManager assigned; nothing to build.", this); return; }
            var cfg = manager.Config;
            if (cfg == null) return;

            var set = theme != null ? theme.BlockSet : null;
            if (set == null)
                Debug.LogWarning("[ArenaBuilder] No EncounterTheme/ThemeBlockSet assigned; building with primitive cubes.", this);

            var terrain = BuildTerrainLookup();

            arenaRoot = new GameObject("ArenaRoot").transform;
            arenaRoot.SetParent(transform, false);

            for (int row = 0; row < cfg.rows; row++)
            for (int col = 0; col < cfg.columns; col++)
            {
                var coord = new GridCoord(col, row);
                terrain.TryGetValue(coord, out var t);   // absent → Passable (default)
                Vector3 center = ArenaLayout.CellCenter(cfg, coord);

                if (t == TerrainType.Impassable)
                {
                    SpawnBlock(set, ArenaBlock.Obstacle, coord, center, cfg.cellSize);
                }
                else
                {
                    SpawnBlock(set, ArenaLayout.BlockFor(t, coord), coord, center, cfg.cellSize);
                    if (ArenaLayout.NeedsHazardMarker(t))
                        SpawnBlock(set, ArenaBlock.HazardMarker, coord, center, cfg.cellSize);
                }
            }

            if (groundPlaneRenderer != null) groundPlaneRenderer.enabled = false;
        }

        /// <summary>Re-bake the NavMeshSurface from physics colliders (units have no colliders → never baked in).</summary>
        public void Bake()
        {
            if (navMeshSurface == null) return;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.BuildNavMesh();
        }

        /// <summary>Destroy the current arena geometry and restore the ground-plane renderer. No leaks between encounters.</summary>
        public void Teardown()
        {
            if (arenaRoot != null)
            {
                if (Application.isPlaying) Destroy(arenaRoot.gameObject);
                else DestroyImmediate(arenaRoot.gameObject);
                arenaRoot = null;
            }
            if (groundPlaneRenderer != null) groundPlaneRenderer.enabled = true;
        }

        private Dictionary<GridCoord, TerrainType> BuildTerrainLookup()
        {
            var map = new Dictionary<GridCoord, TerrainType>();
            var layout = manager != null ? manager.Terrain : null;
            if (layout != null)
                foreach (var cell in layout.Cells)
                    map[cell.coord] = cell.type;
            return map;
        }

        private void SpawnBlock(ThemeBlockSet set, ArenaBlock role, GridCoord coord, Vector3 center, float cellSize)
        {
            var def = set != null ? set.Resolve(role) : null;
            bool obstacle = role == ArenaBlock.Obstacle;
            float height = def != null && def.height > 0.01f ? def.height : DefaultHeight(role);
            float footprint = Mathf.Max(0.05f, cellSize - tileGap);

            GameObject go;
            if (def != null && def.prefab != null)
            {
                go = Instantiate(def.prefab, arenaRoot);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);   // ships with a BoxCollider
                go.transform.SetParent(arenaRoot, false);
            }

            go.transform.localScale = new Vector3(footprint, height, footprint);
            go.name = $"{role}_{coord.col}_{coord.row}";

            // Colliders: only obstacles keep/get one. Units have no colliders (ADR-008), the placement raycast
            // hits the ground Plane, and the NavMesh bakes off physics colliders — so an obstacle's collider IS
            // its non-walkable footprint, and floor/hazard tiles stay collider-free (no raycast/bake interference).
            var existingCol = go.GetComponent<Collider>();
            if (obstacle)
            {
                if (existingCol == null) go.AddComponent<BoxCollider>();
                var mod = go.AddComponent<NavMeshModifier>();
                mod.overrideArea = true;
                mod.area = 1;   // built-in "Not Walkable" area
            }
            else if (existingCol != null)
            {
                DestroySafe(existingCol);
            }

            // Y: floor top sits at floorSurfaceY (just proud of the legacy plane); obstacle base on that surface;
            // hazard marker hovers just above it.
            float yCenter = role switch
            {
                ArenaBlock.Obstacle => floorSurfaceY + height * 0.5f,
                ArenaBlock.HazardMarker => floorSurfaceY + 0.02f + height * 0.5f,
                _ => floorSurfaceY - height * 0.5f,
            };
            go.transform.position = new Vector3(center.x, center.y + yCenter, center.z);

            if (def != null && def.material != null && go.TryGetComponent<Renderer>(out var r))
                r.sharedMaterial = def.material;
        }

        private static float DefaultHeight(ArenaBlock role) => role switch
        {
            ArenaBlock.Obstacle => 2f,
            ArenaBlock.HazardMarker => 0.25f,
            _ => 0.2f,
        };

        private static void DestroySafe(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
        }

#if UNITY_EDITOR
        // Editor-only preview affordances so the arena can be eyeballed without entering Play.
        // Nothing here ships in a player build.
        [ContextMenu("Arena/Build Preview (geometry only)")]
        private void EditorBuildPreview() => BuildGeometry();

        [ContextMenu("Arena/Build + Bake NavMesh")]
        private void EditorBuildAndBake() => Build();

        [ContextMenu("Arena/Bake NavMesh")]
        private void EditorBakeNavMesh() => Bake();

        [ContextMenu("Arena/Clear")]
        private void EditorClear() => Teardown();
#endif
    }
}
