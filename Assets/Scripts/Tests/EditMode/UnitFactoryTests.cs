using System.Reflection;
using System.Text.RegularExpressions;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Round-trip + guard tests for the UnitDTO &lt;-&gt; runtime conversion layer.
    /// Each test builds a throwaway unit GameObject (UnitRoot + UnitCustomization)
    /// and resolves UnitDefinition_SO through an in-memory UnitDefinitionDatabase.
    /// </summary>
    public class UnitFactoryTests
    {
        private GameObject go;
        private UnitRoot root;
        private UnitCustomization custom;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            root = go.AddComponent<UnitRoot>();
            custom = go.AddComponent<UnitCustomization>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        /// <summary>UnitDefinition_SO.unitId is a private [SerializeField]; set it via reflection for tests.</summary>
        private static UnitDefinition_SO MakeDefinition(string unitId)
        {
            var def = ScriptableObject.CreateInstance<UnitDefinition_SO>();
            typeof(UnitDefinition_SO)
                .GetField("unitId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(def, unitId);
            return def;
        }

        [Test]
        public void ApplyTo_SetsIdentity_AndAppliesDefinition()
        {
            var def = MakeDefinition("warrior_01");
            var db = new UnitDefinitionDatabase(new[] { def });
            var dto = new UnitDTO("u1", "Conan", "warrior_01");

            UnitFactory.FromDTO(dto, root, db);

            Assert.AreEqual("u1", root.UnitId);
            Assert.AreEqual("Conan", root.DisplayName);
            Assert.AreSame(def, custom.Definition);

            Object.DestroyImmediate(def);
        }

        [Test]
        public void ToDTO_CapturesIdentity_AndDefinitionId()
        {
            var def = MakeDefinition("mage_07");
            root.SetIdentity("u2", "Merlin");
            custom.Apply(def);

            var dto = UnitFactory.FromUnit(root);

            Assert.AreEqual("u2", dto.Id);
            Assert.AreEqual("Merlin", dto.DisplayName);
            Assert.AreEqual("mage_07", dto.UnitDefinitionId);
            Assert.AreEqual(1, dto.SaveVersion);

            Object.DestroyImmediate(def);
        }

        [Test]
        public void RoundTrip_PreservesIdentityAndDefinition()
        {
            var def = MakeDefinition("rogue_03");
            var db = new UnitDefinitionDatabase(new[] { def });
            var original = new UnitDTO("u3", "Robin", "rogue_03");

            UnitFactory.FromDTO(original, root, db);
            var result = UnitFactory.FromUnit(root);

            Assert.AreEqual(original.Id, result.Id);
            Assert.AreEqual(original.DisplayName, result.DisplayName);
            Assert.AreEqual(original.UnitDefinitionId, result.UnitDefinitionId);

            Object.DestroyImmediate(def);
        }

        [Test]
        public void RoundTrip_DtoToUnitToDto_ReproducesEqualDto()
        {
            var def = MakeDefinition("paladin_02");
            var db = new UnitDefinitionDatabase(new[] { def });
            var original = new UnitDTO("u7", "Joan", "paladin_02");

            // DTO -> Unit -> DTO
            UnitFactory.FromDTO(original, root, db);
            var result = UnitFactory.FromUnit(root);

            // Whole-DTO value equality over all serialized fields (UnitDTO.Equals),
            // not just field-by-field — the round trip is lossless for this slice.
            Assert.AreEqual(original, result);
            Assert.AreEqual(original.GetHashCode(), result.GetHashCode());

            Object.DestroyImmediate(def);
        }

        [Test]
        public void ApplyTo_UnknownDefinitionId_SetsIdentity_LeavesVisuals()
        {
            var db = new UnitDefinitionDatabase(); // empty
            var dto = new UnitDTO("u4", "Nameless", "does_not_exist");

            LogAssert.Expect(LogType.Warning, new Regex("does_not_exist"));
            UnitFactory.FromDTO(dto, root, db);

            Assert.AreEqual("u4", root.UnitId);
            Assert.IsNull(custom.Definition);
        }

        [Test]
        public void ApplyTo_NullDatabase_StillSetsIdentity()
        {
            var dto = new UnitDTO("u5", "Solo", "warrior_01");

            Assert.DoesNotThrow(() => UnitFactory.FromDTO(dto, root, null));
            Assert.AreEqual("u5", root.UnitId);
            Assert.IsNull(custom.Definition);
        }

        [Test]
        public void ApplyTo_NullArgs_DoNotThrow()
        {
            var db = new UnitDefinitionDatabase();
            Assert.DoesNotThrow(() => UnitFactory.FromDTO(null, root, db));
            Assert.DoesNotThrow(() => UnitFactory.FromDTO(new UnitDTO("x", "y", null), null, db));
        }

        [Test]
        public void ToDTO_NullUnit_ReturnsNull()
        {
            Assert.IsNull(UnitFactory.FromUnit(null));
        }

        [Test]
        public void ToDTO_NoDefinitionApplied_LeavesDefinitionIdNull()
        {
            root.SetIdentity("u6", "Bare");
            var dto = UnitFactory.FromUnit(root);

            Assert.AreEqual("u6", dto.Id);
            Assert.IsNull(dto.UnitDefinitionId);
        }

        [Test]
        public void UnitDefinitionDatabase_ResolvesById_AndGuardsBadInput()
        {
            var def = MakeDefinition("abc");
            var db = new UnitDefinitionDatabase();
            db.Register(def);
            db.Register(null);                    // ignored
            db.Register(MakeDefinition(string.Empty)); // ignored (no id)

            Assert.AreSame(def, db.GetUnitDefinition("abc"));
            Assert.IsNull(db.GetUnitDefinition("nope"));
            Assert.IsNull(db.GetUnitDefinition(null));
            Assert.IsNull(db.GetUnitDefinition(string.Empty));

            Object.DestroyImmediate(def);
        }
    }
}
