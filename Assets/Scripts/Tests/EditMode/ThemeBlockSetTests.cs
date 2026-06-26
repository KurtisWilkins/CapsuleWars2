using CapsuleWars.Data.Arena;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Slice B — the theme block set's role → block mapping + primitive-cube fallback (so the arena builds
    /// with no authored assets) and the thin-floor / raised-obstacle default invariant.
    /// </summary>
    public class ThemeBlockSetTests
    {
        private ThemeBlockSet set;

        [SetUp] public void SetUp() => set = ScriptableObject.CreateInstance<ThemeBlockSet>();
        [TearDown] public void TearDown() { if (set != null) Object.DestroyImmediate(set); }

        [Test]
        public void Resolve_ReturnsADistinctDefPerRole()
        {
            var a = set.Resolve(ArenaBlock.FloorA);
            var b = set.Resolve(ArenaBlock.FloorB);
            var o = set.Resolve(ArenaBlock.Obstacle);
            var h = set.Resolve(ArenaBlock.HazardMarker);

            Assert.NotNull(a); Assert.NotNull(b); Assert.NotNull(o); Assert.NotNull(h);
            Assert.AreNotSame(a, b);
            Assert.AreNotSame(a, o);
            Assert.AreNotSame(o, h);
        }

        [Test]
        public void DefaultSet_UsesPrimitiveFallback_WhenNoPrefab()
        {
            Assert.IsTrue(ThemeBlockSet.UsesPrimitive(set.Resolve(ArenaBlock.FloorA)));
            Assert.IsTrue(ThemeBlockSet.UsesPrimitive(set.Resolve(ArenaBlock.Obstacle)));
            Assert.IsTrue(ThemeBlockSet.UsesPrimitive(null));
        }

        [Test]
        public void DefaultHeights_FloorIsThin_ObstacleIsRaised()
        {
            float floor = set.Resolve(ArenaBlock.FloorA).height;
            float obstacle = set.Resolve(ArenaBlock.Obstacle).height;
            Assert.Less(floor, 0.5f, "floor tiles should be thin slabs");
            Assert.Greater(obstacle, floor, "obstacles should be raised above the floor");
        }
    }
}
