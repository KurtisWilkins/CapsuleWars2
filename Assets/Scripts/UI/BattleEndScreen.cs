using System.Collections.Generic;
using System.Linq;
using System.Text;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Listens to <see cref="BattleStateManager.OnBattleEnded"/> and shows a
    /// Victory/Defeat banner with a top-5 leaderboard. Plain UnityEngine.UI
    /// for M3; TMP/polish in M10.
    /// </summary>
    public class BattleEndScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text leaderboardText;
        [SerializeField] private Button restartButton;
        [SerializeField] private BattleStateManager stateManager;

        private void Awake()
        {
            if (stateManager == null) stateManager = FindAnyObjectByType<BattleStateManager>();
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (stateManager != null) stateManager.OnBattleEnded += OnBattleEnded;
        }

        private void OnDisable()
        {
            if (stateManager != null) stateManager.OnBattleEnded -= OnBattleEnded;
        }

        private void OnBattleEnded(BattleResult result, IReadOnlyList<BattleLeaderboardEntry> leaderboard)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (titleText != null)
            {
                titleText.text = result.IsDraw
                    ? "DRAW"
                    : (result.PlayerWon ? "VICTORY" : "DEFEAT");
            }

            if (subtitleText != null)
            {
                string reasonLabel = result.Reason switch
                {
                    BattleEndReason.KnockOut => "All enemies down",
                    BattleEndReason.SuddenDeath => "Sudden death",
                    BattleEndReason.Draw => "Mutual KO",
                    _ => string.Empty
                };
                subtitleText.text = $"{reasonLabel} · {result.Duration:F1}s";
            }

            if (leaderboardText != null) leaderboardText.text = FormatLeaderboard(leaderboard);
        }

        private static string FormatLeaderboard(IReadOnlyList<BattleLeaderboardEntry> entries)
        {
            if (entries == null || entries.Count == 0) return "(no stats recorded)";

            var byDamage = entries.OrderByDescending(e => e.DamageDealt).Take(5).ToList();
            var byTank = entries.OrderByDescending(e => e.DamageTaken).Take(5).ToList();
            var byKills = entries.OrderByDescending(e => e.Kills).Take(5).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("TOP DAMAGE");
            foreach (var e in byDamage)
                sb.AppendLine($"  {e.DisplayName}: {e.DamageDealt}");
            sb.AppendLine();
            sb.AppendLine("TOP TANK (damage taken)");
            foreach (var e in byTank)
                sb.AppendLine($"  {e.DisplayName}: {e.DamageTaken}");
            sb.AppendLine();
            sb.AppendLine("KILLS");
            foreach (var e in byKills)
                sb.AppendLine($"  {e.DisplayName}: {e.Kills}");
            return sb.ToString();
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
