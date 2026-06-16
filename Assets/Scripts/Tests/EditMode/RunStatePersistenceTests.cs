using System.Collections.Generic;
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
    /// Round-trip tests for run-state persistence: RunState &lt;-&gt; RunStateDTO
    /// mapping, JSON serialization, and the RunStore disk save/load. Verifies
    /// run-scoped party equipment survives the trip.
    /// </summary>
    public class RunStatePersistenceTests
    {
        [TearDown]
        public void Teardown()
        {
            RunStore.Delete();   // never leave a run.json behind
        }

        private static RunState MakeRun()
        {
            var nodes = new List<MapNode>
            {
                new MapNode(0, NodeType.Combat, "Fight"),
                new MapNode(1, NodeType.Shop, "Shop"),
                new MapNode(2, NodeType.Boss, "Boss"),
            };
            var state = new RunState(new RunMap(nodes), 50)
            {
                IsBossEncounter = true,
                RewardsGranted = true,
            };
            state.AdvanceNode();   // CurrentFloor 0 -> 1, marks node 0 visited

            var p1 = new UnitDTO("u1", "Conan", "warrior_01");
            p1.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "iron_sword"));
            var p2 = new UnitDTO("u2", "Merlin", "mage_01");
            state.SetParty(new[] { p1, p2 });
            state.AddRecruit(new UnitDTO("r1", "Recruit", "rogue_01"));
            return state;
        }

        [Test]
        public void ToDTO_CapturesAllFields_AndPartyEquipment()
        {
            var dto = MakeRun().ToDTO();

            Assert.AreEqual(1, dto.SaveVersion);
            Assert.AreEqual(1, dto.CurrentFloor);
            Assert.AreEqual(50, dto.Gold);
            Assert.IsTrue(dto.IsBossEncounter);
            Assert.IsTrue(dto.RewardsGranted);

            Assert.AreEqual(3, dto.Nodes.Count);
            Assert.AreEqual((int)NodeType.Boss, dto.Nodes[2].Type);
            Assert.IsTrue(dto.Nodes[0].Visited);

            Assert.AreEqual(2, dto.Party.Count);
            Assert.AreEqual(1, dto.Party[0].Equipment.Count);
            Assert.AreEqual("iron_sword", dto.Party[0].Equipment[0].equipmentId);
            Assert.AreEqual(1, dto.Recruits.Count);
        }

        [Test]
        public void FromDTO_ReconstructsRun()
        {
            var restored = RunState.FromDTO(MakeRun().ToDTO());

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.CurrentFloor);
            Assert.AreEqual(50, restored.Gold);
            Assert.IsTrue(restored.IsBossEncounter);
            Assert.AreEqual(3, restored.Map.Count);
            Assert.AreEqual(NodeType.Boss, restored.Map.Get(2).Type);
            Assert.IsTrue(restored.Map.Get(0).Visited);
            Assert.AreEqual(2, restored.Party.Count);
            Assert.AreEqual("iron_sword", restored.Party[0].Equipment[0].equipmentId);
            Assert.AreEqual(1, restored.Recruits.Count);
        }

        [Test]
        public void FromDTO_Null_ReturnsNull()
        {
            Assert.IsNull(RunState.FromDTO(null));
        }

        [Test]
        public void JsonRoundTrip_PreservesRunAndEquipment()
        {
            var dto = MakeRun().ToDTO();
            var json = JsonConvert.SerializeObject(dto);
            var back = JsonConvert.DeserializeObject<RunStateDTO>(json);

            Assert.AreEqual(dto.CurrentFloor, back.CurrentFloor);
            Assert.AreEqual(dto.Gold, back.Gold);
            Assert.AreEqual(dto.Nodes.Count, back.Nodes.Count);
            Assert.AreEqual(dto.Party.Count, back.Party.Count);
            Assert.AreEqual("iron_sword", back.Party[0].Equipment[0].equipmentId);
        }

        [Test]
        public void RunStore_SaveLoad_DiskRoundTrip()
        {
            var dto = MakeRun().ToDTO();

            RunStore.Save(dto);
            Assert.IsTrue(RunStore.Exists());

            var loaded = RunStore.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(dto.CurrentFloor, loaded.CurrentFloor);
            Assert.AreEqual(dto.Party.Count, loaded.Party.Count);
            Assert.AreEqual("iron_sword", loaded.Party[0].Equipment[0].equipmentId);
        }

        [Test]
        public void RunStore_Delete_RemovesSave()
        {
            RunStore.Save(MakeRun().ToDTO());
            Assert.IsTrue(RunStore.Exists());

            RunStore.Delete();
            Assert.IsFalse(RunStore.Exists());
            Assert.IsNull(RunStore.Load());
        }

        [Test]
        public void RunSession_SaveTryLoad_RoundTrips()
        {
            RunSession.Clear();
            Assert.IsFalse(RunSession.HasSavedRun);

            RunSession.StartNew(MakeRun());   // StartNew persists
            Assert.IsTrue(RunSession.HasSavedRun);

            RunSession.Current = null;        // simulate app restart (in-memory gone)
            Assert.IsTrue(RunSession.TryLoad());
            Assert.IsNotNull(RunSession.Current);
            Assert.AreEqual(1, RunSession.Current.CurrentFloor);
            Assert.AreEqual("iron_sword", RunSession.Current.Party[0].Equipment[0].equipmentId);

            RunSession.Clear();
            Assert.IsFalse(RunSession.HasSavedRun);
        }
    }
}
