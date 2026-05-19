using System.Collections.Generic;
using System.Text;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using CapsuleWars.Data.Classes;
using CapsuleWars.Units.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Text overlay listing active class synergies for the player team.
    /// Updates whenever <see cref="SynergyResolver"/> recomputes (battle
    /// start + after each KO).
    /// Legacy UI Text for M6; TMP polish in M10 with the rest of the UI.
    /// </summary>
    public class SynergyDisplay : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private BattleStateManager stateManager;
        [SerializeField] private Team team = Team.Player;

        private readonly Dictionary<UnitClass_SO, int> counts = new();

        private void Awake()
        {
            if (stateManager == null) stateManager = FindAnyObjectByType<BattleStateManager>();
        }

        private void OnEnable()
        {
            if (stateManager != null && stateManager.Synergies != null)
                stateManager.Synergies.OnRecomputed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (stateManager != null && stateManager.Synergies != null)
                stateManager.Synergies.OnRecomputed -= Refresh;
        }

        private void Refresh()
        {
            if (text == null) return;
            var registry = CombatServices.Registry;
            if (registry == null) { text.text = string.Empty; return; }

            counts.Clear();
            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null || u.IsDowned) continue;
                if (u.Team != team) continue;
                var root = u.GameObject != null ? u.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Status == null) continue;
                var cls = root.Status.UnitClass;
                if (cls == null) continue;

                counts.TryGetValue(cls, out int c);
                counts[cls] = c + 1;
            }

            var sb = new StringBuilder();
            sb.AppendLine("SYNERGIES");
            if (counts.Count == 0)
            {
                sb.AppendLine("  (none)");
            }
            else
            {
                foreach (var kv in counts)
                {
                    var tier = kv.Key.GetActiveTier(kv.Value);
                    string status = tier != null ? $"Tier {tier.threshold}+" : "(below threshold)";
                    // Use the SO asset name for display until I2 Localization is wired in
                    // (deferred from M0 — proper LocalizationManager.GetTranslation call lands
                    // when I2 asmdef + LanguageSource are set up, likely M10 polish).
                    sb.AppendLine($"  {kv.Key.name}: {kv.Value} {status}");
                }
            }
            text.text = sb.ToString();
        }
    }
}
