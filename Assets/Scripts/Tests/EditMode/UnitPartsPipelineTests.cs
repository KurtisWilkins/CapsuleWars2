using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Parts pipeline (M9 stage 3b): UnitDTO.Parts persist through LegacyUnitDTO,
    /// PartCatalog resolves ids, and UnitFactory.FromDTO prefers explicit parts
    /// over a whole-unit definition when a part database is supplied.
    /// </summary>
    public class UnitPartsPipelineTests
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

        private static UnitDefinition_SO MakeDef(string id)
        {
            var d = ScriptableObject.CreateInstance<UnitDefinition_SO>();
            typeof(UnitDefinition_SO).GetField("unitId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, id);
            return d;
        }

        private static PartCatalog_SO MakeCatalog(List<PartCatalog_SO.PartEntry> parts, List<PartCatalog_SO.PaletteEntry> palettes)
        {
            var c = ScriptableObject.CreateInstance<PartCatalog_SO>();
            typeof(PartCatalog_SO).GetField("parts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, parts);
            typeof(PartCatalog_SO).GetField("palettes", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, palettes);
            return c;
        }

        [Test]
        public void Parts_RoundTrip_ThroughLegacyUnit()
        {
            var dto = new UnitDTO("u1", "Rogue", null);
            dto.Parts.Add(new UnitPartDTO(PartSlot.Body, "body_a"));
            dto.Parts.Add(new UnitPartDTO(PartSlot.LeftHand, "hand_a"));
            dto.PaletteId = "pal_a";

            var legacy = LegacyUnitDTO.FromUnit(dto);
            var back = UnitDTO.FromLegacy(legacy);

            Assert.AreEqual(2, back.Parts.Count);
            Assert.AreEqual(PartSlot.Body, back.Parts[0].slot);
            Assert.AreEqual("body_a", back.Parts[0].partId);
            Assert.AreEqual("hand_a", back.Parts[1].partId);
            Assert.AreEqual("pal_a", back.PaletteId);
        }

        [Test]
        public void Catalog_ResolvesPartAndPalette_ById()
        {
            var part = MakePart("body_a", PartSlot.Body);
            var pal = MakePalette("pal_a");
            var catalog = MakeCatalog(
                new List<PartCatalog_SO.PartEntry> { new PartCatalog_SO.PartEntry { part = part } },
                new List<PartCatalog_SO.PaletteEntry> { new PartCatalog_SO.PaletteEntry { palette = pal } });

            Assert.AreSame(part, catalog.GetPart("body_a"));
            Assert.AreSame(pal, catalog.GetPalette("pal_a"));
            Assert.IsNull(catalog.GetPart("nope"));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(part);
            Object.DestroyImmediate(pal);
        }

        [Test]
        public void FromDTO_WithParts_PrefersPartsOverDefinition()
        {
            var go = new GameObject("u");
            var root = go.AddComponent<UnitRoot>();
            var custom = go.AddComponent<UnitCustomization>();

            var part = MakePart("body_a", PartSlot.Body);
            var catalog = MakeCatalog(
                new List<PartCatalog_SO.PartEntry> { new PartCatalog_SO.PartEntry { part = part } },
                new List<PartCatalog_SO.PaletteEntry>());
            var def = MakeDef("def1");
            var defDb = new UnitDefinitionDatabase(new[] { def });

            var dto = new UnitDTO("u9", "Gen", "def1");   // has BOTH a definition id and parts
            dto.Parts.Add(new UnitPartDTO(PartSlot.Body, "body_a"));

            UnitFactory.FromDTO(dto, root, defDb, catalog);

            Assert.AreEqual("u9", root.UnitId);
            // Parts path was taken, so the definition was NOT applied.
            Assert.IsNull(custom.Definition, "explicit parts should take precedence over the definition");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(part);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(def);
        }
    }
}
