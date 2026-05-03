using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Weapons
{
    /// <summary>
    /// Defines one weapon class (1H Sword, 2H Bow, Unarmed, ...). Drives
    /// the Animator sub-state machine selection via <see cref="WeaponTypeId"/>
    /// and supplies attack range, cooldown, and attack-state count.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponClass", menuName = "CapsuleWars/Weapon Class", order = 20)]
    public class WeaponClass_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and the Database lookup.")]
        [SerializeField] private string weaponClassId;

        [Tooltip("I2 term key for the weapon class display name.")]
        [SerializeField] private string nameTermKey;

        [Tooltip("Animator parameter value for selecting this weapon's sub-state machine. Stable across builds.")]
        [SerializeField] private int weaponTypeId;

        [Tooltip("How many AttackN states the sub-state machine exposes (1..N).")]
        [SerializeField] private int attackCount = 1;

        [Tooltip("Distance at which the unit halts and starts swinging (world units).")]
        [SerializeField] private float attackRange = 2f;

        [Tooltip("Seconds between attack triggers. Includes wind-up + recovery.")]
        [SerializeField] private float attackCooldown = 1.5f;

        [SerializeField] private WeaponHandedness handedness = WeaponHandedness.OneHanded;

        public string WeaponClassId => weaponClassId;
        public string NameTermKey => nameTermKey;
        public int WeaponTypeId => weaponTypeId;
        public int AttackCount => attackCount < 1 ? 1 : attackCount;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public WeaponHandedness Handedness => handedness;
    }
}
