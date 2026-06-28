using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Abilities;
using CapsuleWars.Data.Classes;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// BTS-F part 2: the class→ability registry resolves a class's kit (by reference + by stable ClassId), and
    /// AbilityController.SetAbilities replaces + rebuilds its runtimes. Pure/EditMode — the live in-combat casting
    /// of those abilities is Play-gated.
    /// </summary>
    public class ClassAbilityLoadoutTests
    {
        private static void SetField(object t, string f, object v) =>
            t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, v);

        [Test]
        public void AbilitiesFor_ResolvesByReference_AndFallsBackToClassId()
        {
            var archer = ScriptableObject.CreateInstance<UnitClass_SO>();
            SetField(archer, "classId", "archer");
            var monk = ScriptableObject.CreateInstance<UnitClass_SO>();
            SetField(monk, "classId", "monk");

            var shot = ScriptableObject.CreateInstance<Ability_SO>();
            var volley = ScriptableObject.CreateInstance<Ability_SO>();

            var set = ScriptableObject.CreateInstance<ClassAbilitySet_SO>();
            SetField(set, "entries", new List<ClassAbilitySet_SO.Entry>
            {
                new ClassAbilitySet_SO.Entry { unitClass = archer, abilities = new[] { shot, volley } },
            });

            var kit = set.AbilitiesFor(archer);
            Assert.IsNotNull(kit);
            CollectionAssert.AreEquivalent(new[] { shot, volley }, kit, "by reference");

            // a DIFFERENT UnitClass_SO instance with the same ClassId still resolves (churn-resistant)
            var archerClone = ScriptableObject.CreateInstance<UnitClass_SO>();
            SetField(archerClone, "classId", "archer");
            Assert.AreSame(kit, set.AbilitiesFor(archerClone), "falls back to ClassId when the reference differs");

            Assert.IsNull(set.AbilitiesFor(monk), "class not in the set → null");
            Assert.IsNull(set.AbilitiesFor((UnitClass_SO)null));
            Assert.IsNull(set.AbilitiesFor("nope"));

            foreach (var o in new Object[] { archer, monk, shot, volley, set, archerClone }) Object.DestroyImmediate(o);
        }

        [Test]
        public void AbilityController_SetAbilities_ReplacesAndRebuildsRuntimes()
        {
            var go = new GameObject("u");
            var controller = go.AddComponent<AbilityController>();   // Awake builds from empty → 0 runtimes

            var a1 = ScriptableObject.CreateInstance<Ability_SO>();
            var a2 = ScriptableObject.CreateInstance<Ability_SO>();

            controller.SetAbilities(new[] { a1, a2 });
            Assert.AreEqual(2, controller.Runtimes.Count, "installs the kit (one runtime per ability)");

            controller.SetAbilities(new[] { a1 });
            Assert.AreEqual(1, controller.Runtimes.Count, "replaces, does not append");

            controller.SetAbilities(null);
            Assert.AreEqual(0, controller.Runtimes.Count, "null clears");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(a1);
            Object.DestroyImmediate(a2);
        }
    }
}
