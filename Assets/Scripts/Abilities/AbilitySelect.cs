using System;
using System.Collections.Generic;
using CapsuleWars.Core;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Pure, testable list-selection helpers shared by the filter strategies. No Unity-runtime deps, so the
    /// selection logic (sort-and-trim, deterministic random pick, keep-where) is EditMode-tested directly while
    /// the filter SOs supply the per-unit key/predicate from runtime state.
    /// </summary>
    public static class AbilitySelect
    {
        /// <summary>Keep the N candidates with the lowest <paramref name="keyOf"/> value (ascending); trim the rest.</summary>
        public static void KeepLowestN(List<IUnitRef> candidates, Func<IUnitRef, float> keyOf, int n)
        {
            if (candidates == null || keyOf == null) return;
            if (n < 0) n = 0;
            if (candidates.Count <= n) return;
            candidates.Sort((a, b) => keyOf(a).CompareTo(keyOf(b)));
            candidates.RemoveRange(n, candidates.Count - n);
        }

        /// <summary>Keep N candidates chosen with the supplied rng (deterministic for tests), via partial Fisher-Yates.</summary>
        public static void KeepRandomN(List<IUnitRef> candidates, int n, Random rng)
        {
            if (candidates == null || rng == null) return;
            if (n < 0) n = 0;
            if (candidates.Count <= n) return;
            for (int i = 0; i < n; i++)
            {
                int j = i + rng.Next(candidates.Count - i);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }
            candidates.RemoveRange(n, candidates.Count - n);
        }

        /// <summary>Remove candidates that fail <paramref name="keep"/> (keeps matches, preserves order).</summary>
        public static void KeepWhere(List<IUnitRef> candidates, Predicate<IUnitRef> keep)
        {
            if (candidates == null || keep == null) return;
            candidates.RemoveAll(c => !keep(c));
        }
    }
}
