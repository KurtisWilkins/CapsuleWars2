using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Run.Encounters;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Slice C (C1) — the seeded encounter terrain generator: deterministic from (seed ^ nodeId), counts within
    /// the definition's ranges, and zone constraints (player deploy zone kept clear; enemy zone optional).
    /// </summary>
    public class EncounterGeneratorTests
    {
        private readonly List<EncounterDefinition> created = new List<EncounterDefinition>();

        [TearDown]
        public void TearDown()
        {
            foreach (var d in created) if (d != null) Object.DestroyImmediate(d);
            created.Clear();
        }

        private EncounterDefinition Def(int minO, int maxO, int minH, int maxH, bool keepPlayer = true, bool allowEnemy = true)
        {
            var d = ScriptableObject.CreateInstance<EncounterDefinition>();
            d.minObstacles = minO; d.maxObstacles = maxO; d.obstaclesPerFloor = 0f;
            d.minHazards = minH; d.maxHazards = maxH;
            d.keepPlayerZoneClear = keepPlayer; d.allowEnemyZone = allowEnemy;
            created.Add(d);
            return d;
        }

        private static DeploymentGridConfig Config() => new DeploymentGridConfig
        {
            columns = 7, rows = 9, cellSize = 3.5f, origin = Vector3.zero,
            playerRowMin = 0, playerRowMax = 2, enemyRowMin = 6, enemyRowMax = 8,
        };

        private static int Count(TerrainLayout layout, TerrainType type)
        {
            int n = 0;
            foreach (var c in layout.Cells) if (c.type == type) n++;
            return n;
        }

        [Test]
        public void Deterministic_SameInputs_SameLayout()
        {
            var def = Def(2, 5, 0, 2);
            var cfg = Config();
            var a = EncounterGenerator.GenerateTerrain(def, cfg, seed: 777, nodeId: 3, floor: 1);
            var b = EncounterGenerator.GenerateTerrain(def, cfg, seed: 777, nodeId: 3, floor: 1);

            Assert.AreEqual(a.Cells.Count, b.Cells.Count);
            for (int i = 0; i < a.Cells.Count; i++)
            {
                Assert.AreEqual(a.Cells[i].coord, b.Cells[i].coord, $"cell {i} coord differs");
                Assert.AreEqual(a.Cells[i].type, b.Cells[i].type, $"cell {i} type differs");
            }
        }

        [Test]
        public void Counts_WithinDefinitionRanges_AtFloorZero()
        {
            var def = Def(2, 4, 1, 2);
            var layout = EncounterGenerator.GenerateTerrain(def, Config(), seed: 12, nodeId: 5, floor: 0);

            int obstacles = Count(layout, TerrainType.Impassable);
            int hazards = Count(layout, TerrainType.Hazard);
            Assert.GreaterOrEqual(obstacles, 2);
            Assert.LessOrEqual(obstacles, 4);
            Assert.GreaterOrEqual(hazards, 1);
            Assert.LessOrEqual(hazards, 2);
        }

        [Test]
        public void ObstaclesScaleWithFloor()
        {
            var def = Def(2, 2, 0, 0);   // fixed 2 obstacles at floor 0
            def.obstaclesPerFloor = 1f;  // +1 per floor
            var deep = EncounterGenerator.GenerateTerrain(def, Config(), seed: 9, nodeId: 1, floor: 4);
            Assert.AreEqual(2 + 4, Count(deep, TerrainType.Impassable), "floor 4 → 2 + 4*1 obstacles");
        }

        [Test]
        public void KeepsPlayerZoneClear()
        {
            var def = Def(8, 12, 2, 4, keepPlayer: true);
            var cfg = Config();
            var layout = EncounterGenerator.GenerateTerrain(def, cfg, seed: 1, nodeId: 1, floor: 3);

            foreach (var c in layout.Cells)
                Assert.IsFalse(cfg.InPlayerZone(c.coord), $"terrain placed in the player deploy zone at {c.coord}");
        }

        [Test]
        public void AllowEnemyZoneFalse_ConfinesToNeutralRows()
        {
            var def = Def(8, 12, 0, 0, keepPlayer: true, allowEnemy: false);
            var cfg = Config();
            var layout = EncounterGenerator.GenerateTerrain(def, cfg, seed: 2, nodeId: 2, floor: 3);

            foreach (var c in layout.Cells)
            {
                Assert.IsFalse(cfg.InPlayerZone(c.coord), $"terrain in player zone at {c.coord}");
                Assert.IsFalse(cfg.InEnemyZone(c.coord), $"terrain in enemy zone at {c.coord} (should be neutral-only)");
            }
        }

        [Test]
        public void NoCellUsedTwice()
        {
            var def = Def(5, 8, 2, 3);
            var layout = EncounterGenerator.GenerateTerrain(def, Config(), seed: 55, nodeId: 7, floor: 2);

            var seen = new HashSet<GridCoord>();
            foreach (var c in layout.Cells)
                Assert.IsTrue(seen.Add(c.coord), $"cell {c.coord} used by more than one terrain entry");
        }

        // ---- Roster size (C2) ----

        [Test]
        public void RosterSize_WithinRange_AtFloorZero()
        {
            var def = Def(0, 0, 0, 0);
            def.minEnemies = 2; def.maxEnemies = 4; def.enemiesPerFloor = 0f; def.bossEnemyCount = 0;
            int n = EncounterGenerator.RosterSize(def, seed: 4, nodeId: 2, floor: 0, isBoss: false);
            Assert.GreaterOrEqual(n, 2);
            Assert.LessOrEqual(n, 4);
        }

        [Test]
        public void RosterSize_Boss_UsesFixedCount()
        {
            var def = Def(0, 0, 0, 0);
            def.bossEnemyCount = 3;
            Assert.AreEqual(3, EncounterGenerator.RosterSize(def, seed: 1, nodeId: 1, floor: 5, isBoss: true));
        }

        [Test]
        public void RosterSize_ScalesWithFloor()
        {
            var def = Def(0, 0, 0, 0);
            def.minEnemies = 2; def.maxEnemies = 2; def.enemiesPerFloor = 1f; def.bossEnemyCount = 0;
            Assert.AreEqual(2 + 3, EncounterGenerator.RosterSize(def, seed: 8, nodeId: 1, floor: 3, isBoss: false));
        }

        // ---- Enemy placement cells (C3) ----

        [Test]
        public void EnemyCells_AreInEnemyZone_AndAvoidImpassable()
        {
            var cfg = Config();
            var grid = new DeploymentGrid(cfg);
            grid.SetTerrain(new GridCoord(0, 6), TerrainType.Impassable);
            grid.SetTerrain(new GridCoord(3, 7), TerrainType.Impassable);

            var cells = EncounterGenerator.EnemyCells(cfg, grid, count: 5, seed: 3, nodeId: 4);

            Assert.AreEqual(5, cells.Count);
            foreach (var c in cells)
            {
                Assert.IsTrue(cfg.InEnemyZone(c), $"enemy cell {c} not in the enemy zone");
                Assert.IsFalse(grid.IsImpassable(c), $"enemy spawned on an obstacle at {c}");
            }
        }

        [Test]
        public void EnemyCells_Deterministic()
        {
            var cfg = Config();
            var grid = new DeploymentGrid(cfg);
            var a = EncounterGenerator.EnemyCells(cfg, grid, 4, seed: 99, nodeId: 6);
            var b = EncounterGenerator.EnemyCells(cfg, grid, 4, seed: 99, nodeId: 6);
            CollectionAssert.AreEqual(a, b);
        }
    }
}
