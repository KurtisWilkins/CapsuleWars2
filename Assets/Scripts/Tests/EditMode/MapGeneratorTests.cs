using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests MapGenerator output shape — node counts, ordering, boss-last invariant.
    /// </summary>
    public class MapGeneratorTests
    {
        [Test]
        public void GenerateDefault_HasFiveFloors()
        {
            var map = MapGenerator.GenerateDefault();
            Assert.AreEqual(5, map.Count);
        }

        [Test]
        public void GenerateDefault_LastNodeIsBoss()
        {
            var map = MapGenerator.GenerateDefault();
            Assert.AreEqual(NodeType.Boss, map.Get(map.Count - 1).Type);
        }

        [Test]
        public void Generate_RespectsTotalFloorsArgument()
        {
            Assert.AreEqual(8, MapGenerator.Generate(8).Count);
            Assert.AreEqual(3, MapGenerator.Generate(3).Count);
        }

        [Test]
        public void Generate_LastFloorIsAlwaysBoss()
        {
            for (int n = 2; n <= 10; n++)
            {
                var map = MapGenerator.Generate(n);
                Assert.AreEqual(NodeType.Boss, map.Get(map.Count - 1).Type, $"Boss should be last in {n}-floor map");
            }
        }

        [Test]
        public void Generate_ClampsBelowTwoToTwo()
        {
            Assert.AreEqual(2, MapGenerator.Generate(0).Count);
            Assert.AreEqual(2, MapGenerator.Generate(-5).Count);
        }

        [Test]
        public void Generate_NodesIndexedSequentially()
        {
            var map = MapGenerator.Generate(7);
            for (int i = 0; i < map.Count; i++)
            {
                Assert.AreEqual(i, map.Get(i).Index);
            }
        }
    }
}
