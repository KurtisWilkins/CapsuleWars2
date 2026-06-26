using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using UnityEngine;

namespace CapsuleWars.Run.Encounters
{
    /// <summary>
    /// Pure, seeded generation of an encounter's terrain (Slice C, iteration 1). Given an
    /// <see cref="EncounterDefinition"/> + the grid config + a run seed + node id + floor, produces a
    /// <see cref="TerrainLayout"/> of obstacles/hazards that <c>DeploymentManager.SetTerrain</c> stamps onto the
    /// grid (which <c>ArenaBuilder</c> renders + bakes into the NavMesh). Deterministic from (seed ^ nodeId), so
    /// the same node always lays out the same — no extra save needed. No scene deps → EditMode-testable.
    /// Enemy roster/placement is Slice C2/C3 and lives elsewhere.
    /// </summary>
    public static class EncounterGenerator
    {
        public static TerrainLayout GenerateTerrain(EncounterDefinition def, DeploymentGridConfig config,
                                                    int seed, int nodeId, int floor)
        {
            var layout = new TerrainLayout();
            if (def == null || config == null) return layout;

            // Mix the node id into the seed (large odd multiplier spreads adjacent nodes apart).
            var rng = new System.Random(seed ^ (nodeId * 73856093));

            var cells = EligibleCells(def, config);
            if (cells.Count == 0) return layout;
            Shuffle(cells, rng);

            int obstacleMax = Mathf.Max(def.minObstacles, def.maxObstacles);
            int obstacles = rng.Next(def.minObstacles, obstacleMax + 1) + Mathf.FloorToInt(Mathf.Max(0, floor) * def.obstaclesPerFloor);
            obstacles = Mathf.Clamp(obstacles, 0, cells.Count);

            int hazardMax = Mathf.Max(def.minHazards, def.maxHazards);
            int hazards = rng.Next(def.minHazards, hazardMax + 1);
            hazards = Mathf.Clamp(hazards, 0, cells.Count - obstacles);

            int i = 0;
            for (int n = 0; n < obstacles; n++, i++) layout.Add(cells[i], TerrainType.Impassable);
            for (int n = 0; n < hazards; n++, i++) layout.Add(cells[i], TerrainType.Hazard);
            return layout;
        }

        /// <summary>Cells terrain may occupy: neutral always; player rows only if not kept clear; enemy rows only if allowed.</summary>
        private static List<GridCoord> EligibleCells(EncounterDefinition def, DeploymentGridConfig config)
        {
            var list = new List<GridCoord>();
            for (int row = 0; row < config.rows; row++)
                for (int col = 0; col < config.columns; col++)
                {
                    var c = new GridCoord(col, row);
                    if (config.InPlayerZone(c) && def.keepPlayerZoneClear) continue;
                    if (config.InEnemyZone(c) && !def.allowEnemyZone) continue;
                    list.Add(c);
                }
            return list;
        }

        // Fisher-Yates using the seeded rng so the pick order is deterministic.
        private static void Shuffle(List<GridCoord> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
