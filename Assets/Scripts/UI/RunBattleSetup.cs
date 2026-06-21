using CapsuleWars.Core;
using CapsuleWars.Run;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.UI
{
    /// <summary>
    /// Battle-scene-side applier. On Awake, drains <see cref="PurchasedItems"/>
    /// for each unit it can find in the scene and equips the items on the
    /// matching UnitRoot via UnitStatusController.Equip.
    /// Also bumps enemy stats on boss encounters (RunSession.Current.IsBossEncounter).
    /// </summary>
    public class RunBattleSetup : MonoBehaviour
    {
        [Tooltip("Boss enemy stat multiplier applied to MaxHp/Atk at battle start.")]
        [SerializeField, Min(1f)] private float bossStatMultiplier = 1.5f;

        private void Start()
        {
            var roots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);

            // Apply purchased equipment per unit.
            for (int i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.Status == null) continue;
                var purchased = PurchasedItems.Drain(root.UnitId);
                for (int j = 0; j < purchased.Count; j++)
                {
                    var item = purchased[j];
                    if (item != null) root.Status.Equip(item.Slot, item);
                }
            }

            // Enemy scaling: per-depth difficulty (RunState.DifficultyMultiplier) times the
            // boss multiplier on boss encounters.
            if (RunSession.IsActive)
            {
                var s = RunSession.Current;
                float depthMul = Mathf.Max(1f, s.DifficultyMultiplier);
                float mul = depthMul * (s.IsBossEncounter ? bossStatMultiplier : 1f);
                if (mul > 1.001f)
                {
                    for (int i = 0; i < roots.Length; i++)
                    {
                        var root = roots[i];
                        if (root == null || root.Team != Team.Enemy) continue;
                        BoostUnit(root, mul);
                    }
                }
            }
        }

        private static void BoostUnit(UnitRoot root, float multiplier)
        {
            if (root.Health == null) return;
            // Bump current and max HP by setting the unit to a higher percent —
            // effectively gives it more HP for this battle without mutating
            // the prefab's serialized base stat.
            root.Health.RestoreToPercent(multiplier);
        }
    }
}
