using CapsuleWars.Combat.Deployment;
using CapsuleWars.Data.Arena;
using CapsuleWars.UI.Arena;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Slice B — the pure placement math behind the runtime arena builder: checkerboard parity, the
    /// terrain → block-role mapping, and cell → world centers (1:1 with the deployment grid).
    /// </summary>
    public class ArenaLayoutTests
    {
        [Test]
        public void IsFloorA_AlternatesChessStyle()
        {
            Assert.IsTrue(ArenaLayout.IsFloorA(new GridCoord(0, 0)));
            Assert.IsFalse(ArenaLayout.IsFloorA(new GridCoord(1, 0)));
            Assert.IsFalse(ArenaLayout.IsFloorA(new GridCoord(0, 1)));
            Assert.IsTrue(ArenaLayout.IsFloorA(new GridCoord(1, 1)));
            Assert.IsFalse(ArenaLayout.IsFloorA(new GridCoord(2, 3)));   // col+row = 5 (odd)
            Assert.IsTrue(ArenaLayout.IsFloorA(new GridCoord(3, 3)));    // col+row = 6 (even)
        }

        [Test]
        public void BlockFor_ImpassableIsObstacle_RegardlessOfParity()
        {
            Assert.AreEqual(ArenaBlock.Obstacle, ArenaLayout.BlockFor(TerrainType.Impassable, new GridCoord(0, 0)));
            Assert.AreEqual(ArenaBlock.Obstacle, ArenaLayout.BlockFor(TerrainType.Impassable, new GridCoord(1, 0)));
        }

        [Test]
        public void BlockFor_PassableAndHazardAreCheckerboardFloor()
        {
            Assert.AreEqual(ArenaBlock.FloorA, ArenaLayout.BlockFor(TerrainType.Passable, new GridCoord(0, 0)));
            Assert.AreEqual(ArenaBlock.FloorB, ArenaLayout.BlockFor(TerrainType.Passable, new GridCoord(1, 0)));
            // A hazard cell still gets a (walkable) floor tile; its marker is added separately.
            Assert.AreEqual(ArenaBlock.FloorA, ArenaLayout.BlockFor(TerrainType.Hazard, new GridCoord(0, 0)));
            Assert.AreEqual(ArenaBlock.FloorB, ArenaLayout.BlockFor(TerrainType.Hazard, new GridCoord(1, 0)));
        }

        [Test]
        public void NeedsHazardMarker_OnlyForHazard()
        {
            Assert.IsTrue(ArenaLayout.NeedsHazardMarker(TerrainType.Hazard));
            Assert.IsFalse(ArenaLayout.NeedsHazardMarker(TerrainType.Passable));
            Assert.IsFalse(ArenaLayout.NeedsHazardMarker(TerrainType.Impassable));
        }

        [Test]
        public void CellCenter_MatchesGridConfig()
        {
            var cfg = new DeploymentGridConfig { columns = 7, rows = 9, cellSize = 3.5f, origin = Vector3.zero };
            var c = new GridCoord(2, 4);
            Assert.AreEqual(cfg.CellToWorld(c), ArenaLayout.CellCenter(cfg, c));
        }

        [Test]
        public void CellCenter_NullConfig_IsZero()
        {
            Assert.AreEqual(Vector3.zero, ArenaLayout.CellCenter(null, new GridCoord(3, 3)));
        }
    }
}
