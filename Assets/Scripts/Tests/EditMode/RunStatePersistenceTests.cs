using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Round-trip tests for run-state persistence: RunState &lt;-&gt; RunStateDTO mapping
    /// (graph nodes + edges + seed + position), JSON serialization, and disk save/load.
    /// </summary>
    public class RunStatePersistenceTests
    {
        [TearDown]
        public void Teardown() => RunStore.Delete();

        // 3-row chain: Fight(0) -> Shop(1) -> Boss(2). Travel to depth 1, clearing both.
        private static RunState MakeRun()
        {
            var n0 = new MapNode(0, 0, 0, NodeType.Combat, "Fight");
            var n1 = new MapNode(1, 1, 0, NodeType.Shop, "Shop");
            var n2 = new MapNode(2, 2, 0, NodeType.Boss, "Boss");
            n0.AddEdge(1);
            n1.AddEdge(2);

            var state = new RunState(new RunMap(new List<MapNode> { n0, n1, n2 }), 50, seed: 1234)
            {
                IsBossEncounter = true,
                RewardsGranted = true,
            };
            state.TravelTo(0); state.MarkCurrentCleared();
            state.TravelTo(1); state.MarkCurrentCleared();   // current = node 1, depth 1

            var p1 = new UnitDTO("u1", "Conan", "warrior_01");
            p1.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "iron_sword"));
            var p2 = new UnitDTO("u2", "Merlin", "mage_01");
            state.SetParty(new[] { p1, p2 });
            state.AddRecruit(new UnitDTO("r1", "Recruit", "rogue_01"));
            state.SetPlacement("u1", new GridCoord(3, 1));
            return state;
        }

        [Test]
        public void ToDTO_CapturesGraphSeedPositionAndEquipment()
        {
            var dto = MakeRun().ToDTO();

            Assert.AreEqual(2, dto.SaveVersion);
            Assert.AreEqual(1, dto.CurrentFloor);
            Assert.AreEqual(1, dto.CurrentNodeId);
            Assert.AreEqual(1234, dto.Seed);
            Assert.AreEqual(50, dto.Gold);
            Assert.IsTrue(dto.IsBossEncounter);
            Assert.IsTrue(dto.RewardsGranted);

            Assert.AreEqual(3, dto.Nodes.Count);
            Assert.AreEqual((int)NodeType.Boss, dto.Nodes[2].Type);
            Assert.IsTrue(dto.Nodes[0].Visited);
            Assert.AreEqual(1, dto.Nodes[1].Row);
            CollectionAssert.Contains(dto.Nodes[0].Edges, 1);

            Assert.AreEqual(2, dto.Party.Count);
            Assert.AreEqual("iron_sword", dto.Party[0].Equipment[0].equipmentId);
            Assert.AreEqual(1, dto.Recruits.Count);
        }

        [Test]
        public void FromDTO_ReconstructsGraphAndPosition()
        {
            var restored = RunState.FromDTO(MakeRun().ToDTO());

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.CurrentFloor);
            Assert.AreEqual(1, restored.CurrentNodeId);
            Assert.AreEqual(1234, restored.Seed);
            Assert.AreEqual(3, restored.Map.Count);
            Assert.AreEqual(NodeType.Boss, restored.Map.Get(2).Type);
            Assert.IsTrue(restored.Map.Get(0).Visited);
            CollectionAssert.Contains(restored.Map.Get(0).Edges, 1);
            Assert.AreEqual(2, restored.Party.Count);
            Assert.AreEqual("iron_sword", restored.Party[0].Equipment[0].equipmentId);
            Assert.AreEqual(1, restored.Recruits.Count);
        }

        [Test]
        public void Placements_SurviveRoundTrip()
        {
            var restored = RunState.FromDTO(MakeRun().ToDTO());
            Assert.AreEqual(1, restored.Placements.Count);
            Assert.IsTrue(restored.Placements.TryGetValue("u1", out var coord));
            Assert.AreEqual(new GridCoord(3, 1), coord);
        }

        [Test]
        public void FromDTO_Null_ReturnsNull() => Assert.IsNull(RunState.FromDTO(null));

        [Test]
        public void FromDTO_PreV2Save_ReturnsNull()
        {
            var dto = MakeRun().ToDTO();
            dto.SaveVersion = 1;   // legacy linear save
            Assert.IsNull(RunState.FromDTO(dto));
        }

        [Test]
        public void JsonRoundTrip_PreservesRunGraphAndEquipment()
        {
            var dto = MakeRun().ToDTO();
            var back = JsonConvert.DeserializeObject<RunStateDTO>(JsonConvert.SerializeObject(dto));

            Assert.AreEqual(dto.CurrentFloor, back.CurrentFloor);
            Assert.AreEqual(dto.CurrentNodeId, back.CurrentNodeId);
            Assert.AreEqual(dto.Seed, back.Seed);
            Assert.AreEqual(dto.Nodes.Count, back.Nodes.Count);
            CollectionAssert.AreEqual(dto.Nodes[0].Edges, back.Nodes[0].Edges);
            Assert.AreEqual("iron_sword", back.Party[0].Equipment[0].equipmentId);
        }

        [Test]
        public void RunStore_SaveLoadDelete_DiskRoundTrip()
        {
            var dto = MakeRun().ToDTO();
            RunStore.Save(dto);
            Assert.IsTrue(RunStore.Exists());

            var loaded = RunStore.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(dto.CurrentNodeId, loaded.CurrentNodeId);
            Assert.AreEqual("iron_sword", loaded.Party[0].Equipment[0].equipmentId);

            RunStore.Delete();
            Assert.IsFalse(RunStore.Exists());
            Assert.IsNull(RunStore.Load());
        }

        [Test]
        public void RunSession_SaveTryLoad_RoundTrips()
        {
            RunSession.Clear();
            Assert.IsFalse(RunSession.HasSavedRun);

            RunSession.StartNew(MakeRun());
            Assert.IsTrue(RunSession.HasSavedRun);

            RunSession.Current = null;        // simulate app restart
            Assert.IsTrue(RunSession.TryLoad());
            Assert.IsNotNull(RunSession.Current);
            Assert.AreEqual(1, RunSession.Current.CurrentNodeId);
            Assert.AreEqual("iron_sword", RunSession.Current.Party[0].Equipment[0].equipmentId);

            RunSession.Clear();
            Assert.IsFalse(RunSession.HasSavedRun);
        }

        [Test]
        public void Inventory_SurvivesRoundTrip()
        {
            // BTS-G: loose dropped items survive RunState -> RunStateDTO -> RunState (id + rolled tier/seed).
            var state = MakeRun();
            state.AddItem(new UnitEquipmentDTO(EquipmentSlot.Helmet, "iron_helm") { tier = 2, seed = 99, displayName = "Helm of Health" });
            state.AddItem(new UnitEquipmentDTO(EquipmentSlot.Chest, "leather_chest"));

            var restored = RunState.FromDTO(state.ToDTO());

            Assert.AreEqual(2, restored.Inventory.Count);
            Assert.AreEqual("iron_helm", restored.Inventory[0].equipmentId);
            Assert.AreEqual(EquipmentSlot.Helmet, restored.Inventory[0].slot);
            Assert.AreEqual(2, restored.Inventory[0].tier);
            Assert.AreEqual(99, restored.Inventory[0].seed);
            Assert.AreEqual("leather_chest", restored.Inventory[1].equipmentId);
        }

        [Test]
        public void Inventory_SurvivesJsonRoundTrip()
        {
            var state = MakeRun();
            state.AddItem(new UnitEquipmentDTO(EquipmentSlot.Legs, "greaves") { tier = 1 });

            var back = JsonConvert.DeserializeObject<RunStateDTO>(JsonConvert.SerializeObject(state.ToDTO()));

            Assert.AreEqual(1, back.Inventory.Count);
            Assert.AreEqual("greaves", back.Inventory[0].equipmentId);
            Assert.AreEqual(EquipmentSlot.Legs, back.Inventory[0].slot);
        }
    }
}
