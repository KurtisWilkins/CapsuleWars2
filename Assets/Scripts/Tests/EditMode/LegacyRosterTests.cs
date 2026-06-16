using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Roster cap / recruit / release logic on LegacyProfileDTO, the
    /// UnitDTO -> LegacyUnitDTO recruit mapping, the roguelike recruit-drop stub,
    /// and the RunState recruit pool. Pure data — no scene load.
    /// </summary>
    public class LegacyRosterTests
    {
        private static LegacyUnitDTO Unit(string id) => new LegacyUnitDTO(id, id, "def");

        [Test]
        public void NewProfile_DefaultCapIs100()
        {
            var p = new LegacyProfileDTO();
            Assert.AreEqual(100, p.EffectiveCap);
            Assert.IsFalse(p.IsAtCap);
        }

        [Test]
        public void TryAdd_AddsUnit_AndRejectsDuplicateId()
        {
            var p = new LegacyProfileDTO();
            Assert.IsTrue(p.TryAdd(Unit("u1")));
            Assert.AreEqual(1, p.Count);
            Assert.IsFalse(p.TryAdd(Unit("u1")));
            Assert.AreEqual(1, p.Count);
        }

        [Test]
        public void TryAdd_RejectsWhenAtCap()
        {
            var p = new LegacyProfileDTO { RosterCap = 2 };
            Assert.IsTrue(p.TryAdd(Unit("a")));
            Assert.IsTrue(p.TryAdd(Unit("b")));
            Assert.IsTrue(p.IsAtCap);
            Assert.IsFalse(p.TryAdd(Unit("c")));
            Assert.AreEqual(2, p.Count);
        }

        [Test]
        public void Release_FreesACapSlot()
        {
            var p = new LegacyProfileDTO { RosterCap = 1 };
            Assert.IsTrue(p.TryAdd(Unit("a")));
            Assert.IsFalse(p.TryAdd(Unit("b")));   // at cap
            Assert.IsTrue(p.Release("a"));
            Assert.IsTrue(p.TryAdd(Unit("b")));    // room now
            Assert.IsTrue(p.Contains("b"));
            Assert.IsFalse(p.Contains("a"));
        }

        [Test]
        public void Release_UnknownId_ReturnsFalse()
        {
            var p = new LegacyProfileDTO();
            Assert.IsFalse(p.Release("nope"));
        }

        [Test]
        public void EffectiveCap_GuardsZeroFromMalformedSave()
        {
            var p = new LegacyProfileDTO { RosterCap = 0 };
            Assert.AreEqual(100, p.EffectiveCap);
        }

        [Test]
        public void LegacyUnitDTO_FromUnit_MapsIdentity()
        {
            var dto = new UnitDTO("u9", "Rogue", "unit_sample");
            var legacy = LegacyUnitDTO.FromUnit(dto);
            Assert.AreEqual("u9", legacy.Id);
            Assert.AreEqual("Rogue", legacy.DisplayName);
            Assert.AreEqual("unit_sample", legacy.UnitDefinitionId);
            Assert.IsNotNull(legacy.Lifetime);
        }

        [Test]
        public void RecruitGenerator_ProducesUniqueIds_AndResolvableDefinition()
        {
            var a = RoguelikeRecruitGenerator.Generate(1, 0);
            var b = RoguelikeRecruitGenerator.Generate(1, 1);
            Assert.AreNotEqual(a.Id, b.Id);
            Assert.IsFalse(string.IsNullOrEmpty(a.DisplayName));
            Assert.AreEqual("unit_sample", a.UnitDefinitionId);
        }

        [Test]
        public void RunState_RecruitPool_AddsAndRemoves()
        {
            var state = new RunState(MapGenerator.Generate(2));
            var u = new UnitDTO("r1", "R", "unit_sample");
            state.AddRecruit(u);
            Assert.AreEqual(1, state.Recruits.Count);
            state.RemoveRecruit(u);
            Assert.AreEqual(0, state.Recruits.Count);
        }
    }
}
