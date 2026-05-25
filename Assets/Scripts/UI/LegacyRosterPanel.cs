using System.Text;
using CapsuleWars.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Read-only text display of the persistent legacy roster.
    /// Refreshes on enable. Call <see cref="Refresh"/> after any
    /// LegacyStore.Save() to update.
    /// </summary>
    public class LegacyRosterPanel : MonoBehaviour
    {
        [SerializeField] private Text rosterText;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            if (rosterText == null) return;
            var profile = LegacyStore.Current;
            if (profile == null || profile.Units == null || profile.Units.Count == 0)
            {
                rosterText.text = "LEGACY ROSTER\n  (empty)";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"LEGACY ROSTER ({profile.Units.Count})");
            for (int i = 0; i < profile.Units.Count; i++)
            {
                var u = profile.Units[i];
                if (u == null) continue;
                sb.AppendLine($"  {u.DisplayName} [{u.Id}]");
                sb.AppendLine($"    Battles: {u.Lifetime.BattlesParticipated}  Kills: {u.Lifetime.Kills}  Dmg dealt: {u.Lifetime.DamageDealt}  Dmg taken: {u.Lifetime.DamageTaken}  Faints: {u.Lifetime.Faints}");
            }
            rosterText.text = sb.ToString();
        }
    }
}
