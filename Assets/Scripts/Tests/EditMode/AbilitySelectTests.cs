using System;
using System.Collections.Generic;
using CapsuleWars.Abilities;
using CapsuleWars.Core;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// The pure selection cores behind the ability filter strategies (Docs/05): keep-lowest-N by key,
    /// deterministic keep-random-N, and keep-where. The filter SOs supply the per-unit key/predicate at runtime.
    /// </summary>
    public class AbilitySelectTests
    {
        private sealed class Mock : IUnitRef
        {
            public string Id;
            public float Key;
            public GameObject GameObject => null;
            public Transform Transform => null;
            public Team Team => Team.Player;
            public bool IsDowned => false;
            public override string ToString() => Id;
        }

        private static List<IUnitRef> List(params Mock[] m) => new List<IUnitRef>(m);

        [Test]
        public void KeepLowestN_KeepsTheNLowestByKey()
        {
            var a = new Mock { Id = "a", Key = 0.9f };
            var b = new Mock { Id = "b", Key = 0.2f };
            var c = new Mock { Id = "c", Key = 0.5f };
            var list = List(a, b, c);

            AbilitySelect.KeepLowestN(list, u => ((Mock)u).Key, 2);

            Assert.AreEqual(2, list.Count);
            CollectionAssert.Contains(list, b);   // 0.2
            CollectionAssert.Contains(list, c);   // 0.5
            CollectionAssert.DoesNotContain(list, a);
        }

        [Test]
        public void KeepLowestN_NoChangeWhenCountAtOrBelowN()
        {
            var list = List(new Mock { Id = "a", Key = 1f }, new Mock { Id = "b", Key = 2f });
            AbilitySelect.KeepLowestN(list, u => ((Mock)u).Key, 5);
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void KeepRandomN_TrimsToN_AndIsDeterministicPerSeed()
        {
            List<IUnitRef> Build() => List(
                new Mock { Id = "a" }, new Mock { Id = "b" }, new Mock { Id = "c" }, new Mock { Id = "d" });

            var l1 = Build();
            AbilitySelect.KeepRandomN(l1, 2, new System.Random(123));
            var l2 = Build();
            AbilitySelect.KeepRandomN(l2, 2, new System.Random(123));

            Assert.AreEqual(2, l1.Count);
            CollectionAssert.AreEqual(
                l1.ConvertAll(u => u.ToString()),
                l2.ConvertAll(u => u.ToString()),
                "same seed should pick the same subset in the same order");
        }

        [Test]
        public void KeepWhere_RemovesNonMatches_PreservesOrder()
        {
            var a = new Mock { Id = "a", Key = 1f };
            var b = new Mock { Id = "b", Key = 0f };
            var c = new Mock { Id = "c", Key = 1f };
            var list = List(a, b, c);

            AbilitySelect.KeepWhere(list, u => ((Mock)u).Key > 0.5f);

            Assert.AreEqual(2, list.Count);
            Assert.AreSame(a, list[0]);
            Assert.AreSame(c, list[1]);
        }
    }
}
