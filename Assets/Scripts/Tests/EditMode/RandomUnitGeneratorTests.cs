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
    /// RandomUnitGenerator draws units from the player's unlocked pool only,
    /// reproducibly under a seeded RNG.
    /// </summary>
    public class RandomUnitGeneratorTests
    {
        private static BodyPart_SO MakePart(string id, PartSlot slot)
        {
            var p = ScriptableObject.CreateInstance<BodyPart_SO>();
            typeof(BodyPart_SO).GetField("partId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, id);
            typeof(BodyPart_SO).GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, slot);
            return p;
        }

        private static Palette_SO MakePalette(string id)
        {
            var p = ScriptableObject.CreateInstance<Palette_SO>();
            typeof(Palette_SO).GetField("paletteId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, id);
            return p;
        }

        private static PartCatalog_SO MakeCatalog(List<PartCatalog_SO.PartEntry> parts, List<PartCatalog_SO.PaletteEntry> palettes)
        {
            var c = ScriptableObject.CreateInstance<PartCatalog_SO>();
            typeof(PartCatalog_SO).GetField("parts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, parts);
            typeof(PartCatalog_SO).GetField("palettes", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, palettes);
            return c;
        }

        private static (PartCatalog_SO catalog, Object[] cleanup) BuildCatalog()
        {
            var bodyOwned = MakePart("body_a", PartSlot.Body);
            var handOwned = MakePart("hand_a", PartSlot.LeftHand);
            var handLocked = MakePart("hand_b", PartSlot.LeftHand);
            var pal = MakePalette("pal_a");
            var catalog = MakeCatalog(
                new List<PartCatalog_SO.PartEntry>
                {
                    new PartCatalog_SO.PartEntry { part = bodyOwned },
                    new PartCatalog_SO.PartEntry { part = handOwned },
                    new PartCatalog_SO.PartEntry { part = handLocked },
                },
                new List<PartCatalog_SO.PaletteEntry>
                {
                    new PartCatalog_SO.PaletteEntry { palette = pal },
                });
            return (catalog, new Object[] { catalog, bodyOwned, handOwned, handLocked, pal });
        }

        private static void Cleanup(Object[] objs)
        {
            foreach (var o in objs) if (o != null) Object.DestroyImmediate(o);
        }

        [Test]
        public void Generate_DrawsOnlyOwnedParts_AndPalette()
        {
            var (catalog, cleanup) = BuildCatalog();
            var profile = new PlayerProfileDTO();
            profile.GrantPart("body_a");
            profile.GrantPart("hand_a");   // hand_b deliberately NOT owned
            profile.GrantPalette("pal_a");

            var dto = RandomUnitGenerator.Generate(catalog, profile, new System.Random(1), 1, 0);

            foreach (var p in dto.Parts) Assert.IsTrue(profile.HasPart(p.partId), $"unowned part {p.partId}");
            Assert.IsTrue(dto.Parts.Exists(p => p.slot == PartSlot.Body && p.partId == "body_a"));
            Assert.IsTrue(dto.Parts.Exists(p => p.slot == PartSlot.LeftHand && p.partId == "hand_a"));
            Assert.IsFalse(dto.Parts.Exists(p => p.partId == "hand_b"));
            Assert.AreEqual("pal_a", dto.PaletteId);

            Cleanup(cleanup);
        }

        [Test]
        public void Generate_IsDeterministicForSameSeed()
        {
            var (catalog, cleanup) = BuildCatalog();
            var profile = new PlayerProfileDTO();
            profile.GrantPart("body_a");
            profile.GrantPart("hand_a");
            profile.GrantPalette("pal_a");

            var a = RandomUnitGenerator.Generate(catalog, profile, new System.Random(42), 2, 3);
            var b = RandomUnitGenerator.Generate(catalog, profile, new System.Random(42), 2, 3);

            Assert.AreEqual(a.Id, b.Id);
            Assert.AreEqual(a.Parts.Count, b.Parts.Count);
            for (int i = 0; i < a.Parts.Count; i++)
            {
                Assert.AreEqual(a.Parts[i].slot, b.Parts[i].slot);
                Assert.AreEqual(a.Parts[i].partId, b.Parts[i].partId);
            }
            Assert.AreEqual(a.PaletteId, b.PaletteId);

            Cleanup(cleanup);
        }

        [Test]
        public void Generate_EmptyProfile_ProducesNoParts()
        {
            var (catalog, cleanup) = BuildCatalog();
            var profile = new PlayerProfileDTO();   // owns nothing

            var dto = RandomUnitGenerator.Generate(catalog, profile, new System.Random(1), 1, 0);

            Assert.AreEqual(0, dto.Parts.Count);
            Assert.IsTrue(string.IsNullOrEmpty(dto.PaletteId));
            Assert.AreEqual("rogue_1_0", dto.Id);

            Cleanup(cleanup);
        }
    }
}
