using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Classes;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;

namespace CapsuleWars.Combat.Stats
{
    /// <summary>
    /// Computes active class synergy buffs and pushes them to each
    /// unit's UnitStatusController. Recomputed at battle start, on KO,
    /// and on revive (synergy might cross a threshold either way).
    /// </summary>
    public class SynergyResolver
    {
        private readonly ICombatRegistry registry;
        private readonly Dictionary<(Team, UnitClass_SO), int> classCounts = new();
        private readonly Dictionary<UnitStatusController, List<StatBuff>> bufferBuilder = new();

        public SynergyResolver(ICombatRegistry registry)
        {
            this.registry = registry;
        }

        public event Action OnRecomputed;

        public void RecomputeSynergies()
        {
            if (registry == null) return;

            classCounts.Clear();
            bufferBuilder.Clear();

            // First pass: count live units per (team, class) and seed buffer for every status controller.
            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                // IUnitRef is an interface, so '== null' is a plain reference check
                // that does NOT catch a destroyed UnityEngine.Object. Test the
                // underlying object too, before touching u.GameObject (which throws
                // on a destroyed unit).
                if (u == null || (u is UnityEngine.Object obj && obj == null)) continue;
                var root = u.GameObject != null ? u.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Status == null) continue;

                bufferBuilder[root.Status] = new List<StatBuff>();

                if (u.IsDowned) continue;
                var cls = root.Status.UnitClass;
                if (cls == null) continue;

                var key = (u.Team, cls);
                classCounts.TryGetValue(key, out int count);
                classCounts[key] = count + 1;
            }

            // Second pass: for each (team, class) that has an active tier, add its team buffs to every
            // unit of that class+team.
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null || (u is UnityEngine.Object o && o == null) || u.IsDowned) continue;
                var root = u.GameObject != null ? u.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (root == null || root.Status == null) continue;
                var cls = root.Status.UnitClass;
                if (cls == null) continue;

                var key = (u.Team, cls);
                if (!classCounts.TryGetValue(key, out int count)) continue;
                var tier = cls.GetActiveTier(count);
                if (tier == null || tier.teamBuffs == null) continue;

                if (bufferBuilder.TryGetValue(root.Status, out var list))
                {
                    list.AddRange(tier.teamBuffs);
                }
            }

            // Push computed buffs to each status controller.
            foreach (var kv in bufferBuilder)
            {
                kv.Key.SetSynergyBuffs(kv.Value);
            }

            OnRecomputed?.Invoke();
        }
    }
}
