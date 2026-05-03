using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests damage math + downed-flag transitions on UnitHealthController
    /// without loading a scene. Each test builds a throwaway GameObject with
    /// UnitStatusController + UnitHealthController and tears it down after.
    /// </summary>
    public class HealthControllerTests
    {
        private GameObject go;
        private UnitStatusController status;
        private UnitHealthController health;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            status = go.AddComponent<UnitStatusController>();
            health = go.AddComponent<UnitHealthController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void StartsAtMaxHp()
        {
            Assert.AreEqual(status.MaxHp, health.CurrentHp);
            Assert.IsFalse(health.IsDowned);
        }

        [Test]
        public void TakeDamage_ReducesHp()
        {
            int before = health.CurrentHp;
            health.TakeDamage(10, null);
            Assert.AreEqual(before - 10, health.CurrentHp);
            Assert.IsFalse(health.IsDowned);
        }

        [Test]
        public void TakeDamage_AlwaysDealsAtLeastOne()
        {
            int before = health.CurrentHp;
            health.TakeDamage(0, null);
            Assert.AreEqual(before - 1, health.CurrentHp);

            health.TakeDamage(-50, null);
            Assert.AreEqual(before - 2, health.CurrentHp);
        }

        [Test]
        public void TakeDamage_ClampsAtZero_AndFiresOnDowned()
        {
            int downedFireCount = 0;
            health.OnDowned += _ => downedFireCount++;

            health.TakeDamage(99999, null);
            Assert.AreEqual(0, health.CurrentHp);
            Assert.IsTrue(health.IsDowned);
            Assert.AreEqual(1, downedFireCount);
        }

        [Test]
        public void TakeDamage_AfterDowned_DoesNothing()
        {
            int downedFireCount = 0;
            int damageFireCount = 0;
            health.OnDowned += _ => downedFireCount++;
            health.OnDamageTaken += _ => damageFireCount++;

            health.TakeDamage(99999, null);
            health.TakeDamage(10, null);
            health.TakeDamage(10, null);

            Assert.AreEqual(0, health.CurrentHp);
            Assert.AreEqual(1, downedFireCount, "OnDowned must fire exactly once");
            Assert.AreEqual(1, damageFireCount, "OnDamageTaken must not fire after downed");
        }

        [Test]
        public void OnDamageTaken_FiresWithCorrectAmount()
        {
            int observed = 0;
            health.OnDamageTaken += e => observed = e.Amount;

            health.TakeDamage(7, null);
            Assert.AreEqual(7, observed);
        }

        [Test]
        public void OnHealthChanged_FiresWithCurrentHp()
        {
            int last = -1;
            health.OnHealthChanged += hp => last = hp;

            health.TakeDamage(15, null);
            Assert.AreEqual(health.CurrentHp, last);
        }

        [Test]
        public void RestoreToPercent_SetsHpAndClearsDowned()
        {
            health.TakeDamage(99999, null);
            Assert.IsTrue(health.IsDowned);

            health.RestoreToPercent(0.5f);
            Assert.AreEqual(Mathf.RoundToInt(status.MaxHp * 0.5f), health.CurrentHp);
            Assert.IsFalse(health.IsDowned);
        }

        [Test]
        public void RestoreToPercent_ClampsArguments()
        {
            health.RestoreToPercent(2f);
            Assert.AreEqual(status.MaxHp, health.CurrentHp);

            health.RestoreToPercent(-1f);
            Assert.AreEqual(1, health.CurrentHp); // never less than 1 alive
        }
    }
}
