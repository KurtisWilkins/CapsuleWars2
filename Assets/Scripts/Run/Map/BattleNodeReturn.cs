using System.Collections.Generic;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CapsuleWars.Run.Map
{
    /// <summary>
    /// Lives in the Battle scene. Hooks <see cref="BattleStateManager.OnBattleEnded"/>;
    /// when the battle resolves, it updates the run state and returns to the
    /// map scene. No-op when not in a run (so M3 standalone testing still works).
    /// </summary>
    public class BattleNodeReturn : MonoBehaviour
    {
        [Tooltip("Map scene to return to after the battle resolves.")]
        [SerializeField] private string mapSceneName = "Test_M7_Map";

        [Tooltip("Gold awarded for winning a regular Combat node.")]
        [SerializeField, Min(0)] private int goldOnCombatWin = 25;

        [Tooltip("Gold awarded for winning a Boss node.")]
        [SerializeField, Min(0)] private int goldOnBossWin = 100;

        [Tooltip("Part catalog the random recruit generator draws unlocked parts from. Optional — without it recruits are identity-only.")]
        [SerializeField] private PartCatalog_SO partCatalog;

        [Header("Loot drops (BTS-G)")]
        [Tooltip("Loot table rolled on a regular Combat win — drops into the run inventory. None = no item drop.")]
        [SerializeField] private LootTable_SO combatLootTable;

        [Tooltip("Loot table rolled on an Elite win (rarity-skewed). None = falls back to the combat table.")]
        [SerializeField] private LootTable_SO eliteLootTable;

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

        private void OnBattleEnded(BattleResult result, IReadOnlyList<BattleLeaderboardEntry> _)
        {
            if (!RunSession.IsActive) return;
            var state = RunSession.Current;

            if (result.PlayerWon)
            {
                state.AddGold(state.IsBossEncounter ? goldOnBossWin : goldOnCombatWin);

                // Non-boss wins (Combat/Elite): an equipment drop + a roguelike unit recruit.
                // Boss = gold-only (Docs/07). The drop is rolled deterministically per node, so a
                // reload reproduces it, and lands in the run's loose inventory (BTS-G).
                if (!state.IsBossEncounter)
                {
                    var nodeType = state.CurrentNode != null ? state.CurrentNode.Type : NodeType.Combat;
                    var table = nodeType == NodeType.Elite && eliteLootTable != null ? eliteLootTable : combatLootTable;
                    if (table != null)
                        LootGrant.GrantTo(state, table, state.Seed ^ (state.CurrentNodeId * 31 + 17));

                    state.AddRecruit(RandomUnitGenerator.Generate(
                        partCatalog, LegacyStore.Current?.PlayerProfile, null,
                        state.CurrentFloor, state.Recruits.Count));
                }

                // Clear the node; the player picks their next node on the map. The
                // controller stitches a new segment when a top-row boss is cleared.
                state.MarkCurrentCleared();

                // Persist NOW: the win path didn't save before the scene load, so a loose
                // drop (and the gold/recruit) would be lost on the way back to the map.
                RunSession.Save();
            }
            else
            {
                state.IsLost = true;
            }

            state.IsBossEncounter = false;
            if (!string.IsNullOrEmpty(mapSceneName))
                SceneManager.LoadScene(mapSceneName);
        }
    }
}
