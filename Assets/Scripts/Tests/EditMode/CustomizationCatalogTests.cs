using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// PartCatalog_SO + CustomizationUnlocks: starter seeding, unlock-by-catalog-cost,
    /// and ownership/slot filtering. Parts are built via reflection (private ids).
    /// </summary>
    public class CustomizationCatalogTests
    {
        private static BodyPart_SO MakePart(string id, PartSlot slot)
        {
            var p = ScriptableObject.CreateInstance<BodyPart_SO>();
            typeof(BodyPart_SO).GetField("partId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, id);
            typeof(BodyPart_SO).GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, slot);
            return p;
        }

        private static PartCatalog_SO MakeCatalog(List<PartCatalog_SO.PartEntry> parts)
        {
            var c = ScriptableObject.CreateInstance<PartCatalog_SO>();
            typeof(PartCatalog_SO).GetField("parts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, parts);
            return c;
        }

        [Test]
        public void SeedStarters_GrantsOnlyStarters()
        {
            var starter = MakePart("body_basic", PartSlot.Body);
            var locked = MakePart("hand_claw", PartSlot.LeftHand);
            var catalog = MakeCatalog(new List<PartCatalog_SO.PartEntry>
            {
                new PartCatalog_SO.PartEntry { part = starter, cost = 3, starter = true },
                new PartCatalog_SO.PartEntry { part = locked, cost = 2, starter = false },
            });
            var profile = new PlayerProfileDTO();

            CustomizationUnlocks.SeedStarters(catalog, profile);

            Assert.IsTrue(profile.HasPart("body_basic"));
            Assert.IsFalse(profile.HasPart("hand_claw"));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(starter);
            Object.DestroyImmediate(locked);
        }

        [Test]
        public void TryUnlockPart_SpendsCatalogCost()
        {
            var part = MakePart("hand_claw", PartSlot.LeftHand);
            var catalog = MakeCatalog(new List<PartCatalog_SO.PartEntry>
            {
                new PartCatalog_SO.PartEntry { part = part, cost = 2, starter = false },
            });
            var profile = new PlayerProfileDTO();
            profile.AddPoints(5);

            Assert.IsTrue(CustomizationUnlocks.TryUnlockPart(catalog, profile, "hand_claw"));
            Assert.AreEqual(3, profile.UnlockPoints);
            Assert.IsTrue(profile.HasPart("hand_claw"));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(part);
        }

        [Test]
        public void TryUnlockPart_UnknownId_Fails()
        {
            var catalog = MakeCatalog(new List<PartCatalog_SO.PartEntry>());
            var profile = new PlayerProfileDTO();
            profile.AddPoints(10);

            Assert.IsFalse(CustomizationUnlocks.TryUnlockPart(catalog, profile, "nope"));
            Assert.AreEqual(10, profile.UnlockPoints);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void UnlockedPartsForSlot_FiltersByOwnershipAndSlot()
        {
            var owned = MakePart("hand_a", PartSlot.LeftHand);
            var notOwned = MakePart("hand_b", PartSlot.LeftHand);
            var otherSlot = MakePart("foot_a", PartSlot.LeftFoot);
            var catalog = MakeCatalog(new List<PartCatalog_SO.PartEntry>
            {
                new PartCatalog_SO.PartEntry { part = owned },
                new PartCatalog_SO.PartEntry { part = notOwned },
                new PartCatalog_SO.PartEntry { part = otherSlot },
            });
            var profile = new PlayerProfileDTO();
            profile.GrantPart("hand_a");
            profile.GrantPart("foot_a");

            var hands = CustomizationUnlocks.UnlockedPartsForSlot(catalog, profile, PartSlot.LeftHand);
            Assert.AreEqual(1, hands.Count);
            Assert.AreSame(owned, hands[0]);

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(owned);
            Object.DestroyImmediate(notOwned);
            Object.DestroyImmediate(otherSlot);
        }
    }
}
