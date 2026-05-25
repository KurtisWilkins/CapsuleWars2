using CapsuleWars.Persistence.Dto;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Round-trip JSON serialization and lifetime merge math for legacy DTOs.
    /// Does not write to disk (LegacyStore file I/O is a Unity-side integration concern).
    /// </summary>
    public class LegacyPersistenceTests
    {
        [Test]
        public void LegacyProfile_RoundTripsThroughJson()
        {
            var profile = new LegacyProfileDTO();
            profile.Units.Add(new LegacyUnitDTO("player_01", "Hero")
            {
                Lifetime = { Kills = 5, DamageDealt = 200, BattlesParticipated = 3 }
            });
            profile.Units.Add(new LegacyUnitDTO("player_02", "Sidekick"));

            var json = JsonConvert.SerializeObject(profile);
            var restored = JsonConvert.DeserializeObject<LegacyProfileDTO>(json);

            Assert.AreEqual(2, restored.Units.Count);
            Assert.AreEqual("player_01", restored.Units[0].Id);
            Assert.AreEqual("Hero", restored.Units[0].DisplayName);
            Assert.AreEqual(5, restored.Units[0].Lifetime.Kills);
            Assert.AreEqual(200, restored.Units[0].Lifetime.DamageDealt);
            Assert.AreEqual(3, restored.Units[0].Lifetime.BattlesParticipated);
        }

        [Test]
        public void FindById_ReturnsMatchingUnit()
        {
            var profile = new LegacyProfileDTO();
            profile.Units.Add(new LegacyUnitDTO("a", "Alpha"));
            profile.Units.Add(new LegacyUnitDTO("b", "Beta"));

            Assert.AreEqual("Beta", profile.FindById("b").DisplayName);
            Assert.IsNull(profile.FindById("nope"));
            Assert.IsNull(profile.FindById(null));
            Assert.IsNull(profile.FindById(""));
        }

        [Test]
        public void MergeBattle_AccumulatesStats()
        {
            var stats = new LifetimeStatsDTO();
            stats.MergeBattle(damageDealt: 50, damageTaken: 20, kills: 1, fainted: false);
            stats.MergeBattle(damageDealt: 75, damageTaken: 30, kills: 2, fainted: true);

            Assert.AreEqual(125, stats.DamageDealt);
            Assert.AreEqual(50, stats.DamageTaken);
            Assert.AreEqual(3, stats.Kills);
            Assert.AreEqual(1, stats.Faints);
            Assert.AreEqual(2, stats.BattlesParticipated);
        }

        [Test]
        public void MergeBattle_DoesNotIncrementFaintsWhenFlagFalse()
        {
            var stats = new LifetimeStatsDTO();
            stats.MergeBattle(0, 0, 0, fainted: false);
            stats.MergeBattle(0, 0, 0, fainted: false);
            Assert.AreEqual(0, stats.Faints);
            Assert.AreEqual(2, stats.BattlesParticipated);
        }

        [Test]
        public void SaveVersion_PreservedThroughRoundTrip()
        {
            var profile = new LegacyProfileDTO { SaveVersion = 7 };
            var json = JsonConvert.SerializeObject(profile);
            var restored = JsonConvert.DeserializeObject<LegacyProfileDTO>(json);
            Assert.AreEqual(7, restored.SaveVersion);
        }

        [Test]
        public void NewProfile_HasEmptyUnitsList()
        {
            var profile = new LegacyProfileDTO();
            Assert.IsNotNull(profile.Units);
            Assert.AreEqual(0, profile.Units.Count);
            Assert.AreEqual(1, profile.SaveVersion);
        }
    }
}
