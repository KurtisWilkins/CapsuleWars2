using System.Collections.Generic;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
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
                state.AdvanceNode();
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
