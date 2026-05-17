using System.Collections.Generic;

namespace CapsuleWars.Combat.State
{
    /// <summary>
    /// Carries the downed-this-battle flag forward between consecutive
    /// battles in a session. On battle start, units whose ID is in
    /// <see cref="WasDownedPreviousBattle"/> are restored to 50% max HP
    /// instead of 100%. No stacking: being downed twice in a row still
    /// only deducts 50% — the rule looks one battle back, not cumulative.
    ///
    /// Static within the Unity session. Persists across scene reloads
    /// (the static lives as long as the domain) but not across editor
    /// restarts. M7+ will swap this for run-scoped state on RunStateDTO.
    /// </summary>
    public static class BattleRosterState
    {
        private static readonly HashSet<string> wasDownedPreviousBattle = new();
        private static readonly HashSet<string> wasDownedThisBattle = new();

        public static bool WasDownedPreviousBattle(string unitId)
        {
            return !string.IsNullOrEmpty(unitId) && wasDownedPreviousBattle.Contains(unitId);
        }

        public static void MarkDownedThisBattle(string unitId)
        {
            if (string.IsNullOrEmpty(unitId)) return;
            wasDownedThisBattle.Add(unitId);
        }

        /// <summary>
        /// Call once at battle end. Previous-battle flag becomes "did you
        /// just get downed?" so the next battle's start can apply the rule.
        /// </summary>
        public static void CommitBattleResult()
        {
            wasDownedPreviousBattle.Clear();
            foreach (var id in wasDownedThisBattle) wasDownedPreviousBattle.Add(id);
            wasDownedThisBattle.Clear();
        }

        /// <summary>Resets all carry-forward state. Use between test runs / dev resets.</summary>
        public static void ClearAll()
        {
            wasDownedPreviousBattle.Clear();
            wasDownedThisBattle.Clear();
        }
    }
}
