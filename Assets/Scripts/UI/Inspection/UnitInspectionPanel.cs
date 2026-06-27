using System.Text;
using CapsuleWars.Abilities;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI.Inspection
{
    /// <summary>
    /// Inspects a unit and shows its identity, element/class, computed stats,
    /// abilities, and equipment/body parts. Stats come from the real source
    /// (UnitStatusController's modified getters), and the panel refreshes live
    /// via OnStatsChanged / OnStatusApplied / OnStatusExpired — so equipping an
    /// item in the customization screen updates the displayed stats immediately.
    ///
    /// Works during deployment (roster + placed units) and during battle; the
    /// caller decides what to inspect via <see cref="Show"/>. uGUI + legacy Text
    /// to match the existing M3 UI; drop a UIThemeApplier on the panel root for
    /// theming. Build the panel hierarchy in-scene/as a prefab and wire the Text
    /// refs; the deployment grid (Slice C) calls Show(unit) on selection.
    /// </summary>
    public class UnitInspectionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text nameText;
        [SerializeField] private Text elementClassText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text abilitiesText;
        [SerializeField] private Text equipmentText;
        [SerializeField] private Button closeButton;

        private UnitRoot current;
        private UnitStatusController status;
        private AbilityController abilities;
        private UnitCustomization customization;

        public UnitRoot Current => current;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDisable() => Unsubscribe();

        /// <summary>Inspect a unit (no-op-hides on null). Subscribes for live refresh.</summary>
        public void Show(UnitRoot unit)
        {
            if (unit == null) { Hide(); return; }

            Unsubscribe();
            current = unit;
            status = unit.Status;
            abilities = unit.GetComponentInChildren<AbilityController>();
            customization = unit.GetComponentInChildren<UnitCustomization>();
            Subscribe();

            if (panelRoot != null) panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            Unsubscribe();
            current = null;
            status = null;
            abilities = null;
            customization = null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Subscribe()
        {
            if (status == null) return;
            status.OnStatsChanged += Refresh;
            status.OnStatusApplied += OnStatusChanged;
            status.OnStatusExpired += OnStatusChanged;
        }

        private void Unsubscribe()
        {
            if (status == null) return;
            status.OnStatsChanged -= Refresh;
            status.OnStatusApplied -= OnStatusChanged;
            status.OnStatusExpired -= OnStatusChanged;
        }

        private void OnStatusChanged(StatusEffect_SO _) => Refresh();

        public void Refresh()
        {
            if (current == null) return;

            if (nameText != null) nameText.text = current.DisplayName;

            if (elementClassText != null)
            {
                string element = status != null && status.PrimaryElement != null
                    ? Label(status.PrimaryElement.ElementId, status.PrimaryElement.NameTermKey) : "—";
                string cls = status != null && status.UnitClass != null
                    ? Label(status.UnitClass.ClassId, status.UnitClass.NameTermKey) : "—";
                elementClassText.text = $"Element: {element}    Class: {cls}";
            }

            if (statsText != null && status != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"HP   {status.MaxHp}");
                sb.AppendLine($"ATK  {status.Atk}");
                sb.AppendLine($"DEF  {status.Def}");
                sb.Append($"SPD  {status.Speed:0.0}");
                statsText.text = sb.ToString();
            }

            if (abilitiesText != null) abilitiesText.text = FormatAbilities();
            if (equipmentText != null) equipmentText.text = FormatEquipment();
        }

        private string FormatAbilities()
        {
            if (abilities == null || abilities.Runtimes.Count == 0) return "Abilities: (none)";

            var sb = new StringBuilder("Abilities:");
            foreach (var rt in abilities.Runtimes)
            {
                if (rt?.Ability == null) continue;
                string name = Label(rt.Ability.AbilityId, rt.Ability.NameTermKey);
                sb.Append('\n').Append(rt.IsLocked ? $"  {name} (locked)" : $"  {name}");
            }
            return sb.ToString();
        }

        private string FormatEquipment()
        {
            var sb = new StringBuilder("Equipment:");
            int shown = 0;
            if (status != null)
            {
                foreach (var eq in status.Equipment)
                {
                    if (eq.item == null) continue;
                    sb.Append('\n').Append($"  {eq.slot}: {Label(eq.item.EquipmentId, eq.item.NameTermKey)}");
                    shown++;
                }
            }
            if (shown == 0) sb.Append("\n  (none)");

            // Body parts are cosmetic (armor carries the stats); show the applied
            // whole-unit definition when one is set.
            if (customization != null && customization.Definition != null)
                sb.Append('\n').Append($"Body: {customization.Definition.UnitId}");

            // Head is a swappable part (Head-as-part-type) — show the chosen head version.
            if (customization != null)
            {
                var parts = customization.AppliedParts;
                for (int i = 0; i < parts.Count; i++)
                    if (parts[i].slot == PartSlot.Head && parts[i].part != null)
                    {
                        sb.Append('\n').Append($"Head: {parts[i].part.PartId}");
                        break;
                    }
            }

            return sb.ToString();
        }

        // Prefer the readable stable id; fall back to the I2 term key. (A future
        // pass can localize via I2 once UI strings are wired.)
        private static string Label(string id, string termKey)
        {
            if (!string.IsNullOrEmpty(id)) return id;
            return string.IsNullOrEmpty(termKey) ? "—" : termKey;
        }
    }
}
