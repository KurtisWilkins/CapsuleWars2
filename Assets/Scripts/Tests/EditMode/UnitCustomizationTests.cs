using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Units.Customization;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// UnitCustomization records the parts it applies (AppliedParts) so the paper-doll
    /// customization screen can read the live body loadout back, edit it incrementally,
    /// and capture it for persistence. Re-applying a smaller set is the "unequip" path.
    /// (Paper-doll customization rework — extends ADR-012.)
    /// </summary>
    public class UnitCustomizationTests
    {
        private static BodyPart_SO MakePart(string id, PartSlot slot)
        {
            var p = ScriptableObject.CreateInstance<BodyPart_SO>();
            typeof(BodyPart_SO).GetField("partId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, id);
            typeof(BodyPart_SO).GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, slot);
            return p;
        }

        private static BodyPart_SO FindPart(IReadOnlyList<PartAssignment> set, PartSlot slot)
        {
            for (int i = 0; i < set.Count; i++)
                if (set[i].slot == slot) return set[i].part;
            return null;
        }

        [Test]
        public void ApplyParts_RecordsAppliedAssignments_ForReadback()
        {
            var go = new GameObject("u");
            var custom = go.AddComponent<UnitCustomization>();
            var bodyA = MakePart("body_a", PartSlot.Body);
            var headA = MakePart("head_a", PartSlot.HeadProp);

            custom.ApplyParts(new List<PartAssignment>
            {
                new PartAssignment { slot = PartSlot.Body, part = bodyA },
                new PartAssignment { slot = PartSlot.HeadProp, part = headA },
            }, null);

            Assert.AreEqual(2, custom.AppliedParts.Count);
            Assert.AreSame(bodyA, FindPart(custom.AppliedParts, PartSlot.Body));
            Assert.AreSame(headA, FindPart(custom.AppliedParts, PartSlot.HeadProp));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(bodyA);
            Object.DestroyImmediate(headA);
        }

        [Test]
        public void ApplyParts_ReapplyingSmallerSet_Replaces_NotAppends()
        {
            var go = new GameObject("u");
            var custom = go.AddComponent<UnitCustomization>();
            var bodyA = MakePart("body_a", PartSlot.Body);
            var headA = MakePart("head_a", PartSlot.HeadProp);

            custom.ApplyParts(new List<PartAssignment>
            {
                new PartAssignment { slot = PartSlot.Body, part = bodyA },
                new PartAssignment { slot = PartSlot.HeadProp, part = headA },
            }, null);

            // Drop the head prop — the "unequip a cosmetic slot" path.
            custom.ApplyParts(new List<PartAssignment>
            {
                new PartAssignment { slot = PartSlot.Body, part = bodyA },
            }, null);

            Assert.AreEqual(1, custom.AppliedParts.Count);
            Assert.AreSame(bodyA, FindPart(custom.AppliedParts, PartSlot.Body));
            Assert.IsNull(FindPart(custom.AppliedParts, PartSlot.HeadProp));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(bodyA);
            Object.DestroyImmediate(headA);
        }

        [Test]
        public void ApplyParts_SkipsNullParts_InReadback()
        {
            var go = new GameObject("u");
            var custom = go.AddComponent<UnitCustomization>();
            var bodyA = MakePart("body_a", PartSlot.Body);

            custom.ApplyParts(new List<PartAssignment>
            {
                new PartAssignment { slot = PartSlot.Body, part = bodyA },
                new PartAssignment { slot = PartSlot.HeadProp, part = null },
            }, null);

            Assert.AreEqual(1, custom.AppliedParts.Count, "null-part assignments should not be recorded for capture");
            Assert.AreSame(bodyA, FindPart(custom.AppliedParts, PartSlot.Body));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(bodyA);
        }
    }
}
