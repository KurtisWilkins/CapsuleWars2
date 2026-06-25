using CapsuleWars.Combat.Deployment;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Slice A — the per-cell terrain model that generalizes the binary "blocked" flag:
    /// Impassable blocks placement + reports Blocked; Hazard is placeable per
    /// <see cref="DeploymentGridConfig.allowPlaceOnHazard"/> + reports Hazard; the legacy
    /// SetBlocked/IsBlocked API stays compatible (blocked == Impassable); and TerrainLayout
    /// stamps cells onto the grid. Existing placement/zone tests still pass (no hazards set).
    /// </summary>
    public class DeploymentTerrainTests
    {
        private static DeploymentGridConfig Config(bool allowPlaceOnHazard = true) => new DeploymentGridConfig
        {
            columns = 9,
            rows = 7,
            cellSize = 1.5f,
            origin = Vector3.zero,
            playerRowMin = 0,
            playerRowMax = 2,
            allowPlaceOnHazard = allowPlaceOnHazard,
        };

        // ---- Impassable ----

        [Test]
        public void Impassable_BlocksPlacement_AndReportsBlocked()
        {
            var grid = new DeploymentGrid(Config());
            var c = new GridCoord(1, 1);   // in the player zone

            grid.SetTerrain(c, TerrainType.Impassable);

            Assert.IsTrue(grid.IsImpassable(c));
            Assert.IsFalse(grid.IsDeployable(c), "Impassable must not be deployable");
            Assert.AreEqual(CellState.Blocked, grid.GetState(c));
            Assert.IsFalse(grid.TryPlace(c, "u1"), "cannot place on Impassable");
        }

        // ---- Hazard ----

        [Test]
        public void Hazard_PlaceableByDefault_ReportsHazard()
        {
            var grid = new DeploymentGrid(Config(allowPlaceOnHazard: true));
            var c = new GridCoord(2, 1);

            grid.SetTerrain(c, TerrainType.Hazard);

            Assert.IsTrue(grid.IsHazard(c));
            Assert.AreEqual(CellState.Hazard, grid.GetState(c));
            Assert.IsTrue(grid.IsDeployable(c), "Hazard is deployable when allowPlaceOnHazard is on");
            Assert.IsTrue(grid.TryPlace(c, "u1"), "can place on a Hazard by default");
            Assert.AreEqual(CellState.Occupied, grid.GetState(c), "an occupied hazard reads Occupied");
        }

        [Test]
        public void Hazard_BlocksPlacement_WhenConfigForbids()
        {
            var grid = new DeploymentGrid(Config(allowPlaceOnHazard: false));
            var c = new GridCoord(2, 1);

            grid.SetTerrain(c, TerrainType.Hazard);

            Assert.IsFalse(grid.IsDeployable(c), "Hazard is not deployable when the rule forbids it");
            Assert.IsFalse(grid.TryPlace(c, "u1"));
            Assert.AreEqual(CellState.Hazard, grid.GetState(c), "still reports Hazard for feedback");
        }

        // ---- Legacy blocked API stays compatible ----

        [Test]
        public void SetBlocked_RoundTripsThroughImpassable()
        {
            var grid = new DeploymentGrid(Config());
            var c = new GridCoord(3, 1);

            grid.SetBlocked(c, true);
            Assert.IsTrue(grid.IsBlocked(c));
            Assert.AreEqual(TerrainType.Impassable, grid.GetTerrain(c));
            Assert.AreEqual(CellState.Blocked, grid.GetState(c));

            grid.SetBlocked(c, false);
            Assert.IsFalse(grid.IsBlocked(c));
            Assert.AreEqual(TerrainType.Passable, grid.GetTerrain(c));
        }

        [Test]
        public void SetBlockedFalse_LeavesHazardIntact()
        {
            var grid = new DeploymentGrid(Config());
            var c = new GridCoord(4, 1);

            grid.SetTerrain(c, TerrainType.Hazard);
            grid.SetBlocked(c, false);   // legacy "unblock" must not wipe a hazard

            Assert.AreEqual(TerrainType.Hazard, grid.GetTerrain(c));
        }

        // ---- Terrain map + layout ----

        [Test]
        public void GetTerrain_DefaultsToPassable_AndIsSparse()
        {
            var grid = new DeploymentGrid(Config());
            Assert.AreEqual(TerrainType.Passable, grid.GetTerrain(new GridCoord(5, 1)));
            Assert.AreEqual(0, grid.TerrainCells.Count, "Passable cells are not stored");

            grid.SetTerrain(new GridCoord(5, 1), TerrainType.Impassable);
            grid.SetTerrain(new GridCoord(5, 1), TerrainType.Passable);   // back to passable removes the entry
            Assert.AreEqual(0, grid.TerrainCells.Count);
        }

        [Test]
        public void TerrainCells_ExposesOnlyNonPassable()
        {
            var grid = new DeploymentGrid(Config());
            grid.SetTerrain(new GridCoord(0, 1), TerrainType.Impassable);
            grid.SetTerrain(new GridCoord(1, 1), TerrainType.Hazard);

            Assert.AreEqual(2, grid.TerrainCells.Count);
            Assert.AreEqual(TerrainType.Impassable, grid.TerrainCells[new GridCoord(0, 1)]);
            Assert.AreEqual(TerrainType.Hazard, grid.TerrainCells[new GridCoord(1, 1)]);
        }

        [Test]
        public void TerrainLayout_ApplyTo_StampsCells()
        {
            var grid = new DeploymentGrid(Config());
            var layout = new TerrainLayout();
            layout.Add(new GridCoord(0, 1), TerrainType.Impassable);
            layout.Add(new GridCoord(1, 1), TerrainType.Hazard);

            layout.ApplyTo(grid);

            Assert.IsTrue(grid.IsImpassable(new GridCoord(0, 1)));
            Assert.IsTrue(grid.IsHazard(new GridCoord(1, 1)));
            Assert.AreEqual(2, layout.Cells.Count);
        }

        [Test]
        public void Clear_RemovesOccupants_ButKeepsTerrain()
        {
            var grid = new DeploymentGrid(Config());
            grid.SetTerrain(new GridCoord(0, 0), TerrainType.Impassable);
            grid.TryPlace(new GridCoord(1, 0), "u1");

            grid.Clear();

            Assert.IsFalse(grid.IsOccupied(new GridCoord(1, 0)), "placements cleared");
            Assert.IsTrue(grid.IsImpassable(new GridCoord(0, 0)), "terrain survives a placement Clear");
        }
    }
}
