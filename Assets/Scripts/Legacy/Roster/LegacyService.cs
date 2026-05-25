using System.Collections.Generic;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using CapsuleWars.Persistence;
using UnityEngine;

namespace CapsuleWars.Legacy.Roster
{
    /// <summary>
    /// Battle-scene-side bridge. On <see cref="BattleStateManager.OnBattleEnded"/>,
    /// matches each leaderboard entry against the persistent legacy roster
    /// by <c>UnitId</c> and merges per-battle stats into the legacy unit's
    /// lifetime totals. Saves to disk after each successful merge.
    /// Non-legacy units (enemies, one-off units) are silently ignored.
    /// </summary>
    public class LegacyService : MonoBehaviour
    {
        private BattleStateManager stateManager;

        private void Awake()
        {
            stateManager = FindAnyObjectByType<BattleStateManager>();
            if (stateManager != null) stateManager.OnBattleEnded += OnBattleEnded;
        }

        private void OnDestroy()
        {
            if (stateManager != null) stateManager.OnBattleEnded -= OnBattleEnded;
        }

        private void OnBattleEnded(BattleResult result, IReadOnlyList<BattleLeaderboardEntry> leaderboard)
        {
            var profile = LegacyStore.Current;
            if (profile == null || leaderboard == null || leaderboard.Count == 0) return;

            bool anyMerged = false;
            for (int i = 0; i < leaderboard.Count; i++)
            {
                var entry = leaderboard[i];
                var unit = profile.FindById(entry.UnitId);
                if (unit == null) continue;

                // For M8 we don't track per-unit faints in the leaderboard struct;
                // approximate: a faint happened if the unit took damage AND we
                // know it went down (lost team in a defeat is a coarse signal).
                bool fainted = result.WinningTeam.HasValue
                               && result.WinningTeam.Value != Team.Player
                               && entry.DamageTaken > 0;
                unit.Lifetime.MergeBattle(entry.DamageDealt, entry.DamageTaken, entry.Kills, fainted);
                anyMerged = true;
            }

            if (anyMerged) LegacyStore.Save();
        }
    }
}
