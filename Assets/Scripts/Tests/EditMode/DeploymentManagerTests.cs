using CapsuleWars.Combat.Deployment;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests DeploymentManager placement: it snaps the unit transform to the cell
    /// and updates the grid, and rejects non-deployable cells. Uses throwaway
    /// GameObjects (no scene). Grid is lazy so Awake isn't required.
    /// </summary>
    public class DeploymentManagerTests
    {
        private GameObject mgrGo;
        private DeploymentManager manager;

        [SetUp]
        public void Setup()
        {
            mgrGo = new GameObject("DeploymentManager");
            manager = mgrGo.AddComponent<DeploymentManager>();
            // default config: 9x7, cellSize 1.5, origin (0,0,0), player rows 0..2
        }

        [TearDown]
        public void Teardown()
        {
            if (mgrGo != null) Object.DestroyImmediate(mgrGo);
        }

        private static UnitRoot MakeUnit(string id)
        {
            var go = new GameObject(id);
            var root = go.AddComponent<UnitRoot>();
            root.SetIdentity(id, id);
            return root;
        }

        [Test]
        public void PlaceUnit_OccupiesCell_AndSnapsTransform()
        {
            var unit = MakeUnit("u1");
            manager.RegisterUnit(unit);

            Assert.IsTrue(manager.PlaceUnit("u1", new GridCoord(3, 1)));
            Assert.IsTrue(manager.Grid.IsOccupied(new GridCoord(3, 1)));

            // CellToWorld((3,1)) with default config = (3*1.5, y, 1*1.5); y preserved (0).
            Assert.AreEqual(new Vector3(4.5f, 0f, 1.5f), unit.transform.position);

            Object.DestroyImmediate(unit.gameObject);
        }

        [Test]
        public void PlaceUnit_RejectsOutsideZone_AndUnregistered()
        {
            var unit = MakeUnit("u1");
            manager.RegisterUnit(unit);

            Assert.IsFalse(manager.PlaceUnit("u1", new GridCoord(3, 5)), "row 5 is outside the player zone");
            Assert.IsFalse(manager.PlaceUnit("ghost", new GridCoord(3, 1)), "unregistered unit");

            Object.DestroyImmediate(unit.gameObject);
        }

        [Test]
        public void ApplyPlacements_PlacesRegisteredUnits()
        {
            var u1 = MakeUnit("u1");
            var u2 = MakeUnit("u2");
            manager.RegisterUnit(u1);
            manager.RegisterUnit(u2);

            manager.ApplyPlacements(new System.Collections.Generic.Dictionary<string, GridCoord>
            {
                { "u1", new GridCoord(0, 0) },
                { "u2", new GridCoord(6, 2) },
            });

            Assert.IsTrue(manager.Grid.IsOccupied(new GridCoord(0, 0)));
            Assert.IsTrue(manager.Grid.IsOccupied(new GridCoord(6, 2)));

            Object.DestroyImmediate(u1.gameObject);
            Object.DestroyImmediate(u2.gameObject);
        }

        [Test]
        public void AutoArrange_PlacesUnitsInDeployZone_EvenWhenSpawnedOutside()
        {
            var u1 = MakeUnit("u1");
            var u2 = MakeUnit("u2");
            u1.transform.position = new Vector3(100f, 0f, 100f);   // far outside any cell/zone
            u2.transform.position = new Vector3(-50f, 0f, -50f);
            manager.RegisterUnit(u1);
            manager.RegisterUnit(u2);

            manager.AutoArrange();

            var placements = manager.GetPlacements();
            Assert.AreEqual(2, placements.Count);
            foreach (var kv in placements)
                Assert.IsTrue(manager.Config.InPlayerZone(kv.Value), $"{kv.Key} not placed in the deploy zone");

            Object.DestroyImmediate(u1.gameObject);
            Object.DestroyImmediate(u2.gameObject);
        }

        [Test]
        public void Tokens_PlaceRemoveClear_WithoutLiveUnits()
        {
            Assert.IsTrue(manager.PlaceToken("u1", new GridCoord(2, 1)));
            Assert.IsTrue(manager.Grid.IsOccupied(new GridCoord(2, 1)));
            Assert.IsTrue(manager.GetPlacements().ContainsKey("u1"));

            Assert.IsFalse(manager.PlaceToken("u2", new GridCoord(2, 7)), "row 7 is outside the player zone");

            Assert.IsTrue(manager.RemoveToken("u1"));
            Assert.IsFalse(manager.Grid.IsOccupied(new GridCoord(2, 1)));

            manager.PlaceToken("a", new GridCoord(0, 0));
            manager.PlaceToken("b", new GridCoord(1, 0));
            manager.ClearAll();
            Assert.AreEqual(0, manager.GetPlacements().Count);
        }
    }
}
