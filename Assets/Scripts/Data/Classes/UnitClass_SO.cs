using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

namespace CapsuleWars.Data.Classes
{
    /// <summary>
    /// One synergy tier for a unit class. When at least <see cref="threshold"/>
    /// units of this class are deployed (per team), <see cref="teamBuffs"/>
    /// apply to all units of this class on that team.
    /// </summary>
    [Serializable]
    public class ClassSynergyTier
    {
        [Tooltip("Number of same-class units required for this tier to activate.")]
        [Min(1)] public int threshold = 3;

        [Tooltip("I2 term key for a localized tier description.")]
        public string descTermKey;

        [Tooltip("Buffs applied to every unit of this class on the team when active.")]
        public List<StatBuff> teamBuffs = new();

        [Tooltip("Buffs applied to the WHOLE team (any class) when active. Rare — high tiers / support classes (Docs/09).")]
        public List<StatBuff> globalBuffs = new();

        [Tooltip("Behavioral [code] effects (heal-on-kill, heal-on-hit, …) granted to same-class units when active (Docs/09). Applied by the unit's ISynergyBehaviorSink, not the stat layer.")]
        public List<SynergyEffect> synergyEffects = new();
    }

    /// <summary>
    /// Defines a unit class (Warrior, Mage, Healer, …). Carries class-level
    /// synergy thresholds + buffs that apply when the deployed team has
    /// enough units of this class.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitClass", menuName = "CapsuleWars/Classes/Unit Class", order = 100)]
    public class UnitClass_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and Database lookup.")]
        [SerializeField] private string classId;

        [Tooltip("I2 term key for class display name (e.g. Class.Warrior.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("I2 term key for class description.")]
        [SerializeField] private string descTermKey;

        [SerializeField] private Sprite icon;

        [Tooltip("Synergy tiers in ascending threshold order (e.g. 3, 5, 7). Highest met threshold wins.")]
        [SerializeField] private List<ClassSynergyTier> tiers = new();

        public string ClassId => classId;
        public string NameTermKey => nameTermKey;
        public string DescTermKey => descTermKey;
        public Sprite Icon => icon;
        public IReadOnlyList<ClassSynergyTier> Tiers => tiers;

        /// <summary>
        /// Returns the highest-threshold tier whose threshold is met by
        /// <paramref name="deployedCount"/>, or null if no tier activates.
        /// </summary>
        public ClassSynergyTier GetActiveTier(int deployedCount)
        {
            ClassSynergyTier active = null;
            if (tiers == null) return null;
            for (int i = 0; i < tiers.Count; i++)
            {
                var t = tiers[i];
                if (t == null) continue;
                if (deployedCount < t.threshold) continue;
                if (active == null || t.threshold > active.threshold) active = t;
            }
            return active;
        }
    }
}
