using CapsuleWars.Combat.Deployment;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests the deployment grid model: cell↔world mapping, deploy-zone/bounds
    /// rules, cell-state classification, and placement/move/remove logic.
    /// </summary>
    public class DeploymentGridTests
    {
        private static DeploymentGridConfig Config() => new DeploymentGridConfig
        {
            columns = 9,
            rows = 7,
            cellSize = 1.5f,
            origin = new Vector3(10f, 0f, -5f),
            playerRowMin = 0,
            playerRowMax = 2,
        };

        // ---- Config: bounds + zone + world mapping ----

        [Test]
        public void InBounds_RespectsDimensions()
        {
            var cfg = Config();
            Assert.IsTrue(cfg.InBounds(new GridCoord(0, 0)));
            Assert.IsTrue(cfg.InBounds(new GridCoord(8, 6)));
            Assert.IsFalse(cfg.InBounds(new GridCoord(9, 0)));
            Assert.IsFalse(cfg.InBounds(new GridCoord(0, 7)));
            Assert.IsFalse(cfg.InBounds(new GridCoord(-1, 0)));
        }

        [Test]
        public void InPlayerZone_OnlyWithinRowRange()
        {
            var cfg = Config();
            Assert.IsTrue(cfg.InPlayerZone(new GridCoord(4, 0)));
            Assert.IsTrue(cfg.InPlayerZone(new GridCoord(4, 2)));
            Assert.IsFalse(cfg.InPlayerZone(new GridCoord(4, 3)));   // beyond playerRowMax
            Assert.IsFalse(cfg.InPlayerZone(new GridCoord(9, 0)));   // out of bounds
        }

        [Test]
        public void PlayerAndEnemyZones_AreDisjoint_WithNeutralMiddle()
        {
            var cfg = new DeploymentGridConfig
            {
                columns = 7, rows = 9, cellSize = 3.5f, origin = Vector3.zero,
                playerRowMin = 0, playerRowMax = 2,
                enemyRowMin = 6, enemyRowMax = 8,
            };

            // Player zone = near rows 0-2; enemy zone = far rows 6-8.
            Assert.IsTrue(cfg.InPlayerZone(new GridCoord(3, 0)));
            Assert.IsTrue(cfg.InPlayerZone(new GridCoord(3, 2)));
            Assert.IsTrue(cfg.InEnemyZone(new GridCoord(3, 6)));
            Assert.IsTrue(cfg.InEnemyZone(new GridCoord(3, 8)));

            // No cell is ever in both zones; neutral middle rows are in neither.
            for (int row = 0; row < cfg.rows; row++)
            {
                var c = new GridCoord(3, row);
                Assert.IsFalse(cfg.InPlayerZone(c) && cfg.InEnemyZone(c), $"row {row} in both zones");
                if (row >= 3 && row <= 5)
                    Assert.IsFalse(cfg.InPlayerZone(c) || cfg.InEnemyZone(c), $"neutral row {row} should be in neither zone");
            }
        }

        [Test]
        public void CellToWorld_UsesCellSize()
        {
            var cfg = new DeploymentGridConfig { columns = 7, rows = 9, cellSize = 3.5f, origin = Vector3.zero };
            Assert.AreEqual(new Vector3(2 * 3.5f, 0f, 4 * 3.5f), cfg.CellToWorld(new GridCoord(2, 4)));
        }

        [Test]
        public void CellToWorld_AndBack_RoundTrips()
        {
            var cfg = Config();
            var coord = new GridCoord(3, 5);
            Vector3 world = cfg.CellToWorld(coord);
            Assert.AreEqual(new Vector3(10f + 3 * 1.5f, 0f, -5f + 5 * 1.5f), world);
            Assert.AreEqual(coord, cfg.WorldToCell(world));
        }

        [Test]
        public void WorldToCell_SnapsToNearest()
        {
            var cfg = Config();
            // Slightly off the exact center of cell (2,1) should still snap to it.
            Vector3 near = cfg.CellToWorld(new GridCoord(2, 1)) + new Vector3(0.4f, 0f, -0.4f);
            Assert.AreEqual(new GridCoord(2, 1), cfg.WorldToCell(near));
        }

        // ---- Cell state ----

        [Test]
        public void GetState_ClassifiesCells()
        {
            var grid = new DeploymentGrid(Config());

            Assert.AreEqual(CellState.OutOfBounds, grid.GetState(new GridCoord(9, 0)));
            Assert.AreEqual(CellState.OutsideZone, grid.GetState(new GridCoord(4, 5)));   // in bounds, beyond zone
            Assert.AreEqual(CellState.Empty, grid.GetState(new GridCoord(4, 1)));         // deployable + free

            grid.SetBlocked(new GridCoord(1, 1), true);
            Assert.AreEqual(CellState.Blocked, grid.GetState(new GridCoord(1, 1)));

            grid.TryPlace(new GridCoord(2, 1), "u1");
            Assert.AreEqual(CellState.Occupied, grid.GetState(new GridCoord(2, 1)));
        }

        // ---- Placement ----

        [Test]
        public void TryPlace_SucceedsOnDeployableEmptyCell()
        {
            var grid = new DeploymentGrid(Config());
            Assert.IsTrue(grid.TryPlace(new GridCoord(3, 1), "u1"));
            Assert.IsTrue(grid.IsOccupied(new GridCoord(3, 1)));
            Assert.IsTrue(grid.TryGetOccupant(new GridCoord(3, 1), out var id));
            Assert.AreEqual("u1", id);
        }

        [Test]
        public void TryPlace_FailsOutsideZone_Blocked_Occupied_OrEmptyId()
        {
            var grid = new DeploymentGrid(Config());

            Assert.IsFalse(grid.TryPlace(new GridCoord(4, 5), "u1"));   // outside zone
            grid.SetBlocked(new GridCoord(0, 0), true);
            Assert.IsFalse(grid.TryPlace(new GridCoord(0, 0), "u1"));   // blocked

            Assert.IsTrue(grid.TryPlace(new GridCoord(1, 1), "u1"));
            Assert.IsFalse(grid.TryPlace(new GridCoord(1, 1), "u2"));   // occupied
            Assert.IsFalse(grid.TryPlace(new GridCoord(2, 1), ""));     // empty id
        }

        [Test]
        public void TryPlace_MovesOccupant_OneCellPerUnit()
        {
            var grid = new DeploymentGrid(Config());
            grid.TryPlace(new GridCoord(1, 1), "u1");
            grid.TryPlace(new GridCoord(5, 2), "u1");   // same id placed again elsewhere

            Assert.IsFalse(grid.IsOccupied(new GridCoord(1, 1)), "prior cell should be cleared");
            Assert.IsTrue(grid.IsOccupied(new GridCoord(5, 2)));
            Assert.AreEqual(1, grid.Occupants.Count);
        }

        [Test]
        public void TryMove_MovesToValidEmptyCell_RejectsInvalid()
        {
            var grid = new DeploymentGrid(Config());
            grid.TryPlace(new GridCoord(1, 1), "u1");

            Assert.IsTrue(grid.TryMove(new GridCoord(1, 1), new GridCoord(2, 2)));
            Assert.IsTrue(grid.IsOccupied(new GridCoord(2, 2)));
            Assert.IsFalse(grid.IsOccupied(new GridCoord(1, 1)));

            grid.TryPlace(new GridCoord(3, 1), "u2");
            Assert.IsFalse(grid.TryMove(new GridCoord(2, 2), new GridCoord(3, 1)), "target occupied");
            Assert.IsFalse(grid.TryMove(new GridCoord(2, 2), new GridCoord(4, 5)), "target outside zone");
            Assert.IsFalse(grid.TryMove(new GridCoord(7, 0), new GridCoord(6, 0)), "no occupant at source");
        }

        [Test]
        public void Remove_ByCell_AndByOccupant()
        {
            var grid = new DeploymentGrid(Config());
            grid.TryPlace(new GridCoord(1, 1), "u1");
            grid.TryPlace(new GridCoord(2, 1), "u2");

            Assert.IsTrue(grid.RemoveAt(new GridCoord(1, 1)));
            Assert.IsFalse(grid.RemoveAt(new GridCoord(1, 1)));   // already gone

            Assert.IsTrue(grid.RemoveOccupant("u2"));
            Assert.IsFalse(grid.RemoveOccupant("u2"));
            Assert.AreEqual(0, grid.Occupants.Count);
        }
    }
}
